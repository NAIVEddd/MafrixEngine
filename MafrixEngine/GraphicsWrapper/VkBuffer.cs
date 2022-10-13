using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace MafrixEngine.GraphicsWrapper
{
    public class VkBuffer : IDisposable
    {
        private VkContext vkContext;
        public ulong bufferSize;
        public Buffer buffer;
        public DeviceMemory memory;

        public VkBuffer(VkContext ctx)
        {
            vkContext = ctx;
            bufferSize = 0;
        }

        public VkBuffer(VkContext ctx, ulong bufSize, BufferUsageFlags flag)
        {
            vkContext = ctx;
            bufferSize = bufSize;
            CreateBuffer(bufSize, flag, MemoryPropertyFlags.DeviceLocalBit,
                out buffer, out memory);
        }

        public unsafe void Init(ulong bufSize, BufferUsageFlags flag)
        {
            Debug.Assert(bufferSize == 0);
            bufferSize = bufSize;
            CreateBuffer(bufferSize, flag, MemoryPropertyFlags.DeviceLocalBit,
                out buffer, out memory);
        }

        public unsafe void Init<T>(T[] data, BufferUsageFlags flag) where T : unmanaged
        {
            Debug.Assert(bufferSize == 0);
            bufferSize = (ulong)(sizeof(T) * data.Length);
            CreateBuffer(bufferSize, flag, MemoryPropertyFlags.DeviceLocalBit,
                out buffer, out memory);
        }

        public unsafe void UpdateData<T>(T[] data, CommandPool pool, Queue queue, StagingBuffer stage) where T : unmanaged
        {
            Debug.Assert(bufferSize > 0);
            Debug.Assert((ulong)(sizeof(T) * data.Length) <= bufferSize);

            var stCommand = new SingleTimeCommand(vk, device, pool, queue);
            fixed (void* ptr = data)
            {
                stage.CopyDataToBuffer(stCommand, buffer, ptr, (uint)bufferSize);
            }
        }

        private Vk vk { get { return vkContext.vk; } }
        private Device device { get { return vkContext.device; } }
        private PhysicalDevice physicalDevice { get { return vkContext.physicalDevice; } }
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
            vk.FreeMemory(device, memory, null);
            vk.DestroyBuffer(device, buffer, null);
        }
    }
}
