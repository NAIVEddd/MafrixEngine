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
    public class SingleTimeCommand
    {
        private Vk vk;
        private Device device;
        private CommandPool commandPool;
        private Queue graphicsQueue;

        public SingleTimeCommand(Vk vk, Device device, CommandPool commandPool, Queue graphicsQueue)
        {
            this.vk = vk;
            this.device = device;
            this.commandPool = commandPool;
            this.graphicsQueue = graphicsQueue;
        }

        public unsafe void BeginSingleTimeCommands(out CommandBuffer commandBuffer)
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
            beginInfo.Flags = CommandBufferUsageFlags.OneTimeSubmitBit;
            vk.BeginCommandBuffer(commandBuffer, beginInfo);
        }

        public unsafe void EndSingleTimeCommands(CommandBuffer commandBuffer)
        {
            vk.EndCommandBuffer(commandBuffer);

            var submitInfo = new SubmitInfo(StructureType.SubmitInfo);
            submitInfo.CommandBufferCount = 1;
            submitInfo.PCommandBuffers = &commandBuffer;
            vk.QueueSubmit(graphicsQueue, 1, submitInfo, default);
            vk.QueueWaitIdle(graphicsQueue);

            vk.FreeCommandBuffers(device, commandPool, 1, commandBuffer);
        }
    }
}
