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
using Image = SixLabors.ImageSharp.Image;
using VulkanImage = Silk.NET.Vulkan.Image;

namespace MafrixEngine.GraphicsWrapper
{
    public class VkSampler : IDisposable
    {
        public VkSampler()
        {
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class VkTexture : IDisposable
    {
        UInt32 mipLevels;
        private VkContext vkContext;
        private Vk vk { get => vkContext.vk; }
        private PhysicalDevice physicalDevice { get => vkContext.physicalDevice; }
        private Device device { get => vkContext.device; }
        private StagingBuffer staging;
        public VulkanImage image;
        public DeviceMemory imageMemory;
        public ImageView imageView;
        public VkTexture(VkContext vkContext, SingleTimeCommand stCommand, StagingBuffer staging,
            Image<Rgba32> texture)
        {
            this.vkContext = vkContext;
            this.staging = staging;
            CreateTextureImage(texture, stCommand, out image, out imageMemory);
            CreateImageView(image, mipLevels, Format.R8G8B8A8Srgb, ImageAspectFlags.ColorBit, out imageView);
        }

        private unsafe void CreateTextureImage(Image<Rgba32> texture, SingleTimeCommand stCommand, out VulkanImage image, out DeviceMemory deviceMemory)
        {
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
            TransitionImageLayout(stCommand, image, mipLevels, Format.R8G8B8A8Srgb,
            ImageLayout.Undefined, ImageLayout.TransferDstOptimal);

            staging.CopyDataToImage(stCommand, image, (uint)width, (uint)height, array.Span, (uint)imageSize);

            GenerateMipmaps(stCommand, image, Format.R8G8B8A8Srgb, (uint)width, (uint)height, mipLevels);
        }

        private unsafe void CreateImageView(VulkanImage image, uint mipLevels, Format format, ImageAspectFlags aspectFlags, out ImageView imageView)
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
            out VulkanImage image, out DeviceMemory imageMemory)
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

        private unsafe void TransitionImageLayout(SingleTimeCommand stCommand, VulkanImage image, uint mipLevels, Format format,
            ImageLayout oldLayout, ImageLayout newLayout)
        {
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
            if (newLayout == ImageLayout.DepthStencilAttachmentOptimal)
            {
                barrier.SubresourceRange.AspectMask = ImageAspectFlags.DepthBit;
                if (HasStencilComponent(format))
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
            }
            else if (oldLayout == ImageLayout.TransferDstOptimal && newLayout == ImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
                barrier.DstAccessMask = AccessFlags.ShaderReadBit;
                sourceStage = PipelineStageFlags.TransferBit;
                destinationStage = PipelineStageFlags.FragmentShaderBit;
            }
            else if (oldLayout == ImageLayout.Undefined && newLayout == ImageLayout.DepthStencilAttachmentOptimal)
            {
                barrier.SrcAccessMask = 0;
                barrier.DstAccessMask =
                    AccessFlags.DepthStencilAttachmentReadBit |
                    AccessFlags.DepthStencilAttachmentWriteBit;
                sourceStage = PipelineStageFlags.TopOfPipeBit;
                destinationStage = PipelineStageFlags.EarlyFragmentTestsBit;
            }
            else
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

        private unsafe void GenerateMipmaps(SingleTimeCommand stCommand, VulkanImage image, Format imageFormat, uint width, uint height, uint mipLevels)
        {
            vk.GetPhysicalDeviceFormatProperties(physicalDevice, imageFormat, out var formatProperties);
            if (0 == (formatProperties.OptimalTilingFeatures & FormatFeatureFlags.SampledImageFilterLinearBit))
            {
                throw new Exception("texture image format does not support linear blitting.");
            }
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
            for (var i = 1; i < mipLevels; i++)
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
                blit.DstOffsets[1].X = (int)(mipWidth > 1 ? mipWidth / 2 : 1);
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

        private bool HasStencilComponent(Format format)
        {
            return format == Format.D32SfloatS8Uint || format == Format.X8D24UnormPack32;
        }

        private unsafe UInt32 FindMemoryType(UInt32 typeFilter, MemoryPropertyFlags properties)
        {
            var memProperties = new PhysicalDeviceMemoryProperties();
            vk.GetPhysicalDeviceMemoryProperties(physicalDevice, out memProperties);
            for (UInt32 i = 0; i < memProperties.MemoryTypeCount; i++)
            {
                if ((typeFilter & (1 << (int)i)) != 0 &&
                    (memProperties.MemoryTypes[(int)i].PropertyFlags & properties) != 0)
                {
                    return i;
                }
            }

            throw new Exception("failed to find suitable memory type!");
        }

        public unsafe void Dispose()
        {
            vk.FreeMemory(device, imageMemory, null);
            vk.DestroyImageView(device, imageView, null);
            vk.DestroyImage(device, image, null);
        }
    }
}
