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
}
