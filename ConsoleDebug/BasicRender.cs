using MafrixEngine.Cameras;
using MafrixEngine.GraphicsWrapper;
using MafrixEngine.Source.Interface;
using MafrixEngine.Source.DataStruct;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buffer = Silk.NET.Vulkan.Buffer;
using mat4 = Silk.NET.Maths.Matrix4X4<float>;
using vec4 = Silk.NET.Maths.Vector4D<float>;
using Silk.NET.Assimp;
using Camera = MafrixEngine.Cameras.Camera;
using MafrixEngine.Input;
using Silk.NET.Input;

namespace ConsoleDebug
{
    internal class BasicRender : RenderBase, IRender
    {
        struct  UniformBuffers
        {
            public VkBuffer scene;
            public VkBuffer offscreen;
        }
        UniformBuffers uniformBuffers;

	    struct UboVSscene
        {
            public mat4 projection;
            public mat4 view;
            public mat4 model;
            public mat4 depthBiasMVP;
            public vec4 lightPos;
            // Used for depth map visualization
            float zNear;
            float zFar;
        }
        UboVSscene uboVSscene;

        struct UboOffscreenVS
        {
            public mat4 depthMVP;
	    }
        UboOffscreenVS uboOffscreenVS;

        struct Pipelines
        {
            public Pipeline offscreen;
            public Pipeline sceneShadow;
            public Pipeline sceneShadowPCF;
            public Pipeline debug;
	    }
        Pipelines pipelines;
        PipelineLayout pipelineLayout;
        DescriptorPool descriptorPool;
        DescriptorSetLayout descriptorSetLayout;

        struct DescriptorSets
        {
            public DescriptorSet offscreen;
            public DescriptorSet scene;
            public DescriptorSet debug;
        }
        DescriptorSets descriptorSets;

        struct FrameBufferAttachment
        {
            public Image image;
            public DeviceMemory mem;
            public ImageView view;
        };
        struct OffscreenPass
        {
            public int width, height;
            public VkFramebuffer frameBuffer;
            public FrameBufferAttachment depth;
            public RenderPass renderPass;
            public Sampler depthSampler;
            public DescriptorImageInfo descriptor;
        }
        private VkFramebuffer frameBuffers;
        private OffscreenPass offscreenPass;
        private RenderPass sceneRenderPass;
        private List<Action<CommandBuffer>> actions = new List<Action<CommandBuffer>>();

        public IWindow Window { get => base.window; set => base.window = value; }
        private int currentFrame = 0;
        public unsafe void Draw(double delta)
        {
            vkContext.vk.QueueWaitIdle(graphicsQueue);
            var result = vkSwapchain.AcquireNextImage(ulong.MaxValue, imageAvailableSemaphores[currentFrame], default, out var imageIndex);
            if (result == Result.ErrorOutOfDateKhr)
            {
                // RecreateSwapChain()
                return;
            }
            //var fence = inFlightFences[imageIndex];
            //var imageFence = imagesInFlight[imageIndex];
            //if (imageFence.Handle != 0)
            //{
            //    vkContext.vk.WaitForFences(vkContext.device, 1, in imageFence, Vk.True, ulong.MaxValue);
            //}
            //imagesInFlight[imageIndex] = fence;
            SubmitInfo submitInfo = new SubmitInfo { SType = StructureType.SubmitInfo };
            submitInfo.CommandBufferCount = 1;
            var commandbuffer = commandBuffers[imageIndex];
            submitInfo.PCommandBuffers = &commandbuffer;
            var signalSemaphore = renderFinishedSemaphores[imageIndex];
            submitInfo.SignalSemaphoreCount = 1;
            submitInfo.PSignalSemaphores = &signalSemaphore;
            if (vkContext.vk.QueueSubmit
                            (graphicsQueue, 1, &submitInfo, default) != Result.Success)
            {
                throw new Exception("failed to submit draw command buffer!");
            }

            fixed (SwapchainKHR* swapchainPtr = &vkSwapchain.swapchain)
            {
                PresentInfoKHR presentInfo = new PresentInfoKHR
                {
                    SType = StructureType.PresentInfoKhr,
                    WaitSemaphoreCount = 1,
                    PWaitSemaphores = &signalSemaphore,
                    SwapchainCount = 1,
                    PSwapchains = swapchainPtr,
                    PImageIndices = &imageIndex
                };

                result = vkSwapchain.khrSwapchain.QueuePresent(graphicsQueue, &presentInfo);
            }
            currentFrame = (currentFrame + 1) % vkSwapchain.ImageCount;
        }

        public void SetCamera(Camera camera)
        {
            throw new NotImplementedException();
        }

        public void ShutDown()
        {
            throw new NotImplementedException();
        }

        HardwareInput input;
        KeyboardMapping kbMap;
        MouseMapping mouseMap;
        Camera camera;
        DateTime startTime = DateTime.Now;

        StaticSceneProvider staticScene;
        public void StartUp()
        {
            base.Initialize("ConsoleDebug");
            staticScene = new StaticSceneProvider(new ValueTuple<string, string, int>("Asserts/sponza", "Sponza.gltf", -1));

            PrepareFrameBuffer();
            PrepareUniformBuffers();
            SetupDescriptorSetLayout();
            PreparePipelines();
            SetupDescriptorPool();
            SetupDescriptorSets();
            staticScene.render = this;
            staticScene.StartUp();
            BuildCommandBuffers();

            input = new HardwareInput(window.CreateInput());
            kbMap = new KeyboardMapping(input.inputContext.Keyboards[0]);
            mouseMap = new MouseMapping(input.inputContext.Mice[0]);
            var pos = new Vector3D<float>(15.0f, 15.0f, 15.0f);
            var dir = new Vector3D<float>(0, 0, 0) - pos;
            camera = new Camera(new CameraCoordinate(pos, dir, new Vector3D<float>(0.0f, -1.0f, 0.0f)),
                            new ProjectInfo(45.0f, (float)vkSwapchain.extent.Width / (float)vkSwapchain.extent.Height));

            startTime = DateTime.Now;

            kbMap.AddKeyBinding(Key.W, camera.OnForward);
            kbMap.AddKeyBinding(Key.S, camera.OnBackward);
            kbMap.AddKeyBinding(Key.A, camera.OnLeft);
            kbMap.AddKeyBinding(Key.D, camera.OnRight);
            kbMap.AddKeyBinding(Key.Escape, window.Close);
            mouseMap.OnLeftClick += MouseMap_OnLeftClick;
            mouseMap.OnMouseMove += MouseMap_OnMouseMove;
        }
        private void MouseMap_OnMouseMove(IMouse arg1, Vector2D<float> arg2)
        {
            if (arg1.Cursor.CursorMode == CursorMode.Raw)
            {
                camera.OnRotate(arg2.X, arg2.Y);
            }
        }

        private void MouseMap_OnLeftClick(IMouse arg1, Vector2D<float> arg2)
        {
            if (mouseMap.cursor.CursorMode == CursorMode.Raw)
            {
                mouseMap.cursor.CursorMode = CursorMode.Normal;
            }
            else if (mouseMap.cursor.CursorMode == CursorMode.Normal)
            {
                mouseMap.cursor.CursorMode = CursorMode.Raw;
            }
        }

        private unsafe void BuildCommandBuffers()
        {
            var cmdBufInfo = new CommandBufferBeginInfo(StructureType.CommandBufferBeginInfo);
            var clearValues = new ClearValue[2];
            var viewport = new Viewport();
            var scissor = new Rect2D();
            for (int i = 0; i < commandBuffers.Length; i++)
            {
                vkContext.vk.BeginCommandBuffer(commandBuffers[i], cmdBufInfo);

                // First render pass:
                //   Generate shadow map
                {
                    var offscreenBI = new RenderPassBeginInfo(StructureType.RenderPassBeginInfo);
                    offscreenBI.RenderPass = offscreenPass.renderPass;
                    offscreenBI.Framebuffer = offscreenPass.frameBuffer.framebuffers[i];
                    offscreenBI.RenderArea.Extent.Width = (uint)offscreenPass.width;
                    offscreenBI.RenderArea.Extent.Height = (uint)offscreenPass.height;
                    offscreenBI.ClearValueCount = 1;
                    clearValues[0].DepthStencil = new ClearDepthStencilValue(1.0f, 0);
                    fixed(ClearValue* ptr = clearValues)
                    {
                        offscreenBI.PClearValues = ptr;
                    }
                    vkContext.vk.CmdBeginRenderPass(commandBuffers[i], offscreenBI, SubpassContents.Inline);

                    viewport = new Viewport(0, 0, offscreenPass.width, offscreenPass.height, 0.0f, 1.0f);
                    vkContext.vk.CmdSetViewport(commandBuffers[i], 0, 1, viewport);
                    scissor = new Rect2D(null, new Extent2D((uint)offscreenPass.width, (uint)offscreenPass.height));
                    vkContext.vk.CmdSetScissor(commandBuffers[i], 0, 1, scissor);
                    vkContext.vk.CmdBindPipeline(commandBuffers[i], PipelineBindPoint.Graphics, pipelines.offscreen);
                    vkContext.vk.CmdBindDescriptorSets(commandBuffers[i], PipelineBindPoint.Graphics, pipelineLayout, 0, 1, descriptorSets.offscreen, 0, 0);
                    /// Model draw
                    foreach (var item in actions)
                    {
                        item(commandBuffers[i]);
                    }
                    vkContext.vk.CmdEndRenderPass(commandBuffers[i]);
                }

                // Second pass: Scene rendering with applied shadow map
                {
                    clearValues[1] = clearValues[0];
                    clearValues[0].Color = new ClearColorValue();
                    var sceneBI = new RenderPassBeginInfo(StructureType.RenderPassBeginInfo);
                    sceneBI.RenderPass = sceneRenderPass;
                    sceneBI.Framebuffer = frameBuffers.framebuffers[i];
                    sceneBI.RenderArea.Extent = frameBuffers.frameExtent;
                    sceneBI.ClearValueCount = 2;
                    fixed (ClearValue* ptr = clearValues)
                    {
                        sceneBI.PClearValues = ptr;
                    }
                    vkContext.vk.CmdBeginRenderPass(commandBuffers[i], sceneBI, SubpassContents.Inline);
                    viewport = new Viewport(0, 0, frameBuffers.frameExtent.Width, frameBuffers.frameExtent.Height, 0.0f, 1.0f);
                    vkContext.vk.CmdSetViewport(commandBuffers[i], 0, 1, viewport);
                    scissor = new Rect2D(null, frameBuffers.frameExtent);
                    vkContext.vk.CmdSetScissor(commandBuffers[i], 0, 1, scissor);
                    vkContext.vk.CmdBindPipeline(commandBuffers[i], PipelineBindPoint.Graphics, pipelines.sceneShadow);
                    vkContext.vk.CmdBindDescriptorSets(commandBuffers[i], PipelineBindPoint.Graphics, pipelineLayout, 0, 1, descriptorSets.scene, 0, 0);
                    /// model draw
                    foreach (var item in actions)
                    {
                        item(commandBuffers[i]);
                    }
                    vkContext.vk.CmdEndRenderPass(commandBuffers[i]);
                }
                vkContext.vk.EndCommandBuffer(commandBuffers[i]);
            }
        }

        private unsafe void SetupDescriptorPool()
        {
            var uniformSize = new DescriptorPoolSize(DescriptorType.UniformBuffer, 3);
            var imageSize = new DescriptorPoolSize(DescriptorType.CombinedImageSampler, 3);
            var poolSizes = new DescriptorPoolSize[]
            {
                uniformSize, imageSize,
            };
            var poolInfo = new DescriptorPoolCreateInfo(StructureType.DescriptorPoolCreateInfo);
            poolInfo.PoolSizeCount = (uint)poolSizes.Length;
            poolInfo.MaxSets = 3;
            fixed (DescriptorPoolSize* ptr = poolSizes)
            {
                poolInfo.PPoolSizes = ptr;
            }
            vkContext.vk.CreateDescriptorPool(vkContext.device, poolInfo, null, out descriptorPool);
        }

        private unsafe void SetupDescriptorSets()
        {
            // Offscreen shadow map generation
            {
                var offscreenAI = new DescriptorSetAllocateInfo(StructureType.DescriptorSetAllocateInfo);
                offscreenAI.DescriptorPool = descriptorPool;
                offscreenAI.DescriptorSetCount = 1;
                fixed(DescriptorSetLayout* ptr = &descriptorSetLayout)
                {
                    offscreenAI.PSetLayouts = ptr;
                    vkContext.vk.AllocateDescriptorSets(vkContext.device, offscreenAI, out descriptorSets.offscreen);
                }
                var writeOff0 = new WriteDescriptorSet(StructureType.WriteDescriptorSet);
                writeOff0.DstSet = descriptorSets.offscreen;
                writeOff0.DescriptorType = DescriptorType.UniformBuffer;
                writeOff0.DstBinding = 0;
                writeOff0.DescriptorCount = 1;
                var offscreenBufferInfo = new DescriptorBufferInfo(uniformBuffers.offscreen.buffer, 0, uniformBuffers.offscreen.bufferSize);
                writeOff0.PBufferInfo = &offscreenBufferInfo;
                var writeOffsceneSets = new WriteDescriptorSet[]
                {
                    writeOff0,
                };
                vkContext.vk.UpdateDescriptorSets(vkContext.device, writeOffsceneSets, null);
            }
            // Scene rendering with shadow map applied
            {
                var sceneAI = new DescriptorSetAllocateInfo(StructureType.DescriptorSetAllocateInfo);
                sceneAI.DescriptorPool = descriptorPool;
                sceneAI.DescriptorSetCount = 1;
                fixed (DescriptorSetLayout* ptr = &descriptorSetLayout)
                {
                    sceneAI.PSetLayouts = ptr;
                    vkContext.vk.AllocateDescriptorSets(vkContext.device, sceneAI, out descriptorSets.scene);
                }
                var writeScene0 = new WriteDescriptorSet(StructureType.WriteDescriptorSet);
                writeScene0.DstSet = descriptorSets.scene;
                writeScene0.DescriptorType = DescriptorType.UniformBuffer;
                writeScene0.DstBinding = 0;
                writeScene0.DescriptorCount = 1;
                var sceneBufferInfo = new DescriptorBufferInfo(uniformBuffers.scene.buffer, 0, uniformBuffers.scene.bufferSize);
                writeScene0.PBufferInfo = &sceneBufferInfo;
                var writeScene1 = new WriteDescriptorSet(StructureType.WriteDescriptorSet);
                writeScene1.DstSet = descriptorSets.scene;
                writeScene1.DescriptorType = DescriptorType.CombinedImageSampler;
                writeScene1.DstBinding = 1;
                writeScene1.DescriptorCount = 1;
                var sceneImageInfo = new DescriptorImageInfo(offscreenPass.depthSampler, offscreenPass.depth.view, ImageLayout.DepthStencilReadOnlyOptimal);
                writeScene1.PImageInfo = &sceneImageInfo;
                var writeScene = new WriteDescriptorSet[]
                {
                    writeScene0, writeScene1,
                };
                vkContext.vk.UpdateDescriptorSets(vkContext.device, writeScene, null);
            }
        }

        private unsafe void SetupDescriptorSetLayout()
        {
            var bind0 = new DescriptorSetLayoutBinding(0, DescriptorType.UniformBuffer, 1,
                ShaderStageFlags.VertexBit);
            var bind1 = new DescriptorSetLayoutBinding(1, DescriptorType.CombinedImageSampler, 1,
                ShaderStageFlags.FragmentBit);
            var bindings = new DescriptorSetLayoutBinding[]
            {
                bind0, bind1,
            };
            var createInfo = new DescriptorSetLayoutCreateInfo(StructureType.DescriptorSetLayoutCreateInfo);
            createInfo.BindingCount = (uint)bindings.Length;
            fixed(DescriptorSetLayoutBinding* ptr = bindings)
            {
                createInfo.PBindings = ptr;
            }
            vkContext.vk.CreateDescriptorSetLayout(vkContext.device, createInfo, null, out var setLayout);

            var pipelineLayoutCI = new PipelineLayoutCreateInfo(StructureType.PipelineLayoutCreateInfo);
            pipelineLayoutCI.PSetLayouts = &setLayout;
            pipelineLayoutCI.SetLayoutCount = 1;
            vkContext.vk.CreatePipelineLayout(vkContext.device, pipelineLayoutCI, null, out pipelineLayout);
            descriptorSetLayout = setLayout;
        }

        private void PreparePipelines()
        {
            PrepareOffscreenPipeline();
            PrepareScenePipeline();
        }

        private void PrepareScenePipeline()
        {
            var shaderDefines = new ShaderDefine[2];
            shaderDefines[0] = new ShaderDefine("./Shaders/scene.vert.spv", ShaderStageFlags.VertexBit);
            shaderDefines[1] = new ShaderDefine("./Shaders/scene.frag.spv", ShaderStageFlags.FragmentBit);
            var pipelineInfos = new PipelineInfo(vkContext, shaderDefines);
            var descriptorSetLayout = pipelineInfos.setLayoutInfo.SetLayout;
            var PoolSizes = pipelineInfos.setLayoutInfo.PoolSizes;
            VkPipelineBuilder pipelineBuilder = new VkPipelineBuilder(vkContext);
            pipelineBuilder.BindInputAssemblyState(PrimitiveTopology.TriangleList);
            pipelineBuilder.BindVertexInput<Vertex>(default);
            pipelineBuilder.BindRasterizationState(depthBiasEnable: Vk.True);
            //pipelineBuilder.BindViewportState(new Extent2D((uint)offscreenPass.width, (uint)offscreenPass.height));
            pipelineBuilder.BindMultisampleState();
            pipelineBuilder.BindColorBlendState(new VkPipelineBuilder.ColorBlendMask[]
            {
                new VkPipelineBuilder.ColorBlendMask(ColorComponentFlags.None, false)
            });
            pipelineBuilder.BindShaders(shaderDefines);
            pipelineBuilder.BindDepthStencilState(true, true, CompareOp.LessOrEqual);
            pipelineBuilder.BindDynamicState(new DynamicState[] { DynamicState.Viewport, DynamicState.Scissor });
            pipelineBuilder.BindShaderStages(pipelineInfos.pipelineShaderStageCreateInfos);
            pipelineBuilder.BindRenderPass(sceneRenderPass, 0);
            pipelineBuilder.Build();
            pipelines.sceneShadow = pipelineBuilder.pipeline;
        }

        private void PrepareOffscreenPipeline()
        {
            var shaderDefines = new ShaderDefine[1];
            shaderDefines[0] = new ShaderDefine("./Shaders/offscreen.vert.spv", ShaderStageFlags.VertexBit);
            var pipelineInfos = new PipelineInfo(vkContext, shaderDefines);
            VkPipelineBuilder pipelineBuilder = new VkPipelineBuilder(vkContext);
            pipelineBuilder.BindShaders(shaderDefines);
            pipelineBuilder.BindInputAssemblyState(PrimitiveTopology.TriangleList);
            pipelineBuilder.BindVertexInput<Vertex>();
            pipelineBuilder.BindRasterizationState(depthBiasEnable:Vk.True);
            pipelineBuilder.BindMultisampleState();
            pipelineBuilder.BindDepthStencilState(true, true, CompareOp.LessOrEqual);
            pipelineBuilder.BindDynamicState(new DynamicState[] { DynamicState.Viewport, DynamicState.Scissor, DynamicState.DepthBias});
            pipelineBuilder.BindShaderStages(pipelineInfos.pipelineShaderStageCreateInfos);
            pipelineBuilder.BindRenderPass(offscreenPass.renderPass, 0);
            pipelineBuilder.Build();
            pipelines.offscreen = pipelineBuilder.pipeline;
        }

        private unsafe void PrepareUniformBuffers()
        {
            uniformBuffers.offscreen = new VkBuffer(vkContext, (ulong)sizeof(UboOffscreenVS), BufferUsageFlags.UniformBufferBit);
            uniformBuffers.scene = new VkBuffer(vkContext, (ulong)sizeof(UboVSscene), BufferUsageFlags.UniformBufferBit);
        }

        private Image depthImage;
        private ImageView depthView;
        private DeviceMemory depthMemory;
        private unsafe void PrepareFrameBuffer()
        {
            offscreenPass.width = 2048;
            offscreenPass.height = 2048;
            CreateDepthImage(ImageUsageFlags.DepthStencilAttachmentBit | ImageUsageFlags.SampledBit,
                offscreenPass.width, offscreenPass.height,
                out offscreenPass.depth.image,
                out offscreenPass.depth.view,
                out offscreenPass.depth.mem);
            //var imageCI = new ImageCreateInfo(StructureType.ImageCreateInfo);
            //imageCI.ImageType = ImageType.Type2D;
            //imageCI.Extent.Width = 2048;
            //imageCI.Extent.Height = 2048;
            //imageCI.Extent.Depth = 1;
            //imageCI.MipLevels = 1;
            //imageCI.ArrayLayers = 1;
            //imageCI.Samples = SampleCountFlags.Count1Bit;
            //imageCI.Tiling = ImageTiling.Optimal;
            //imageCI.Format = Format.D32Sfloat;
            //imageCI.Usage = ImageUsageFlags.DepthStencilAttachmentBit | ImageUsageFlags.SampledBit;
            //vkContext.vk.CreateImage(vkContext.device, in imageCI, null, out offscreenPass.depth.image);

            //vkContext.vk.GetImageMemoryRequirements(vkContext.device, offscreenPass.depth.image, out var memReqs);
            //var memAlloc = new MemoryAllocateInfo(StructureType.MemoryAllocateInfo);
            //memAlloc.AllocationSize = memReqs.Size;
            //memAlloc.MemoryTypeIndex = FindMemoryType(memReqs.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit);
            //vkContext.vk.AllocateMemory(vkContext.device, memAlloc, null, out offscreenPass.depth.mem);
            //vkContext.vk.BindImageMemory(vkContext.device, offscreenPass.depth.image, offscreenPass.depth.mem, 0);

            //var depthStencilView = new ImageViewCreateInfo(StructureType.ImageViewCreateInfo);
            //depthStencilView.ViewType = ImageViewType.Type2D;
            //depthStencilView.Format = Format.D32Sfloat;
            //depthStencilView.SubresourceRange.AspectMask = ImageAspectFlags.DepthBit;
            //depthStencilView.SubresourceRange.BaseMipLevel = 0;
            //depthStencilView.SubresourceRange.LevelCount = 1;
            //depthStencilView.SubresourceRange.BaseArrayLayer = 0;
            //depthStencilView.SubresourceRange.LayerCount = 1;
            //depthStencilView.Image = offscreenPass.depth.image;
            //vkContext.vk.CreateImageView(vkContext.device, depthStencilView, null, out offscreenPass.depth.view);

            Filter shadowmapFilter = Filter.Linear;
            var sampler = new SamplerCreateInfo(StructureType.SamplerCreateInfo);
            sampler.MagFilter = shadowmapFilter;
            sampler.MinFilter = shadowmapFilter;
            sampler.MipmapMode = SamplerMipmapMode.Linear;
            sampler.AddressModeU = SamplerAddressMode.ClampToEdge;
            sampler.AddressModeV = sampler.AddressModeU;
            sampler.AddressModeW = sampler.AddressModeU;
            sampler.MipLodBias = 0.0f;
            sampler.MaxAnisotropy = 1.0f;
            sampler.MinLod = 0.0f;
            sampler.MaxLod = 1.0f;
            vkContext.vk.CreateSampler(vkContext.device, sampler, null, out offscreenPass.depthSampler);

            prepareSceneRenderPass();
            prepareOffscreenRenderPass();

            var vkFramebuffer = new VkFramebuffer(vkContext, vkSwapchain.ImageCount);
            vkFramebuffer.SetExtent(new Extent2D((uint)offscreenPass.width, (uint)offscreenPass.height));
            vkFramebuffer.SetRenderpass(offscreenPass.renderPass);
            for (int i = 0; i < vkSwapchain.ImageCount; i++)
            {
                vkFramebuffer.AddAttachment(i, 0, offscreenPass.depth.view);
            }
            vkFramebuffer.Build();
            offscreenPass.frameBuffer = vkFramebuffer;

            CreateDepthImage(ImageUsageFlags.DepthStencilAttachmentBit,
                window.Size.X, window.Size.Y,
                out depthImage, out depthView, out depthMemory);
            var sceneFramebuffer = new VkFramebuffer(vkContext, vkSwapchain.ImageCount);
            sceneFramebuffer.SetExtent(new Extent2D((uint)window.Size.X, (uint)window.Size.Y));
            sceneFramebuffer.SetRenderpass(sceneRenderPass);
            for (int i = 0; i < vkSwapchain.ImageCount; i++)
            {
                sceneFramebuffer.AddAttachment(i, 0, vkSwapchain.imageViews[i]);
                sceneFramebuffer.AddAttachment(i, 1, depthView);
            }
            sceneFramebuffer.Build();
            frameBuffers = sceneFramebuffer;
        }

        private unsafe void CreateDepthImage(ImageUsageFlags imageUsage, int width, int height, out Image image, out ImageView view, out DeviceMemory memory)
        {
            var imageCI = new ImageCreateInfo(StructureType.ImageCreateInfo);
            imageCI.ImageType = ImageType.Type2D;
            imageCI.Extent.Width = (uint)width;
            imageCI.Extent.Height = (uint)height;
            imageCI.Extent.Depth = 1;
            imageCI.MipLevels = 1;
            imageCI.ArrayLayers = 1;
            imageCI.Samples = SampleCountFlags.Count1Bit;
            imageCI.Tiling = ImageTiling.Optimal;
            imageCI.Format = Format.D32Sfloat;
            imageCI.Usage = imageUsage;
            vkContext.vk.CreateImage(vkContext.device, in imageCI, null, out image);

            vkContext.vk.GetImageMemoryRequirements(vkContext.device, offscreenPass.depth.image, out var memReqs);
            var memAlloc = new MemoryAllocateInfo(StructureType.MemoryAllocateInfo);
            memAlloc.AllocationSize = memReqs.Size;
            memAlloc.MemoryTypeIndex = FindMemoryType(memReqs.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit);
            vkContext.vk.AllocateMemory(vkContext.device, memAlloc, null, out memory);
            vkContext.vk.BindImageMemory(vkContext.device, image, memory, 0);

            var depthStencilView = new ImageViewCreateInfo(StructureType.ImageViewCreateInfo);
            depthStencilView.ViewType = ImageViewType.Type2D;
            depthStencilView.Format = Format.D32Sfloat;
            depthStencilView.SubresourceRange.AspectMask = ImageAspectFlags.DepthBit;
            depthStencilView.SubresourceRange.BaseMipLevel = 0;
            depthStencilView.SubresourceRange.LevelCount = 1;
            depthStencilView.SubresourceRange.BaseArrayLayer = 0;
            depthStencilView.SubresourceRange.LayerCount = 1;
            depthStencilView.Image = offscreenPass.depth.image;
            vkContext.vk.CreateImageView(vkContext.device, depthStencilView, null, out view);
        }

        private void prepareSceneRenderPass()
        {
            var vkRenderpass = new VkRenderPassBuilder(vkContext.vk, vkContext.device);
            {
                vkRenderpass.AddAttachment(new AttachmentDescription(
                        null, vkSwapchain.format, SampleCountFlags.Count1Bit, AttachmentLoadOp.Clear, AttachmentStoreOp.Store,
                        null, null, ImageLayout.Undefined, ImageLayout.PresentSrcKhr));
                vkRenderpass.AddAttachment(new AttachmentDescription(
                        null, Format.D32Sfloat, SampleCountFlags.Count1Bit, AttachmentLoadOp.Clear, AttachmentStoreOp.Store,
                        null, null, ImageLayout.Undefined, ImageLayout.DepthStencilAttachmentOptimal));
            }
            {
                var subpassDep = new VkSubpassDesc();
                subpassDep.AddColorAttachment(0);
                subpassDep.DepthStencilAttachmentRef = 1;
                vkRenderpass.AddSubpass(subpassDep);
            }
            sceneRenderPass = vkRenderpass.Build();
        }

        private void prepareOffscreenRenderPass()
        {
            var vkRenderpass = new VkRenderPassBuilder(vkContext.vk, vkContext.device);
            {
                vkRenderpass.AddAttachment(new AttachmentDescription(
                        null, Format.D32Sfloat, SampleCountFlags.Count1Bit, AttachmentLoadOp.Clear, AttachmentStoreOp.Store,
                        null, null, ImageLayout.Undefined, ImageLayout.DepthStencilReadOnlyOptimal
                    ));
            }
            {
                var subpassDep = new VkSubpassDesc();
                subpassDep.DepthStencilAttachmentRef = 0;
                vkRenderpass.AddSubpass(subpassDep);
            }
            offscreenPass.renderPass = vkRenderpass.Build();
        }

        private unsafe UInt32 FindMemoryType(UInt32 typeFilter, MemoryPropertyFlags properties)
        {
            var memProperties = new PhysicalDeviceMemoryProperties();
            vkContext.vk.GetPhysicalDeviceMemoryProperties(vkContext.physicalDevice, out memProperties);
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

        public void UpdateModels()
        {
            throw new NotImplementedException();
        }

        public void AddStaticDraw(Action<CommandBuffer> action)
        {
            actions.Add(action);
        }
    }
}
