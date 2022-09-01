using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MafrixEngine.GraphicsWrapper;
using Silk.NET.Maths;
using Silk.NET.Assimp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using SPIRVCross;
using static SPIRVCross.SPIRV;
using MafrixEngine.ModelLoaders;
using Silk.NET.Core;

namespace UnitTest
{
    public class TestSpirvCross
    {
        public string vertShader = "Asserts/Shaders/base.vert.spv";
        public string fragShader = "Asserts/Shaders/base.frag.spv";


        [Fact]
        public unsafe void TestSpirvCross_Api()
        {
            byte[] vertBytes = System.IO.File.ReadAllBytes(vertShader);
            byte[] fragBytes = System.IO.File.ReadAllBytes(fragShader);

            SpvId* spirv;
            fixed(byte* ptr = vertBytes)
            {
                spirv = (SpvId*)ptr;
            }
            uint word_count = (uint)vertBytes.Length / 4;

            spvc_context context = default;
            spvc_parsed_ir ir;
            spvc_compiler compiler_glsl;
            spvc_compiler_options options;
            spvc_resources resources;
            spvc_reflected_resource* list = default;
            nuint count = default;

            spvc_context_create(&context);
            spvc_context_parse_spirv(context, spirv, word_count, &ir);
            spvc_context_create_compiler(context, spvc_backend.Glsl, ir, spvc_capture_mode.TakeOwnership, &compiler_glsl);

            // basic reflection
            spvc_compiler_create_shader_resources(compiler_glsl, &resources);
            spvc_resources_get_resource_list_for_type(resources, spvc_resource_type.UniformBuffer, (spvc_reflected_resource*)&list, &count);
            for(uint i = 0; i < count; i++)
            {
                var id = list[i].id;
                var set = spvc_compiler_get_decoration(compiler_glsl, id, SpvDecoration.SpvDecorationDescriptorSet);
                uint binding = spvc_compiler_get_decoration(compiler_glsl, id, SpvDecoration.SpvDecorationBinding);
                uint offset = spvc_compiler_get_decoration(compiler_glsl, id, SpvDecoration.SpvDecorationOffset);
                spvc_type type = spvc_compiler_get_type_handle(compiler_glsl, list[i].type_id);

                //var nameB = spvc_compiler_get_name(compiler_glsl, id);
                //var name = Marshal.PtrToStringAnsi((IntPtr)nameB);
                //var tyNameB = spvc_compiler_get_name(compiler_glsl, list[i].type_id);
                //var tyName = Marshal.PtrToStringAnsi((IntPtr)tyNameB);

                nuint size = 0;
                spvc_compiler_get_declared_struct_size(compiler_glsl, type, &size);
                Assert.Equal(Unsafe.SizeOf<UniformBufferObject>(), (int)size);
            }

            fixed (byte* ptr = fragBytes)
            {
                spirv = (SpvId*)ptr;
            }
            word_count = (uint)fragBytes.Length / 4;
            spvc_context_parse_spirv(context, spirv, word_count, &ir);
            spvc_context_create_compiler(context, spvc_backend.Glsl, ir, spvc_capture_mode.TakeOwnership, &compiler_glsl);

            // basic reflection
            spvc_compiler_create_shader_resources(compiler_glsl, &resources);
            spvc_resources_get_resource_list_for_type(resources, spvc_resource_type.UniformBuffer, (spvc_reflected_resource*)&list, &count);
            Assert.Equal(0u, count);
            spvc_resources_get_resource_list_for_type(resources, spvc_resource_type.SampledImage, (spvc_reflected_resource*)&list, &count);
            Assert.Equal(1u, count);
            for (uint i = 0; i < count; i++)
            {
                var id = list[i].id;
                var set = spvc_compiler_get_decoration(compiler_glsl, id, SpvDecoration.SpvDecorationDescriptorSet);
                uint binding = spvc_compiler_get_decoration(compiler_glsl, id, SpvDecoration.SpvDecorationBinding);
                uint offset = spvc_compiler_get_decoration(compiler_glsl, id, SpvDecoration.SpvDecorationOffset);
                spvc_type type = spvc_compiler_get_type_handle(compiler_glsl, list[i].type_id);

                Assert.Equal(1u, binding);

                nuint size = 0;
                spvc_compiler_get_declared_struct_size(compiler_glsl, type, &size);
                Assert.Equal(0, (int)size);
            }
        }

        [Fact]
        public unsafe void TestDescriptorSetLayoutInfo()
        {
            var vkContext = new VkContext();
            vkContext.Initialize("TestPipelineInfo", new Version32(0, 0, 1));

            var info = new DescriptorSetLayoutInfo(vkContext.vk, vkContext.device);
            var vertInfo = new ShaderInfo(vkContext.vk, ShaderStageFlags.VertexBit, "MafrixEngine.Shaders.triangle.vert.spv");
            foreach(var binding in vertInfo.layoutBindings)
            {
                info.AddBinding(0, binding);
            }
            Assert.Equal(1u, info.SetCount);
            //Assert.Single(info.GetLayoutBindings(0));

            var fragInfo = new ShaderInfo(vkContext.vk, ShaderStageFlags.FragmentBit, "MafrixEngine.Shaders.triangle.frag.spv");
            foreach(var binding in fragInfo.layoutBindings)
            {
                info.AddBinding(0, binding);
            }
            Assert.Equal(1u, info.SetCount);
            //Assert.Equal(2, info.GetLayoutBindings(0).Length);
        }

        [Fact]
        public unsafe void TestShaderInfo()
        {
            var vk = Vk.GetApi();
            var vertInfo = new ShaderInfo(vk, ShaderStageFlags.VertexBit, "MafrixEngine.Shaders.triangle.vert.spv");
            Assert.NotNull(vertInfo);
            Assert.Equal(1, vertInfo.BindingCount);

            var fragInfo = new ShaderInfo(vk, ShaderStageFlags.FragmentBit, "MafrixEngine.Shaders.triangle.frag.spv");
            Assert.NotNull(fragInfo);
            Assert.Equal(1, fragInfo.BindingCount);
        }

        [Fact]
        public unsafe void TestPipelineInfo()
        {
            var vkContext = new VkContext();
            vkContext.Initialize("TestPipelineInfo", new Version32(0,0,1));

            var vk = vkContext.vk;
            var shaderDefines = new ShaderDefine[2];
            shaderDefines[0] = new ShaderDefine(vertShader, ShaderStageFlags.VertexBit, false);
            shaderDefines[1] = new ShaderDefine(fragShader, ShaderStageFlags.FragmentBit, false);

            var pipelineInfo = new PipelineInfo(vk, vkContext.device, shaderDefines);
            Assert.NotNull(pipelineInfo);
            //Assert.Equal(2, pipelineInfo.setLayoutBindings.Length);
        }
    }
}
