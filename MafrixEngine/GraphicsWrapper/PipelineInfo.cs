using System;
using System.IO;
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
using System.Security.AccessControl;
using ThirdPartyLib;
using System.Diagnostics;
using File = System.IO.File;

namespace MafrixEngine.GraphicsWrapper
{
    public class ShaderIncludedInfo
    {
        private SpvReflectShaderModule module;
        public byte[] ShaderBytes;
        private ShaderStageFlags shaderStage;
        private uint bindingCount;
        private uint setCount;
        public List<ValueTuple<uint, DescriptorSetLayoutBinding>> bindings;
        public unsafe ShaderIncludedInfo(string shaderName)
        {
            bindings = new List<ValueTuple<uint, DescriptorSetLayoutBinding>>();

            ShaderBytes = File.ReadAllBytes(shaderName);
            var modules = stackalloc SpvReflectShaderModule[1];
            fixed (void* vertPtr = ShaderBytes)
            {
                SpirvReflect.spvReflectCreateShaderModule((nuint)ShaderBytes.Length, vertPtr, modules);
            }
            module = modules[0];
            shaderStage = FromSpvStage(module.shader_stage);
            bindingCount = module.descriptor_binding_count;
            setCount = module.descriptor_set_count;
            for (uint i = 0; i < bindingCount; i++)
            {
                bindings.Add(GetSetLayoutBinding(i));
            }
        }
        public unsafe ShaderIncludedInfo(byte[] bytes)
        {
            bindings = new List<ValueTuple<uint, DescriptorSetLayoutBinding>>();

            ShaderBytes = bytes;
            var modules = stackalloc SpvReflectShaderModule[1];
            fixed (void* vertPtr = ShaderBytes)
            {
                SpirvReflect.spvReflectCreateShaderModule((nuint)ShaderBytes.Length, vertPtr, modules);
            }
            module = modules[0];
            shaderStage = FromSpvStage(module.shader_stage);
            bindingCount = module.descriptor_binding_count;
            setCount = module.descriptor_set_count;
            for (uint i = 0; i < bindingCount; i++)
            {
                bindings.Add(GetSetLayoutBinding(i));
            }
        }
        private ShaderStageFlags FromSpvStage(SpvReflectShaderStageFlagBits flagBits)
        {
            ShaderStageFlags flag = ShaderStageFlags.None;
            switch (flagBits)
            {
                case SpvReflectShaderStageFlagBits.SPV_REFLECT_SHADER_STAGE_VERTEX_BIT:
                    flag = ShaderStageFlags.VertexBit;
                    break;
                case SpvReflectShaderStageFlagBits.SPV_REFLECT_SHADER_STAGE_TESSELLATION_CONTROL_BIT:
                    flag = ShaderStageFlags.TessellationControlBit;
                    break;
                case SpvReflectShaderStageFlagBits.SPV_REFLECT_SHADER_STAGE_TESSELLATION_EVALUATION_BIT:
                    flag = ShaderStageFlags.TessellationEvaluationBit;
                    break;
                case SpvReflectShaderStageFlagBits.SPV_REFLECT_SHADER_STAGE_GEOMETRY_BIT:
                    flag = ShaderStageFlags.GeometryBit;
                    break;
                case SpvReflectShaderStageFlagBits.SPV_REFLECT_SHADER_STAGE_FRAGMENT_BIT:
                    flag = ShaderStageFlags.FragmentBit;
                    break;
                case SpvReflectShaderStageFlagBits.SPV_REFLECT_SHADER_STAGE_COMPUTE_BIT:
                    flag = ShaderStageFlags.ComputeBit;
                    break;
                case SpvReflectShaderStageFlagBits.SPV_REFLECT_SHADER_STAGE_TASK_BIT_NV:
                    flag = ShaderStageFlags.TaskBitNV;
                    break;
                case SpvReflectShaderStageFlagBits.SPV_REFLECT_SHADER_STAGE_MESH_BIT_NV:
                    flag = ShaderStageFlags.MeshBitNV;
                    break;
                case SpvReflectShaderStageFlagBits.SPV_REFLECT_SHADER_STAGE_RAYGEN_BIT_KHR:
                    flag = ShaderStageFlags.RaygenBitKhr;
                    break;
                case SpvReflectShaderStageFlagBits.SPV_REFLECT_SHADER_STAGE_ANY_HIT_BIT_KHR:
                    flag = ShaderStageFlags.AnyHitBitKhr;
                    break;
                case SpvReflectShaderStageFlagBits.SPV_REFLECT_SHADER_STAGE_CLOSEST_HIT_BIT_KHR:
                    flag = ShaderStageFlags.ClosestHitBitKhr;
                    break;
                case SpvReflectShaderStageFlagBits.SPV_REFLECT_SHADER_STAGE_MISS_BIT_KHR:
                    flag = ShaderStageFlags.MissBitKhr;
                    break;
                case SpvReflectShaderStageFlagBits.SPV_REFLECT_SHADER_STAGE_INTERSECTION_BIT_KHR:
                    flag = ShaderStageFlags.IntersectionBitKhr;
                    break;
                case SpvReflectShaderStageFlagBits.SPV_REFLECT_SHADER_STAGE_CALLABLE_BIT_KHR:
                    flag = ShaderStageFlags.CallableBitKhr;
                    break;
                default:
                    break;
            }
            return flag;
        }

        private DescriptorType FromSpvDescriptorType(SpvReflectDescriptorType descriptorType)
        {
            DescriptorType descType;
            switch (descriptorType)
            {
                case SpvReflectDescriptorType.SPV_REFLECT_DESCRIPTOR_TYPE_SAMPLER:
                    descType = DescriptorType.Sampler;
                    break;
                case SpvReflectDescriptorType.SPV_REFLECT_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER:
                    descType = DescriptorType.CombinedImageSampler;
                    break;
                case SpvReflectDescriptorType.SPV_REFLECT_DESCRIPTOR_TYPE_SAMPLED_IMAGE:
                    descType = DescriptorType.SampledImage;
                    break;
                case SpvReflectDescriptorType.SPV_REFLECT_DESCRIPTOR_TYPE_STORAGE_IMAGE:
                    descType = DescriptorType.StorageImage;
                    break;
                case SpvReflectDescriptorType.SPV_REFLECT_DESCRIPTOR_TYPE_UNIFORM_TEXEL_BUFFER:
                    descType = DescriptorType.UniformTexelBuffer;
                    break;
                case SpvReflectDescriptorType.SPV_REFLECT_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER:
                    descType = DescriptorType.StorageTexelBuffer;
                    break;
                case SpvReflectDescriptorType.SPV_REFLECT_DESCRIPTOR_TYPE_UNIFORM_BUFFER:
                    descType = DescriptorType.UniformBuffer;
                    break;
                case SpvReflectDescriptorType.SPV_REFLECT_DESCRIPTOR_TYPE_STORAGE_BUFFER:
                    descType = DescriptorType.StorageBuffer;
                    break;
                case SpvReflectDescriptorType.SPV_REFLECT_DESCRIPTOR_TYPE_UNIFORM_BUFFER_DYNAMIC:
                    descType = DescriptorType.UniformBufferDynamic;
                    break;
                case SpvReflectDescriptorType.SPV_REFLECT_DESCRIPTOR_TYPE_STORAGE_BUFFER_DYNAMIC:
                    descType = DescriptorType.StorageBufferDynamic;
                    break;
                case SpvReflectDescriptorType.SPV_REFLECT_DESCRIPTOR_TYPE_INPUT_ATTACHMENT:
                    descType = DescriptorType.InputAttachment;
                    break;
                case SpvReflectDescriptorType.SPV_REFLECT_DESCRIPTOR_TYPE_ACCELERATION_STRUCTURE_KHR:
                    descType = DescriptorType.AccelerationStructureKhr;
                    break;
                default:
                    descType = (DescriptorType)(-1);
                    break;
            }
            return descType;
        }

        private unsafe ValueTuple<uint, DescriptorSetLayoutBinding> GetSetLayoutBinding(uint idx)
        {
            var binding = module.descriptor_bindings[idx];
            var vkBinding = new DescriptorSetLayoutBinding();
            vkBinding.Binding = binding.binding;
            vkBinding.DescriptorType = FromSpvDescriptorType(binding.descriptor_type);
            vkBinding.DescriptorCount = binding.count;
            vkBinding.StageFlags = shaderStage;
            return (binding.set, vkBinding);
        }

        private string BindingToString(DescriptorSetLayoutBinding binding)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Binding : " + binding.Binding.ToString());
            sb.AppendLine("Binding count: " + binding.DescriptorCount.ToString());
            sb.AppendLine("Binding type: " + binding.DescriptorType.ToString());
            return sb.ToString();
        }
        public override string ToString()
        {
            var desc = new StringBuilder();
            desc.Append("Shader Stage is: " + shaderStage.ToString());
            desc.Append("; Set count: " + setCount.ToString() + ", binding count: " + bindingCount.ToString());
            foreach (var (set, binding) in bindings)
            {
                desc.Append(BindingToString(binding));
            }
            return desc.ToString();
        }
    }

    public class SetLayoutInfo
    {
        private List<List<DescriptorSetLayoutBinding>> setBindings;
        private Dictionary<DescriptorType, uint> typeCount;
        private DescriptorSetLayout[] setLayouts;
        private PipelineLayout pipelineLayout;
        private DescriptorPool[] pools;
        public DescriptorPool[] GetDescriptorPools { get { return pools; } }
        public PipelineLayout GetPipelineLayout { get { return pipelineLayout; } }
        public DescriptorSetLayout[] GetDescriptorSetLayout { get { return setLayouts; } }

#if DEBUG
        public DescriptorSetLayout SetLayout { get { return setLayouts[0]; } }
#endif
        public SetLayoutInfo()
        {
            setBindings = new List<List<DescriptorSetLayoutBinding>>();
            typeCount = new Dictionary<DescriptorType, uint>();
        }
        public void Add(ShaderIncludedInfo shader)
        {
            foreach (var (set, binding) in shader.bindings)
            {
                while (set >= setBindings.Count)
                {
                    setBindings.Add(new List<DescriptorSetLayoutBinding>());
                }
                setBindings[(int)set].Add(binding);
                if (typeCount.TryGetValue(binding.DescriptorType, out var c))
                {
                    typeCount[binding.DescriptorType] = c + binding.DescriptorCount;
                }
                else
                {
                    typeCount[binding.DescriptorType] = binding.DescriptorCount;
                }
            }
        }
        public unsafe void Build(VkContext vkCtx)
        {
            // build DescriptorSetLayout
            setLayouts = new DescriptorSetLayout[setBindings.Count];
            for (var i = 0; i < setBindings.Count; i++)
            {
                var bindings = setBindings[i].ToArray();
                fixed (DescriptorSetLayoutBinding* bPtr = bindings)
                {
                    var slCreateInfo = new DescriptorSetLayoutCreateInfo();
                    slCreateInfo.SType = StructureType.DescriptorSetLayoutCreateInfo;
                    slCreateInfo.BindingCount = (uint)bindings.Length;
                    slCreateInfo.PBindings = bPtr;
                    vkCtx.vk.CreateDescriptorSetLayout(vkCtx.device, in slCreateInfo, null, out var layout);
                    setLayouts[i] = layout;
                }
            }

            // build PipelineLayout
            fixed (DescriptorSetLayout* slPtr = setLayouts)
            {
                var createInfo = new PipelineLayoutCreateInfo();
                createInfo.SType = StructureType.PipelineLayoutCreateInfo;
                createInfo.SetLayoutCount = (uint)setLayouts.Length;
                createInfo.PSetLayouts = slPtr;
                vkCtx.vk.CreatePipelineLayout(vkCtx.device, in createInfo, null, out pipelineLayout);
            }
        }
#if DEBUG
        public DescriptorPoolSize[] PoolSizes;
#endif
        public unsafe DescriptorPool BuildDescriptorPool(VkContext vkCtx)
        {
            // build DescriptorPools
            var pool = new DescriptorPool();
            var poolSizes = new DescriptorPoolSize[typeCount.Count];
            for (var i = 0; i < poolSizes.Length; i++)
            {
                var (t, c) = typeCount.ElementAt(i);
                poolSizes[i].DescriptorCount = c;
                poolSizes[i].Type = t;
            }
            var dpCreateInfo = new DescriptorPoolCreateInfo();
            dpCreateInfo.SType = StructureType.DescriptorPoolCreateInfo;
            dpCreateInfo.MaxSets = (uint)setLayouts.Length;
            dpCreateInfo.PoolSizeCount = (uint)poolSizes.Length;
            fixed (DescriptorPoolSize* psPtr = poolSizes)
            {
                dpCreateInfo.PPoolSizes = psPtr;
                vkCtx.vk.CreateDescriptorPool(vkCtx.device, in dpCreateInfo, null, out pool);
            }

            PoolSizes = poolSizes;

            return pool;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            BindingsToString(sb);
            TypeCountToString(sb);
            return sb.ToString();

            void TypeCountToString(StringBuilder sb)
            {
                foreach (var (t, c) in typeCount)
                {
                    sb.AppendLine("Type '" + t.ToString() + "' has " + c.ToString());
                }
            }
            void BindingsToString(StringBuilder sb)
            {
                for (var idx = 0; idx < setBindings.Count; idx++)
                {
                    sb.AppendLine("Set index: " + idx.ToString());
                    foreach (var binding in setBindings[idx])
                    {
                        BindingToString(sb, binding);
                    }
                }
            }
            void BindingToString(StringBuilder sb, DescriptorSetLayoutBinding binding)
            {
                sb.AppendLine("  Binding : " + binding.Binding.ToString());
                sb.AppendLine("  Binding count: " + binding.DescriptorCount.ToString());
                sb.AppendLine("  Binding type: " + binding.DescriptorType.ToString());
            }
        }
    }

    public struct ShaderDefine
    {
        public string name;
        public ShaderStageFlags shaderStage;
        public bool isManifestResource;
        public ShaderDefine(string n, ShaderStageFlags stageFlags, bool isManifest = false)
        {
            name = n;
            shaderStage = stageFlags;
            isManifestResource = isManifest;
        }

        public byte[] Load()
        {
            if (isManifestResource)
            {
                return LoadEmbeddedResourceBytes(name);
            }
            else
            {
                return File.ReadAllBytes(name);
            }
        }
        internal static byte[] LoadEmbeddedResourceBytes(string path)
        {
            using (var s = typeof(ShaderDefine).Assembly.GetManifestResourceStream(path))
            {
                using (var ms = new MemoryStream())
                {
                    s!.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }
    }

    public class PipelineInfo
    {
        private Vk vk;
        private Device device;
        public SetLayoutInfo setLayoutInfo;
        public PipelineLayout Layout;
        public PipelineShaderStageCreateInfo[] pipelineShaderStageCreateInfos;

        public PipelineInfo(VkContext vkCtx, ShaderDefine[] shaderDefines)
        {
            this.vk = vkCtx.vk;
            this.device = vkCtx.device;

            var info = new SetLayoutInfo();

            pipelineShaderStageCreateInfos = new PipelineShaderStageCreateInfo[shaderDefines.Length];
            for (var i = 0; i < shaderDefines.Length; i++)
            {
                var code = shaderDefines[i].Load();

                // new version
                var spvShaderInfo = new ShaderIncludedInfo(code);
                info.Add(spvShaderInfo);
             
                pipelineShaderStageCreateInfos[i] = ToStageCreateInfo(code, shaderDefines[i]);
            }
            info.Build(vkCtx);
            info.BuildDescriptorPool(vkCtx);
            setLayoutInfo = info;
            Layout = info.GetPipelineLayout;
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
