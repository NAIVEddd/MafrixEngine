using System;
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
using Buffer = Silk.NET.Vulkan.Buffer;

namespace MafrixEngine.GraphicsWrapper
{
    public class StagingBuffer : IDisposable
    {
        public static uint DefaultBufferSize = 1024 * 1024 * 5; // 5MB
        private Vk vk;
        private PhysicalDevice physicalDevice;
        private Device device;
        private Buffer buffer;
        private DeviceMemory deviceMemory;
        private uint bufferSize;

        public StagingBuffer(Vk vk, PhysicalDevice physicalDevice, Device device)
        {
            this.vk = vk;
            this.physicalDevice = physicalDevice;
            this.device = device;
            CreateStagtBuffer(DefaultBufferSize);
            bufferSize = DefaultBufferSize;
        }

        public StagingBuffer(Vk vk, PhysicalDevice physicalDevice, Device device, uint size)
        {
            this.vk = vk;
            this.physicalDevice = physicalDevice;
            this.device = device;
            CreateStagtBuffer(size);
            bufferSize = size;
        }

        public unsafe void CopyDataToBuffer(SingleTimeCommand command, Buffer destination, void* srcData, uint length)
        {
            if(bufferSize < length)
            {
                bufferSize = length;
                CreateStagtBuffer(length);
            }

            // copy data to staging buffer
            void* address = null;
            vk.MapMemory(device, deviceMemory, 0, length, 0, ref address);
            Unsafe.CopyBlock(address, srcData, length);
            vk.UnmapMemory(device, deviceMemory);

            // copy data from staging to destination buffer.
            command.BeginSingleTimeCommands(out var commandBuffer);
            var copyRegion = new BufferCopy(0, 0, length);
            vk.CmdCopyBuffer(commandBuffer, buffer, destination, 1, copyRegion);
            command.EndSingleTimeCommands(commandBuffer);
        }

        public unsafe void CopyDataToImage(SingleTimeCommand command, Image destination, uint width, uint height, Span<byte> srcData, uint length)
        {
            if(bufferSize < length)
            {
                CreateStagtBuffer(length);
            }

            // copy data to staging buffer
            void* address = null;
            vk.MapMemory(device, deviceMemory, 0, length, 0, ref address);
            var dest = new Span<byte>(address, (int)length);
            srcData.CopyTo(dest);
            vk.UnmapMemory(device, deviceMemory);

            var region = new BufferImageCopy();
            region.BufferOffset = 0;
            region.BufferRowLength = 0;
            region.BufferImageHeight = 0;
            region.ImageSubresource.AspectMask = ImageAspectFlags.ColorBit;
            region.ImageSubresource.MipLevel = 0;
            region.ImageSubresource.BaseArrayLayer = 0;
            region.ImageSubresource.LayerCount = 1;
            region.ImageOffset = new Offset3D(0, 0, 0);
            region.ImageExtent = new Extent3D(width, height, 1);

            command.BeginSingleTimeCommands(out var commandBuffer);
            vk.CmdCopyBufferToImage(commandBuffer, buffer, destination,
                ImageLayout.TransferDstOptimal, 1, region);
            command.EndSingleTimeCommands(commandBuffer);
        }

        public void Dispose()
        {
            FreeResource();
        }

        private unsafe void FreeResource()
        {
            vk.DestroyBuffer(device, buffer, null);
            vk.FreeMemory(device, deviceMemory, null);
        }

        private unsafe void CreateStagtBuffer(uint size)
        {
            if(buffer.Handle != 0)
            {
                FreeResource();
            }
            var createInfo = new BufferCreateInfo(StructureType.BufferCreateInfo);
            createInfo.Size = size;
            createInfo.Usage = BufferUsageFlags.TransferDstBit |
                BufferUsageFlags.TransferSrcBit;
            createInfo.SharingMode = SharingMode.Exclusive;
            if(vk.CreateBuffer(device, createInfo, null, out buffer) != Result.Success)
            {
                throw new Exception("StagingBuffer: failed to create buffer.");
            }

            vk.GetBufferMemoryRequirements(device, buffer, out var memoryRequirements);

            var memAllocInfo = new MemoryAllocateInfo(StructureType.MemoryAllocateInfo);
            memAllocInfo.AllocationSize = memoryRequirements.Size;
            memAllocInfo.MemoryTypeIndex =
                FindMemoryType(memoryRequirements.MemoryTypeBits,
                    MemoryPropertyFlags.HostVisibleBit |
                    MemoryPropertyFlags.HostCoherentBit);
            vk.AllocateMemory(device, memAllocInfo, null, out deviceMemory);

            vk.BindBufferMemory(device, buffer, deviceMemory, 0);
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
    }
}
