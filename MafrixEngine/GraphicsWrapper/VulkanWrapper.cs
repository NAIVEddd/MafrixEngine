using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using MafrixEngine.ModelLoaders;
using MafrixEngine.Cameras;
using MafrixEngine.Source.Interface;

using Image = Silk.NET.Vulkan.Image;
using Semaphore = Silk.NET.Vulkan.Semaphore;
using SlImage = SixLabors.ImageSharp.Image;
using glTFLoader.Schema;

namespace MafrixEngine.GraphicsWrapper
{
    using Buffer = Silk.NET.Vulkan.Buffer;
    using Vec2 = Vector2D<float>;
    using Vec3 = Vector3D<float>;
    using Mat4 = Matrix4X4<float>;
    using Camera = Cameras.Camera;
    using Sampler = Silk.NET.Vulkan.Sampler;

    public struct Mesh : IRenderable, IDisposable
    {
        public UniformBufferObject matrices;
        public Buffer[] uniformBuffer;
        public DeviceMemory[] uniformMemory;
        public Vertex[] vertices;
        public UInt32[] indices;
        public Buffer vertexBuffer;
        public DeviceMemory vertexBufferMemory;
        public Buffer indicesBuffer;
        public DeviceMemory indiceBufferMemory;
        public DescriptorPool descriptorPool;
        public DescriptorSet[] descriptorSets;
        public Mat4 matrix;
        public float frameRotate;
        public Gltf2RootNode gltf2;

        private Vk vk;
        private Device device;
        public Mesh(Vk vk, Device device)
        {
            this.vk = vk;
            this.device = device;
            matrices = default;
            uniformBuffer = default;
            uniformMemory = default;
            vertices = default;
            indices = default;
            vertexBuffer = default;
            vertexBufferMemory = default;
            indicesBuffer = default;
            indiceBufferMemory = default;
            descriptorPool = default;
            descriptorSets = default;
            matrix = default;
            frameRotate = default;
            gltf2 = default;
        }

        public void BindCommand(Vk vk, CommandBuffer commandBuffer, Action<int> action)
        {
            gltf2.BindCommand(vk, commandBuffer, vertexBuffer, indicesBuffer, action);
        }

        public unsafe void Dispose()
        {
            vk.FreeMemory(device, indiceBufferMemory, null);
            vk.FreeMemory(device, vertexBufferMemory, null);
            vk.DestroyBuffer(device, vertexBuffer, null);
            vk.DestroyBuffer(device, indicesBuffer, null);
            foreach (var memory in uniformMemory)
            {
                vk.FreeMemory(device, memory, null);
            }
            foreach (var buffer in uniformBuffer)
            {
                vk.DestroyBuffer(device, buffer, null);
            }
            vk.DestroyDescriptorPool(device, descriptorPool, null);

            gltf2.Dispose();
        }
    }

    public class VulkanWrapper : IDisposable
    {
        public Camera camera;

        private VkContext vkContext;
        private Vk vk { get => vkContext.vk; }
        public Instance instance { get => vkContext.instance; }
        public PhysicalDevice physicalDevice { get => vkContext.physicalDevice; }
        public Device device { get => vkContext.device; }
        public Queue graphicsQueue;
        public IWindow window;
        public KhrSurface khrSurface;
        public SurfaceKHR surface;
        public KhrSwapchain khrSwapchain;
        public SwapchainKHR swapchain;
        public RenderPass renderPass;
        public DescriptorSetLayout descriptorSetLayout;
        public PipelineLayout pipelineLayout;
        public Pipeline graphicsPipeline;
        public Framebuffer[] swapchainFramebuffers;

        public Mesh[] meshes;

        public string textureName;
        public UInt32 mipLevels;

        public Image depthImage;
        public DeviceMemory depthImageMemory;
        public ImageView depthImageView;
        public Sampler textureSampler;
        public CommandPool commandPool;
        public CommandBuffer[] commandBuffers;
        public Semaphore[] imageAvailableSemaphores;
        public Semaphore[] renderFinishedSemaphores;
        public Fence[] inFlightFences;
        public Fence[] imagesInFlight;
        public uint currentFrame;
        public Image[] swapchainImages;
        public Format swapchainImageFormat;
        public Extent2D swapchainExtent;
        public ImageView[] swapchainImageViews;
        private VkDescriptorPollSize poolSizeInfo;

        private StagingBuffer staging;

        private ExtDebugUtils debugUtils;
        private DebugUtilsMessengerEXT debugMessager;

        public const int MaxFrameInFlight = 8;
        /// <summary>
        /// Extensions and ValidationLayers
        /// </summary>
        private string[] deviceExtensions = { KhrSwapchain.ExtensionName };
#if DEBUG
        public const bool EnableValidationLayers = true;
#else
        public const bool EnableValidationLayers = false;
#endif
        public unsafe static int GetArrayByteSize<T>(T[] array)
        {
            return Marshal.SizeOf<T>() * array.Length;
        }

        public VulkanWrapper()
        {
            window = InitWindow();
            vkContext = new VkContext();
        }

        public IWindow InitWindow()
        {
            var opts = WindowOptions.DefaultVulkan with
            {
                Size = new Vector2D<int>(1920, 1080),
                Title = "Vulkan"
            };
            var window = Window.Create(opts);
            window.Initialize();

            if (window!.VkSurface is null)
            {
                throw new NotSupportedException("Windowing platform doesn't support Vulkan.");
            }

            return window;
        }

        public void InitVulkan()
        {
            vkContext.Initialize("Render", new Version32(0, 0, 1));
            vk.TryGetInstanceExtension(instance, out khrSurface);
            vk.TryGetInstanceExtension(instance, out khrSwapchain);
            vk.GetDeviceQueue(device, 0, 0, out graphicsQueue);
            CreateSurface();
            // extensions
            SetupDebugMessager();

            staging = new StagingBuffer(vk, physicalDevice, device);

            CreateSwapChain();
            CreateImageViews();
            CreateRenderPass();
            CreateGraphicsPipeline();
            CreateCommandPool();
            CreateDepthResources();
            CreateFramebuffers();
            LoadModel();
            CreateTextureSampler();
            CreateVertexBuffer();
            CreateIndexBuffer();
            CreateUniformBuffers();
            CreateDescriptorPool();
            CreateDescriptorSets();
            CreateCommandBuffers();
            CreateSyncObjects();

            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i].vertices = Array.Empty<Vertex>();
                meshes[i].indices = Array.Empty<uint>();
                meshes[i].gltf2.vertices = Array.Empty<Vertex>();
                meshes[i].gltf2.indices = Array.Empty<uint>();
            }

            var pos = new Vec3(35.0f, 200.0f, 35.0f);
            var dir = new Vec3(200.0f, 245.0f, 200.0f) - pos;
            camera = new Camera(new CameraCoordinate(pos, dir, new Vec3(0.0f, -1.0f, 0.0f)),
                            new ProjectInfo(45.0f, (float)swapchainExtent.Width / (float)swapchainExtent.Height));
            startTime = DateTime.Now;
        }

        public void MainLoop()
        {
            window.Render += DrawFrame;
            window.Run();
            vk.DeviceWaitIdle(device);
        }
        private unsafe void DrawFrame(double obj)
        {
            var fence = inFlightFences[currentFrame];
            vk.WaitForFences(device, 1, in fence, Vk.True, ulong.MaxValue);

            uint imageIndex;
            Result result = khrSwapchain.AcquireNextImage(device, swapchain, ulong.MaxValue, imageAvailableSemaphores[currentFrame], default, &imageIndex);
            if(result == Result.ErrorOutOfDateKhr)
            {
                // RecreateSwapChain()
                return;
            }
            else if(result != Result.Success && result != Result.SuboptimalKhr)
            {
                throw new Exception("failed to acquire swapchain images.");
            }

            var imageFence = imagesInFlight[imageIndex];
            if (imageFence.Handle != 0)
            {
                vk.WaitForFences(device, 1, in imageFence, Vk.True, ulong.MaxValue);
            }
            imagesInFlight[imageIndex] = fence;

            UpdateUniformBuffer(imageIndex);

            SubmitInfo submitInfo = new SubmitInfo { SType = StructureType.SubmitInfo };

            Semaphore[] waitSemaphores = { imageAvailableSemaphores[currentFrame] };
            PipelineStageFlags[] waitStages = { PipelineStageFlags.ColorAttachmentOutputBit };
            submitInfo.WaitSemaphoreCount = 1;
            var signalSemaphore = renderFinishedSemaphores[currentFrame];
            fixed (Semaphore* waitSemaphoresPtr = waitSemaphores)
            {
                fixed (PipelineStageFlags* waitStagesPtr = waitStages)
                {
                    submitInfo.PWaitSemaphores = waitSemaphoresPtr;
                    submitInfo.PWaitDstStageMask = waitStagesPtr;

                    submitInfo.CommandBufferCount = 1;
                    var buffer = commandBuffers[imageIndex];
                    submitInfo.PCommandBuffers = &buffer;

                    submitInfo.SignalSemaphoreCount = 1;
                    submitInfo.PSignalSemaphores = &signalSemaphore;

                    vk.ResetFences(device, 1, &fence);

                    if (vk.QueueSubmit
                            (graphicsQueue, 1, &submitInfo, fence) != Result.Success)
                    {
                        throw new Exception("failed to submit draw command buffer!");
                    }
                }
            }

            fixed (SwapchainKHR* swapchainPtr = &swapchain)
            {
                PresentInfoKHR presentInfo = new PresentInfoKHR
                {
                    SType = StructureType.PresentInfoKhr,
                    WaitSemaphoreCount = 1,
                    PWaitSemaphores = &signalSemaphore,
                    SwapchainCount = 1,
                    PSwapchains = swapchainPtr,
                    PImageIndices = &imageIndex
                };

                result = khrSwapchain.QueuePresent(graphicsQueue, &presentInfo);
            }

            if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr)// || framebufferResized)
            {
                //framebufferResized = false;
                //RecreateSwapChain();
            }
            else if (result != Result.Success)
            {
                throw new Exception("failed to present swap chain image!");
            }

            currentFrame = (currentFrame+1) % MaxFrameInFlight;
        }

        private DateTime startTime;
        private unsafe void UpdateUniformBuffer(uint index)
        {
            var time = (float)(DateTime.Now - startTime).TotalSeconds;

            camera.GetProjAndView(out var proj, out var view);

            var uboptr = stackalloc UniformBufferObject[1];
            uboptr->proj = proj;
            uboptr->view = view;

            foreach (var mesh in meshes)
            {
                var descs = mesh.gltf2.DescriptorSetCount;
                var offset = index * descs * sizeof(UniformBufferObject);
                mesh.gltf2.UpdateUniformBuffer(out var modelMatrices);
                void* data = null;
                ulong datasize = (ulong)(Unsafe.SizeOf<UniformBufferObject>());
                var model = Matrix4X4.CreateRotationY<float>(time * mesh.frameRotate);
                for (var i = 0; i < modelMatrices.Length; i++)
                {
                    uboptr->model = modelMatrices[i] * mesh.matrix * model;
                    var idx = index * descs + i;
                    vk.MapMemory(device, mesh.uniformMemory[idx], 0, datasize, 0, ref data);
                    Unsafe.CopyBlock(data, uboptr, (uint)datasize);
                    vk.UnmapMemory(device, mesh.uniformMemory[idx]);
                }
            }
        }

        private unsafe void CreateSurface()
        {
            surface = window.VkSurface!.Create<AllocationCallbacks>(instance.ToHandle(), null).ToSurface();
        }

        private unsafe void SetupDebugMessager()
        {
            if (!EnableValidationLayers) return;
            if (!vk.TryGetInstanceExtension(instance, out debugUtils)) return;

            var createInfo = new DebugUtilsMessengerCreateInfoEXT(StructureType.DebugUtilsMessengerCreateInfoExt);
            createInfo.MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt |
                                         DebugUtilsMessageSeverityFlagsEXT.WarningBitExt |
                                         DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt;
            createInfo.MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt |
                                     DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt |
                                     DebugUtilsMessageTypeFlagsEXT.ValidationBitExt;
            createInfo.PfnUserCallback = (DebugUtilsMessengerCallbackFunctionEXT)DebugCallback;

            if(debugUtils.CreateDebugUtilsMessenger(instance, in createInfo, null, out debugMessager) != Result.Success)
            {
                throw new Exception("Failed to create debug messager");
            }
        }

        private unsafe uint DebugCallback
        (
            DebugUtilsMessageSeverityFlagsEXT messageSeverity,
            DebugUtilsMessageTypeFlagsEXT messageTypes,
            DebugUtilsMessengerCallbackDataEXT* pCallbackData,
            void* pUserData
        )
        {
            if(messageSeverity > DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt)
            {
                Console.WriteLine($"{messageSeverity} {messageTypes}" + Marshal.PtrToStringAnsi((nint)pCallbackData->PMessage));
            }
            return Vk.False;
        }

        public struct SwapChainSupportDetails
        {
            public SurfaceCapabilitiesKHR Capabilities { get; set; }
            public SurfaceFormatKHR[] Formats { get; set; }
            public PresentModeKHR[] PresentModes { get; set; }
        }
        private unsafe SwapChainSupportDetails QuerySwapChainSupport(PhysicalDevice device)
        {
            var details = new SwapChainSupportDetails();
            khrSurface.GetPhysicalDeviceSurfaceCapabilities(device, surface, out var surfaceCapabilities);
            details.Capabilities = surfaceCapabilities;
            var formatCount = 0u;
            khrSurface.GetPhysicalDeviceSurfaceFormats(device, surface, &formatCount, null);
            if(formatCount != 0)
            {
                details.Formats = new SurfaceFormatKHR[formatCount];
                using var mem = GlobalMemory.Allocate((int)formatCount * sizeof(SurfaceFormatKHR));
                var formats = (SurfaceFormatKHR*)Unsafe.AsPointer(ref mem.GetPinnableReference());

                khrSurface.GetPhysicalDeviceSurfaceFormats(device, surface, &formatCount, formats);
                for(var i = 0; i < formatCount; i++)
                {
                    details.Formats[i] = formats[i];
                }
            }

            var presentModeCount = 0u;
            khrSurface.GetPhysicalDeviceSurfacePresentModes(device, surface, &presentModeCount, null);
            if(presentModeCount != 0)
            {
                details.PresentModes = new PresentModeKHR[presentModeCount];
                using var mem = GlobalMemory.Allocate((int) presentModeCount * sizeof(PresentModeKHR));
                var modes = (PresentModeKHR*)Unsafe.AsPointer(ref mem.GetPinnableReference());

                khrSurface.GetPhysicalDeviceSurfacePresentModes(device, surface, &presentModeCount, modes);
                for(var i = 0; i < presentModeCount; i++)
                {
                    details.PresentModes[i] = modes[i];
                }
            }

            return details;
        }
        
        public struct QueueFamilyIndices
        {
            public uint? GraphicsFamily { get; set; }
            public uint? PresentFamily { get; set; }
            public bool IsComplete()
            {
                return GraphicsFamily.HasValue && PresentFamily.HasValue;
            }
        }
        private unsafe QueueFamilyIndices FindQueueFamilies(PhysicalDevice device)
        {
            var indices = new QueueFamilyIndices();

            uint queryFamilyCount = 0;
            vk.GetPhysicalDeviceQueueFamilyProperties(device, &queryFamilyCount, null);

            using var mem = GlobalMemory.Allocate((int)queryFamilyCount * sizeof(QueueFamilyProperties));
            var queueFamilies = (QueueFamilyProperties*)Unsafe.AsPointer(ref mem.GetPinnableReference());

            vk.GetPhysicalDeviceQueueFamilyProperties(device, &queryFamilyCount, queueFamilies);
            for(var i = 0u; i < queryFamilyCount; i++)
            {
                var queueFamily = queueFamilies[i];
                if(queueFamily.QueueFlags.HasFlag(QueueFlags.GraphicsBit))
                {
                    indices.GraphicsFamily = i;
                }

                khrSurface.GetPhysicalDeviceSurfaceSupport(device, i, surface, out var presentSupport);
                if(presentSupport == Vk.True)
                {
                    indices.PresentFamily = i;
                }

                if(indices.IsComplete())
                {
                    break;
                }
            }
            return indices;
        }

        private unsafe bool CreateSwapChain()
        {
            var swapChainSupport = QuerySwapChainSupport(physicalDevice);
            var surfaceFormat = ChooseSwapSurfaceFormat(swapChainSupport.Formats);
            var presentMode = ChooseSwapPresentMode(swapChainSupport.PresentModes);
            var extent = ChooseSwapExtent(swapChainSupport.Capabilities);

            if (extent.Width == 0 || extent.Height == 0)
                return false;

            var imageCount = swapChainSupport.Capabilities.MinImageCount + 1;
            if(swapChainSupport.Capabilities.MaxImageCount > 0 &&
                imageCount > swapChainSupport.Capabilities.MaxImageCount)
            {
                imageCount = swapChainSupport.Capabilities.MaxImageCount;
            }

            var createInfo = new SwapchainCreateInfoKHR(StructureType.SwapchainCreateInfoKhr);
            createInfo.Surface = surface;
            createInfo.MinImageCount = imageCount;
            createInfo.ImageFormat = surfaceFormat.Format;
            createInfo.ImageColorSpace = surfaceFormat.ColorSpace;
            createInfo.ImageExtent = extent;
            createInfo.ImageArrayLayers = 1;
            createInfo.ImageUsage = ImageUsageFlags.ColorAttachmentBit;

            var indices = FindQueueFamilies(physicalDevice);
            uint[] queueFamilyIndices = { indices.GraphicsFamily!.Value, indices.PresentFamily!.Value };
            fixed(uint* queueFamily = queueFamilyIndices)
            {
                if (indices.GraphicsFamily.Value != indices.PresentFamily.Value)
                {
                    createInfo.ImageSharingMode = SharingMode.Concurrent;
                    createInfo.QueueFamilyIndexCount = 2;
                    createInfo.PQueueFamilyIndices = queueFamily;
                }
                else
                {
                    createInfo.ImageSharingMode = SharingMode.Exclusive;
                }
                createInfo.PreTransform = swapChainSupport.Capabilities.CurrentTransform;
                createInfo.CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr;
                createInfo.PresentMode = presentMode;
                createInfo.Clipped = Vk.True;
                createInfo.OldSwapchain = default;
                if(!vk.TryGetDeviceExtension(instance, device, out khrSwapchain))
                {
                    throw new NotSupportedException("KHR_swapchain extension not found.");
                }
                if(khrSwapchain.CreateSwapchain(device, createInfo, null, out swapchain) != Result.Success)
                {
                    throw new Exception("failed to create swapchain.");
                }
            }

            khrSwapchain.GetSwapchainImages(device, swapchain, &imageCount, null);
            swapchainImages = new Image[imageCount];
            khrSwapchain.GetSwapchainImages(device, swapchain, &imageCount, swapchainImages);

            swapchainImageFormat = surfaceFormat.Format;
            swapchainExtent = extent;

            return true;
        }
        private Extent2D ChooseSwapExtent(SurfaceCapabilitiesKHR capabilities)
        {
            if(capabilities.CurrentExtent.Width != uint.MaxValue)
            {
                return capabilities.CurrentExtent;
            }

            var actualExtent = new Extent2D
            {
                Height = (uint)window.FramebufferSize.Y,
                Width = (uint)window.FramebufferSize.X
            };
            actualExtent.Width = new[]
            {
                capabilities.MinImageExtent.Width,
                new[] {capabilities.MaxImageExtent.Width, actualExtent.Width }.Min()
            }.Max();
            actualExtent.Height = new[]
            {
                capabilities.MinImageExtent.Height,
                new[] {capabilities.MaxImageExtent.Height, actualExtent.Height}.Min()
            }.Max();
            return actualExtent;
        }
        private PresentModeKHR ChooseSwapPresentMode(PresentModeKHR[] presentModes)
        {
            foreach(var mode in presentModes)
            {
                if(mode == PresentModeKHR.MailboxKhr)
                {
                    return mode;
                }
            }
            return PresentModeKHR.FifoKhr;
        }
        private SurfaceFormatKHR ChooseSwapSurfaceFormat(SurfaceFormatKHR[] formats)
        {
            foreach(var format in formats)
            {
                if(format.Format == Format.B8G8R8A8Unorm)
                {
                    return format;
                }
            }
            return formats[0];
        }

        private unsafe void CreateImageViews()
        {
            swapchainImageViews = new ImageView[swapchainImages.Length];

            for(var i = 0; i < swapchainImages.Length; i++)
            {
                CreateImageView(swapchainImages[i], 1, swapchainImageFormat, ImageAspectFlags.ColorBit, out var imageView);
                
                swapchainImageViews[i] = imageView;
            }
        }

        private unsafe void CreateRenderPass()
        {
            var colorAttachment = new AttachmentDescription();
            colorAttachment.Format = swapchainImageFormat;
            colorAttachment.Samples = SampleCountFlags.Count1Bit;
            colorAttachment.LoadOp = AttachmentLoadOp.Clear;
            colorAttachment.StoreOp = AttachmentStoreOp.Store;
            colorAttachment.StencilLoadOp = AttachmentLoadOp.DontCare;
            colorAttachment.StencilStoreOp = AttachmentStoreOp.DontCare;
            colorAttachment.InitialLayout = ImageLayout.Undefined;
            colorAttachment.FinalLayout = ImageLayout.PresentSrcKhr;
            var depthAttachment = new AttachmentDescription();
            FindDepthFormat(out depthAttachment.Format);
            depthAttachment.Samples = SampleCountFlags.Count1Bit;
            depthAttachment.LoadOp = AttachmentLoadOp.Clear;
            depthAttachment.StoreOp = AttachmentStoreOp.DontCare;
            depthAttachment.StencilLoadOp = AttachmentLoadOp.DontCare;
            depthAttachment.StencilStoreOp = AttachmentStoreOp.DontCare;
            depthAttachment.InitialLayout = ImageLayout.Undefined;
            depthAttachment.FinalLayout = ImageLayout.DepthStencilAttachmentOptimal;

            var colorAttachmentRef = new AttachmentReference(0, ImageLayout.ColorAttachmentOptimal);
            var depthAttachmentRef = new AttachmentReference(1, ImageLayout.DepthStencilAttachmentOptimal);
            var subpass = new SubpassDescription();
            subpass.PipelineBindPoint = PipelineBindPoint.Graphics;
            subpass.ColorAttachmentCount = 1;
            subpass.PColorAttachments = &colorAttachmentRef;
            subpass.PDepthStencilAttachment = &depthAttachmentRef;

            var dependency = new SubpassDependency();
            dependency.SrcSubpass = Vk.SubpassExternal;
            dependency.DstSubpass = 0;
            dependency.SrcStageMask =
                PipelineStageFlags.ColorAttachmentOutputBit |
                PipelineStageFlags.EarlyFragmentTestsBit;
            dependency.DstStageMask =
                PipelineStageFlags.ColorAttachmentOutputBit |
                PipelineStageFlags.EarlyFragmentTestsBit;
            dependency.SrcAccessMask = 0;
            dependency.DstAccessMask =
                AccessFlags.ColorAttachmentWriteBit |
                AccessFlags.DepthStencilAttachmentWriteBit;

            var attachments = stackalloc AttachmentDescription[2];
            attachments[0] = colorAttachment;
            attachments[1] = depthAttachment;
            var renderPassInfo = new RenderPassCreateInfo(StructureType.RenderPassCreateInfo);
            renderPassInfo.AttachmentCount = 2;
            renderPassInfo.PAttachments = attachments;
            renderPassInfo.SubpassCount = 1;
            renderPassInfo.PSubpasses = &subpass;
            renderPassInfo.DependencyCount = 1;
            renderPassInfo.PDependencies = &dependency;

            if(vk.CreateRenderPass(device, in renderPassInfo, null, out renderPass) != Result.Success)
            {
                throw new Exception("failed to create render pass.");
            }
        }

        private unsafe void CreateGraphicsPipeline()
        {
            // parse DescriptorSetLayout from shader.spirv
            var shaderDefines = new ShaderDefine[2];
            shaderDefines[0] = new ShaderDefine("MafrixEngine.Shaders.triangle.vert.spv", ShaderStageFlags.VertexBit);
            shaderDefines[1] = new ShaderDefine("MafrixEngine.Shaders.triangle.frag.spv", ShaderStageFlags.FragmentBit);
            // using some class to simplfy pipeline create
            var pipelineInfos = new PipelineInfo(vk, device, shaderDefines);
            descriptorSetLayout = pipelineInfos.setLayoutInfo.GetDescriptorSetLayouts()[0];
            poolSizeInfo = new VkDescriptorPollSize(pipelineInfos.setLayoutInfo);

            VkPipelineBuilder pipelineBuilder = new VkPipelineBuilder(vk, device);
            pipelineBuilder.BindInputAssemblyState(PrimitiveTopology.TriangleList);
            pipelineBuilder.BindViewportState(swapchainExtent);
            pipelineBuilder.BindRasterizationState(PolygonMode.Fill, CullModeFlags.None);
            pipelineBuilder.BindMultisampleState();
            pipelineBuilder.BindDepthStencilState(true, true, CompareOp.Less);
            var masks = new VkPipelineBuilder.ColorBlendMask[1];
            masks[0] = new VkPipelineBuilder.ColorBlendMask(
                ColorComponentFlags.RBit |
                ColorComponentFlags.GBit |
                ColorComponentFlags.BBit |
                ColorComponentFlags.ABit, false);
            pipelineBuilder.BindColorBlendState(masks);
            pipelineBuilder.BindVertexInput<Vertex>(default);
            pipelineBuilder.BindRenderPass(renderPass, 0);
            pipelineBuilder.BindPlipelineLayout(pipelineInfos.setLayoutInfo.GetDescriptorSetLayouts());
            pipelineBuilder.BindShaderStages(pipelineInfos.pipelineShaderStageCreateInfos);
            pipelineBuilder.Build();
            pipelineLayout = pipelineBuilder.pipelineLayout;
            graphicsPipeline = pipelineBuilder.pipeline;
        }

        private unsafe void CreateBuffer(ulong size, BufferUsageFlags usage,
            MemoryPropertyFlags property, out Buffer buffer, out DeviceMemory bufferMemory)
        {
            var bufferInfo = new BufferCreateInfo(StructureType.BufferCreateInfo);
            bufferInfo.Size = size;
            bufferInfo.Usage = usage;
            bufferInfo.SharingMode = SharingMode.Exclusive;
            if (vk.CreateBuffer(device, bufferInfo, null, out buffer) != Result.Success)
            {
                throw new Exception("failed to create vertex buffer!");
            }

            // allocate memory
            var memRequirements = new MemoryRequirements();
            vk.GetBufferMemoryRequirements(device, buffer, out memRequirements);

            var allocInfo = new MemoryAllocateInfo(StructureType.MemoryAllocateInfo);
            allocInfo.AllocationSize = memRequirements.Size;
            allocInfo.MemoryTypeIndex =
                FindMemoryType(memRequirements.MemoryTypeBits, property);
            if (vk.AllocateMemory(device, allocInfo, null, out bufferMemory) != Result.Success)
            {
                throw new Exception("failed to allocate vertex buffer memory!");
            }

            vk.BindBufferMemory(device, buffer, bufferMemory, 0);
        }

        private unsafe void FindSupportedFormat(Format[] candidates,
            ImageTiling tiling, FormatFeatureFlags flags, out Format targ)
        {
            foreach(var format in candidates)
            {
                vk.GetPhysicalDeviceFormatProperties(physicalDevice, format, out var props);
                if(tiling == ImageTiling.Linear && (props.LinearTilingFeatures & flags) == flags)
                {
                    targ = format;
                    return;
                } else if(tiling == ImageTiling.Optimal && (props.OptimalTilingFeatures & flags) == flags)
                {
                    targ = format;
                    return;
                }
            }
            throw new Exception("failed to find supported format.");
        }

        private bool HasStencilComponent(Format format)
        {
            return format == Format.D32SfloatS8Uint || format == Format.X8D24UnormPack32;
        }

        private unsafe void FindDepthFormat(out Format format)
        {
            var formats = new Format[]
            {
                Format.D32Sfloat, Format.D32SfloatS8Uint,
                Format.X8D24UnormPack32
            };
            FindSupportedFormat(formats, ImageTiling.Optimal,
                FormatFeatureFlags.DepthStencilAttachmentBit,
                out format);
        }

        private unsafe void CreateDepthResources()
        {
            FindDepthFormat(out Format depthFormat);
            CreateImage(swapchainExtent.Width, swapchainExtent.Height, 1,
                depthFormat, ImageTiling.Optimal,
                ImageUsageFlags.DepthStencilAttachmentBit,
                MemoryPropertyFlags.DeviceLocalBit,
                out depthImage, out depthImageMemory);
            CreateImageView(depthImage, 1, depthFormat, ImageAspectFlags.DepthBit, out depthImageView);
            TransitionImageLayout(depthImage, 1, depthFormat,
                ImageLayout.Undefined, ImageLayout.DepthStencilAttachmentOptimal);
        }

        private unsafe void CreateTextureImage(string name, out Image image, out DeviceMemory deviceMemory)
        {
            using var texture = SlImage.Load<Rgba32>(name);
            var memoryGroup = texture.GetPixelMemoryGroup();
            var imageSize = memoryGroup.TotalLength * sizeof(Rgba32);
            Memory<byte> array = new byte[imageSize];

            var block = MemoryMarshal.Cast<byte, Rgba32>(array.Span);
            foreach (var memory in memoryGroup)
            {
                memory.Span.CopyTo(block);
                block = block.Slice(memory.Length);
            }
            var width = texture.Width;
            var height = texture.Height;
            mipLevels = (UInt32)Math.Floor(Math.Log2(Math.Max(width, height))) + 1;

            CreateImage((uint)width, (uint)height, mipLevels,
                Format.R8G8B8A8Srgb,
                ImageTiling.Optimal,
                ImageUsageFlags.TransferSrcBit |
                ImageUsageFlags.TransferDstBit |
                ImageUsageFlags.SampledBit,
                MemoryPropertyFlags.DeviceLocalBit,
                out image, out deviceMemory);
            TransitionImageLayout(image, mipLevels, Format.R8G8B8A8Srgb,
                ImageLayout.Undefined, ImageLayout.TransferDstOptimal);

            var stCommand = new SingleTimeCommand(vk, device, commandPool, graphicsQueue);
            staging.CopyDataToImage(stCommand, image, (uint)width, (uint)height, array.Span, (uint)imageSize);

            GenerateMipmaps(image, Format.R8G8B8A8Srgb, (uint)width, (uint)height, mipLevels);
        }

        private unsafe void GenerateMipmaps(Image image, Format imageFormat, uint width, uint height, uint mipLevels)
        {
            vk.GetPhysicalDeviceFormatProperties(physicalDevice, imageFormat, out var formatProperties);
            if(0 == (formatProperties.OptimalTilingFeatures & FormatFeatureFlags.SampledImageFilterLinearBit))
            {
                throw new Exception("texture image format does not support linear blitting.");
            }
            var stCommand = new SingleTimeCommand(vk, device, commandPool, graphicsQueue);
            stCommand.BeginSingleTimeCommands(out var commandBuffer);

            var barrier = new ImageMemoryBarrier(StructureType.ImageMemoryBarrier);
            barrier.Image = image;
            barrier.SrcQueueFamilyIndex = Vk.QueueFamilyIgnored;
            barrier.DstQueueFamilyIndex = Vk.QueueFamilyIgnored;
            barrier.SubresourceRange.AspectMask = ImageAspectFlags.ColorBit;
            barrier.SubresourceRange.BaseArrayLayer = 0;
            barrier.SubresourceRange.LayerCount = 1;
            barrier.SubresourceRange.LevelCount = 1;

            var mipWidth = width;
            var mipHeight = height;
            for(var i = 1; i < mipLevels; i++)
            { 
                barrier.SubresourceRange.BaseMipLevel = (uint)i - 1;
                barrier.OldLayout = ImageLayout.TransferDstOptimal;
                barrier.NewLayout = ImageLayout.TransferSrcOptimal;
                barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
                barrier.DstAccessMask = AccessFlags.TransferReadBit;

                vk.CmdPipelineBarrier(commandBuffer,
                    PipelineStageFlags.TransferBit,
                    PipelineStageFlags.TransferBit, 0,
                    0, null,
                    0, null, 1, barrier);

                var blit = new ImageBlit();
                blit.SrcOffsets[0].X = 0;
                blit.SrcOffsets[0].Z = 0;
                blit.SrcOffsets[0].Y = 0;
                blit.SrcOffsets[1].X = (int)mipWidth;
                blit.SrcOffsets[1].Y = (int)mipHeight;
                blit.SrcOffsets[1].Z = 1;
                blit.SrcSubresource.AspectMask = ImageAspectFlags.ColorBit;
                blit.SrcSubresource.MipLevel = (uint)i - 1;
                blit.SrcSubresource.BaseArrayLayer = 0;
                blit.SrcSubresource.LayerCount = 1;
                blit.DstOffsets[0].X = 0;
                blit.DstOffsets[0].Y = 0;
                blit.DstOffsets[0].Z = 0;
                blit.DstOffsets[1].X = (int) (mipWidth > 1 ? mipWidth / 2 : 1);
                blit.DstOffsets[1].Y = (int)(mipHeight > 1 ? mipHeight / 2 : 1);
                blit.DstOffsets[1].Z = 1;
                blit.DstSubresource.AspectMask = ImageAspectFlags.ColorBit;
                blit.DstSubresource.MipLevel = (uint)i;
                blit.DstSubresource.BaseArrayLayer = 0;
                blit.DstSubresource.LayerCount = 1;

                vk.CmdBlitImage(commandBuffer,
                    image, ImageLayout.TransferSrcOptimal,
                    image, ImageLayout.TransferDstOptimal,
                    1, blit, Filter.Linear);

                barrier.OldLayout = ImageLayout.TransferSrcOptimal;
                barrier.NewLayout = ImageLayout.ShaderReadOnlyOptimal;
                barrier.SrcAccessMask = AccessFlags.TransferReadBit;
                barrier.DstAccessMask = AccessFlags.ShaderReadBit;
                vk.CmdPipelineBarrier(commandBuffer,
                    PipelineStageFlags.TransferBit,
                    PipelineStageFlags.FragmentShaderBit, 0,
                    0, null,
                    0, null, 1, barrier);

                if (mipWidth > 1) mipWidth /= 2;
                if (mipHeight > 1) mipHeight /= 2;
            }

            barrier.SubresourceRange.BaseMipLevel = mipLevels - 1;
            barrier.OldLayout = ImageLayout.TransferDstOptimal;
            barrier.NewLayout = ImageLayout.ShaderReadOnlyOptimal;
            barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
            barrier.DstAccessMask = AccessFlags.ShaderReadBit;

            vk.CmdPipelineBarrier(commandBuffer,
                PipelineStageFlags.TransferBit,
                PipelineStageFlags.FragmentShaderBit, 0,
                0, null,
                0, null, 1, barrier);

            stCommand.EndSingleTimeCommands(commandBuffer);
        }

        private unsafe void CreateTextureSampler()
        {
            vk.GetPhysicalDeviceProperties(physicalDevice, out var properties);
            var samplerInfo = new SamplerCreateInfo(StructureType.SamplerCreateInfo);
            samplerInfo.MagFilter = Filter.Linear;
            samplerInfo.MinFilter = Filter.Linear;
            samplerInfo.AddressModeU = SamplerAddressMode.Repeat;
            samplerInfo.AddressModeV = SamplerAddressMode.Repeat;
            samplerInfo.AddressModeW = SamplerAddressMode.Repeat;
            samplerInfo.AnisotropyEnable = Vk.True;
            samplerInfo.MaxAnisotropy = properties.Limits.MaxSamplerAnisotropy;
            samplerInfo.BorderColor = BorderColor.IntOpaqueBlack;
            samplerInfo.UnnormalizedCoordinates = Vk.False;
            samplerInfo.CompareEnable = Vk.False;
            samplerInfo.CompareOp = CompareOp.Always;
            samplerInfo.MipmapMode = SamplerMipmapMode.Linear;
            samplerInfo.MipLodBias = 0.0f;
            samplerInfo.MinLod = 0.0f;
            samplerInfo.MaxLod = 0.0f;
            if(vk.CreateSampler(device, samplerInfo, null, out textureSampler) != Result.Success)
            {
                throw new Exception("failed to create texture sampler.");
            }

        }

        private unsafe void CreateImageView(Image image, uint mipLevels, Format format, ImageAspectFlags aspectFlags, out ImageView imageView)
        {
            var viewInfo = new ImageViewCreateInfo(StructureType.ImageViewCreateInfo);
            viewInfo.Image = image;
            viewInfo.ViewType = ImageViewType.Type2D;
            viewInfo.Format = format;
            viewInfo.SubresourceRange.AspectMask = aspectFlags;
            viewInfo.SubresourceRange.BaseMipLevel = 0;
            viewInfo.SubresourceRange.LevelCount = mipLevels;
            viewInfo.SubresourceRange.BaseArrayLayer = 0;
            viewInfo.SubresourceRange.LayerCount = 1;
            imageView = new ImageView();
            if (vk.CreateImageView(device, viewInfo, null, out imageView) != Result.Success)
            {
                throw new Exception("failed to create texture image view");
            }
        }

        private unsafe void CreateImage(uint width, uint height, uint mipLevels, Format format,
            ImageTiling tiling, ImageUsageFlags usage,
            MemoryPropertyFlags properties,
            out Image image, out DeviceMemory imageMemory)
        {
            var imageInfo = new ImageCreateInfo(StructureType.ImageCreateInfo);
            imageInfo.ImageType = ImageType.Type2D;
            imageInfo.Extent.Width = width;
            imageInfo.Extent.Height = height;
            imageInfo.Extent.Depth = 1;
            imageInfo.MipLevels = mipLevels;
            imageInfo.ArrayLayers = 1;
            imageInfo.Format = format;
            imageInfo.Tiling = tiling;
            imageInfo.InitialLayout = ImageLayout.Undefined;
            imageInfo.Usage = usage;
            imageInfo.SharingMode = SharingMode.Exclusive;
            imageInfo.Samples = SampleCountFlags.Count1Bit;
            if (vk.CreateImage(device, imageInfo, null, out image) != Result.Success)
            {
                throw new Exception("failed to create image.");
            }

            var memRequirements = new MemoryRequirements();
            vk.GetImageMemoryRequirements(device, image, out memRequirements);

            var allocInfo = new MemoryAllocateInfo(StructureType.MemoryAllocateInfo);
            allocInfo.AllocationSize = memRequirements.Size;
            allocInfo.MemoryTypeIndex = FindMemoryType(memRequirements.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit);
            if (vk.AllocateMemory(device, allocInfo, null, out imageMemory) != Result.Success)
            {
                throw new Exception("failed to allocate image memory.");
            }
            vk.BindImageMemory(device, image, imageMemory, 0);
        }

        private unsafe void TransitionImageLayout(Image image, uint mipLevels, Format format,
            ImageLayout oldLayout, ImageLayout newLayout)
        {
            var stCommand = new SingleTimeCommand(vk, device, commandPool, graphicsQueue);
            stCommand.BeginSingleTimeCommands(out var commandBuffer);

            var barrier = new ImageMemoryBarrier(StructureType.ImageMemoryBarrier);
            barrier.OldLayout = oldLayout;
            barrier.NewLayout = newLayout;
            barrier.SrcQueueFamilyIndex = Vk.QueueFamilyIgnored;
            barrier.DstQueueFamilyIndex = Vk.QueueFamilyIgnored;
            barrier.Image = image;
            barrier.SubresourceRange.BaseMipLevel = 0;
            barrier.SubresourceRange.LevelCount = mipLevels;
            barrier.SubresourceRange.BaseArrayLayer = 0;
            barrier.SubresourceRange.LayerCount = 1;
            if(newLayout == ImageLayout.DepthStencilAttachmentOptimal)
            {
                barrier.SubresourceRange.AspectMask = ImageAspectFlags.DepthBit;
                if(HasStencilComponent(format))
                {
                    barrier.SubresourceRange.AspectMask |= ImageAspectFlags.StencilBit;
                }
            }
            else
            {
                barrier.SubresourceRange.AspectMask = ImageAspectFlags.ColorBit;
            }

            var sourceStage = new PipelineStageFlags();
            var destinationStage = new PipelineStageFlags();
            if (oldLayout == ImageLayout.Undefined && newLayout == ImageLayout.TransferDstOptimal)
            {
                barrier.SrcAccessMask = 0;
                barrier.DstAccessMask = AccessFlags.TransferWriteBit;
                sourceStage = PipelineStageFlags.TopOfPipeBit;
                destinationStage = PipelineStageFlags.TransferBit;
            } else if (oldLayout == ImageLayout.TransferDstOptimal && newLayout == ImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
                barrier.DstAccessMask = AccessFlags.ShaderReadBit;
                sourceStage = PipelineStageFlags.TransferBit;
                destinationStage = PipelineStageFlags.FragmentShaderBit;
            } else if(oldLayout == ImageLayout.Undefined && newLayout == ImageLayout.DepthStencilAttachmentOptimal)
            {
                barrier.SrcAccessMask = 0;
                barrier.DstAccessMask =
                    AccessFlags.DepthStencilAttachmentReadBit |
                    AccessFlags.DepthStencilAttachmentWriteBit;
                sourceStage = PipelineStageFlags.TopOfPipeBit;
                destinationStage = PipelineStageFlags.EarlyFragmentTestsBit;
            } else
            {
                throw new InvalidDataException("unsupported layout transition.");
            }
            vk.CmdPipelineBarrier(commandBuffer,
                sourceStage, destinationStage,
                0,
                0, null,
                0, null,
                1, barrier);

            stCommand.EndSingleTimeCommands(commandBuffer);
        }

        private unsafe void LoadModel()
        {
            var modelPathNames = new ValueTuple<string, string>[]
            {
                ("Asserts/sponza", "Sponza.gltf"),
                //("Asserts/gaz-66", "scene.gltf")
            };

            // load model
            var stCommand = new SingleTimeCommand(vk, device, commandPool, graphicsQueue);
            meshes = new Mesh[modelPathNames.Length];
            for (var i = 0; i < modelPathNames.Length; i++)
            {
                meshes[i] = new Mesh(vk, device);
                var (path, name) = modelPathNames[i];
                var loader = new Gltf2Loader(path, name);
                meshes[i].gltf2 = loader.Parse(vkContext, stCommand, staging);
                meshes[i].vertices = meshes[i].gltf2.vertices;
                meshes[i].indices = meshes[i].gltf2.indices;
            }

            meshes[0].matrix = Matrix4X4.CreateScale<float>(5.0f) * Matrix4X4.CreateTranslation<float>(new Vec3(-400, 0, 0));
            meshes[0].frameRotate = Scalar.DegreesToRadians<float>(30.0f);
            //meshes[1].matrix = Matrix4X4.CreateScale<float>(0.03f);
            //meshes[1].frameRotate = Scalar.DegreesToRadians<float>(77.0f);
        }

        private unsafe void CreateVertexBuffer()
        {
            ulong bufferSize = (ulong)0;

            var stCommand = new SingleTimeCommand(vk, device, commandPool, graphicsQueue);

            for(var m = 0; m < meshes.Length; m++)
            {
                bufferSize = (ulong)GetArrayByteSize(meshes[m].vertices);
                CreateBuffer(bufferSize,
                    BufferUsageFlags.TransferDstBit |
                    BufferUsageFlags.VertexBufferBit,
                    MemoryPropertyFlags.DeviceLocalBit,
                    out meshes[m].vertexBuffer, out meshes[m].vertexBufferMemory);
                fixed (void* ptr = meshes[m].vertices)
                {
                    staging.CopyDataToBuffer(stCommand, meshes[m].vertexBuffer, ptr, (uint)bufferSize);
                }
            }
        }

        private unsafe void CreateIndexBuffer()
        {
            ulong bufferSize = (ulong)0;

            var stCommand = new SingleTimeCommand(vk, device, commandPool, graphicsQueue);

            for(var m = 0; m < meshes.Length; m++)
            {
                bufferSize = (ulong)GetArrayByteSize(meshes[m].indices);
                CreateBuffer(bufferSize,
                    BufferUsageFlags.TransferDstBit |
                    BufferUsageFlags.IndexBufferBit,
                    MemoryPropertyFlags.DeviceLocalBit,
                    out meshes[m].indicesBuffer, out meshes[m].indiceBufferMemory);
                fixed (void* ptr = meshes[m].indices)
                {
                    staging.CopyDataToBuffer(stCommand, meshes[m].indicesBuffer, ptr, (uint)bufferSize);
                }
            }
        }

        private unsafe void CreateUniformBuffers()
        {
            
            for(var m = 0; m < meshes.Length; m++)
            {
                var setCount = MaxFrameInFlight * meshes[m].gltf2.DescriptorSetCount;
                ulong bufferSize = (ulong) (Unsafe.SizeOf<UniformBufferObject>() * meshes[m].gltf2.DescriptorSetCount);
                meshes[m].uniformBuffer = new Buffer[setCount];
                meshes[m].uniformMemory = new DeviceMemory[setCount];
                for (int i = 0; i < setCount; i++)
                {
                    CreateBuffer(bufferSize, BufferUsageFlags.UniformBufferBit,
                        MemoryPropertyFlags.HostVisibleBit |
                        MemoryPropertyFlags.HostCoherentBit,
                        out meshes[m].uniformBuffer[i], out meshes[m].uniformMemory[i]);
                }
            }
        }

        private unsafe void CreateDescriptorPool()
        {
            var poolInfo = new DescriptorPoolCreateInfo(StructureType.DescriptorPoolCreateInfo);

            for(var m = 0; m < meshes.Length; m++)
            {
                var poolSize = new DescriptorPoolSize[poolSizeInfo.poolSizes.Length];
                for (var i = 0; i < poolSize.Length; i++)
                {
                    var tmp = poolSizeInfo.poolSizes[i];
                    poolSize[i] = new DescriptorPoolSize(tmp.Type, tmp.DescriptorCount * (uint)(MaxFrameInFlight * meshes[m].gltf2.DescriptorSetCount));
                }
                poolInfo.PoolSizeCount = (uint)poolSize.Length;
                fixed (DescriptorPoolSize* poolSizePtr = poolSize)
                {
                    poolInfo.PPoolSizes = poolSizePtr;
                }
                poolInfo.MaxSets = (uint) (MaxFrameInFlight * meshes[m].gltf2.DescriptorSetCount);
                if (vk.CreateDescriptorPool(device, poolInfo, null, out meshes[m].descriptorPool) != Result.Success)
                {
                    throw new Exception("failed to create descriptor pool.");
                }
            }
        }

        private unsafe void CreateDescriptorSets()
        {
            WriteDescriptorSet[] descriptorWrites = new WriteDescriptorSet[2];
            for(var m = 0; m < meshes.Length; m++)
            {
                var setCount = MaxFrameInFlight * meshes[m].gltf2.DescriptorSetCount;

                var layouts = new DescriptorSetLayout[setCount];
                for (var i = 0; i < setCount; i++)
                {
                    layouts[i] = descriptorSetLayout;
                }

                var allocInfo = new DescriptorSetAllocateInfo(StructureType.DescriptorSetAllocateInfo);
                allocInfo.DescriptorPool = meshes[m].descriptorPool;
                allocInfo.DescriptorSetCount = (uint)setCount;
                fixed (DescriptorSetLayout* ptr = layouts)
                {
                    allocInfo.PSetLayouts = ptr;
                    meshes[m].descriptorSets = new DescriptorSet[setCount];
                    if (vk.AllocateDescriptorSets(device, &allocInfo, meshes[m].descriptorSets) != Result.Success)
                    {
                        throw new Exception("failed to allocate descriptor sets.");
                    }
                }

                descriptorWrites[0].SType = StructureType.WriteDescriptorSet;
                descriptorWrites[1].SType = StructureType.WriteDescriptorSet;
                for (var i = 0; i < MaxFrameInFlight; i++)
                {
                    var bufferInfo = new DescriptorBufferInfo();
                    bufferInfo.Buffer = meshes[m].uniformBuffer[i];
                    bufferInfo.Offset = 0;
                    bufferInfo.Range = (ulong)Unsafe.SizeOf<UniformBufferObject>();

                    var imageInfo = new DescriptorImageInfo();
                    imageInfo.ImageLayout = ImageLayout.ShaderReadOnlyOptimal;
                    //imageInfo.ImageView = meshes[m].textureView;
                    imageInfo.Sampler = textureSampler;

                    descriptorWrites[0].DstBinding = 0;
                    descriptorWrites[0].DstArrayElement = 0;
                    descriptorWrites[0].DescriptorType = DescriptorType.UniformBuffer;
                    descriptorWrites[0].DescriptorCount = 1;
                    //descriptorWrites[0].PBufferInfo = &bufferInfo;

                    descriptorWrites[1].DstBinding = 1;
                    descriptorWrites[1].DstArrayElement = 0;
                    descriptorWrites[1].DescriptorType = DescriptorType.CombinedImageSampler;
                    descriptorWrites[1].DescriptorCount = 1;
                    //descriptorWrites[1].PImageInfo = &imageInfo;

                    meshes[m].gltf2.UpdateDescriptorSets(
                        vk, device,
                        descriptorWrites, imageInfo, bufferInfo,
                        meshes[m].descriptorSets, meshes[m].uniformBuffer, i * meshes[m].gltf2.DescriptorSetCount);
                }
            }
        }

        private unsafe UInt32 FindMemoryType(UInt32 typeFilter, MemoryPropertyFlags properties)
        {
            var memProperties = new PhysicalDeviceMemoryProperties();
            vk.GetPhysicalDeviceMemoryProperties(physicalDevice, out memProperties);
            for(UInt32 i = 0; i < memProperties.MemoryTypeCount; i++)
            {
                if((typeFilter & (1 << (int)i)) != 0 &&
                    (memProperties.MemoryTypes[(int)i].PropertyFlags & properties) != 0)
                {
                    return i;
                }
            }

            throw new Exception("failed to find suitable memory type!");
        }

        private unsafe void CreateFramebuffers()
        {
            swapchainFramebuffers = new Framebuffer[swapchainImageViews.Length];

            var attachments = stackalloc ImageView[2];
            attachments[1] = depthImageView;
            for(var i = 0; i < swapchainImageViews.Length; i++)
            {
                attachments[0] = swapchainImageViews[i];
                var framebufferInfo = new FramebufferCreateInfo(StructureType.FramebufferCreateInfo);
                framebufferInfo.RenderPass = renderPass;
                framebufferInfo.AttachmentCount = 2;
                framebufferInfo.PAttachments = attachments;
                framebufferInfo.Width = swapchainExtent.Width;
                framebufferInfo.Height = swapchainExtent.Height;
                framebufferInfo.Layers = 1;

                var framebuffer = new Framebuffer();
                if(vk.CreateFramebuffer(device, framebufferInfo, null, out framebuffer) != Result.Success)
                {
                    throw new Exception("failed to create framebuffer!");
                }
                swapchainFramebuffers[i] = framebuffer;
            }
        }

        private unsafe void CreateCommandPool()
        {
            var queueFamilyIndices = FindQueueFamilies(physicalDevice);

            var poolInfo = new CommandPoolCreateInfo
            {
                SType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = queueFamilyIndices.GraphicsFamily!.Value
            };

            if (vk.CreateCommandPool(device, poolInfo, null, out commandPool) != Result.Success)
            {
                throw new Exception("failed to create command pool!");
            }
        }

        private unsafe void CreateCommandBuffers()
        {
            commandBuffers = new CommandBuffer[swapchainFramebuffers.Length];

            var allocInfo = new CommandBufferAllocateInfo
            {
                SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = commandPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = (uint)commandBuffers.Length
            };

            if (vk.AllocateCommandBuffers(device, &allocInfo, commandBuffers) != Result.Success)
            {
                throw new Exception("failed to allocate command buffers!");
            }

            var clearValues = stackalloc ClearValue[2];
            clearValues[0].Color = new ClearColorValue(0, 0, 0, 1);
            clearValues[1].DepthStencil = new ClearDepthStencilValue(1.0f, 0);
            for (var i = 0; i < commandBuffers.Length; i++)
            {
                var beginInfo = new CommandBufferBeginInfo { SType = StructureType.CommandBufferBeginInfo };

                if (vk.BeginCommandBuffer(commandBuffers[i], &beginInfo) != Result.Success)
                {
                    throw new Exception("failed to begin recording command buffer!");
                }

                var renderPassInfo = new RenderPassBeginInfo
                {
                    SType = StructureType.RenderPassBeginInfo,
                    RenderPass = renderPass,
                    Framebuffer = swapchainFramebuffers[i],
                    RenderArea = { Offset = new Offset2D { X = 0, Y = 0 }, Extent = swapchainExtent }
                };

                renderPassInfo.ClearValueCount = 2;
                renderPassInfo.PClearValues = clearValues;

                vk.CmdBeginRenderPass(commandBuffers[i], &renderPassInfo, SubpassContents.Inline);

                vk.CmdBindPipeline(commandBuffers[i], PipelineBindPoint.Graphics, graphicsPipeline);

                for (var m = 0; m < meshes.Length; m++)
                {
                    meshes[m].BindCommand(vk, commandBuffers[i], BindDescriptorSets);

                    void BindDescriptorSets(int nodeIndex)
                    {
                        var idx = i * meshes[m].gltf2.DescriptorSetCount + nodeIndex;
                        vk.CmdBindDescriptorSets(commandBuffers[i], PipelineBindPoint.Graphics, pipelineLayout, 0, 1, meshes[m].descriptorSets[idx], 0, null);
                    }
                }


                vk.CmdEndRenderPass(commandBuffers[i]);

                if (vk.EndCommandBuffer(commandBuffers[i]) != Result.Success)
                {
                    throw new Exception("failed to record command buffer!");
                }
            }
        }

        private unsafe void CreateSyncObjects()
        {
            imageAvailableSemaphores = new Semaphore[MaxFrameInFlight];
            renderFinishedSemaphores = new Semaphore[MaxFrameInFlight];
            inFlightFences = new Fence[MaxFrameInFlight];
            imagesInFlight = new Fence[MaxFrameInFlight];

            var semaphoreInfo = new SemaphoreCreateInfo(StructureType.SemaphoreCreateInfo);

            var fenceInfo = new FenceCreateInfo(StructureType.FenceCreateInfo);
            fenceInfo.Flags = FenceCreateFlags.SignaledBit;
            for(var i = 0; i < MaxFrameInFlight; i++)
            {
                Semaphore imgAvSema, renderFinSema;
                Fence inFlightFence;
                if(vk.CreateSemaphore(device, semaphoreInfo, null, out imgAvSema) != Result.Success ||
                   vk.CreateSemaphore(device, semaphoreInfo, null, out renderFinSema) != Result.Success ||
                   vk.CreateFence(device, fenceInfo, null, out inFlightFence) != Result.Success)
                {
                    throw new Exception("failed to create synchonization objects for a frame!");
                }
                imageAvailableSemaphores[i] = imgAvSema;
                renderFinishedSemaphores[i] = renderFinSema;
                inFlightFences[i] = inFlightFence;
            }
        }

        public unsafe void Cleanup()
        {
            for(var i = 0; i < MaxFrameInFlight; i++)
            {
                vk.DestroySemaphore(device, imageAvailableSemaphores[i], null);
                vk.DestroySemaphore(device, renderFinishedSemaphores[i], null);
                vk.DestroyFence(device, inFlightFences[i], null);
            }
            vk.DestroyCommandPool(device, commandPool, null);
            foreach(var framebuffer in swapchainFramebuffers)
            {
                vk.DestroyFramebuffer(device, framebuffer, null);
            }
            vk.DestroySampler(device, textureSampler, null);
            vk.DestroyImageView(device, depthImageView, null);
            vk.DestroyImage(device, depthImage, null);
            vk.FreeMemory(device, depthImageMemory, null);

            foreach(var mesh in meshes)
            {
                mesh.Dispose();
            }

            vk.DestroyDescriptorSetLayout(device, descriptorSetLayout, null);
            vk.DestroyPipeline(device, graphicsPipeline, null);
            vk.DestroyPipelineLayout(device, pipelineLayout, null);
            vk.DestroyRenderPass(device, renderPass, null);
            foreach(var imageView in swapchainImageViews)
            {
                vk.DestroyImageView(device, imageView, null);
            }
            khrSwapchain.DestroySwapchain(device, swapchain, null);

            staging.Dispose();

            if(EnableValidationLayers)
            {
                debugUtils.DestroyDebugUtilsMessenger(instance, debugMessager, null);
            }
            khrSurface.DestroySurface(instance, surface, null);
            vkContext.Dispose();
            window.Close();
            window.Dispose();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
