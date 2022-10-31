using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.SDL;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace MafrixEngine.GraphicsWrapper
{
    public class VkSwapchain : IDisposable
    {
        private VkContext vkContext;
        public KhrSurface khrSurface;
        public SurfaceKHR surface;
        public KhrSwapchain khrSwapchain;
        public SwapchainKHR swapchain;
        public IWindow window;
        public Image[] images;
        public ImageView[] imageViews;
        public Format format;
        public Extent2D extent;
        public int ImageCount { get => imageViews.Length; }
        public VkSwapchain(VkContext vkContext, KhrSurface khrSurface, SurfaceKHR surface, IWindow window)
        {
            this.vkContext = vkContext;
            this.khrSurface = khrSurface;
            this.surface = surface;
            this.window = window;
            vkContext.vk.TryGetInstanceExtension(vkContext.instance, out khrSwapchain);

        }
        public Result AcquireNextImage(ulong timeout, Semaphore semaphore, Fence fence, out uint imageIndex)
        {
            uint index = 0;
            var res = khrSwapchain.AcquireNextImage(vkContext.device, swapchain, timeout, semaphore, fence, ref index);
            imageIndex = index;
            return res;
        }

        public unsafe void Create()
        {
            var swapChainSupport = QuerySwapChainSupport(vkContext.physicalDevice);
            var surfaceFormat = ChooseSwapSurfaceFormat(swapChainSupport.Formats);
            var presentMode = ChooseSwapPresentMode(swapChainSupport.PresentModes);
            var extent = ChooseSwapExtent(swapChainSupport.Capabilities);

            if (extent.Width == 0 || extent.Height == 0)
                return;

            var imageCount = swapChainSupport.Capabilities.MinImageCount + 1;
            if (swapChainSupport.Capabilities.MaxImageCount > 0 &&
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

            var indices = FindQueueFamilies(vkContext.physicalDevice);
            uint[] queueFamilyIndices = { indices.GraphicsFamily!.Value, indices.PresentFamily!.Value };
            fixed (uint* queueFamily = queueFamilyIndices)
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
                if (!vkContext.vk.TryGetDeviceExtension(vkContext.instance, vkContext.device, out khrSwapchain))
                {
                    throw new NotSupportedException("KHR_swapchain extension not found.");
                }
                if (khrSwapchain.CreateSwapchain(vkContext.device, createInfo, null, out swapchain) != Result.Success)
                {
                    throw new Exception("failed to create swapchain.");
                }
            }

            khrSwapchain.GetSwapchainImages(vkContext.device, swapchain, &imageCount, null);
            images = new Image[imageCount];
            khrSwapchain.GetSwapchainImages(vkContext.device, swapchain, &imageCount, images);

            format = surfaceFormat.Format;
            this.extent = extent;

            CreateImageViews();
        }
        private void CreateImageViews()
        {
            imageViews = new ImageView[images.Length];

            for (var i = 0; i < images.Length; i++)
            {
                CreateImageView(images[i], 1, format, ImageAspectFlags.ColorBit, out var imageView);

                imageViews[i] = imageView;
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
            if (vkContext.vk.CreateImageView(vkContext.device, viewInfo, null, out imageView) != Result.Success)
            {
                throw new Exception("failed to create texture image view");
            }
        }
        private struct SwapChainSupportDetails
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
            if (formatCount != 0)
            {
                details.Formats = new SurfaceFormatKHR[formatCount];
                using var mem = GlobalMemory.Allocate((int)formatCount * sizeof(SurfaceFormatKHR));
                var formats = (SurfaceFormatKHR*)Unsafe.AsPointer(ref mem.GetPinnableReference());

                khrSurface.GetPhysicalDeviceSurfaceFormats(device, surface, &formatCount, formats);
                for (var i = 0; i < formatCount; i++)
                {
                    details.Formats[i] = formats[i];
                }
            }

            var presentModeCount = 0u;
            khrSurface.GetPhysicalDeviceSurfacePresentModes(device, surface, &presentModeCount, null);
            if (presentModeCount != 0)
            {
                details.PresentModes = new PresentModeKHR[presentModeCount];
                using var mem = GlobalMemory.Allocate((int)presentModeCount * sizeof(PresentModeKHR));
                var modes = (PresentModeKHR*)Unsafe.AsPointer(ref mem.GetPinnableReference());

                khrSurface.GetPhysicalDeviceSurfacePresentModes(device, surface, &presentModeCount, modes);
                for (var i = 0; i < presentModeCount; i++)
                {
                    details.PresentModes[i] = modes[i];
                }
            }

            return details;
        }
        private SurfaceFormatKHR ChooseSwapSurfaceFormat(SurfaceFormatKHR[] formats)
        {
            foreach (var format in formats)
            {
                if (format.Format == Format.B8G8R8A8Unorm)
                {
                    return format;
                }
            }
            return formats[0];
        }
        private PresentModeKHR ChooseSwapPresentMode(PresentModeKHR[] presentModes)
        {
            foreach (var mode in presentModes)
            {
                if (mode == PresentModeKHR.MailboxKhr)
                {
                    return mode;
                }
            }
            return PresentModeKHR.FifoKhr;
        }
        private Extent2D ChooseSwapExtent(SurfaceCapabilitiesKHR capabilities)
        {
            if (capabilities.CurrentExtent.Width != uint.MaxValue)
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

        public unsafe void Dispose()
        {
            foreach(var view in imageViews)
            {
                vkContext.vk.DestroyImageView(vkContext.device, view, null);
            }
            khrSwapchain.DestroySwapchain(vkContext.device, swapchain, null);
        }
    }
}
