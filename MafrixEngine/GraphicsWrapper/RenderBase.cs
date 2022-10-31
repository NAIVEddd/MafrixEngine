using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace MafrixEngine.GraphicsWrapper
{
    public unsafe class RenderBase : IDisposable
    {
        public VkContext vkContext { get; private set; }
        public Queue graphicsQueue;
        public IWindow window;
        public KhrSurface khrSurface;
        public SurfaceKHR surface;
        public VkSwapchain vkSwapchain;
        public CommandPool commandPool;
        public CommandBuffer[] commandBuffers;
        public Semaphore[] imageAvailableSemaphores;
        public Semaphore[] renderFinishedSemaphores;
        public Fence[] inFlightFences;
        public Fence[] imagesInFlight;
        private ExtDebugUtils debugUtils;
        private DebugUtilsMessengerEXT debugMessager;
        public StagingBuffer Staging { get; set; }
        public SingleTimeCommand SingleCommand { get; set; }

        public RenderBase()
        {
            vkContext = new VkContext();
        }

        public void Initialize(string renderName = "RenderBase", uint versionMajor = 0, uint versionMinor = 0)
        {
            Debug.Assert(window != null);
            vkContext.Initialize(renderName, new Version32(versionMajor, versionMinor, 1));
            vkContext.vk.TryGetInstanceExtension(vkContext.instance, out khrSurface);
            vkContext.vk.GetDeviceQueue(vkContext.device, 0, 0, out graphicsQueue);
            surface = window.VkSurface!.Create<AllocationCallbacks>(vkContext.instance.ToHandle(), null).ToSurface();
            SetupDebugMessager();
            Staging = new StagingBuffer(vkContext);
            CreateSwapChain();
            CreateCommandPool();
            CreateCommandBuffers();
            CreateSyncObjects();
            SingleCommand = new SingleTimeCommand(vkContext.vk, vkContext.device, commandPool, graphicsQueue);
        }

        private unsafe void SetupDebugMessager()
        {
            if (!vkContext.vk.TryGetInstanceExtension(vkContext.instance, out debugUtils)) return;

            var createInfo = new DebugUtilsMessengerCreateInfoEXT(StructureType.DebugUtilsMessengerCreateInfoExt);
            createInfo.MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt |
                                         DebugUtilsMessageSeverityFlagsEXT.WarningBitExt |
                                         DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt;
            createInfo.MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt |
                                     DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt |
                                     DebugUtilsMessageTypeFlagsEXT.ValidationBitExt;
            createInfo.PfnUserCallback = (DebugUtilsMessengerCallbackFunctionEXT)DebugCallback;

            if (debugUtils.CreateDebugUtilsMessenger(vkContext.instance, in createInfo, null, out debugMessager) != Result.Success)
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
            if (messageSeverity > DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt)
            {
                Console.WriteLine($"{messageSeverity} {messageTypes}" + Marshal.PtrToStringAnsi((nint)pCallbackData->PMessage));
            }
            return Vk.False;
        }

        private unsafe bool CreateSwapChain()
        {
            vkSwapchain = new VkSwapchain(vkContext, khrSurface, surface, window);
            vkSwapchain.Create();

            return true;
        }
        private unsafe void CreateCommandPool()
        {
            var queueFamilyIndices = FindQueueFamilies(vkContext.physicalDevice);

            var poolInfo = new CommandPoolCreateInfo
            {
                SType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = queueFamilyIndices.GraphicsFamily!.Value
            };

            if (vkContext.vk.CreateCommandPool(vkContext.device, poolInfo, null, out commandPool) != Result.Success)
            {
                throw new Exception("failed to create command pool!");
            }
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
            vkContext.vk.GetPhysicalDeviceQueueFamilyProperties(device, &queryFamilyCount, null);

            using var mem = GlobalMemory.Allocate((int)queryFamilyCount * sizeof(QueueFamilyProperties));
            var queueFamilies = (QueueFamilyProperties*)Unsafe.AsPointer(ref mem.GetPinnableReference());

            vkContext.vk.GetPhysicalDeviceQueueFamilyProperties(device, &queryFamilyCount, queueFamilies);
            for (var i = 0u; i < queryFamilyCount; i++)
            {
                var queueFamily = queueFamilies[i];
                if (queueFamily.QueueFlags.HasFlag(QueueFlags.GraphicsBit))
                {
                    indices.GraphicsFamily = i;
                }

                khrSurface.GetPhysicalDeviceSurfaceSupport(device, i, surface, out var presentSupport);
                if (presentSupport == Vk.True)
                {
                    indices.PresentFamily = i;
                }

                if (indices.IsComplete())
                {
                    break;
                }
            }
            return indices;
        }
        private unsafe void CreateCommandBuffers()
        {
            commandBuffers = new CommandBuffer[vkSwapchain.ImageCount];

            var allocInfo = new CommandBufferAllocateInfo
            {
                SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = commandPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = (uint)commandBuffers.Length
            };

            if (vkContext.vk.AllocateCommandBuffers(vkContext.device, &allocInfo, commandBuffers) != Result.Success)
            {
                throw new Exception("failed to allocate command buffers!");
            }
        }
        private unsafe void CreateSyncObjects()
        {
            var MaxFrameInFlight = vkSwapchain.ImageCount;
            imageAvailableSemaphores = new Semaphore[MaxFrameInFlight];
            renderFinishedSemaphores = new Semaphore[MaxFrameInFlight];
            inFlightFences = new Fence[MaxFrameInFlight];
            imagesInFlight = new Fence[MaxFrameInFlight];

            var semaphoreInfo = new SemaphoreCreateInfo(StructureType.SemaphoreCreateInfo);

            var fenceInfo = new FenceCreateInfo(StructureType.FenceCreateInfo);
            fenceInfo.Flags = FenceCreateFlags.SignaledBit;
            for (var i = 0; i < MaxFrameInFlight; i++)
            {
                Semaphore imgAvSema, renderFinSema;
                Fence inFlightFence;
                if (vkContext.vk.CreateSemaphore(vkContext.device, semaphoreInfo, null, out imgAvSema) != Result.Success ||
                   vkContext.vk.CreateSemaphore(vkContext.device, semaphoreInfo, null, out renderFinSema) != Result.Success ||
                   vkContext.vk.CreateFence(vkContext.device, fenceInfo, null, out inFlightFence) != Result.Success)
                {
                    throw new Exception("failed to create synchonization objects for a frame!");
                }
                imageAvailableSemaphores[i] = imgAvSema;
                renderFinishedSemaphores[i] = renderFinSema;
                inFlightFences[i] = inFlightFence;
            }
        }
        public void Dispose()
        {
            for (var i = 0; i < vkSwapchain.ImageCount; i++)
            {
                vkContext.vk.DestroySemaphore(vkContext.device, imageAvailableSemaphores[i], null);
                vkContext.vk.DestroySemaphore(vkContext.device, renderFinishedSemaphores[i], null);
                vkContext.vk.DestroyFence(vkContext.device, inFlightFences[i], null);
            }
            Staging.Dispose();
            vkContext.vk.DestroyCommandPool(vkContext.device, commandPool, null);
            debugUtils.DestroyDebugUtilsMessenger(vkContext.instance, debugMessager, null);
            vkSwapchain.Dispose();
            khrSurface.DestroySurface(vkContext.instance, surface, null);
            vkContext.Dispose();
        }
    }
}
