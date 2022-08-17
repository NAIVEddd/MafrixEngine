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
using SPIRVCross;
using static SPIRVCross.SPIRV;

namespace MafrixEngine.GraphicsWrapper
{
    public class DescriptorSetLayoutInfo
    {
        private Vk vk;
        private Dictionary<ValueTuple<uint, uint>, DescriptorSetLayoutBinding> keyValueBindings;
        private DescriptorSetLayoutBinding[][] layoutBindings;
        private uint setCount;
        public uint SetCount => setCount;

        public DescriptorSetLayoutInfo(Vk vk)
        {
            this.vk = vk;
            keyValueBindings = new Dictionary<ValueTuple<uint, uint>, DescriptorSetLayoutBinding>();
            setCount = 0;
        }

        public void AddBinding(uint set, DescriptorSetLayoutBinding binding)
        {
            if(set + 1 > setCount)
            {
                setCount = set + 1;
            }
            var bind = binding.Binding;
            keyValueBindings.Add((set, bind), binding);
        }

        public DescriptorSetLayoutBinding[] GetLayoutBindings(uint set)
        {
            var bindings = new List<DescriptorSetLayoutBinding>();
            foreach(var binding in keyValueBindings.Keys)
            {
                if(binding.Item1 == set)
                {
                    bindings.Add(keyValueBindings[binding]);
                }
            }
            return bindings.ToArray();
        }
    }

    public class ShaderInfo
    {
        private Vk vk;
        private ShaderStageFlags stageFlags;
        public byte[] shaderCode;
        public int BindingCount 
        {
            get { return layoutBindings.Length; }
        }
        public DescriptorSetLayoutBinding[] layoutBindings;

        public ShaderInfo(Vk vk, ShaderStageFlags stageFlags, string shaderName, bool isManifestResource = true)
        {
            this.vk = vk;
            this.stageFlags = stageFlags;

            if(isManifestResource)
            {
                shaderCode = LoadEmbeddedResourceBytes(shaderName);
            }
            else
            {
                shaderCode = System.IO.File.ReadAllBytes(shaderName);
            }
            ParseInfo(shaderCode);
        }

        private unsafe void ParseInfo(byte[] shaderBytes)
        {
            SpvId* spirv;
            fixed (byte* ptr = shaderBytes)
            {
                spirv = (SpvId*)ptr;
            }
            uint word_count = (uint)shaderBytes.Length / 4;

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

            var bindingList = new DescriptorSetLayoutBinding[2][];
            bindingList[0] = GetResourceList(compiler_glsl, resources, spvc_resource_type.UniformBuffer, out list, out count);
            bindingList[1] = GetResourceList(compiler_glsl, resources, spvc_resource_type.SampledImage, out list, out count);

            AggregateBindings(bindingList, out layoutBindings);

            spvc_context_destroy(context);
        }

        public static void AggregateBindings(DescriptorSetLayoutBinding[][] bindingList, out DescriptorSetLayoutBinding[] layoutBindings)
        {
            var count = 0;
            foreach(var binding in bindingList)
            {
                count += binding.Length;
            }
            layoutBindings = new DescriptorSetLayoutBinding[count];

            var copydest = new Memory<DescriptorSetLayoutBinding>(layoutBindings);
            var offset = 0;
            foreach(var binding in bindingList)
            {
                var copySrc = new Memory<DescriptorSetLayoutBinding>(binding);
                copySrc.CopyTo(copydest.Slice(offset));
                offset += binding.Length;
            }
        }

        /// <summary>
        ///     parse resources to DescriptorSetBinding[]
        /// </summary>
        /// <param name="compiler_glsl"></param>
        /// <param name="resources">
        ///     struct ShaderResources {
        ///      SmallVector<Resource> uniform_buffers;
        ///      SmallVector<Resource> storage_buffers;
        ///      SmallVector<Resource> stage_inputs;
        ///      SmallVector<Resource> stage_outputs;
        ///      SmallVector<Resource> subpass_inputs;
        ///      SmallVector<Resource> storage_images;
        ///      SmallVector<Resource> sampled_images;
        ///      SmallVector<Resource> atomic_counters;
        ///      SmallVector<Resource> acceleration_structures;
        ///      // There can only be one push constant block,
        ///      // but keep the vector in case this restriction is lifted in the future.
        ///      SmallVector<Resource> push_constant_buffers;
        ///      // For Vulkan GLSL and HLSL source,
        ///      // these correspond to separate texture2D and samplers respectively.
        ///      SmallVector<Resource> separate_images;
        ///      SmallVector<Resource> separate_samplers;
        ///      SmallVector<BuiltInResource> builtin_inputs;
        ///      SmallVector<BuiltInResource> builtin_outputs;
        ///  };
        /// </param>
        /// <param name="resource_Type"></param>
        /// <param name="list"></param>
        /// <param name="count"></param>
        /// <returns type="DescriptorSetLayoutBinding[]"></returns>
        private unsafe DescriptorSetLayoutBinding[] GetResourceList(spvc_compiler compiler_glsl,
            spvc_resources resources, spvc_resource_type resource_Type,
            out spvc_reflected_resource* list, out nuint count)
        {
            var descriptorType = DescriptorTypeCast(resource_Type);

            spvc_reflected_resource* tmpList = default;
            nuint tmpCount = default;
            spvc_compiler_create_shader_resources(compiler_glsl, &resources);
            spvc_resources_get_resource_list_for_type(resources, resource_Type, (spvc_reflected_resource*)&tmpList, &tmpCount);
            list = tmpList;
            count = tmpCount;

            var bindings = new DescriptorSetLayoutBinding[count];
            for (uint i = 0; i < count; i++)
            {
                var id = list[i].id;
                var set = spvc_compiler_get_decoration(compiler_glsl, id, SpvDecoration.SpvDecorationDescriptorSet);
                uint binding = spvc_compiler_get_decoration(compiler_glsl, id, SpvDecoration.SpvDecorationBinding);
                uint offset = spvc_compiler_get_decoration(compiler_glsl, id, SpvDecoration.SpvDecorationOffset);
                spvc_type type = spvc_compiler_get_type_handle(compiler_glsl, list[i].type_id);

                nuint size = 0;
                spvc_compiler_get_declared_struct_size(compiler_glsl, type, &size);

                var layoutBinding = new DescriptorSetLayoutBinding();
                layoutBinding.Binding = binding;
                layoutBinding.DescriptorType = descriptorType;
                layoutBinding.DescriptorCount = 1;
                layoutBinding.StageFlags = stageFlags;
                layoutBinding.PImmutableSamplers = null;
                bindings[i] = layoutBinding;
            }
            return bindings;
        }

        private DescriptorType DescriptorTypeCast(spvc_resource_type resource_Type)
        {
            DescriptorType res = default;
            switch (resource_Type)
            {
                case spvc_resource_type.UniformBuffer:
                    res = DescriptorType.UniformBuffer;
                    break;
                case spvc_resource_type.StorageBuffer:
                    res = DescriptorType.StorageBuffer;
                    break;
                case spvc_resource_type.StorageImage:
                    res = DescriptorType.StorageImage;
                    break;
                case spvc_resource_type.SampledImage:
                    res = DescriptorType.CombinedImageSampler;
                    break;
                default:
                    throw new NotSupportedException("not supported spvc_resource_type:" + resource_Type.ToString());
            }

            return res;
        }

        internal static byte[] LoadEmbeddedResourceBytes(string path)
        {
            using (var s = typeof(ShaderInfo).Assembly.GetManifestResourceStream(path))
            {
                using (var ms = new MemoryStream())
                {
                    s!.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }
    }

    public struct ShaderDefine
    {
        public string name;
        public ShaderStageFlags shaderStage;
        public bool isManifestResource;
        public ShaderDefine(string n, ShaderStageFlags stageFlags, bool isManifest = true)
        {
            name = n;
            shaderStage = stageFlags;
            isManifestResource = isManifest;
        }
    }

    public class PipelineInfo
    {
        private Vk vk;
        private Device device;
        public DescriptorSetLayoutInfo setLayoutInfo;
        public DescriptorSetLayoutBinding[] setLayoutBindings;
        public PipelineShaderStageCreateInfo[] pipelineShaderStageCreateInfos;

        public PipelineInfo(Vk vk, Device device, ShaderDefine[] shaderDefines)
        {
            this.vk = vk;
            this.device = device;
            setLayoutInfo = new DescriptorSetLayoutInfo(vk);

            var shaderInfos = new ShaderInfo[shaderDefines.Length];
            var layoutBindings = new DescriptorSetLayoutBinding[shaderDefines.Length][];
            pipelineShaderStageCreateInfos = new PipelineShaderStageCreateInfo[shaderDefines.Length];
            for (var i = 0; i < shaderDefines.Length; i++)
            {
                shaderInfos[i] = new ShaderInfo(vk, shaderDefines[i].shaderStage,
                    shaderDefines[i].name, shaderDefines[i].isManifestResource);
                layoutBindings[i] = shaderInfos[i].layoutBindings;
                foreach (var binding in shaderInfos[i].layoutBindings)
                {
                    setLayoutInfo.AddBinding(0, binding); 
                }

                pipelineShaderStageCreateInfos[i] = ToStageCreateInfo(shaderInfos[i].shaderCode, shaderDefines[i]);
            }

            ShaderInfo.AggregateBindings(layoutBindings, out setLayoutBindings);
        }

        private unsafe PipelineShaderStageCreateInfo ToStageCreateInfo(byte[] code, ShaderDefine shaderDefine)
        {
            var stageCreateInfo = new PipelineShaderStageCreateInfo(StructureType.PipelineShaderStageCreateInfo);
            stageCreateInfo.Stage = shaderDefine.shaderStage;
            stageCreateInfo.Module = CreateShaderModule(code);
            stageCreateInfo.PName = (byte*)SilkMarshal.StringToPtr("main");
            return stageCreateInfo;
        }

        private unsafe ShaderModule CreateShaderModule(byte[] code)
        {
            var createInfo = new ShaderModuleCreateInfo(StructureType.ShaderModuleCreateInfo);
            createInfo.CodeSize = (nuint)code.Length;
            fixed (byte* ptr = code)
            {
                createInfo.PCode = (uint*)ptr;
            }
            var shaderModule = new ShaderModule();
            if (vk.CreateShaderModule(device, in createInfo, null, out shaderModule) != Result.Success)
            {
                throw new Exception("failed to create shader module.");
            }
            return shaderModule;
        }

        unsafe ~PipelineInfo()
        {
            foreach(var module in pipelineShaderStageCreateInfos)
            {
                vk.DestroyShaderModule(device, module.Module, null);
            }
        }
    }
}
