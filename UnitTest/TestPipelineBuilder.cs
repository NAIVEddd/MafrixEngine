using MafrixEngine.GraphicsWrapper;
using MafrixEngine.ModelLoaders;
using Silk.NET.Core;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    public class TestPipelineBuilder
    {
        [Fact]
        public void TestVkInstance()
        {
            var vkCtx = new VkContext();
            vkCtx.Initialize("Test", new Version32(0, 0, 1));

            var testRenderPass = new VkRenderPassBuilder(vkCtx.vk, vkCtx.device);
            var pipeline1 = new VkPipelineBuilder(vkCtx);
            pipeline1.BindInputAssemblyState();
            pipeline1.BindRasterizationState();
            pipeline1.BindColorBlendState(new VkPipelineBuilder.ColorBlendMask[] { new VkPipelineBuilder.ColorBlendMask(ColorComponentFlags.None, false) });
            pipeline1.BindMultisampleState();
            pipeline1.BindViewportState(new Extent2D(1920, 1080));
            pipeline1.BindDepthStencilState(true, true);
            pipeline1.BindDynamicState(new DynamicState[] { });
            pipeline1.BindShaderStages(new PipelineShaderStageCreateInfo[] { });
            pipeline1.BindVertexInput<Vertex>(default);
            testRenderPass.AddPipeline(pipeline1);
            testRenderPass.Build();
        }
    }
}
