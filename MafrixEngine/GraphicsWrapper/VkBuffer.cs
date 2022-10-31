using System;
using System.Collections;
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
using Queue = Silk.NET.Vulkan.Queue;

namespace MafrixEngine.GraphicsWrapper
{
    public class VkBuffer : IDisposable
    {
        private VkContext vkContext;
        public ulong bufferSize;
        public Buffer buffer;
        public DeviceMemory memory;
        private bool isFast;

        public VkBuffer(VkContext ctx, bool fastMode = false)
        {
            vkContext = ctx;
            bufferSize = 0;
            isFast = fastMode;
        }

        public VkBuffer(VkContext ctx, ulong bufSize, BufferUsageFlags flag, bool fastMode = false)
        {
            vkContext = ctx;
            bufferSize = bufSize;
            isFast = fastMode;
            if (isFast)
            {
                CreateBuffer(bufferSize, flag, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
                    out buffer, out memory);
            }
            else
            {
                CreateBuffer(bufferSize, flag, MemoryPropertyFlags.DeviceLocalBit,
                    out buffer, out memory);
            }
        }

        public unsafe void Init(ulong bufSize, BufferUsageFlags flag)
        {
            Debug.Assert(bufferSize == 0);
            bufferSize = bufSize;
            if(isFast)
            {
                CreateBuffer(bufferSize, flag, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
                    out buffer, out memory);
            }
            else
            {
                CreateBuffer(bufferSize, flag, MemoryPropertyFlags.DeviceLocalBit,
                    out buffer, out memory);
            }
            
        }

        public unsafe void Init<T>(T[] data, BufferUsageFlags flag) where T : unmanaged
        {
            Debug.Assert(bufferSize == 0);
            bufferSize = (ulong)(sizeof(T) * data.Length);
            if (isFast)
            {
                CreateBuffer(bufferSize, flag, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
                    out buffer, out memory);
            }
            else
            {
                CreateBuffer(bufferSize, flag, MemoryPropertyFlags.DeviceLocalBit,
                    out buffer, out memory);
            }
        }

        public unsafe void UpdateData<T>(T[] data, CommandPool pool, Queue queue, StagingBuffer stage) where T : unmanaged
        {
            Debug.Assert(bufferSize > 0);
            Debug.Assert((ulong)(sizeof(T) * data.Length) <= bufferSize);

            if(isFast)
            {
                void* dstPtr = null;
                fixed (void* srcPtr = data)
                {
                    vk.MapMemory(device, memory, 0, (uint)bufferSize, 0, ref dstPtr);
                    Unsafe.CopyBlock(dstPtr, srcPtr, (uint)(uint)bufferSize);
                    vk.UnmapMemory(device, memory);
                }
                
            }
            else
            {
                var stCommand = new SingleTimeCommand(vk, device, pool, queue);
                fixed (void* ptr = data)
                {
                    stage.CopyDataToBuffer(stCommand, buffer, ptr, (uint)bufferSize);
                }
            }
        }

        public unsafe void UpdateData<T>(T data, SingleTimeCommand stCommand, StagingBuffer stage) where T : unmanaged
        {
            Debug.Assert(bufferSize > 0);
            Debug.Assert((ulong)sizeof(T) <= bufferSize);

            if (isFast)
            {
                void* dstPtr = null;
                vk.MapMemory(device, memory, 0, (uint)bufferSize, 0, ref dstPtr);
                Unsafe.CopyBlock(dstPtr, &data, (uint)(uint)bufferSize);
                vk.UnmapMemory(device, memory);
            }
            else
            {
                stage.CopyDataToBuffer(stCommand, buffer, &data, (uint)bufferSize);
            }
        }

        public unsafe void UpdateData<T>(T[] data, SingleTimeCommand stCommand, StagingBuffer stage) where T : unmanaged
        {
            Debug.Assert(bufferSize > 0);
            Debug.Assert((ulong)(sizeof(T) * data.Length) <= bufferSize);

            if (isFast)
            {
                void* dstPtr = null;
                fixed (void* srcPtr = data)
                {
                    vk.MapMemory(device, memory, 0, (uint)bufferSize, 0, ref dstPtr);
                    Unsafe.CopyBlock(dstPtr, srcPtr, (uint)(uint)bufferSize);
                    vk.UnmapMemory(device, memory);
                }
            }
            else
            {
                fixed (void* ptr = data)
                {
                    stage.CopyDataToBuffer(stCommand, buffer, ptr, (uint)bufferSize);
                }
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
