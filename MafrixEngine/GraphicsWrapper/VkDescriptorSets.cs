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
using System.Diagnostics;

namespace MafrixEngine.GraphicsWrapper
{
    public unsafe class VkDescriptorPool : IDisposable
    {
        private VkContext vkContext;
        public DescriptorPool[] pools;
        private VkDescriptorSets[] sets;
        public int frameCount = -1;
        public int setCount = -1;
        public int primitiveCount = -1;
        public VkDescriptorPool(VkContext vkCtx, int frameCount)
        {
            this.vkContext = vkCtx;
            this.frameCount = frameCount;
            pools = new DescriptorPool[frameCount];
            sets = new VkDescriptorSets[frameCount];
        }

        public void Initialize(DescriptorSetLayout[] setLayouts, DescriptorPoolSize[] poolSizes, int primitives)
        {
            this.setCount = setLayouts.Length;
            this.primitiveCount = primitives;

            var createInfo = new DescriptorPoolCreateInfo(StructureType.DescriptorPoolCreateInfo);
            createInfo.MaxSets = (uint)(setCount * primitives);
            createInfo.PoolSizeCount = (uint)poolSizes.Length;

            // per primitive have descriptor count
            for (int i = 0; i < poolSizes.Length; i++)
            {
                poolSizes[i].DescriptorCount = (uint)(poolSizes[i].DescriptorCount * primitives);
            }
            
            fixed (DescriptorPoolSize* pSizes = poolSizes)
            {
                createInfo.PPoolSizes = pSizes;

                for (int i = 0; i < frameCount; i++)
                {
                    if (vkContext.vk.CreateDescriptorPool(vkContext.device, in createInfo, null, out pools[i]) != Result.Success)
                    {
                        throw new Exception("failed to create descriptor pool.");
                    }
                }
            }

            // set count to origin state
            for (int i = 0; i < poolSizes.Length; i++)
            {
                poolSizes[i].DescriptorCount = (uint)(poolSizes[i].DescriptorCount / primitives);
            }

            // Do the rest work
            AllocateDescriptorSet(setLayouts);
        }

        public VkDescriptorSets GetDescriptorSet(int frameIndex)
        {
            return sets[frameCount];
        }

        public VkDescriptorWriter GetDescriptorWriter(int frameIndex, int primitiveIndex)
        {
            return sets[frameIndex].GetDescriptorWriter(primitiveIndex);
        }

        public void BindCommand(CommandBuffer cmdBuffer, PipelineLayout pipelineLayout,
            int frameIndex, int primitiveIndex, PipelineBindPoint bindPoint = PipelineBindPoint.Graphics)
        {
            sets[frameIndex].BindDescriptorSets(cmdBuffer, pipelineLayout, primitiveIndex, bindPoint);
        }

        private void AllocateDescriptorSet(DescriptorSetLayout[] setLayouts)
        {
            for (int i = 0; i < frameCount; i++)
            {
                sets[i] = new VkDescriptorSets(vkContext, this, i);
                sets[i].Initialize(setLayouts);
            }
        }


        public void Dispose()
        {
            foreach (var s in sets)
            {
                s.Dispose();
            }
            foreach (var p in pools)
            {
                vkContext.vk.DestroyDescriptorPool(vkContext.device, p, null);
            }
        }
    }

    public unsafe class VkDescriptorSets : IDisposable
    {
        private VkContext vkContext;
        private DescriptorPool descriptorPool;
        private int setCount;
        private int primitiveCount;
        public DescriptorSet[] descriptorSets;

        public VkDescriptorSets(VkContext vkCtx, VkDescriptorPool pool, int poolIndex)
        {
            this.vkContext = vkCtx;
            descriptorPool = pool.pools[poolIndex];
            setCount = pool.setCount;
            primitiveCount = pool.primitiveCount;
            descriptorSets = new DescriptorSet[primitiveCount * setCount];
        }

        public void Initialize(DescriptorSetLayout[] setLayouts)
        {
            var allocInfo = new DescriptorSetAllocateInfo(StructureType.DescriptorSetAllocateInfo);
            allocInfo.DescriptorPool = descriptorPool;
            allocInfo.DescriptorSetCount = (uint)setLayouts.Length;

            var tmpSets = new DescriptorSet[setLayouts.Length];
            var copyOffset = 0;
            fixed(DescriptorSetLayout* pLayout = setLayouts)
            {
                fixed(DescriptorSet* pSets = tmpSets)
                {
                    allocInfo.PSetLayouts = pLayout;
                    for (int i = 0; i < primitiveCount; i++)
                    {
                        if (vkContext.vk.AllocateDescriptorSets(vkContext.device, in allocInfo, pSets) != Result.Success)
                        {
                            throw new Exception("failed to allocate descriptor sets.");
                        }
                        tmpSets.CopyTo(descriptorSets, copyOffset);
                        copyOffset += tmpSets.Length;
                    }
                }
            }
        }

        public void BindDescriptorSets(CommandBuffer cmdBuffer, PipelineLayout pipelineLayout,
            int primitiveIndex, PipelineBindPoint bindPoint = PipelineBindPoint.Graphics)
        {
            var firstIndex = (uint)(primitiveIndex * setCount);
            fixed(DescriptorSet* pSet = &descriptorSets[firstIndex])
            {
                vkContext.vk.CmdBindDescriptorSets(cmdBuffer, bindPoint, pipelineLayout, 0, (uint)setCount, pSet, 0, null);
            }
        }

        public VkDescriptorWriter GetDescriptorWriter(int primitiveIndex)
        {
            var offset = primitiveIndex * setCount;
            return new VkDescriptorWriter(vkContext, descriptorSets, offset, setCount);
        }

        public void Dispose()
        {
            //vkContext.vk.FreeDescriptorSets(vkContext.device, descriptorPool, descriptorSets);
        }
    }

    public unsafe class VkDescriptorWriter
    {
        private VkContext vkContext;
        private List<WriteDescriptorSet> writeDescriptors;
        private int offset;
        private int setCount;
        private DescriptorSet[] descriptorSets;
        public VkDescriptorWriter(VkContext vkContext, DescriptorSet[] descriptorSets, int indexOffset, int setCount)
        {
            this.vkContext = vkContext;
            this.writeDescriptors = new List<WriteDescriptorSet>();
            this.descriptorSets = descriptorSets;
            this.offset = indexOffset;
            this.setCount = setCount;
        }
        
        public void WriteBuffer(uint set, uint binding,
            DescriptorBufferInfo bufferInfo, DescriptorType bufferType = DescriptorType.UniformBuffer)
        {
            Debug.Assert(set < setCount);

            var write = new WriteDescriptorSet(StructureType.WriteDescriptorSet);
            write.DescriptorCount = 1;
            write.DescriptorType = bufferType;
            write.DstBinding = binding;
            write.DstSet = descriptorSets[offset + set];
            write.PBufferInfo = &bufferInfo;
            writeDescriptors.Add(write);
        }

        public void WriteImage(uint set, uint binding,
            DescriptorImageInfo imageInfo, DescriptorType bufferType = DescriptorType.CombinedImageSampler)
        {
            Debug.Assert(set < setCount);

            var write = new WriteDescriptorSet(StructureType.WriteDescriptorSet);
            write.DescriptorCount = 1;
            write.DescriptorType = DescriptorType.CombinedImageSampler;
            write.DstBinding = binding;
            write.DstSet = descriptorSets[offset + set];
            write.PImageInfo = &imageInfo;
            writeDescriptors.Add(write);
        }

        public void Write()
        {
            var writes = writeDescriptors.ToArray();
            fixed (WriteDescriptorSet* ptr = writes)
            {
                vkContext.vk.UpdateDescriptorSets(
                    vkContext.device, (uint)writes.Length, ptr,
                    0, null);
            }
        }
    }

    public unsafe class VkOldDescriptorWriter
    {
        private VkContext context;
        private List<WriteDescriptorSet> writeDescriptors;
        //private DescriptorImageInfo[] imageInfos;
        //private DescriptorBufferInfo[] bufferInfos;
        //private int imgIdx;
        //private int bufIdx;
        public VkOldDescriptorWriter(VkContext context, uint bufCount, uint imgCount)
        {
            this.context = context;
            writeDescriptors = new List<WriteDescriptorSet>();
            //this.imageInfos = new DescriptorImageInfo[imgCount];
            //this.bufferInfos = new DescriptorBufferInfo[bufCount];
            //imgIdx = 0;
            //bufIdx = 0;
        }

        //public VkDescriptorWriter WriteBuffer(uint binding, DescriptorBufferInfo bufferInfo, DescriptorSet descriptorSet)
        public VkOldDescriptorWriter WriteBuffer(uint binding, DescriptorBufferInfo bufferInfo, DescriptorType bufferType = DescriptorType.UniformBuffer)
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

        //public VkDescriptorWriter WriteImage(uint binding, DescriptorImageInfo[] imageInfos, DescriptorSet descriptorSet)
        public VkOldDescriptorWriter WriteImage(uint binding, DescriptorImageInfo[] imageInfos)
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
