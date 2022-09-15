using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using MafrixEngine.ModelLoaders;
using MafrixEngine.Source.Interface;

namespace MafrixEngine.GraphicsWrapper
{
    public class VkRenderPass
    {
    }

    public class VkSubpass
    {

    }

    public class VkFrameBuffer
    {

    }

    public class VkPipeline : IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class VkRenderPassBuilder
    {
        private RenderPassCreateInfo renderPassCreateInfo;

        private Vk vk;
        private Device device;
        public VkRenderPassBuilder(Vk vk, Device device)
        {
            this.vk = vk;
            this.device = device;
            renderPassCreateInfo.SType = StructureType.RenderPassCreateInfo;
        }

        private List<AttachmentDescription> attachmentDescriptions;
        public VkRenderPassBuilder AddAttachment(AttachmentDescription attachment)
        {
            attachmentDescriptions.Add(attachment);
            return this;
        }

        private List<SubpassDescription> subpassDescriptions;
        public VkRenderPassBuilder AddSubpass(SubpassDescription subpass)
        {
            subpassDescriptions.Add(subpass);
            return this;
        }

        private List<SubpassDependency> subpassDependencies;
        public VkRenderPassBuilder AddDependency(SubpassDependency dependency)
        {
            subpassDependencies.Add(dependency);
            return this;
        }

        public VkRenderPass Build()
        {


            var renderpass = new VkRenderPass();
            return renderpass;
        }
    }

    public class VkSubpassBuilder
    {

    }


    /// <summary>
    /// typedef struct VkGraphicsPipelineCreateInfo {
        //    VkStructureType sType;
        //    const void* pNext;
        //    VkPipelineCreateFlags flags;
        //    uint32_t stageCount;
        //    const VkPipelineShaderStageCreateInfo* pStages;
        //    const VkPipelineVertexInputStateCreateInfo* pVertexInputState;
        //    const VkPipelineInputAssemblyStateCreateInfo* pInputAssemblyState;
        //    const VkPipelineTessellationStateCreateInfo* pTessellationState;
        //    const VkPipelineViewportStateCreateInfo* pViewportState;
        //    const VkPipelineRasterizationStateCreateInfo* pRasterizationState;
        //    const VkPipelineMultisampleStateCreateInfo* pMultisampleState;
        //    const VkPipelineDepthStencilStateCreateInfo* pDepthStencilState;
        //    const VkPipelineColorBlendStateCreateInfo* pColorBlendState;
        //    const VkPipelineDynamicStateCreateInfo* pDynamicState;
        //    VkPipelineLayout layout;
        //    VkRenderPass renderPass;
        //    uint32_t subpass;
        //    VkPipeline basePipelineHandle;
        //    int32_t basePipelineIndex;
        //}
        //VkGraphicsPipelineCreateInfo;
/// </summary>
/// 
/// TODO: use raw memory to save all needed infomation.
public class VkPipelineBuilder
    {
        private Vk vk;
        private Device device;
        private PipelineInfo pipelineInfo;

        public VkPipelineBuilder(Vk vk, Device device)
        {
            this.vk = vk;
            this.device = device;
        }

        public void DumpPipelineLayout()
        {
            Console.WriteLine("Pipeline layout include these descriptor set: ...");
        }
        public VkPipelineBuilder BindSharder(ShaderDefine shader)
        {
            return this;
        }
        public VkPipelineBuilder BindSharders(Span<ShaderDefine> shaders)
        {
            return this;
        }
        public unsafe VkPipelineBuilder BindVertexInput<T>(T t) where T : IVertexData
        {
            bindingDescription = t.BindingDescription;
            attributeDescription = t.AttributeDescriptions;

            vertexInputState.SType = StructureType.PipelineVertexInputStateCreateInfo;
            vertexInputState.VertexBindingDescriptionCount = 1;
            fixed (VertexInputBindingDescription* bindingDescPtr = &bindingDescription)
            {
                vertexInputState.PVertexBindingDescriptions = bindingDescPtr;
            }
            vertexInputState.VertexAttributeDescriptionCount = (uint)attributeDescription.Length;
            fixed (VertexInputAttributeDescription* attributes = attributeDescription)
            {
                vertexInputState.PVertexAttributeDescriptions = attributes;
            }
            return this;
        }
        private VertexInputBindingDescription bindingDescription;
        private VertexInputAttributeDescription[] attributeDescription;
        private PipelineVertexInputStateCreateInfo vertexInputState;
        public unsafe VkPipelineBuilder BindVertexInputState(VertexInputBindingDescription bindingDesc, VertexInputAttributeDescription[] attributeDesc)
        {
            bindingDescription = bindingDesc;
            attributeDescription = attributeDesc;

            vertexInputState.SType = StructureType.PipelineVertexInputStateCreateInfo;
            vertexInputState.VertexBindingDescriptionCount = 1;
            fixed(VertexInputBindingDescription* bindingDescPtr = &bindingDescription)
            {
                vertexInputState.PVertexBindingDescriptions = bindingDescPtr;
            }
            vertexInputState.VertexAttributeDescriptionCount = (uint)attributeDescription.Length;
            fixed (VertexInputAttributeDescription* attributes = attributeDescription)
            {
                vertexInputState.PVertexAttributeDescriptions = attributes;
            }
            return this;
        }
        private PipelineInputAssemblyStateCreateInfo inputAssemblyState;
        public VkPipelineBuilder BindInputAssemblyState(PrimitiveTopology topology = PrimitiveTopology.TriangleList, bool primitiveRestartEnable = false)
        {
            inputAssemblyState.SType = StructureType.PipelineInputAssemblyStateCreateInfo;
            inputAssemblyState.Topology = topology;
            inputAssemblyState.Flags = 0;
            inputAssemblyState.PrimitiveRestartEnable = primitiveRestartEnable;
            return this;
        }

        public VkPipelineBuilder BindTessellationState()
        {
            return this;
        }
        private Viewport viewport;
        private Rect2D scissor;
        private PipelineViewportStateCreateInfo viewportState;
        public unsafe VkPipelineBuilder BindViewportState(Extent2D extent)
        {
            viewport = new Viewport();
            viewport.X = 0.0f;
            viewport.Y = 0.0f;
            viewport.Width = extent.Width;
            viewport.Height = extent.Height;
            viewport.MinDepth = 0.0f;
            viewport.MaxDepth = 0.1f;
            scissor = new Rect2D(default, extent);
            viewportState.SType = StructureType.PipelineViewportStateCreateInfo;
            fixed(Viewport* viewportPtr = &viewport)
            {
                viewportState.PViewports = viewportPtr;
            }
            fixed(Rect2D* scissorPtr = &scissor)
            {
                viewportState.PScissors = scissorPtr;
            }
            viewportState.ViewportCount = 1;
            viewportState.ScissorCount = 1;
            return this;
        }
        private PipelineRasterizationStateCreateInfo rasterizationState;
        public VkPipelineBuilder BindRasterizationState(PolygonMode polygonMode = PolygonMode.Fill, CullModeFlags cullMode = CullModeFlags.BackBit, FrontFace frontFace = FrontFace.CounterClockwise)
        {
            rasterizationState.SType = StructureType.PipelineRasterizationStateCreateInfo;
            rasterizationState.PolygonMode = polygonMode;
            rasterizationState.CullMode = cullMode;
            rasterizationState.FrontFace = frontFace;

            rasterizationState.DepthClampEnable = Vk.False;
            rasterizationState.RasterizerDiscardEnable = Vk.False;
            rasterizationState.LineWidth = 1.0f;
            rasterizationState.DepthBiasEnable = Vk.False;
            return this;
        }
        private PipelineMultisampleStateCreateInfo multisampling;
        public VkPipelineBuilder BindMultisampleState(SampleCountFlags sampleCount = SampleCountFlags.Count1Bit, bool sampleEnable = false)
        {
            multisampling.SType = StructureType.PipelineMultisampleStateCreateInfo;
            multisampling.SampleShadingEnable = sampleEnable;
            multisampling.RasterizationSamples = sampleCount;
            return this;
        }
        private PipelineDepthStencilStateCreateInfo depthStencil;
        public VkPipelineBuilder BindDepthStencilState(bool depthTest, bool depthWrite, CompareOp depthCompareOp = CompareOp.LessOrEqual,
            bool depthBoundsTest = false, float minDepthBounds = 0.0f, float maxDepthBounds = 1.0f,
            bool stencilTest = false, StencilOpState front = default, StencilOpState back = default)
        {
            depthStencil.SType = StructureType.PipelineDepthStencilStateCreateInfo;
            depthStencil.DepthTestEnable = depthTest;
            depthStencil.DepthWriteEnable = depthWrite;
            depthStencil.DepthCompareOp = depthCompareOp;
            depthStencil.DepthBoundsTestEnable = depthBoundsTest;
            depthStencil.MinDepthBounds = minDepthBounds;
            depthStencil.MaxDepthBounds = maxDepthBounds;
            depthStencil.StencilTestEnable = stencilTest;
            depthStencil.Front = front;
            depthStencil.Back = back;
            return this;
        }
        public struct ColorBlendMask
        {
            public ColorComponentFlags flag;
            public bool blendEnable;
            public ColorBlendMask(ColorComponentFlags f, bool enable)
            {
                flag = f;
                blendEnable = enable;
            }
        }
        private PipelineColorBlendAttachmentState[] colorBlendAttachments;
        private PipelineColorBlendStateCreateInfo colorBlendState;
        public unsafe VkPipelineBuilder BindColorBlendState(ColorBlendMask[] masks)
        {
            colorBlendAttachments = new PipelineColorBlendAttachmentState[masks.Length];
            for (int i = 0; i < colorBlendAttachments.Length; i++)
            {
                colorBlendAttachments[i] = new PipelineColorBlendAttachmentState();
                colorBlendAttachments[i].ColorWriteMask = masks[i].flag;
                colorBlendAttachments[i].BlendEnable = masks[i].blendEnable;
            }
            colorBlendState.SType = StructureType.PipelineColorBlendStateCreateInfo;
            colorBlendState.LogicOpEnable = Vk.False;
            colorBlendState.LogicOp = LogicOp.Copy;
            colorBlendState.AttachmentCount = (uint)colorBlendAttachments.Length;
            fixed(PipelineColorBlendAttachmentState* attachmentsPtr = colorBlendAttachments)
            {
                colorBlendState.PAttachments = attachmentsPtr;
            }
            colorBlendState.BlendConstants[0] = 0.0f;
            colorBlendState.BlendConstants[1] = 0.0f;
            colorBlendState.BlendConstants[2] = 0.0f;
            colorBlendState.BlendConstants[3] = 0.0f;
            return this;
        }
        private DynamicState[] dynamicStates;
        private PipelineDynamicStateCreateInfo dynamicState;
        public unsafe VkPipelineBuilder BindDynamicState(DynamicState[] states)
        {
            dynamicStates = states;
            dynamicState.SType = StructureType.PipelineDynamicStateCreateInfo;
            dynamicState.Flags = 0;
            dynamicState.DynamicStateCount = (uint)dynamicStates.Length;
            fixed(DynamicState* statesPtr = dynamicStates)
            {
                dynamicState.PDynamicStates = statesPtr;
            }
            return this;
        }

        private RenderPass renderPass;
        private int subpass;
        public VkPipelineBuilder BindRenderPass(RenderPass rp, int subpass_)
        {
            renderPass = rp;
            subpass = subpass_;
            return this;
        }
        //public VkPipelineBuilder BindRenderPass(VkRenderPass renderPass)
        //{


        //    return this;
        //}
        private DescriptorSetLayout[] setLayouts;
        public PipelineLayout pipelineLayout;
        public unsafe VkPipelineBuilder BindPlipelineLayout(DescriptorSetLayout[] setLayouts_)
        {
            setLayouts = setLayouts_;
            var pipelineLayoutInfo = new PipelineLayoutCreateInfo(StructureType.PipelineLayoutCreateInfo);
            pipelineLayoutInfo.SetLayoutCount = (uint)setLayouts.Length;
            fixed (DescriptorSetLayout* ptr = setLayouts)
            {
                pipelineLayoutInfo.PSetLayouts = ptr;
            }
            pipelineLayoutInfo.PushConstantRangeCount = 0;

            if (vk.CreatePipelineLayout(device, in pipelineLayoutInfo, null, out pipelineLayout) != Result.Success)
            {
                throw new Exception("failed to create pipeline layout!");
            }
            return this;
        }
        private PipelineShaderStageCreateInfo[] pipelineShaderStageCreateInfos;
        public VkPipelineBuilder BindShaderStages(PipelineShaderStageCreateInfo[] shaders)
        {
            pipelineShaderStageCreateInfos = shaders;
            return this;
        }
        public Pipeline pipeline;
        public unsafe VkPipeline Build()
        {
            var createInfo = new GraphicsPipelineCreateInfo(StructureType.GraphicsPipelineCreateInfo);
            fixed(PipelineVertexInputStateCreateInfo* ptr = &vertexInputState)
            {
                createInfo.PVertexInputState = ptr;
            }
            fixed(PipelineInputAssemblyStateCreateInfo* ptr = &inputAssemblyState)
            {
                createInfo.PInputAssemblyState = ptr;
            }
            fixed(PipelineViewportStateCreateInfo* ptr = &viewportState)
            {
                createInfo.PViewportState = ptr;
            }
            fixed(PipelineRasterizationStateCreateInfo* ptr = &rasterizationState)
            {
                createInfo.PRasterizationState = ptr;
            }
            fixed(PipelineMultisampleStateCreateInfo* ptr = &multisampling)
            {
                createInfo.PMultisampleState = ptr;
            }
            fixed(PipelineDepthStencilStateCreateInfo* ptr = &depthStencil)
            {
                createInfo.PDepthStencilState = ptr;
            }
            fixed (PipelineColorBlendStateCreateInfo* ptr = &colorBlendState)
            {
                createInfo.PColorBlendState = ptr;
            }
            createInfo.Layout = pipelineLayout;
            createInfo.RenderPass = renderPass;
            createInfo.Subpass = (uint)subpass;
            createInfo.BasePipelineHandle = default;
            fixed (PipelineShaderStageCreateInfo* stagePtr = pipelineShaderStageCreateInfos)
            {
                createInfo.StageCount = (uint)pipelineShaderStageCreateInfos.Length;
                createInfo.PStages = stagePtr;
            }
            if (vk.CreateGraphicsPipelines(device, default, 1, in createInfo, null, out this.pipeline) != Result.Success)
            {
                throw new Exception("failed to create graphics pipeline.");
            }

            VkPipeline pipeline = new VkPipeline();
            return pipeline;
        }
    }
}
