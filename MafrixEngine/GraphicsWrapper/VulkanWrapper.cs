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
using MafrixEngine.Source.DataStruct;

using Image = Silk.NET.Vulkan.Image;
using Semaphore = Silk.NET.Vulkan.Semaphore;
using SlImage = SixLabors.ImageSharp.Image;
using glTFLoader.Schema;
using Silk.NET.Input;
using MafrixEngine.Input;
using Silk.NET.GLFW;
using SharpGLTF.Transforms;

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
        public VkBuffer vertexBuffer;
        public VkBuffer indicesBuffer;
        public VkDescriptorPool descriptorPool;
        public GltfRHIAssertInfo meshAssertInfo;
        public Mat4 matrix;
        public float frameRotate;
        public GltfAssertInfo assertInfo;
        public IDescriptor model;

        private Vk vk;
        private Device device;
        public Mesh(VkContext ctx)
        {
            this.vk = ctx.vk;
            this.device = ctx.device;
            matrices = default;
            uniformBuffer = default;
            uniformMemory = default;
            vertices = default;
            indices = default;
            vertexBuffer = new VkBuffer(ctx);
            indicesBuffer = new VkBuffer(ctx);
            descriptorPool = default;
            matrix = default;
            frameRotate = default;
            model = default;
            meshAssertInfo = default;
            assertInfo = default;
        }

        public void BindCommand(Vk vk, CommandBuffer commandBuffer, Action<int> action)
        {
            model.BindCommand(vk, commandBuffer, assertInfo.vertexBuffer.buffer, assertInfo.indicesBuffer.buffer, action);
        }

        public unsafe void Dispose()
        {
            vertexBuffer.Dispose();
            indicesBuffer.Dispose();

            assertInfo.Dispose();

            model.Dispose();
        }
    }

    public class VulkanWrapper : IDisposable //, IRender
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
        public VkSwapchain vkSwapchain;
        public RenderPass renderPass;
        public DescriptorSetLayout descriptorSetLayout;
        public PipelineLayout pipelineLayout;
        public Pipeline graphicsPipeline;
        public VkFramebuffer vkFramebuffer;

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
        private DescriptorPoolSize[] PoolSizes;

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
        public unsafe static int GetArrayByteSize<T>(T[] array) where T : unmanaged
        {
            return sizeof(T) * array.Length;
        }

        public Gltf2Animation[] animations;

        public HardwareInput input;
        private KeyboardMapping kbMap;
        private MouseMapping mouseMap;

        public VulkanWrapper()
        {
            window = InitWindow();
            vkContext = new VkContext();
            input = new HardwareInput(InitInput());
            kbMap = new KeyboardMapping(input.inputContext.Keyboards[0]);
            mouseMap = new MouseMapping(input.inputContext.Mice[0]);
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

        public IInputContext InitInput()
        {
            return window.CreateInput();
        }

        public void InitVulkan()
        {
            vkContext.Initialize("Render", new Version32(0, 0, 1));
            vk.TryGetInstanceExtension(instance, out khrSurface);
            vk.GetDeviceQueue(device, 0, 0, out graphicsQueue);
            CreateSurface();
            // extensions
            SetupDebugMessager();

            staging = new StagingBuffer(vk, physicalDevice, device);

            CreateSwapChain();
            CreateRenderPass();
            CreateGraphicsPipeline();
            CreateCommandPool();
            CreateDepthResources();
            CreateFramebuffers();
            LoadModel();
            CreateCommandBuffers();
            CreateSyncObjects();

            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i].vertices = Array.Empty<Vertex>();
                meshes[i].indices = Array.Empty<uint>();
            }

            // Gltf model camera
            //var pos = new Vec3(35.0f, 200.0f, 35.0f);
            //var dir = new Vec3(200.0f, 245.0f, 200.0f) - pos;
            //camera = new Camera(new CameraCoordinate(pos, dir, new Vec3(0.0f, -1.0f, 0.0f)),
            //                new ProjectInfo(45.0f, (float)swapchainExtent.Width / (float)swapchainExtent.Height));

            // Voxel model camera
            var pos = new Vec3(15.0f, 15.0f, 15.0f);
            var dir = new Vec3(0, 0, 0) - pos;
            camera = new Camera(new CameraCoordinate(pos, dir, new Vec3(0.0f, -1.0f, 0.0f)),
                            new ProjectInfo(45.0f, (float)vkSwapchain.extent.Width / (float)vkSwapchain.extent.Height));

            startTime = DateTime.Now;

            kbMap.AddKeyBinding(Key.W, camera.OnForward);
            kbMap.AddKeyBinding(Key.S, camera.OnBackward);
            kbMap.AddKeyBinding(Key.A, camera.OnLeft);
            kbMap.AddKeyBinding(Key.D, camera.OnRight);
            kbMap.AddKeyBinding(Key.Escape, window.Close);
            mouseMap.OnLeftClick += MouseMap_OnLeftClick;
            mouseMap.OnMouseMove += MouseMap_OnMouseMove;
        }

        private void MouseMap_OnMouseMove(IMouse arg1, Vec2 arg2)
        {
            if(arg1.Cursor.CursorMode == CursorMode.Raw)
            {
                camera.OnRotate(arg2.X, arg2.Y);
            }
        }

        private void MouseMap_OnLeftClick(IMouse arg1, Vec2 arg2)
        {
            if (mouseMap.cursor.CursorMode == CursorMode.Raw)
            {
                mouseMap.cursor.CursorMode = CursorMode.Normal;
            }
            else if (mouseMap.cursor.CursorMode == CursorMode.Normal)
            {
                mouseMap.cursor.CursorMode = CursorMode.Raw;
            }
        }

        public void MainLoop()
        {
            window.Update += kbMap.Update;
            window.Update += mouseMap.Update;
            window.Render += DrawFrame;
            window.Run();
            vk.DeviceWaitIdle(device);
        }
        private unsafe void DrawFrame(double obj)
        {
            var fence = inFlightFences[currentFrame];
            vk.WaitForFences(device, 1, in fence, Vk.True, ulong.MaxValue);

            var result = vkSwapchain.AcquireNextImage(ulong.MaxValue, imageAvailableSemaphores[currentFrame], default, out var imageIndex);
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

            animations?[0].Update(obj);
            UpdateUniformBuffer((uint)imageIndex);

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

            fixed (SwapchainKHR* swapchainPtr = &vkSwapchain.swapchain)
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

                result = vkSwapchain.khrSwapchain.QueuePresent(graphicsQueue, &presentInfo);
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

            currentFrame = (currentFrame+1) % (uint)vkSwapchain.ImageCount;
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
                mesh.assertInfo.Update(camera, (int)currentFrame);
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
            vkSwapchain = new VkSwapchain(vkContext, khrSurface, surface, window);
            vkSwapchain.Create();

            return true;
        }

        private unsafe void CreateRenderPass()
        {
            FindDepthFormat(out var depthFormat);

            var tRenderPass = new VkRenderPassBuilder(vk, device);
            {
                tRenderPass.AddAttachment(new AttachmentDescription(null, vkSwapchain.format,
                    SampleCountFlags.Count1Bit, AttachmentLoadOp.Clear, AttachmentStoreOp.Store,
                    null, null, ImageLayout.Undefined, ImageLayout.PresentSrcKhr));
                tRenderPass.AddAttachment(new AttachmentDescription(null, depthFormat,
                    SampleCountFlags.Count1Bit, AttachmentLoadOp.Clear, AttachmentStoreOp.DontCare,
                    null, null, ImageLayout.Undefined, ImageLayout.DepthStencilAttachmentOptimal));
            }
            {
                var subpassDep = new VkSubpassDesc();
                subpassDep.AddColorAttachment(0);
                subpassDep.DepthStencilAttachmentRef = 1;
                tRenderPass.AddSubpass(subpassDep);
            }
            renderPass = tRenderPass.Build();
        }

        private unsafe void CreateGraphicsPipeline()
        {
            // parse DescriptorSetLayout from shader.spirv
            var shaderDefines = new ShaderDefine[2];
            shaderDefines[0] = new ShaderDefine("./Shaders/triangle.vert.spv", ShaderStageFlags.VertexBit);
            shaderDefines[1] = new ShaderDefine("./Shaders/triangle.frag.spv", ShaderStageFlags.FragmentBit);
            // using some class to simplfy pipeline create

            VkPipelineBuilder pipelineBuilder = new VkPipelineBuilder(vkContext);
            pipelineBuilder.BindShaders(shaderDefines);
            pipelineBuilder.BindInputAssemblyState(PrimitiveTopology.TriangleList);
            pipelineBuilder.BindViewportState(vkSwapchain.extent);
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
            //pipelineBuilder.BindVertexInput<AnimatedVertex>(default);
            pipelineBuilder.BindRenderPass(renderPass, 0);
            pipelineBuilder.Build();
            descriptorSetLayout = pipelineBuilder.pipelineInfo.setLayoutInfo.SetLayout;
            PoolSizes = pipelineBuilder.pipelineInfo.setLayoutInfo.PoolSizes;
            pipelineLayout = pipelineBuilder.pipelineInfo.Layout;
            graphicsPipeline = pipelineBuilder.pipeline;
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
            var extent = vkSwapchain.extent;
            CreateImage(extent.Width, extent.Height, 1,
                depthFormat, ImageTiling.Optimal,
                ImageUsageFlags.DepthStencilAttachmentBit,
                MemoryPropertyFlags.DeviceLocalBit,
                out depthImage, out depthImageMemory);
            CreateImageView(depthImage, 1, depthFormat, ImageAspectFlags.DepthBit, out depthImageView);
            TransitionImageLayout(depthImage, 1, depthFormat,
                ImageLayout.Undefined, ImageLayout.DepthStencilAttachmentOptimal);
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
            // (path, name, animationIndex)
            var modelPathNames = new ValueTuple<string, string, int>[]
            {
                //("Asserts/CesiumMan/glTF", "CesiumMan.gltf", 0),
                //("Asserts/facial_body", "scene.gltf", 1),
                ("Asserts/sponza", "Sponza.gltf", -1),
                //("Asserts/gaz-66", "scene.gltf", -1)
            };

            // load model
            var stCommand = new SingleTimeCommand(vk, device, commandPool, graphicsQueue);
            meshes = new Mesh[modelPathNames.Length];
            for (var i = 0; i < modelPathNames.Length; i++)
            {
                meshes[i] = new Mesh(vkContext);

                /// gltf load model
                var (path, name, animIndex) = modelPathNames[i];
                var loader = new Gltf2Loader(path, name);
                if(animIndex > -1)  // animated model
                {
                    animations = loader.ParseAnimation(vkContext, stCommand, staging);
                    meshes[i].model = animations[0];
                    animations[0].UpdateBuffer = UpdateBufferData;
                    //CreateBuffers(BufferUsageFlags.StorageBufferBit | BufferUsageFlags.TransferDstBit,
                    //        animations[0].jointsInverseMatrix, out animations[0].buffers);
                    //CreateVertexBuffer(i, animations[0].vertexBuffer);
                    //CreateIndexBuffer(i, animations[0].indexBuffer);
                }
                else
                {
                    var gltf2 = loader.Parse(vkContext, stCommand, staging);
                    //meshes[i].vertices = gltf2.vertices;
                    //meshes[i].indices = gltf2.indices;
                    meshes[i].model = gltf2;
                    //CreateVertexBuffer(i, meshes[i].vertices);
                    //CreateIndexBuffer(i, meshes[i].indices);

                    var rhiAssert = new GltfAssertInfo(vkContext, stCommand, staging);
                    rhiAssert.Initialize(gltf2, new DescriptorSetLayout[] { descriptorSetLayout }, PoolSizes, vkSwapchain.ImageCount);
                    meshes[i].assertInfo = rhiAssert;
                    meshes[i].descriptorPool = meshes[i].assertInfo.pool;
                }

                /// voxel load model
                //var voxel = new VoxelLoader();
                //voxel.Build(vkContext, stCommand, staging);
                //meshes[i].vertices = voxel.vertices;
                //meshes[i].indices = voxel.indices;
                //meshes[i].model = voxel;
            }

            meshes[0].matrix = Matrix4X4<float>.Identity;
            meshes[0].frameRotate = Scalar.DegreesToRadians<float>(0.0f);
            //meshes[0].matrix = Matrix4X4.CreateScale<float>(5.0f) * Matrix4X4.CreateTranslation<float>(new Vec3(-400, 0, 0));
            //meshes[0].frameRotate = Scalar.DegreesToRadians<float>(0.0f);
            //meshes[1].matrix = Matrix4X4.CreateScale<float>(0.03f);
            //meshes[1].frameRotate = Scalar.DegreesToRadians<float>(77.0f);
        }

        public void UpdateBufferData(Matrix4X4<float>[] data, VkBuffer buffer)
        {
            buffer.UpdateData(data, commandPool, graphicsQueue, staging);
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
            vkFramebuffer = new VkFramebuffer(vkContext, vkSwapchain.ImageCount);
            vkFramebuffer.SetRenderpass(renderPass);
            vkFramebuffer.SetExtent(vkSwapchain.extent);
            for (int i = 0; i < vkSwapchain.ImageCount; i++)
            {
                vkFramebuffer.AddAttachment(i, 0, vkSwapchain.imageViews[i]);
                vkFramebuffer.AddAttachment(i, 1, depthImageView);
            }
            vkFramebuffer.Build();
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
            commandBuffers = new CommandBuffer[vkFramebuffer.framebuffers.Length];

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
                    Framebuffer = vkFramebuffer.framebuffers[i],
                    RenderArea = { Offset = new Offset2D { X = 0, Y = 0 }, Extent = vkSwapchain.extent }
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
                        meshes[m].descriptorPool.BindCommand(commandBuffers[i], pipelineLayout, i, nodeIndex);
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
            vkFramebuffer.Dispose();

            staging.Dispose();

            if(EnableValidationLayers)
            {
                debugUtils.DestroyDebugUtilsMessenger(instance, debugMessager, null);
            }
            vkSwapchain.Dispose();
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
