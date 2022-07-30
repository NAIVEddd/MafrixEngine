﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Assimp;
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
using Image = Silk.NET.Vulkan.Image;
using Semaphore = Silk.NET.Vulkan.Semaphore;
using SlImage = SixLabors.ImageSharp.Image;

namespace MafrixEngine.GraphicsWrapper
{
    using Buffer = Silk.NET.Vulkan.Buffer;
    using Vec2 = Vector2D<float>;
    using Vec3 = Vector3D<float>;
    using Mat4 = Matrix4X4<float>;
    public struct Vertex
    {
        public Vec3 pos;
        public Vec3 color;
        public Vec2 texCoord;
        public Vertex(Vec3 p, Vec3 c, Vec2 t) => (pos, color, texCoord) = (p, c, t);
        public unsafe static VertexInputBindingDescription GetBindingDescription()
        {
            var bindingDescription = new VertexInputBindingDescription();
            bindingDescription.Binding = 0;
            bindingDescription.Stride = (uint) sizeof(Vertex);
            bindingDescription.InputRate = VertexInputRate.Vertex;

            return bindingDescription;
        }
        public unsafe static VertexInputAttributeDescription[] GetAttributeDescriptions()
        {
            var attributeDescriptions = new VertexInputAttributeDescription[3];
            attributeDescriptions[0].Binding = 0;
            attributeDescriptions[0].Location = 0;
            attributeDescriptions[0].Format = Format.R32G32B32Sfloat;
            attributeDescriptions[0].Offset = (uint)Marshal.OffsetOf<Vertex>("pos").ToInt32();
            attributeDescriptions[1].Binding = 0;
            attributeDescriptions[1].Location = 1;
            attributeDescriptions[1].Format = Format.R32G32B32Sfloat;
            attributeDescriptions[1].Offset = (uint)Marshal.OffsetOf<Vertex>("color").ToInt32();
            attributeDescriptions[2].Binding = 0;
            attributeDescriptions[2].Location = 2;
            attributeDescriptions[2].Format = Format.R32G32Sfloat;
            attributeDescriptions[2].Offset = (uint)Marshal.OffsetOf<Vertex>("texCoord").ToInt32();

            return attributeDescriptions;
        }
    }

    public struct UniformBufferObject
    {
        public Mat4 model;
        public Mat4 view;
        public Mat4 proj;
        public UniformBufferObject(Mat4 m, Mat4 v, Mat4 p) => (model, view, proj) = (m, v, p);
    }

    public class VulkanWrapper : IDisposable
    {
        private Vk vk;
        public Instance instance;
        public PhysicalDevice physicalDevice;
        public Device device;
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
        public Buffer vertexBuffer;
        public DeviceMemory vertexBufferMemory;
        public Buffer indexBuffer;
        public DeviceMemory indexBufferMemory;
        public Buffer[] uniformBuffers;
        public DeviceMemory[] uniformBuffersMemory;
        public string textureName;
        public UInt32 mipLevels;
        public Image textureImage;
        public DeviceMemory textureImageMemory;
        public ImageView textureImageView;
        public Image depthImage;
        public DeviceMemory depthImageMemory;
        public ImageView depthImageView;
        public Sampler textureSampler;
        public DescriptorPool descriptorPool;
        public DescriptorSet[] descriptorSets;
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

        private ExtDebugUtils debugUtils;
        private DebugUtilsMessengerEXT debugMessager;

        public const int MaxFrameInFlight = 8;
        /// <summary>
        /// Extensions and ValidationLayers
        /// </summary>
        private string[] deviceExtensions = { KhrSwapchain.ExtensionName };
#if DEBUG
        public const bool EnableValidationLayers = true;
        private string[][] validationLayerNamesPriorityList =
        {
            new [] {"VK_LAYER_KHRONOS_validation"},
            new [] {"VK_LAYER_LUNARG_standard_validation"},
            new []
            {
                "VK_LAYER_LUNARG_parameter_validation",
                "VK_LAYER_LUNARG_object_tracker",
                "VK_LAYER_LUNARG_core_validation"
            }
        };
        private string[] validationLayers;
        private string[] instanceExtensions = { ExtDebugUtils.ExtensionName };
#else
        public const bool EnableValidationLayers = false;
#endif
        public unsafe static int GetArrayByteSize<T>(T[] array)
        {
            return Marshal.SizeOf<T>() * array.Length;
        }
        public Vertex[] vertices;
        public UInt32[] indices;

        public VulkanWrapper()
        {
            vk = Vk.GetApi();
            window = InitWindow();
        }

        private unsafe void MarshalAssignString(out byte* targ, string s)
        {
            targ = (byte*) Marshal.StringToHGlobalAnsi(s);
        }
        private unsafe void MarshalFreeString(in byte* targ)
        {
            Marshal.FreeHGlobal((IntPtr)targ);
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
            CreateInstance();
            SetupDebugMessager();
            CreateSurface();
            PickPhysicalDevice();
            CreateLogicalDevice();
            CreateSwapChain();
            CreateImageViews();
            CreateRenderPass();
            CreateDescriptorSetLayout();
            CreateGraphicsPipeline();
            CreateCommandPool();
            CreateDepthResources();
            CreateFramebuffers();
            LoadModel();
            CreateTextureImage();
            CreateTextureImageView();
            CreateTextureSampler();
            CreateVertexBuffer();
            CreateIndexBuffer();
            CreateUniformBuffers();
            CreateDescriptorPool();
            CreateDescriptorSets();
            CreateCommandBuffers();
            CreateSyncObjects();

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
            PipelineStageFlags[] waitStages = { PipelineStageFlags.PipelineStageColorAttachmentOutputBit };
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

            currentFrame = (currentFrame + 1) % MaxFrameInFlight;
        }

        private DateTime startTime;
        private unsafe void UpdateUniformBuffer(uint index)
        {
            var time = (float)(DateTime.Now - startTime).TotalSeconds;


            var model = Mat4.Identity;
            //var model = Matrix4X4.CreateRotationY<float>(Scalar.DegreesToRadians<float>(time * 90.0f));
            var view = Matrix4X4.CreateLookAt<float>(new Vec3(35.0f, 35.0f, 35.0f), new Vec3(0.0f), new Vec3(0.0f, -1.0f, 0.0f));
            var proj = Matrix4X4.CreatePerspectiveFieldOfView<float>(Scalar.DegreesToRadians<float>(45.0f),
                (float)swapchainExtent.Width / (float)swapchainExtent.Height, 0.1f, 100.0f);
            proj.M11 *= -1.0f;
            var ubo = new UniformBufferObject(model, view, proj);

            void* data = null;
            ulong datasize = (ulong)Unsafe.SizeOf<UniformBufferObject>();
            vk.MapMemory(device, uniformBuffersMemory[index], 0, datasize, 0, ref data);
            Unsafe.CopyBlock(data, &ubo, (uint)datasize);
            vk.UnmapMemory(device, uniformBuffersMemory[index]);
        }

        private unsafe string[]? GetOptimalValidationLayers()
        {
            uint layerCount = 0;
            vk.EnumerateInstanceLayerProperties(ref layerCount, null);

            var availableLayers = new LayerProperties[layerCount];
            vk.EnumerateInstanceLayerProperties(&layerCount, availableLayers);

            var availableLayerNames = availableLayers.Select(availableLayer => Marshal.PtrToStringAnsi((nint)availableLayer.LayerName)).ToArray();
            foreach(var validationLayerNameSet in validationLayerNamesPriorityList)
            {
                if(validationLayerNameSet.All(validationLayerName => availableLayerNames.Contains(validationLayerName)))
                {
                    return validationLayerNameSet;
                }
            }
            return null;
        }

        private unsafe void CreateSurface()
        {
            surface = window.VkSurface!.Create<AllocationCallbacks>(instance.ToHandle(), null).ToSurface();
        }
        public unsafe void CreateInstance()
        {
            if(EnableValidationLayers)
            {
                validationLayers = GetOptimalValidationLayers();
                if(validationLayers is null)
                {
                    throw new NotSupportedException("Validation layers requested, but not available!");
                }
            }

            var appInfo = new ApplicationInfo();
            appInfo.SType = StructureType.ApplicationInfo;
            MarshalAssignString(out appInfo.PApplicationName, "First Vulkan Application");
            appInfo.ApplicationVersion = new Version32(0, 0, 1);
            //appInfo.PEngineName = (byte*)SilkMarshal.StringToPtr("Mafrix Engine");
            MarshalAssignString(out appInfo.PEngineName, "Mafrix Engine");
            appInfo.EngineVersion = new Version32(0, 0, 1);
            appInfo.ApiVersion = Vk.Version11;

            var createInfo = new InstanceCreateInfo();
            createInfo.SType = StructureType.InstanceCreateInfo;
            createInfo.PApplicationInfo = &appInfo;

            var extensions = window.VkSurface!.GetRequiredExtensions(out var extCount);
#if DEBUG
            // display supported Extension Properties
            {
                uint extensionCount = 0;
                vk.EnumerateInstanceExtensionProperties((byte*)null, &extensionCount, null);
                var extensionsArray = stackalloc ExtensionProperties[(int)extensionCount];
                vk.EnumerateInstanceExtensionProperties((byte*)null, &extensionCount, extensionsArray);
                Console.WriteLine("Instance Extension properties include:");
                for(var i = 0; i < extensionCount; i++)
                {
                    var extension = extensionsArray[i];
                    var name = Marshal.PtrToStringAnsi((IntPtr)extension.ExtensionName);
                    Console.WriteLine(name);
                }
                Console.WriteLine("-------------------------------------");
            }

            var newExtensions = stackalloc byte*[(int)(extCount + instanceExtensions.Length)];
            for(var i = 0; i < extCount; i++)
            {
                newExtensions[i] = extensions[i];
            }
            for(var i = 0; i < instanceExtensions.Length; i++)
            {
                newExtensions[extCount + i] = (byte*)SilkMarshal.StringToPtr(instanceExtensions[i]);
            }
            extCount += (uint) instanceExtensions.Length;
#else
            var newExtensions = extensions;
#endif
            createInfo.EnabledExtensionCount = extCount;
            createInfo.PpEnabledExtensionNames = newExtensions;

#if DEBUG
            createInfo.EnabledLayerCount = (uint)validationLayers.Length;
            createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(validationLayers);
#endif

            if (vk.CreateInstance(createInfo, null, out instance) != Result.Success)
            {
                throw new Exception("Failed to create instance!");
            }
            vk.CurrentInstance = instance;

            if(!vk.TryGetInstanceExtension(instance, out khrSurface))
            {
                throw new NotSupportedException("KHR_surface extension not found.");
            }

            MarshalFreeString(appInfo.PApplicationName);
            MarshalFreeString(appInfo.PEngineName);
            if(EnableValidationLayers)
            {
                SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
            }
        }

        private unsafe void CreateLogicalDevice()
        {
            var indices = FindQueueFamilies(physicalDevice);
            var uniqueQueueFamilies = indices.GraphicsFamily!.Value == indices.PresentFamily!.Value
                ? new[] { indices.GraphicsFamily.Value }
                : new[] { indices.GraphicsFamily.Value, indices.PresentFamily.Value };

            using var mem = GlobalMemory.Allocate((int)uniqueQueueFamilies.Length * sizeof(DeviceQueueCreateInfo));
            var queueCreateInfos = (DeviceQueueCreateInfo*)Unsafe.AsPointer(ref mem.GetPinnableReference());

            var queuePriority = 1f;
            for(var i = 0; i < uniqueQueueFamilies.Length; i++)
            {
                var queueCreateInfo = new DeviceQueueCreateInfo(StructureType.DeviceQueueCreateInfo);
                queueCreateInfo.QueueFamilyIndex = uniqueQueueFamilies[i];
                queueCreateInfo.QueueCount = 1;
                queueCreateInfo.PQueuePriorities = &queuePriority;
                queueCreateInfos[i] = queueCreateInfo;
            }

            var deviceFeatures = new PhysicalDeviceFeatures();
            deviceFeatures.SamplerAnisotropy = Vk.True;
            var createInfo = new DeviceCreateInfo(StructureType.DeviceCreateInfo);
            createInfo.QueueCreateInfoCount = (uint)uniqueQueueFamilies.Length;
            createInfo.PQueueCreateInfos = queueCreateInfos;
            createInfo.PEnabledFeatures = &deviceFeatures;
            createInfo.EnabledExtensionCount = (uint)deviceExtensions.Length;

            var enabledExtensionNames = SilkMarshal.StringArrayToPtr(deviceExtensions);
            createInfo.PpEnabledExtensionNames = (byte**)enabledExtensionNames;

            if(EnableValidationLayers)
            {
                createInfo.EnabledLayerCount = (uint)validationLayers.Length;
                createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(validationLayers);
            }
            else
            {
                createInfo.EnabledLayerCount=0;
            }

            vk.CreateDevice(physicalDevice, in createInfo, null, out device);
            vk.GetDeviceQueue(device, indices.GraphicsFamily.Value, 0, out graphicsQueue);

            vk.CurrentDevice = device;
            if(!vk.TryGetDeviceExtension(instance, device, out khrSwapchain))
            {
                throw new NotSupportedException("KHR_swapchain extension not found.");
            }
        }

        private unsafe void SetupDebugMessager()
        {
            if (!EnableValidationLayers) return;
            if (!vk.TryGetInstanceExtension(instance, out debugUtils)) return;

            var createInfo = new DebugUtilsMessengerCreateInfoEXT(StructureType.DebugUtilsMessengerCreateInfoExt);
            createInfo.MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityVerboseBitExt |
                                         DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityWarningBitExt |
                                         DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityErrorBitExt;
            createInfo.MessageType = DebugUtilsMessageTypeFlagsEXT.DebugUtilsMessageTypeGeneralBitExt |
                                     DebugUtilsMessageTypeFlagsEXT.DebugUtilsMessageTypePerformanceBitExt |
                                     DebugUtilsMessageTypeFlagsEXT.DebugUtilsMessageTypeValidationBitExt;
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
            if(messageSeverity > DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityVerboseBitExt)
            {
                Console.WriteLine($"{messageSeverity} {messageTypes}" + Marshal.PtrToStringAnsi((nint)pCallbackData->PMessage));
            }
            return Vk.False;
        }

        private unsafe void PickPhysicalDevice()
        {
            var devices = vk.GetPhysicalDevices(instance);
            if(!devices.Any())
            {
                throw new NotSupportedException("Failed to find GPUs with vulkan support.");
            }

            physicalDevice = devices.FirstOrDefault(device =>
            {
                var indices = FindQueueFamilies(device);
                var extensionsSupported = CheckDeviceExtensionSupport(device);
                var swapChainAdequate = false;
                if (extensionsSupported)
                {
                    var swapChainSupport = QuerySwapChainSupport(device);
                    swapChainAdequate = swapChainSupport.Formats.Length != 0 && swapChainSupport.PresentModes.Length != 0;
                }
                vk.GetPhysicalDeviceFeatures(device, out var supportedFeatures);
                return indices.IsComplete()
                    && extensionsSupported && swapChainAdequate
                    && supportedFeatures.SamplerAnisotropy;
            });

            if(physicalDevice.Handle == 0)
            {
                throw new Exception("No suitable device.");
            }
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
        private unsafe bool CheckDeviceExtensionSupport(PhysicalDevice device)
        {
            return deviceExtensions.All(ext => vk.IsDeviceExtensionPresent(instance, ext));
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
                if(queueFamily.QueueFlags.HasFlag(QueueFlags.QueueGraphicsBit))
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
            createInfo.ImageUsage = ImageUsageFlags.ImageUsageColorAttachmentBit;

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
                createInfo.CompositeAlpha = CompositeAlphaFlagsKHR.CompositeAlphaOpaqueBitKhr;
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
                if(mode == PresentModeKHR.PresentModeMailboxKhr)
                {
                    return mode;
                }
            }
            return PresentModeKHR.PresentModeFifoKhr;
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
                CreateImageView(swapchainImages[i], 1, swapchainImageFormat, ImageAspectFlags.ImageAspectColorBit, out var imageView);
                
                swapchainImageViews[i] = imageView;
            }
        }

        private unsafe void CreateRenderPass()
        {
            var colorAttachment = new AttachmentDescription();
            colorAttachment.Format = swapchainImageFormat;
            colorAttachment.Samples = SampleCountFlags.SampleCount1Bit;
            colorAttachment.LoadOp = AttachmentLoadOp.Clear;
            colorAttachment.StoreOp = AttachmentStoreOp.Store;
            colorAttachment.StencilLoadOp = AttachmentLoadOp.DontCare;
            colorAttachment.StencilStoreOp = AttachmentStoreOp.DontCare;
            colorAttachment.InitialLayout = ImageLayout.Undefined;
            colorAttachment.FinalLayout = ImageLayout.PresentSrcKhr;
            var depthAttachment = new AttachmentDescription();
            FindDepthFormat(out depthAttachment.Format);
            depthAttachment.Samples = SampleCountFlags.SampleCount1Bit;
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
                PipelineStageFlags.PipelineStageColorAttachmentOutputBit |
                PipelineStageFlags.PipelineStageEarlyFragmentTestsBit;
            dependency.DstStageMask =
                PipelineStageFlags.PipelineStageColorAttachmentOutputBit |
                PipelineStageFlags.PipelineStageEarlyFragmentTestsBit;
            dependency.SrcAccessMask = 0;
            dependency.DstAccessMask =
                AccessFlags.AccessColorAttachmentWriteBit |
                AccessFlags.AccessDepthStencilAttachmentWriteBit;

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
            var vertShaderCode = LoadEmbeddedResourceBytes("MafrixEngine.Shaders.triangle.vert.spv");
            var fragShaderCode = LoadEmbeddedResourceBytes("MafrixEngine.Shaders.triangle.frag.spv");
            var vertShaderModule = CreateShaderModule(vertShaderCode);
            var fragShaderModule = CreateShaderModule(fragShaderCode);

            // init shader stage
            var vertShaderStageInfo = new PipelineShaderStageCreateInfo(StructureType.PipelineShaderStageCreateInfo);
            vertShaderStageInfo.Stage = ShaderStageFlags.ShaderStageVertexBit;
            vertShaderStageInfo.Module = vertShaderModule;
            vertShaderStageInfo.PName = (byte*)SilkMarshal.StringToPtr("main");
            var fragShaderStageInfo = new PipelineShaderStageCreateInfo(StructureType.PipelineShaderStageCreateInfo);
            fragShaderStageInfo.Stage = ShaderStageFlags.ShaderStageFragmentBit;
            fragShaderStageInfo.Module = fragShaderModule;
            fragShaderStageInfo.PName = (byte*)SilkMarshal.StringToPtr("main");

            var shaderStages = stackalloc PipelineShaderStageCreateInfo[2];
            shaderStages[0] = vertShaderStageInfo;
            shaderStages[1] = fragShaderStageInfo;

            var bindingDescription = Vertex.GetBindingDescription();
            var attributeDescriptions = Vertex.GetAttributeDescriptions();

            var vertexInputInfo = new PipelineVertexInputStateCreateInfo(StructureType.PipelineVertexInputStateCreateInfo);
            vertexInputInfo.VertexBindingDescriptionCount = 1;
            vertexInputInfo.PVertexBindingDescriptions = &bindingDescription;
            vertexInputInfo.VertexAttributeDescriptionCount = (uint)attributeDescriptions.Length;
            fixed(VertexInputAttributeDescription* attributes = attributeDescriptions)
            {
                vertexInputInfo.PVertexAttributeDescriptions = attributes;
            }
            var inputAssembly = new PipelineInputAssemblyStateCreateInfo(StructureType.PipelineInputAssemblyStateCreateInfo);
            inputAssembly.Topology = PrimitiveTopology.TriangleList;
            inputAssembly.PrimitiveRestartEnable = Vk.False;

            var viewport = new Viewport();
            viewport.X = 0.0f;
            viewport.Y = 0.0f;
            viewport.Width = swapchainExtent.Width;
            viewport.Height = swapchainExtent.Height;
            viewport.MinDepth = 0.0f;
            viewport.MaxDepth = 1.0f;
            var scissor = new Rect2D(default, swapchainExtent);
            var viewportState = new PipelineViewportStateCreateInfo(StructureType.PipelineViewportStateCreateInfo);
            viewportState.ViewportCount = 1;
            viewportState.PViewports = &viewport;
            viewportState.ScissorCount = 1;
            viewportState.PScissors = &scissor;

            var rasterizer = new PipelineRasterizationStateCreateInfo(StructureType.PipelineRasterizationStateCreateInfo);
            rasterizer.DepthClampEnable = Vk.False;
            rasterizer.RasterizerDiscardEnable = Vk.False;
            rasterizer.PolygonMode = PolygonMode.Fill;
            rasterizer.LineWidth = 1.0f;
            rasterizer.CullMode = CullModeFlags.CullModeBackBit;
            rasterizer.FrontFace = FrontFace.CounterClockwise;
            rasterizer.DepthBiasEnable = Vk.False;
            var multisampling = new PipelineMultisampleStateCreateInfo(StructureType.PipelineMultisampleStateCreateInfo);
            multisampling.SampleShadingEnable = Vk.False;
            multisampling.RasterizationSamples = SampleCountFlags.SampleCount1Bit;

            var colorBlendAttachment = new PipelineColorBlendAttachmentState();
            colorBlendAttachment.ColorWriteMask =
                    ColorComponentFlags.ColorComponentRBit |
                    ColorComponentFlags.ColorComponentGBit |
                    ColorComponentFlags.ColorComponentBBit |
                    ColorComponentFlags.ColorComponentABit;
            colorBlendAttachment.BlendEnable = Vk.False;
            var colorBlending = new PipelineColorBlendStateCreateInfo(StructureType.PipelineColorBlendStateCreateInfo);
            colorBlending.LogicOpEnable = Vk.False;
            colorBlending.LogicOp = LogicOp.Copy;
            colorBlending.AttachmentCount = 1;
            colorBlending.PAttachments = &colorBlendAttachment;
            colorBlending.BlendConstants[0] = 0.0f;
            colorBlending.BlendConstants[1] = 0.0f;
            colorBlending.BlendConstants[2] = 0.0f;
            colorBlending.BlendConstants[3] = 0.0f;

            var pipelineLayoutInfo = new PipelineLayoutCreateInfo(StructureType.PipelineLayoutCreateInfo);
            pipelineLayoutInfo.SetLayoutCount = 1;
            fixed(DescriptorSetLayout* ptr = &descriptorSetLayout)
            {
                pipelineLayoutInfo.PSetLayouts = ptr;
            }
            pipelineLayoutInfo.PushConstantRangeCount = 0;

            if(vk.CreatePipelineLayout(device, in pipelineLayoutInfo, null, out pipelineLayout) != Result.Success)
            {
                throw new Exception("failed to create pipeline layout!");
            }

            var depthStencil = new PipelineDepthStencilStateCreateInfo(StructureType.PipelineDepthStencilStateCreateInfo);
            depthStencil.DepthTestEnable = Vk.True;
            depthStencil.DepthWriteEnable = Vk.True;
            depthStencil.DepthCompareOp = CompareOp.Less;
            depthStencil.DepthBoundsTestEnable = Vk.False;
            depthStencil.MinDepthBounds = 0.0f;
            depthStencil.MaxDepthBounds = 1.0f;
            depthStencil.StencilTestEnable = Vk.False;

            var pipelineInfo = new GraphicsPipelineCreateInfo(StructureType.GraphicsPipelineCreateInfo);
            pipelineInfo.StageCount = 2;
            pipelineInfo.PStages = shaderStages;
            pipelineInfo.PVertexInputState = &vertexInputInfo;
            pipelineInfo.PInputAssemblyState = &inputAssembly;
            pipelineInfo.PViewportState = &viewportState;
            pipelineInfo.PRasterizationState = &rasterizer;
            pipelineInfo.PMultisampleState = &multisampling;
            pipelineInfo.PColorBlendState = &colorBlending;
            pipelineInfo.PDepthStencilState = &depthStencil;
            pipelineInfo.Layout = pipelineLayout;
            pipelineInfo.RenderPass = renderPass;
            pipelineInfo.Subpass = 0;
            pipelineInfo.BasePipelineHandle = default;

            if(vk.CreateGraphicsPipelines(device, default, 1, in pipelineInfo, null, out graphicsPipeline) != Result.Success)
            {
                throw new Exception("failed to create graphics pipeline.");
            }

            vk.DestroyShaderModule(device, vertShaderModule, null);
            vk.DestroyShaderModule(device, fragShaderModule, null);
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
                FormatFeatureFlags.FormatFeatureDepthStencilAttachmentBit,
                out format);
        }

        private unsafe void CreateDepthResources()
        {
            FindDepthFormat(out Format depthFormat);
            CreateImage(swapchainExtent.Width, swapchainExtent.Height, 1,
                depthFormat, ImageTiling.Optimal,
                ImageUsageFlags.ImageUsageDepthStencilAttachmentBit,
                MemoryPropertyFlags.MemoryPropertyDeviceLocalBit,
                out depthImage, out depthImageMemory);
            CreateImageView(depthImage, 1, depthFormat, ImageAspectFlags.ImageAspectDepthBit, out depthImageView);
            TransitionImageLayout(depthImage, 1, depthFormat,
                ImageLayout.Undefined, ImageLayout.DepthStencilAttachmentOptimal);
        }

        private unsafe void CreateTextureImage()
        {
            var filename = "Asserts/viking_room/textures/viking_room.png";
            using var image = SlImage.Load<Rgba32>(filename);
            var memoryGroup = image.GetPixelMemoryGroup();
            var imageSize = memoryGroup.TotalLength * sizeof(Rgba32);
            Memory<byte> array = new byte[imageSize];

            var block = MemoryMarshal.Cast<byte, Rgba32>(array.Span);
            foreach (var memory in memoryGroup)
            {
                memory.Span.CopyTo(block);
                block = block.Slice(memory.Length);
            }
            var width = image.Width;
            var height = image.Height;
            mipLevels = (UInt32)Math.Floor(Math.Log2(Math.Max(width, height))) + 1;

            CreateBuffer((ulong)imageSize, BufferUsageFlags.BufferUsageTransferSrcBit,
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit |
                MemoryPropertyFlags.MemoryPropertyHostCoherentBit,
                out var stagingBuffer, out var stagingBufferMemory);
            void* data = null;
            vk.MapMemory(device, stagingBufferMemory, 0, (ulong)imageSize, 0, ref data);
            var dataSpan = new Span<byte>(data, (int)imageSize);
            array.Span.CopyTo(dataSpan);
            vk.UnmapMemory(device, stagingBufferMemory);

            CreateImage((uint)width, (uint)height, mipLevels,
                Format.R8G8B8A8Srgb,
                ImageTiling.Optimal,
                ImageUsageFlags.ImageUsageTransferSrcBit |
                ImageUsageFlags.ImageUsageTransferDstBit |
                ImageUsageFlags.ImageUsageSampledBit,
                MemoryPropertyFlags.MemoryPropertyDeviceLocalBit,
                out textureImage, out textureImageMemory);
            TransitionImageLayout(textureImage, mipLevels, Format.R8G8B8A8Srgb,
                ImageLayout.Undefined, ImageLayout.TransferDstOptimal);
            CopyBufferToImage(stagingBuffer, textureImage, (uint)width, (uint)height);
            GenerateMipmaps(textureImage, Format.R8G8B8A8Srgb, (uint)width, (uint)height, mipLevels);
            //TransitionImageLayout(textureImage, mipLevels, Format.R8G8B8A8Srgb,
            //    ImageLayout.TransferDstOptimal,
            //    ImageLayout.ShaderReadOnlyOptimal);

            vk.DestroyBuffer(device, stagingBuffer, null);
            vk.FreeMemory(device, stagingBufferMemory, null);
        }

        private unsafe void GenerateMipmaps(Image image, Format imageFormat, uint width, uint height, uint mipLevels)
        {
            vk.GetPhysicalDeviceFormatProperties(physicalDevice, imageFormat, out var formatProperties);
            if(0 == (formatProperties.OptimalTilingFeatures & FormatFeatureFlags.FormatFeatureSampledImageFilterLinearBit))
            {
                throw new Exception("texture image format does not support linear blitting.");
            }
            BeginSingleTimeCommands(out var commandBuffer);

            var barrier = new ImageMemoryBarrier(StructureType.ImageMemoryBarrier);
            barrier.Image = image;
            barrier.SrcQueueFamilyIndex = Vk.QueueFamilyIgnored;
            barrier.DstQueueFamilyIndex = Vk.QueueFamilyIgnored;
            barrier.SubresourceRange.AspectMask = ImageAspectFlags.ImageAspectColorBit;
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
                barrier.SrcAccessMask = AccessFlags.AccessTransferWriteBit;
                barrier.DstAccessMask = AccessFlags.AccessTransferReadBit;

                vk.CmdPipelineBarrier(commandBuffer,
                    PipelineStageFlags.PipelineStageTransferBit,
                    PipelineStageFlags.PipelineStageTransferBit, 0,
                    0, null,
                    0, null, 1, barrier);

                var blit = new ImageBlit();
                blit.SrcOffsets[0].X = 0;
                blit.SrcOffsets[0].Z = 0;
                blit.SrcOffsets[0].Y = 0;
                blit.SrcOffsets[1].X = (int)mipWidth;
                blit.SrcOffsets[1].Y = (int)mipHeight;
                blit.SrcOffsets[1].Z = 1;
                blit.SrcSubresource.AspectMask = ImageAspectFlags.ImageAspectColorBit;
                blit.SrcSubresource.MipLevel = (uint)i - 1;
                blit.SrcSubresource.BaseArrayLayer = 0;
                blit.SrcSubresource.LayerCount = 1;
                blit.DstOffsets[0].X = 0;
                blit.DstOffsets[0].Y = 0;
                blit.DstOffsets[0].Z = 0;
                blit.DstOffsets[1].X = (int) (mipWidth > 1 ? mipWidth / 2 : 1);
                blit.DstOffsets[1].Y = (int)(mipHeight > 1 ? mipHeight / 2 : 1);
                blit.DstOffsets[1].Z = 1;
                blit.DstSubresource.AspectMask = ImageAspectFlags.ImageAspectColorBit;
                blit.DstSubresource.MipLevel = (uint)i;
                blit.DstSubresource.BaseArrayLayer = 0;
                blit.DstSubresource.LayerCount = 1;

                vk.CmdBlitImage(commandBuffer,
                    image, ImageLayout.TransferSrcOptimal,
                    image, ImageLayout.TransferDstOptimal,
                    1, blit, Filter.Linear);

                barrier.OldLayout = ImageLayout.TransferSrcOptimal;
                barrier.NewLayout = ImageLayout.ShaderReadOnlyOptimal;
                barrier.SrcAccessMask = AccessFlags.AccessTransferReadBit;
                barrier.DstAccessMask = AccessFlags.AccessShaderReadBit;
                vk.CmdPipelineBarrier(commandBuffer,
                    PipelineStageFlags.PipelineStageTransferBit,
                    PipelineStageFlags.PipelineStageFragmentShaderBit, 0,
                    0, null,
                    0, null, 1, barrier);

                if (mipWidth > 1) mipWidth /= 2;
                if (mipHeight > 1) mipHeight /= 2;
            }

            barrier.SubresourceRange.BaseMipLevel = mipLevels - 1;
            barrier.OldLayout = ImageLayout.TransferDstOptimal;
            barrier.NewLayout = ImageLayout.ShaderReadOnlyOptimal;
            barrier.SrcAccessMask = AccessFlags.AccessTransferWriteBit;
            barrier.DstAccessMask = AccessFlags.AccessShaderReadBit;

            vk.CmdPipelineBarrier(commandBuffer,
                PipelineStageFlags.PipelineStageTransferBit,
                PipelineStageFlags.PipelineStageFragmentShaderBit, 0,
                0, null,
                0, null, 1, barrier);

            EndSingleTimeCommands(commandBuffer);
        }

        private unsafe void CreateTextureImageView()
        {
            CreateImageView(textureImage, mipLevels, Format.R8G8B8A8Srgb, ImageAspectFlags.ImageAspectColorBit, out textureImageView);
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
            viewInfo.ViewType = ImageViewType.ImageViewType2D;
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
            imageInfo.ImageType = ImageType.ImageType2D;
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
            imageInfo.Samples = SampleCountFlags.SampleCount1Bit;
            if (vk.CreateImage(device, imageInfo, null, out image) != Result.Success)
            {
                throw new Exception("failed to create image.");
            }

            var memRequirements = new MemoryRequirements();
            vk.GetImageMemoryRequirements(device, image, out memRequirements);

            var allocInfo = new MemoryAllocateInfo(StructureType.MemoryAllocateInfo);
            allocInfo.AllocationSize = memRequirements.Size;
            allocInfo.MemoryTypeIndex = FindMemoryType(memRequirements.MemoryTypeBits, MemoryPropertyFlags.MemoryPropertyDeviceLocalBit);
            if (vk.AllocateMemory(device, allocInfo, null, out imageMemory) != Result.Success)
            {
                throw new Exception("failed to allocate image memory.");
            }
            vk.BindImageMemory(device, image, imageMemory, 0);
        }

        private unsafe void TransitionImageLayout(Image image, uint mipLevels, Format format,
            ImageLayout oldLayout, ImageLayout newLayout)
        {
            BeginSingleTimeCommands(out var commandBuffer);

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
                barrier.SubresourceRange.AspectMask = ImageAspectFlags.ImageAspectDepthBit;
                if(HasStencilComponent(format))
                {
                    barrier.SubresourceRange.AspectMask |= ImageAspectFlags.ImageAspectStencilBit;
                }
            }
            else
            {
                barrier.SubresourceRange.AspectMask = ImageAspectFlags.ImageAspectColorBit;
            }

            var sourceStage = new PipelineStageFlags();
            var destinationStage = new PipelineStageFlags();
            if (oldLayout == ImageLayout.Undefined && newLayout == ImageLayout.TransferDstOptimal)
            {
                barrier.SrcAccessMask = 0;
                barrier.DstAccessMask = AccessFlags.AccessTransferWriteBit;
                sourceStage = PipelineStageFlags.PipelineStageTopOfPipeBit;
                destinationStage = PipelineStageFlags.PipelineStageTransferBit;
            } else if (oldLayout == ImageLayout.TransferDstOptimal && newLayout == ImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.AccessTransferWriteBit;
                barrier.DstAccessMask = AccessFlags.AccessShaderReadBit;
                sourceStage = PipelineStageFlags.PipelineStageTransferBit;
                destinationStage = PipelineStageFlags.PipelineStageFragmentShaderBit;
            } else if(oldLayout == ImageLayout.Undefined && newLayout == ImageLayout.DepthStencilAttachmentOptimal)
            {
                barrier.SrcAccessMask = 0;
                barrier.DstAccessMask =
                    AccessFlags.AccessDepthStencilAttachmentReadBit |
                    AccessFlags.AccessDepthStencilAttachmentWriteBit;
                sourceStage = PipelineStageFlags.PipelineStageTopOfPipeBit;
                destinationStage = PipelineStageFlags.PipelineStageEarlyFragmentTestsBit;
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

            EndSingleTimeCommands(commandBuffer);
        }

        private unsafe void CopyBufferToImage(Buffer buffer, Image image, uint width, uint height)
        {
            BeginSingleTimeCommands(out var commandBuffer);

            var region = new BufferImageCopy();
            region.BufferOffset = 0;
            region.BufferRowLength = 0;
            region.BufferImageHeight = 0;
            region.ImageSubresource.AspectMask = ImageAspectFlags.ImageAspectColorBit;
            region.ImageSubresource.MipLevel = 0;
            region.ImageSubresource.BaseArrayLayer = 0;
            region.ImageSubresource.LayerCount = 1;
            region.ImageOffset = new Offset3D(0, 0, 0);
            region.ImageExtent = new Extent3D(width, height, 1);
            vk.CmdCopyBufferToImage(commandBuffer, buffer, image,
                ImageLayout.TransferDstOptimal, 1, region);

            EndSingleTimeCommands(commandBuffer);
        }

        private unsafe void LoadModel()
        {
            using var assimp = Assimp.GetApi();
            var scene = assimp.ImportFile("Asserts/viking_room/scene.gltf", (uint)PostProcessPreset.TargetRealTimeMaximumQuality);
            var vertexMap = new Dictionary<Vertex, uint>();
            var vertices = new List<Vertex>();
            var indices = new List<uint>();

            VisitSceneNode(scene->MRootNode);
            assimp.ReleaseImport(scene);

            this.vertices = vertices.ToArray();
            this.indices = indices.ToArray();

            void VisitSceneNode(Node* node)
            {
                for (int m = 0; m < node->MNumMeshes; m++)
                {
                    var mesh = scene->MMeshes[node->MMeshes[m]];
                    textureName = mesh->MName.AsString;

                    for (int f = 0; f < mesh->MNumFaces; f++)
                    {
                        var face = mesh->MFaces[f];

                        for (int i = 0; i < face.MNumIndices; i++)
                        {
                            uint index = face.MIndices[i];

                            var position = mesh->MVertices[index];
                            var texture = mesh->MTextureCoords[0][(int)index];

                            Vertex vertex = new Vertex
                            {
                                pos = new Vector3D<float>(position.X, position.Y, position.Z),
                                color = new Vector3D<float>(1, 1, 1),
                                //Flip Y for OBJ in Vulkan
                                texCoord = new Vector2D<float>(texture.X, 1.0f - texture.Y)
                            };

                            if (vertexMap.TryGetValue(vertex, out var meshIndex))
                            {
                                indices.Add(meshIndex);
                            }
                            else
                            {
                                indices.Add((uint)vertices.Count);
                                vertexMap[vertex] = (uint)vertices.Count;
                                vertices.Add(vertex);
                            }
                        }
                    }
                }

                for (int c = 0; c < node->MNumChildren; c++)
                {
                    VisitSceneNode(node->MChildren[c]);
                }
            }
        }

        private unsafe void CreateVertexBuffer()
        {
            ulong bufferSize = (ulong)GetArrayByteSize(vertices);

            CreateBuffer(bufferSize, BufferUsageFlags.BufferUsageTransferSrcBit,
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit |
                MemoryPropertyFlags.MemoryPropertyHostCoherentBit,
                out var stagingBuffer, out var stagingBufferMemory);

            // filling the vertex buffer
            void* data = null;
            vk.MapMemory(device, stagingBufferMemory, 0, bufferSize, 0, ref data);
            fixed(Vertex* ptr = vertices)
            {
                Unsafe.CopyBlock(data, (void*)ptr, (uint)bufferSize);
            }
            vk.UnmapMemory(device, stagingBufferMemory);

            CreateBuffer(bufferSize,
                BufferUsageFlags.BufferUsageTransferDstBit |
                BufferUsageFlags.BufferUsageVertexBufferBit,
                MemoryPropertyFlags.MemoryPropertyDeviceLocalBit,
                out vertexBuffer, out vertexBufferMemory);

            CopyBuffer(stagingBuffer, vertexBuffer, bufferSize);

            vk.DestroyBuffer(device, stagingBuffer, null);
            vk.FreeMemory(device, stagingBufferMemory, null);
        }

        private unsafe void CreateIndexBuffer()
        {
            ulong bufferSize = (ulong)GetArrayByteSize(indices);

            CreateBuffer(bufferSize, BufferUsageFlags.BufferUsageTransferSrcBit,
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit |
                MemoryPropertyFlags.MemoryPropertyHostCoherentBit,
                out var stagingBuffer,
                out var stagingBufferMemory);

            void* data = null;
            vk.MapMemory(device, stagingBufferMemory, 0, bufferSize, 0, ref data);
            fixed(void* ptr = indices)
            {
                Unsafe.CopyBlock(data, ptr, (uint)bufferSize);
            }
            vk.UnmapMemory(device, stagingBufferMemory);

            CreateBuffer(bufferSize,
                BufferUsageFlags.BufferUsageTransferDstBit |
                BufferUsageFlags.BufferUsageIndexBufferBit,
                MemoryPropertyFlags.MemoryPropertyDeviceLocalBit,
                out indexBuffer, out indexBufferMemory);
            CopyBuffer(stagingBuffer, indexBuffer, bufferSize);

            vk.DestroyBuffer(device, stagingBuffer, null);
            vk.FreeMemory(device, stagingBufferMemory, null);
        }

        private unsafe void CreateUniformBuffers()
        {
            ulong bufferSize = (ulong) Unsafe.SizeOf<UniformBufferObject>();
            uniformBuffers = new Buffer[MaxFrameInFlight];
            uniformBuffersMemory = new DeviceMemory[MaxFrameInFlight];
            for(int i = 0; i < MaxFrameInFlight; i++)
            {
                CreateBuffer(bufferSize, BufferUsageFlags.BufferUsageUniformBufferBit,
                    MemoryPropertyFlags.MemoryPropertyHostVisibleBit |
                    MemoryPropertyFlags.MemoryPropertyHostCoherentBit,
                    out uniformBuffers[i], out uniformBuffersMemory[i]);
            }
        }

        private unsafe void CreateDescriptorPool()
        {
            var poolSize = stackalloc DescriptorPoolSize[2];
            poolSize[0] = new DescriptorPoolSize(DescriptorType.UniformBuffer, MaxFrameInFlight);
            poolSize[1] = new DescriptorPoolSize(DescriptorType.CombinedImageSampler, MaxFrameInFlight);
            var poolInfo = new DescriptorPoolCreateInfo(StructureType.DescriptorPoolCreateInfo);
            poolInfo.PoolSizeCount = 2;
            poolInfo.PPoolSizes = poolSize;
            poolInfo.MaxSets = MaxFrameInFlight;

            if(vk.CreateDescriptorPool(device, poolInfo, null, out descriptorPool) != Result.Success)
            {
                throw new Exception("failed to create descriptor pool.");
            }
        }

        private unsafe void CreateDescriptorSets()
        {
            var layouts = new DescriptorSetLayout[MaxFrameInFlight];
            for(var i = 0; i < MaxFrameInFlight; i++)
            {
                layouts[i] = descriptorSetLayout;
            }
            var allocInfo = new DescriptorSetAllocateInfo(StructureType.DescriptorSetAllocateInfo);
            allocInfo.DescriptorPool = descriptorPool;
            allocInfo.DescriptorSetCount = MaxFrameInFlight;
            fixed(DescriptorSetLayout* ptr = layouts)
            {
                allocInfo.PSetLayouts = ptr;
                descriptorSets = new DescriptorSet[MaxFrameInFlight];
                if (vk.AllocateDescriptorSets(device, &allocInfo, descriptorSets) != Result.Success)
                {
                    throw new Exception("failed to allocate descriptor sets.");
                }
            }

            var descriptorWrites = stackalloc WriteDescriptorSet[2];
            descriptorWrites[0].SType = StructureType.WriteDescriptorSet;
            descriptorWrites[1].SType = StructureType.WriteDescriptorSet;
            //descriptorWrites[0] = new WriteDescriptorSet(StructureType.WriteDescriptorSet);
            //descriptorWrites[1] = new WriteDescriptorSet(StructureType.WriteDescriptorSet);
            for (var i = 0; i < MaxFrameInFlight; i++)
            {
                var bufferInfo = new DescriptorBufferInfo();
                bufferInfo.Buffer = uniformBuffers[i];
                bufferInfo.Offset = 0;
                bufferInfo.Range = (ulong)Unsafe.SizeOf<UniformBufferObject>();

                var imageInfo = new DescriptorImageInfo();
                imageInfo.ImageLayout = ImageLayout.ShaderReadOnlyOptimal;
                imageInfo.ImageView = textureImageView;
                imageInfo.Sampler = textureSampler;

                descriptorWrites[0].DstSet = descriptorSets[i];
                descriptorWrites[0].DstBinding = 0;
                descriptorWrites[0].DstArrayElement = 0;
                descriptorWrites[0].DescriptorType = DescriptorType.UniformBuffer;
                descriptorWrites[0].DescriptorCount = 1;
                descriptorWrites[0].PBufferInfo = &bufferInfo;

                descriptorWrites[1].DstSet = descriptorSets[i];
                descriptorWrites[1].DstBinding = 1;
                descriptorWrites[1].DstArrayElement = 0;
                descriptorWrites[1].DescriptorType = DescriptorType.CombinedImageSampler;
                descriptorWrites[1].DescriptorCount = 1;
                descriptorWrites[1].PImageInfo = &imageInfo;

                vk.UpdateDescriptorSets(device, 2, descriptorWrites, 0, null);
            }
        }

        private unsafe void BeginSingleTimeCommands(out CommandBuffer commandBuffer)
        {
            var allocInfo = new CommandBufferAllocateInfo(StructureType.CommandBufferAllocateInfo);
            allocInfo.Level = CommandBufferLevel.Primary;
            allocInfo.CommandPool = commandPool;
            allocInfo.CommandBufferCount = 1;

            commandBuffer = new CommandBuffer();
            if (vk.AllocateCommandBuffers(device, allocInfo, out commandBuffer) != Result.Success)
            {
                throw new Exception("failed to create \"CopoyBuffer\"'s CommandBuffer.");
            }

            var beginInfo = new CommandBufferBeginInfo(StructureType.CommandBufferBeginInfo);
            beginInfo.Flags = CommandBufferUsageFlags.CommandBufferUsageOneTimeSubmitBit;
            vk.BeginCommandBuffer(commandBuffer, beginInfo);
        }

        private unsafe void EndSingleTimeCommands(CommandBuffer commandBuffer)
        {
            vk.EndCommandBuffer(commandBuffer);

            var submitInfo = new SubmitInfo(StructureType.SubmitInfo);
            submitInfo.CommandBufferCount = 1;
            submitInfo.PCommandBuffers = &commandBuffer;
            vk.QueueSubmit(graphicsQueue, 1, submitInfo, default);
            vk.QueueWaitIdle(graphicsQueue);

            vk.FreeCommandBuffers(device, commandPool, 1, commandBuffer);
        }

        private unsafe void CopyBuffer(Buffer srcBuffer, Buffer dstBuffer, ulong size)
        {
            BeginSingleTimeCommands(out var commandBuffer);
            var copyRegion = new BufferCopy(0, 0, size);
            vk.CmdCopyBuffer(commandBuffer, srcBuffer, dstBuffer, 1, copyRegion);
            EndSingleTimeCommands(commandBuffer);
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

        private unsafe ShaderModule CreateShaderModule(byte[] code)
        {
            var createInfo = new ShaderModuleCreateInfo(StructureType.ShaderModuleCreateInfo);
            createInfo.CodeSize = (nuint) code.Length;
            fixed (byte* ptr = code)
            {
                createInfo.PCode = (uint*)ptr;
            }
            var shaderModule = new ShaderModule();
            if(vk.CreateShaderModule(device, in createInfo, null, out shaderModule) != Result.Success)
            {
                throw new Exception("failed to create shader module.");
            }
            return shaderModule;
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

                var vertexBuffers = new Buffer[]
                {
                    vertexBuffer,
                };
                //var offsets = 
                vk.CmdBindVertexBuffers(commandBuffers[i], 0, 1, vertexBuffer, 0);
                vk.CmdBindIndexBuffer(commandBuffers[i], indexBuffer, 0, IndexType.Uint32);
                vk.CmdBindDescriptorSets(commandBuffers[i], PipelineBindPoint.Graphics, pipelineLayout, 0, 1, descriptorSets[i], 0, null);

                vk.CmdDrawIndexed(commandBuffers[i], (uint)indices.Length, 1, 0, 0, 0);
                //vk.CmdDraw(commandBuffers[i], (uint)vertices.Length, 1, 0, 0);

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
            fenceInfo.Flags = FenceCreateFlags.FenceCreateSignaledBit;
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

        private unsafe void CreateDescriptorSetLayout()
        {
            var uboLayoutBinding = new DescriptorSetLayoutBinding();
            uboLayoutBinding.Binding = 0;
            uboLayoutBinding.DescriptorType = DescriptorType.UniformBuffer;
            uboLayoutBinding.DescriptorCount = 1;
            uboLayoutBinding.StageFlags = ShaderStageFlags.ShaderStageVertexBit;
            uboLayoutBinding.PImmutableSamplers = null;

            var samplerLayoutBinding = new DescriptorSetLayoutBinding();
            samplerLayoutBinding.Binding = 1;
            samplerLayoutBinding.DescriptorCount = 1;
            samplerLayoutBinding.DescriptorType = DescriptorType.CombinedImageSampler;
            samplerLayoutBinding.PImmutableSamplers = null;
            samplerLayoutBinding.StageFlags = ShaderStageFlags.ShaderStageFragmentBit;

            var bindings = stackalloc DescriptorSetLayoutBinding[2];
            bindings[0] = uboLayoutBinding;
            bindings[1] = samplerLayoutBinding;

            var layoutInfo = new DescriptorSetLayoutCreateInfo(StructureType.DescriptorSetLayoutCreateInfo);
            layoutInfo.BindingCount = 2;
            layoutInfo.PBindings = bindings;
            if(vk.CreateDescriptorSetLayout(device, layoutInfo, null, out descriptorSetLayout) != Result.Success)
            {
                throw new Exception("failed to create descriptor set layout.");
            }
        }

        public unsafe void Cleanup()
        {
            for(var i = 0; i < MaxFrameInFlight; i++)
            {
                vk.DestroySemaphore(device, imageAvailableSemaphores[i], null);
                vk.DestroySemaphore(device, renderFinishedSemaphores[i], null);
                vk.DestroyFence(device, inFlightFences[i], null);

                vk.DestroyBuffer(device, uniformBuffers[i], null);
                vk.FreeMemory(device, uniformBuffersMemory[i], null);
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
            vk.DestroyImageView(device, textureImageView, null);
            vk.DestroyImage(device, textureImage, null);
            vk.FreeMemory(device, textureImageMemory, null);
            vk.DestroyBuffer(device, vertexBuffer, null);
            vk.FreeMemory(device, vertexBufferMemory, null);
            vk.DestroyBuffer(device, indexBuffer, null);
            vk.FreeMemory(device, indexBufferMemory, null);
            vk.DestroyDescriptorPool(device, descriptorPool, null);
            vk.DestroyDescriptorSetLayout(device, descriptorSetLayout, null);
            vk.DestroyPipeline(device, graphicsPipeline, null);
            vk.DestroyPipelineLayout(device, pipelineLayout, null);
            vk.DestroyRenderPass(device, renderPass, null);
            foreach(var imageView in swapchainImageViews)
            {
                vk.DestroyImageView(device, imageView, null);
            }
            khrSwapchain.DestroySwapchain(device, swapchain, null);
            vk.DestroyDevice(device, null);
            if(EnableValidationLayers)
            {
                debugUtils.DestroyDebugUtilsMessenger(instance, debugMessager, null);
            }
            khrSurface.DestroySurface(instance, surface, null);
            vk.DestroyInstance(instance, null);
            window.Close();
            window.Dispose();
        }

        internal static byte[] LoadEmbeddedResourceBytes(string path)
        {
            using (var s = typeof(VulkanWrapper).Assembly.GetManifestResourceStream(path))
            {
                using (var ms = new MemoryStream())
                {
                    s.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
