using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Assimp;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Buffer = Silk.NET.Vulkan.Buffer;
using SixLabors.ImageSharp;

namespace MafrixEngine.GraphicsWrapper
{
    public unsafe class VkDescriptorPool : IDisposable
    {
        private Vk vk;
        private DescriptorPool descriptorPool;
        private int frameCount;
        public VkDescriptorPool(Vk vk, int frameCount)
        {
            this.vk = vk;
            this.frameCount = frameCount;
        }



        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public unsafe class VkDescriptorSets : IDisposable
    {
        private Vk vk;
        private DescriptorPool descriptorPool;
        public VkDescriptorSets(Vk vk, int frameCount)
        {
            this.vk = vk;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public unsafe class VkDescriptorWriter
    {
        private VkContext context;
        private List<WriteDescriptorSet> writeDescriptors;
        private DescriptorImageInfo[] imageInfos;
        private DescriptorBufferInfo[] bufferInfos;
        private int imgIdx;
        private int bufIdx;
        public VkDescriptorWriter(VkContext context, uint bufCount, uint imgCount)
        {
            this.context = context;
            writeDescriptors = new List<WriteDescriptorSet>();
            this.imageInfos = new DescriptorImageInfo[imgCount];
            this.bufferInfos = new DescriptorBufferInfo[bufCount];
            imgIdx = 0;
            bufIdx = 0;
        }

        //public VkDescriptorWriter WriteBuffer(uint binding, DescriptorBufferInfo bufferInfo, DescriptorSet descriptorSet)
        public VkDescriptorWriter WriteBuffer(uint binding, DescriptorBufferInfo bufferInfo, DescriptorType bufferType = DescriptorType.UniformBuffer)
        {
            var write = new WriteDescriptorSet(StructureType.WriteDescriptorSet);
            write.DescriptorCount = 1;
            write.DescriptorType = bufferType;
            write.DstBinding = binding;
            //write.DstSet = descriptorSet;
            write.PBufferInfo = &bufferInfo;
            writeDescriptors.Add(write);
            return this;
        }

        //public VkDescriptorWriter WriteBuffer(uint binding, DescriptorBufferInfo[] bufferInfos, DescriptorSet descriptorSet)
        public VkDescriptorWriter WriteBuffer(uint binding, DescriptorBufferInfo[] bufferInfos)
        {
            var write = new WriteDescriptorSet(StructureType.WriteDescriptorSet);
            write.DescriptorCount = (uint)bufferInfos.Length;
            write.DescriptorType = DescriptorType.UniformBuffer;
            write.DstBinding = binding;
            //write.DstSet = descriptorSet;
            fixed (DescriptorBufferInfo* ptr = bufferInfos)
            {
                write.PBufferInfo = ptr;
            }
            writeDescriptors.Add(write);
            return this;
        }

        //public VkDescriptorWriter WriteImage(uint binding, DescriptorImageInfo imageInfo, DescriptorSet descriptorSet)
        public VkDescriptorWriter WriteImage(uint binding, DescriptorImageInfo imageInfo)
        {
            var write = new WriteDescriptorSet(StructureType.WriteDescriptorSet);
            write.DescriptorCount = 1;
            write.DescriptorType = DescriptorType.CombinedImageSampler;
            write.DstBinding = binding;
            //write.DstSet = descriptorSet;
            write.PImageInfo = &imageInfo;
            writeDescriptors.Add(write);
            return this;
        }

        //public VkDescriptorWriter WriteImage(uint binding, DescriptorImageInfo[] imageInfos, DescriptorSet descriptorSet)
        public VkDescriptorWriter WriteImage(uint binding, DescriptorImageInfo[] imageInfos)
        {
            var write = new WriteDescriptorSet(StructureType.WriteDescriptorSet);
            write.DescriptorCount = (uint)imageInfos.Length;
            write.DescriptorType = DescriptorType.CombinedImageSampler;
            write.DstBinding = binding;
            //write.DstSet = descriptorSet;
            fixed (DescriptorImageInfo* ptr = imageInfos)
            {
                write.PImageInfo = ptr;
            }
            writeDescriptors.Add(write);
            return this;
        }

        public void Write(DescriptorSet descriptorSet)
        {
            var writes = writeDescriptors.ToArray();
            for (var i = 0; i < writes.Length; i++)
            {
                writes[i].DstSet = descriptorSet;
            }
            fixed(WriteDescriptorSet* ptr = writes)
            {
                context.vk.UpdateDescriptorSets(
                    context.device,
                    (uint)writes.Length, ptr,
                    0, null);
            }
        }
    }
}
