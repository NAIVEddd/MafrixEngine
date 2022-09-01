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

namespace MafrixEngine.GraphicsWrapper
{
    public unsafe class VkDescriptorPollSize
    {
        public DescriptorPoolSize[] poolSizes;
        public VkDescriptorPollSize(DescriptorSetLayoutInfo descriptorSetLayoutInfo)
        {
            var typeMap = new Dictionary<DescriptorType, uint>();
            for(var i = 0; i < descriptorSetLayoutInfo.SetCount; i++)
            {
                foreach(var binding in descriptorSetLayoutInfo.GetAllBindings())
                {
                    var ty = binding.DescriptorType;
                    if(typeMap.TryGetValue(ty, out uint value))
                    {
                        typeMap.Remove(ty);
                        typeMap.Add(ty, value + 1);
                    }
                    else
                    {
                        typeMap.Add(ty, 1);
                    }
                }
            }
            poolSizes = new DescriptorPoolSize[typeMap.Count];
            var count = 0;
            foreach(var kvpair in typeMap)
            {
                var poolSize = new DescriptorPoolSize(kvpair.Key, kvpair.Value);
                poolSizes[count] = poolSize;
                count++;
            }
        }
    }

    public unsafe class VkDescriptorPoll : IDisposable
    {
        private Vk vk;
        private DescriptorPool descriptorPool;
        private int frameCount;
        public VkDescriptorPoll(Vk vk, int frameCount)
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
}
