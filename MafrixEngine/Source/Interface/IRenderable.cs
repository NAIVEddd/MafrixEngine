using MafrixEngine.GraphicsWrapper;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace MafrixEngine.Source.Interface
{
    public interface IRenderable
    {
        public void BindCommand(Vk vk, CommandBuffer commandBuffer, Action<int> action);
    }

    public interface IDescriptor : IDisposable
    {
        public int DescriptorSetCount { get; set; }
        public void BindCommand(Vk vk, CommandBuffer commandBuffer,
                Buffer vertices, Buffer indices, Action<int> bindDescriptorSet);
        public DescriptorPool CreateDescriptorPool(VkContext vkContext, DescriptorSetLayout[] setLayouts, DescriptorPoolSize[] poolSizes, int frames);
        public DescriptorSet[] AllocateDescriptorSets(VkContext vkContext, DescriptorPool pool, DescriptorSetLayout[] setLayouts, int frames);
        public void UpdateDescriptorSets(VkContext vkContext,
            Sampler sampler,
            DescriptorSet[] descriptorSets, Buffer[] buffer, int start);
        public void UpdateUniformBuffer(out Matrix4X4<float>[] modelMatrix);
    }
}
