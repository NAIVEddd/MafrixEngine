using System;
using System.Runtime.InteropServices;

/// <summary>
/// Generated code from `SpirvReflect`,
///     source is https://github.com/KhronosGroup/SPIRV-Reflect.git
///     generation tool is https://github.com/dotnet/ClangSharp.git
/// </summary>
/// 
/// <example>
///   ClangSharpPInvokeGenerator.exe 
///	    -F ./
///	    -f .\SpirvReflect\spirv_reflect.h
///	       .\include\spirv\unified1\spirv.h
///	    -x c
///	    -n ThirdPartyLib
///	    -m SpirvReflect
///	    -o ./ClangSharpGen/SpirvReflectSharp.cs
/// </example>
/// 
namespace ThirdPartyLib
{
    public enum SpvReflectResult
    {
        SPV_REFLECT_RESULT_SUCCESS,
        SPV_REFLECT_RESULT_NOT_READY,
        SPV_REFLECT_RESULT_ERROR_PARSE_FAILED,
        SPV_REFLECT_RESULT_ERROR_ALLOC_FAILED,
        SPV_REFLECT_RESULT_ERROR_RANGE_EXCEEDED,
        SPV_REFLECT_RESULT_ERROR_NULL_POINTER,
        SPV_REFLECT_RESULT_ERROR_INTERNAL_ERROR,
        SPV_REFLECT_RESULT_ERROR_COUNT_MISMATCH,
        SPV_REFLECT_RESULT_ERROR_ELEMENT_NOT_FOUND,
        SPV_REFLECT_RESULT_ERROR_SPIRV_INVALID_CODE_SIZE,
        SPV_REFLECT_RESULT_ERROR_SPIRV_INVALID_MAGIC_NUMBER,
        SPV_REFLECT_RESULT_ERROR_SPIRV_UNEXPECTED_EOF,
        SPV_REFLECT_RESULT_ERROR_SPIRV_INVALID_ID_REFERENCE,
        SPV_REFLECT_RESULT_ERROR_SPIRV_SET_NUMBER_OVERFLOW,
        SPV_REFLECT_RESULT_ERROR_SPIRV_INVALID_STORAGE_CLASS,
        SPV_REFLECT_RESULT_ERROR_SPIRV_RECURSION,
        SPV_REFLECT_RESULT_ERROR_SPIRV_INVALID_INSTRUCTION,
        SPV_REFLECT_RESULT_ERROR_SPIRV_UNEXPECTED_BLOCK_DATA,
        SPV_REFLECT_RESULT_ERROR_SPIRV_INVALID_BLOCK_MEMBER_REFERENCE,
        SPV_REFLECT_RESULT_ERROR_SPIRV_INVALID_ENTRY_POINT,
        SPV_REFLECT_RESULT_ERROR_SPIRV_INVALID_EXECUTION_MODE,
    }

    public enum SpvReflectModuleFlagBits
    {
        SPV_REFLECT_MODULE_FLAG_NONE = 0x00000000,
        SPV_REFLECT_MODULE_FLAG_NO_COPY = 0x00000001,
    }

    public enum SpvReflectTypeFlagBits
    {
        SPV_REFLECT_TYPE_FLAG_UNDEFINED = 0x00000000,
        SPV_REFLECT_TYPE_FLAG_VOID = 0x00000001,
        SPV_REFLECT_TYPE_FLAG_BOOL = 0x00000002,
        SPV_REFLECT_TYPE_FLAG_INT = 0x00000004,
        SPV_REFLECT_TYPE_FLAG_FLOAT = 0x00000008,
        SPV_REFLECT_TYPE_FLAG_VECTOR = 0x00000100,
        SPV_REFLECT_TYPE_FLAG_MATRIX = 0x00000200,
        SPV_REFLECT_TYPE_FLAG_EXTERNAL_IMAGE = 0x00010000,
        SPV_REFLECT_TYPE_FLAG_EXTERNAL_SAMPLER = 0x00020000,
        SPV_REFLECT_TYPE_FLAG_EXTERNAL_SAMPLED_IMAGE = 0x00040000,
        SPV_REFLECT_TYPE_FLAG_EXTERNAL_BLOCK = 0x00080000,
        SPV_REFLECT_TYPE_FLAG_EXTERNAL_ACCELERATION_STRUCTURE = 0x00100000,
        SPV_REFLECT_TYPE_FLAG_EXTERNAL_MASK = 0x00FF0000,
        SPV_REFLECT_TYPE_FLAG_STRUCT = 0x10000000,
        SPV_REFLECT_TYPE_FLAG_ARRAY = 0x20000000,
    }

    public enum SpvReflectDecorationFlagBits
    {
        SPV_REFLECT_DECORATION_NONE = 0x00000000,
        SPV_REFLECT_DECORATION_BLOCK = 0x00000001,
        SPV_REFLECT_DECORATION_BUFFER_BLOCK = 0x00000002,
        SPV_REFLECT_DECORATION_ROW_MAJOR = 0x00000004,
        SPV_REFLECT_DECORATION_COLUMN_MAJOR = 0x00000008,
        SPV_REFLECT_DECORATION_BUILT_IN = 0x00000010,
        SPV_REFLECT_DECORATION_NOPERSPECTIVE = 0x00000020,
        SPV_REFLECT_DECORATION_FLAT = 0x00000040,
        SPV_REFLECT_DECORATION_NON_WRITABLE = 0x00000080,
        SPV_REFLECT_DECORATION_RELAXED_PRECISION = 0x00000100,
        SPV_REFLECT_DECORATION_NON_READABLE = 0x00000200,
    }

    public enum SpvReflectResourceType
    {
        SPV_REFLECT_RESOURCE_FLAG_UNDEFINED = 0x00000000,
        SPV_REFLECT_RESOURCE_FLAG_SAMPLER = 0x00000001,
        SPV_REFLECT_RESOURCE_FLAG_CBV = 0x00000002,
        SPV_REFLECT_RESOURCE_FLAG_SRV = 0x00000004,
        SPV_REFLECT_RESOURCE_FLAG_UAV = 0x00000008,
    }

    public enum SpvReflectFormat
    {
        SPV_REFLECT_FORMAT_UNDEFINED = 0,
        SPV_REFLECT_FORMAT_R32_UINT = 98,
        SPV_REFLECT_FORMAT_R32_SINT = 99,
        SPV_REFLECT_FORMAT_R32_SFLOAT = 100,
        SPV_REFLECT_FORMAT_R32G32_UINT = 101,
        SPV_REFLECT_FORMAT_R32G32_SINT = 102,
        SPV_REFLECT_FORMAT_R32G32_SFLOAT = 103,
        SPV_REFLECT_FORMAT_R32G32B32_UINT = 104,
        SPV_REFLECT_FORMAT_R32G32B32_SINT = 105,
        SPV_REFLECT_FORMAT_R32G32B32_SFLOAT = 106,
        SPV_REFLECT_FORMAT_R32G32B32A32_UINT = 107,
        SPV_REFLECT_FORMAT_R32G32B32A32_SINT = 108,
        SPV_REFLECT_FORMAT_R32G32B32A32_SFLOAT = 109,
        SPV_REFLECT_FORMAT_R64_UINT = 110,
        SPV_REFLECT_FORMAT_R64_SINT = 111,
        SPV_REFLECT_FORMAT_R64_SFLOAT = 112,
        SPV_REFLECT_FORMAT_R64G64_UINT = 113,
        SPV_REFLECT_FORMAT_R64G64_SINT = 114,
        SPV_REFLECT_FORMAT_R64G64_SFLOAT = 115,
        SPV_REFLECT_FORMAT_R64G64B64_UINT = 116,
        SPV_REFLECT_FORMAT_R64G64B64_SINT = 117,
        SPV_REFLECT_FORMAT_R64G64B64_SFLOAT = 118,
        SPV_REFLECT_FORMAT_R64G64B64A64_UINT = 119,
        SPV_REFLECT_FORMAT_R64G64B64A64_SINT = 120,
        SPV_REFLECT_FORMAT_R64G64B64A64_SFLOAT = 121,
    }

    public enum SpvReflectVariableFlagBits
    {
        SPV_REFLECT_VARIABLE_FLAGS_NONE = 0x00000000,
        SPV_REFLECT_VARIABLE_FLAGS_UNUSED = 0x00000001,
    }

    public enum SpvReflectDescriptorType
    {
        SPV_REFLECT_DESCRIPTOR_TYPE_SAMPLER = 0,
        SPV_REFLECT_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER = 1,
        SPV_REFLECT_DESCRIPTOR_TYPE_SAMPLED_IMAGE = 2,
        SPV_REFLECT_DESCRIPTOR_TYPE_STORAGE_IMAGE = 3,
        SPV_REFLECT_DESCRIPTOR_TYPE_UNIFORM_TEXEL_BUFFER = 4,
        SPV_REFLECT_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER = 5,
        SPV_REFLECT_DESCRIPTOR_TYPE_UNIFORM_BUFFER = 6,
        SPV_REFLECT_DESCRIPTOR_TYPE_STORAGE_BUFFER = 7,
        SPV_REFLECT_DESCRIPTOR_TYPE_UNIFORM_BUFFER_DYNAMIC = 8,
        SPV_REFLECT_DESCRIPTOR_TYPE_STORAGE_BUFFER_DYNAMIC = 9,
        SPV_REFLECT_DESCRIPTOR_TYPE_INPUT_ATTACHMENT = 10,
        SPV_REFLECT_DESCRIPTOR_TYPE_ACCELERATION_STRUCTURE_KHR = 1000150000,
    }

    public enum SpvReflectShaderStageFlagBits
    {
        SPV_REFLECT_SHADER_STAGE_VERTEX_BIT = 0x00000001,
        SPV_REFLECT_SHADER_STAGE_TESSELLATION_CONTROL_BIT = 0x00000002,
        SPV_REFLECT_SHADER_STAGE_TESSELLATION_EVALUATION_BIT = 0x00000004,
        SPV_REFLECT_SHADER_STAGE_GEOMETRY_BIT = 0x00000008,
        SPV_REFLECT_SHADER_STAGE_FRAGMENT_BIT = 0x00000010,
        SPV_REFLECT_SHADER_STAGE_COMPUTE_BIT = 0x00000020,
        SPV_REFLECT_SHADER_STAGE_TASK_BIT_NV = 0x00000040,
        SPV_REFLECT_SHADER_STAGE_MESH_BIT_NV = 0x00000080,
        SPV_REFLECT_SHADER_STAGE_RAYGEN_BIT_KHR = 0x00000100,
        SPV_REFLECT_SHADER_STAGE_ANY_HIT_BIT_KHR = 0x00000200,
        SPV_REFLECT_SHADER_STAGE_CLOSEST_HIT_BIT_KHR = 0x00000400,
        SPV_REFLECT_SHADER_STAGE_MISS_BIT_KHR = 0x00000800,
        SPV_REFLECT_SHADER_STAGE_INTERSECTION_BIT_KHR = 0x00001000,
        SPV_REFLECT_SHADER_STAGE_CALLABLE_BIT_KHR = 0x00002000,
    }

    public enum SpvReflectGenerator
    {
        SPV_REFLECT_GENERATOR_KHRONOS_LLVM_SPIRV_TRANSLATOR = 6,
        SPV_REFLECT_GENERATOR_KHRONOS_SPIRV_TOOLS_ASSEMBLER = 7,
        SPV_REFLECT_GENERATOR_KHRONOS_GLSLANG_REFERENCE_FRONT_END = 8,
        SPV_REFLECT_GENERATOR_GOOGLE_SHADERC_OVER_GLSLANG = 13,
        SPV_REFLECT_GENERATOR_GOOGLE_SPIREGG = 14,
        SPV_REFLECT_GENERATOR_GOOGLE_RSPIRV = 15,
        SPV_REFLECT_GENERATOR_X_LEGEND_MESA_MESAIR_SPIRV_TRANSLATOR = 16,
        SPV_REFLECT_GENERATOR_KHRONOS_SPIRV_TOOLS_LINKER = 17,
        SPV_REFLECT_GENERATOR_WINE_VKD3D_SHADER_COMPILER = 18,
        SPV_REFLECT_GENERATOR_CLAY_CLAY_SHADER_COMPILER = 19,
    }

    public partial struct SpvReflectNumericTraits
    {
        public Scalar scalar;
        public Vector vector;
        public Matrix matrix;
        public partial struct Scalar
        {
            public uint width;
            public uint signedness;
        }

        public partial struct Vector
        {
            public uint component_count;
        }

        public partial struct Matrix
        {
            public uint column_count;
            public uint row_count;
            public uint stride;
        }
    }

    public partial struct SpvReflectImageTraits
    {
        public SpvDim_ dim;
        public uint depth;
        public uint arrayed;
        public uint ms;
        public uint sampled;
        public SpvImageFormat_ image_format;
    }

    public unsafe partial struct SpvReflectArrayTraits
    {
        public uint dims_count;
        public fixed uint dims[32];
        public fixed uint spec_constant_op_ids[32];
        public uint stride;
    }

    public unsafe partial struct SpvReflectBindingArrayTraits
    {
        public uint dims_count;
        public fixed uint dims[32];
    }

    public unsafe partial struct SpvReflectTypeDescription
    {
        public uint id;
        public SpvOp_ op;
        public sbyte* type_name;
        public sbyte* struct_member_name;
        public SpvStorageClass_ storage_class;
        public uint type_flags;
        public uint decoration_flags;
        public Traits traits;
        public uint member_count;
        public SpvReflectTypeDescription* members;

        public partial struct Traits
        {
            public SpvReflectNumericTraits numeric;
            public SpvReflectImageTraits image;
            public SpvReflectArrayTraits array;
        }
    }

    public unsafe partial struct SpvReflectInterfaceVariable
    {
        public uint spirv_id;
        public sbyte* name;
        public uint location;
        public SpvStorageClass_ storage_class;
        public sbyte* semantic;
        public uint decoration_flags;
        public SpvBuiltIn_ built_in;
        public SpvReflectNumericTraits numeric;
        public SpvReflectArrayTraits array;
        public uint member_count;
        public SpvReflectInterfaceVariable* members;
        public SpvReflectFormat format;
        public SpvReflectTypeDescription* type_description;
        public _word_offset_e__Struct word_offset;

        public partial struct _word_offset_e__Struct
        {
            public uint location;
        }
    }

    public unsafe partial struct SpvReflectBlockVariable
    {
        public uint spirv_id;
        public sbyte* name;
        public uint offset;
        public uint absolute_offset;
        public uint size;
        public uint padded_size;
        public uint decoration_flags;
        public SpvReflectNumericTraits numeric;
        public SpvReflectArrayTraits array;
        public uint flags;
        public uint member_count;
        public SpvReflectBlockVariable* members;
        public SpvReflectTypeDescription* type_description;
    }

    public unsafe partial struct SpvReflectDescriptorBinding
    {
        public uint spirv_id;
        public sbyte* name;
        public uint binding;
        public uint input_attachment_index;
        public uint set;
        public SpvReflectDescriptorType descriptor_type;
        public SpvReflectResourceType resource_type;
        public SpvReflectImageTraits image;
        public SpvReflectBlockVariable block;
        public SpvReflectBindingArrayTraits array;
        public uint count;
        public uint accessed;
        public uint uav_counter_id;
        public SpvReflectDescriptorBinding* uav_counter_binding;
        public SpvReflectTypeDescription* type_description;
        public _word_offset_e__Struct word_offset;
        public uint decoration_flags;
        public partial struct _word_offset_e__Struct
        {
            public uint binding;
            public uint set;
        }
    }

    public unsafe partial struct SpvReflectDescriptorSet
    {
        public uint set;
        public uint binding_count;
        public SpvReflectDescriptorBinding** bindings;
    }

    public unsafe partial struct SpvReflectEntryPoint
    {
        public sbyte* name;
        public uint id;
        public SpvExecutionModel_ spirv_execution_model;
        public SpvReflectShaderStageFlagBits shader_stage;
        public uint input_variable_count;
        public SpvReflectInterfaceVariable** input_variables;
        public uint output_variable_count;
        public SpvReflectInterfaceVariable** output_variables;
        public uint interface_variable_count;
        public SpvReflectInterfaceVariable* interface_variables;
        public uint descriptor_set_count;
        public SpvReflectDescriptorSet* descriptor_sets;
        public uint used_uniform_count;
        public uint* used_uniforms;
        public uint used_push_constant_count;
        public uint* used_push_constants;
        public uint execution_mode_count;
        public SpvExecutionMode_* execution_modes;
        public LocalSize local_size;
        public uint invocations;
        public uint output_vertices;
        public partial struct LocalSize
        {
            public uint x;
            public uint y;
            public uint z;
        }
    }

    public partial struct SpvReflectCapability
    {
        public SpvCapability_ value;
        public uint word_offset;
    }

    public unsafe partial struct SpvReflectShaderModule
    {
        public SpvReflectGenerator generator;
        public sbyte* entry_point_name;
        public uint entry_point_id;
        public uint entry_point_count;
        public SpvReflectEntryPoint* entry_points;
        public SpvSourceLanguage_ source_language;
        public uint source_language_version;
        public sbyte* source_file;
        public sbyte* source_source;
        public uint capability_count;
        public SpvReflectCapability* capabilities;
        public SpvExecutionModel_ spirv_execution_model;
        public SpvReflectShaderStageFlagBits shader_stage;
        public uint descriptor_binding_count;
        public SpvReflectDescriptorBinding* descriptor_bindings;
        public uint descriptor_set_count;
        public _descriptor_sets_e__FixedBuffer descriptor_sets;
        public uint input_variable_count;
        public SpvReflectInterfaceVariable** input_variables;
        public uint output_variable_count;
        public SpvReflectInterfaceVariable** output_variables;
        public uint interface_variable_count;
        public SpvReflectInterfaceVariable* interface_variables;
        public uint push_constant_block_count;
        public SpvReflectBlockVariable* push_constant_blocks;
        public Internal* _internal;
        public unsafe partial struct Internal
        {
            public uint module_flags;
            public nuint spirv_size;
            public uint* spirv_code;
            public uint spirv_word_count;
            public nuint type_description_count;
            public SpvReflectTypeDescription* type_descriptions;
        }

        public partial struct _descriptor_sets_e__FixedBuffer
        {
            public SpvReflectDescriptorSet e0;
            public SpvReflectDescriptorSet e1;
            public SpvReflectDescriptorSet e2;
            public SpvReflectDescriptorSet e3;
            public SpvReflectDescriptorSet e4;
            public SpvReflectDescriptorSet e5;
            public SpvReflectDescriptorSet e6;
            public SpvReflectDescriptorSet e7;
            public SpvReflectDescriptorSet e8;
            public SpvReflectDescriptorSet e9;
            public SpvReflectDescriptorSet e10;
            public SpvReflectDescriptorSet e11;
            public SpvReflectDescriptorSet e12;
            public SpvReflectDescriptorSet e13;
            public SpvReflectDescriptorSet e14;
            public SpvReflectDescriptorSet e15;
            public SpvReflectDescriptorSet e16;
            public SpvReflectDescriptorSet e17;
            public SpvReflectDescriptorSet e18;
            public SpvReflectDescriptorSet e19;
            public SpvReflectDescriptorSet e20;
            public SpvReflectDescriptorSet e21;
            public SpvReflectDescriptorSet e22;
            public SpvReflectDescriptorSet e23;
            public SpvReflectDescriptorSet e24;
            public SpvReflectDescriptorSet e25;
            public SpvReflectDescriptorSet e26;
            public SpvReflectDescriptorSet e27;
            public SpvReflectDescriptorSet e28;
            public SpvReflectDescriptorSet e29;
            public SpvReflectDescriptorSet e30;
            public SpvReflectDescriptorSet e31;
            public SpvReflectDescriptorSet e32;
            public SpvReflectDescriptorSet e33;
            public SpvReflectDescriptorSet e34;
            public SpvReflectDescriptorSet e35;
            public SpvReflectDescriptorSet e36;
            public SpvReflectDescriptorSet e37;
            public SpvReflectDescriptorSet e38;
            public SpvReflectDescriptorSet e39;
            public SpvReflectDescriptorSet e40;
            public SpvReflectDescriptorSet e41;
            public SpvReflectDescriptorSet e42;
            public SpvReflectDescriptorSet e43;
            public SpvReflectDescriptorSet e44;
            public SpvReflectDescriptorSet e45;
            public SpvReflectDescriptorSet e46;
            public SpvReflectDescriptorSet e47;
            public SpvReflectDescriptorSet e48;
            public SpvReflectDescriptorSet e49;
            public SpvReflectDescriptorSet e50;
            public SpvReflectDescriptorSet e51;
            public SpvReflectDescriptorSet e52;
            public SpvReflectDescriptorSet e53;
            public SpvReflectDescriptorSet e54;
            public SpvReflectDescriptorSet e55;
            public SpvReflectDescriptorSet e56;
            public SpvReflectDescriptorSet e57;
            public SpvReflectDescriptorSet e58;
            public SpvReflectDescriptorSet e59;
            public SpvReflectDescriptorSet e60;
            public SpvReflectDescriptorSet e61;
            public SpvReflectDescriptorSet e62;
            public SpvReflectDescriptorSet e63;

            public ref SpvReflectDescriptorSet this[int index]
            {
                get
                {
                    return ref AsSpan()[index];
                }
            }

            public Span<SpvReflectDescriptorSet> AsSpan() => MemoryMarshal.CreateSpan(ref e0, 64);
        }
    }

    public enum SpvSourceLanguage_
    {
        SpvSourceLanguageUnknown = 0,
        SpvSourceLanguageESSL = 1,
        SpvSourceLanguageGLSL = 2,
        SpvSourceLanguageOpenCL_C = 3,
        SpvSourceLanguageOpenCL_CPP = 4,
        SpvSourceLanguageHLSL = 5,
        SpvSourceLanguageCPP_for_OpenCL = 6,
        SpvSourceLanguageSYCL = 7,
        SpvSourceLanguageMax = 0x7fffffff,
    }

    public enum SpvExecutionModel_
    {
        SpvExecutionModelVertex = 0,
        SpvExecutionModelTessellationControl = 1,
        SpvExecutionModelTessellationEvaluation = 2,
        SpvExecutionModelGeometry = 3,
        SpvExecutionModelFragment = 4,
        SpvExecutionModelGLCompute = 5,
        SpvExecutionModelKernel = 6,
        SpvExecutionModelTaskNV = 5267,
        SpvExecutionModelMeshNV = 5268,
        SpvExecutionModelRayGenerationKHR = 5313,
        SpvExecutionModelRayGenerationNV = 5313,
        SpvExecutionModelIntersectionKHR = 5314,
        SpvExecutionModelIntersectionNV = 5314,
        SpvExecutionModelAnyHitKHR = 5315,
        SpvExecutionModelAnyHitNV = 5315,
        SpvExecutionModelClosestHitKHR = 5316,
        SpvExecutionModelClosestHitNV = 5316,
        SpvExecutionModelMissKHR = 5317,
        SpvExecutionModelMissNV = 5317,
        SpvExecutionModelCallableKHR = 5318,
        SpvExecutionModelCallableNV = 5318,
        SpvExecutionModelMax = 0x7fffffff,
    }

    public enum SpvAddressingModel_
    {
        SpvAddressingModelLogical = 0,
        SpvAddressingModelPhysical32 = 1,
        SpvAddressingModelPhysical64 = 2,
        SpvAddressingModelPhysicalStorageBuffer64 = 5348,
        SpvAddressingModelPhysicalStorageBuffer64EXT = 5348,
        SpvAddressingModelMax = 0x7fffffff,
    }

    public enum SpvMemoryModel_
    {
        SpvMemoryModelSimple = 0,
        SpvMemoryModelGLSL450 = 1,
        SpvMemoryModelOpenCL = 2,
        SpvMemoryModelVulkan = 3,
        SpvMemoryModelVulkanKHR = 3,
        SpvMemoryModelMax = 0x7fffffff,
    }

    public enum SpvExecutionMode_
    {
        SpvExecutionModeInvocations = 0,
        SpvExecutionModeSpacingEqual = 1,
        SpvExecutionModeSpacingFractionalEven = 2,
        SpvExecutionModeSpacingFractionalOdd = 3,
        SpvExecutionModeVertexOrderCw = 4,
        SpvExecutionModeVertexOrderCcw = 5,
        SpvExecutionModePixelCenterInteger = 6,
        SpvExecutionModeOriginUpperLeft = 7,
        SpvExecutionModeOriginLowerLeft = 8,
        SpvExecutionModeEarlyFragmentTests = 9,
        SpvExecutionModePointMode = 10,
        SpvExecutionModeXfb = 11,
        SpvExecutionModeDepthReplacing = 12,
        SpvExecutionModeDepthGreater = 14,
        SpvExecutionModeDepthLess = 15,
        SpvExecutionModeDepthUnchanged = 16,
        SpvExecutionModeLocalSize = 17,
        SpvExecutionModeLocalSizeHint = 18,
        SpvExecutionModeInputPoints = 19,
        SpvExecutionModeInputLines = 20,
        SpvExecutionModeInputLinesAdjacency = 21,
        SpvExecutionModeTriangles = 22,
        SpvExecutionModeInputTrianglesAdjacency = 23,
        SpvExecutionModeQuads = 24,
        SpvExecutionModeIsolines = 25,
        SpvExecutionModeOutputVertices = 26,
        SpvExecutionModeOutputPoints = 27,
        SpvExecutionModeOutputLineStrip = 28,
        SpvExecutionModeOutputTriangleStrip = 29,
        SpvExecutionModeVecTypeHint = 30,
        SpvExecutionModeContractionOff = 31,
        SpvExecutionModeInitializer = 33,
        SpvExecutionModeFinalizer = 34,
        SpvExecutionModeSubgroupSize = 35,
        SpvExecutionModeSubgroupsPerWorkgroup = 36,
        SpvExecutionModeSubgroupsPerWorkgroupId = 37,
        SpvExecutionModeLocalSizeId = 38,
        SpvExecutionModeLocalSizeHintId = 39,
        SpvExecutionModeSubgroupUniformControlFlowKHR = 4421,
        SpvExecutionModePostDepthCoverage = 4446,
        SpvExecutionModeDenormPreserve = 4459,
        SpvExecutionModeDenormFlushToZero = 4460,
        SpvExecutionModeSignedZeroInfNanPreserve = 4461,
        SpvExecutionModeRoundingModeRTE = 4462,
        SpvExecutionModeRoundingModeRTZ = 4463,
        SpvExecutionModeStencilRefReplacingEXT = 5027,
        SpvExecutionModeOutputLinesNV = 5269,
        SpvExecutionModeOutputPrimitivesNV = 5270,
        SpvExecutionModeDerivativeGroupQuadsNV = 5289,
        SpvExecutionModeDerivativeGroupLinearNV = 5290,
        SpvExecutionModeOutputTrianglesNV = 5298,
        SpvExecutionModePixelInterlockOrderedEXT = 5366,
        SpvExecutionModePixelInterlockUnorderedEXT = 5367,
        SpvExecutionModeSampleInterlockOrderedEXT = 5368,
        SpvExecutionModeSampleInterlockUnorderedEXT = 5369,
        SpvExecutionModeShadingRateInterlockOrderedEXT = 5370,
        SpvExecutionModeShadingRateInterlockUnorderedEXT = 5371,
        SpvExecutionModeSharedLocalMemorySizeINTEL = 5618,
        SpvExecutionModeRoundingModeRTPINTEL = 5620,
        SpvExecutionModeRoundingModeRTNINTEL = 5621,
        SpvExecutionModeFloatingPointModeALTINTEL = 5622,
        SpvExecutionModeFloatingPointModeIEEEINTEL = 5623,
        SpvExecutionModeMaxWorkgroupSizeINTEL = 5893,
        SpvExecutionModeMaxWorkDimINTEL = 5894,
        SpvExecutionModeNoGlobalOffsetINTEL = 5895,
        SpvExecutionModeNumSIMDWorkitemsINTEL = 5896,
        SpvExecutionModeSchedulerTargetFmaxMhzINTEL = 5903,
        SpvExecutionModeNamedBarrierCountINTEL = 6417,
        SpvExecutionModeMax = 0x7fffffff,
    }

    public enum SpvStorageClass_
    {
        SpvStorageClassUniformConstant = 0,
        SpvStorageClassInput = 1,
        SpvStorageClassUniform = 2,
        SpvStorageClassOutput = 3,
        SpvStorageClassWorkgroup = 4,
        SpvStorageClassCrossWorkgroup = 5,
        SpvStorageClassPrivate = 6,
        SpvStorageClassFunction = 7,
        SpvStorageClassGeneric = 8,
        SpvStorageClassPushConstant = 9,
        SpvStorageClassAtomicCounter = 10,
        SpvStorageClassImage = 11,
        SpvStorageClassStorageBuffer = 12,
        SpvStorageClassCallableDataKHR = 5328,
        SpvStorageClassCallableDataNV = 5328,
        SpvStorageClassIncomingCallableDataKHR = 5329,
        SpvStorageClassIncomingCallableDataNV = 5329,
        SpvStorageClassRayPayloadKHR = 5338,
        SpvStorageClassRayPayloadNV = 5338,
        SpvStorageClassHitAttributeKHR = 5339,
        SpvStorageClassHitAttributeNV = 5339,
        SpvStorageClassIncomingRayPayloadKHR = 5342,
        SpvStorageClassIncomingRayPayloadNV = 5342,
        SpvStorageClassShaderRecordBufferKHR = 5343,
        SpvStorageClassShaderRecordBufferNV = 5343,
        SpvStorageClassPhysicalStorageBuffer = 5349,
        SpvStorageClassPhysicalStorageBufferEXT = 5349,
        SpvStorageClassCodeSectionINTEL = 5605,
        SpvStorageClassDeviceOnlyINTEL = 5936,
        SpvStorageClassHostOnlyINTEL = 5937,
        SpvStorageClassMax = 0x7fffffff,
    }

    public enum SpvDim_
    {
        SpvDim1D = 0,
        SpvDim2D = 1,
        SpvDim3D = 2,
        SpvDimCube = 3,
        SpvDimRect = 4,
        SpvDimBuffer = 5,
        SpvDimSubpassData = 6,
        SpvDimMax = 0x7fffffff,
    }

    public enum SpvSamplerAddressingMode_
    {
        SpvSamplerAddressingModeNone = 0,
        SpvSamplerAddressingModeClampToEdge = 1,
        SpvSamplerAddressingModeClamp = 2,
        SpvSamplerAddressingModeRepeat = 3,
        SpvSamplerAddressingModeRepeatMirrored = 4,
        SpvSamplerAddressingModeMax = 0x7fffffff,
    }

    public enum SpvSamplerFilterMode_
    {
        SpvSamplerFilterModeNearest = 0,
        SpvSamplerFilterModeLinear = 1,
        SpvSamplerFilterModeMax = 0x7fffffff,
    }

    public enum SpvImageFormat_
    {
        SpvImageFormatUnknown = 0,
        SpvImageFormatRgba32f = 1,
        SpvImageFormatRgba16f = 2,
        SpvImageFormatR32f = 3,
        SpvImageFormatRgba8 = 4,
        SpvImageFormatRgba8Snorm = 5,
        SpvImageFormatRg32f = 6,
        SpvImageFormatRg16f = 7,
        SpvImageFormatR11fG11fB10f = 8,
        SpvImageFormatR16f = 9,
        SpvImageFormatRgba16 = 10,
        SpvImageFormatRgb10A2 = 11,
        SpvImageFormatRg16 = 12,
        SpvImageFormatRg8 = 13,
        SpvImageFormatR16 = 14,
        SpvImageFormatR8 = 15,
        SpvImageFormatRgba16Snorm = 16,
        SpvImageFormatRg16Snorm = 17,
        SpvImageFormatRg8Snorm = 18,
        SpvImageFormatR16Snorm = 19,
        SpvImageFormatR8Snorm = 20,
        SpvImageFormatRgba32i = 21,
        SpvImageFormatRgba16i = 22,
        SpvImageFormatRgba8i = 23,
        SpvImageFormatR32i = 24,
        SpvImageFormatRg32i = 25,
        SpvImageFormatRg16i = 26,
        SpvImageFormatRg8i = 27,
        SpvImageFormatR16i = 28,
        SpvImageFormatR8i = 29,
        SpvImageFormatRgba32ui = 30,
        SpvImageFormatRgba16ui = 31,
        SpvImageFormatRgba8ui = 32,
        SpvImageFormatR32ui = 33,
        SpvImageFormatRgb10a2ui = 34,
        SpvImageFormatRg32ui = 35,
        SpvImageFormatRg16ui = 36,
        SpvImageFormatRg8ui = 37,
        SpvImageFormatR16ui = 38,
        SpvImageFormatR8ui = 39,
        SpvImageFormatR64ui = 40,
        SpvImageFormatR64i = 41,
        SpvImageFormatMax = 0x7fffffff,
    }

    public enum SpvImageChannelOrder_
    {
        SpvImageChannelOrderR = 0,
        SpvImageChannelOrderA = 1,
        SpvImageChannelOrderRG = 2,
        SpvImageChannelOrderRA = 3,
        SpvImageChannelOrderRGB = 4,
        SpvImageChannelOrderRGBA = 5,
        SpvImageChannelOrderBGRA = 6,
        SpvImageChannelOrderARGB = 7,
        SpvImageChannelOrderIntensity = 8,
        SpvImageChannelOrderLuminance = 9,
        SpvImageChannelOrderRx = 10,
        SpvImageChannelOrderRGx = 11,
        SpvImageChannelOrderRGBx = 12,
        SpvImageChannelOrderDepth = 13,
        SpvImageChannelOrderDepthStencil = 14,
        SpvImageChannelOrdersRGB = 15,
        SpvImageChannelOrdersRGBx = 16,
        SpvImageChannelOrdersRGBA = 17,
        SpvImageChannelOrdersBGRA = 18,
        SpvImageChannelOrderABGR = 19,
        SpvImageChannelOrderMax = 0x7fffffff,
    }

    public enum SpvImageChannelDataType_
    {
        SpvImageChannelDataTypeSnormInt8 = 0,
        SpvImageChannelDataTypeSnormInt16 = 1,
        SpvImageChannelDataTypeUnormInt8 = 2,
        SpvImageChannelDataTypeUnormInt16 = 3,
        SpvImageChannelDataTypeUnormShort565 = 4,
        SpvImageChannelDataTypeUnormShort555 = 5,
        SpvImageChannelDataTypeUnormInt101010 = 6,
        SpvImageChannelDataTypeSignedInt8 = 7,
        SpvImageChannelDataTypeSignedInt16 = 8,
        SpvImageChannelDataTypeSignedInt32 = 9,
        SpvImageChannelDataTypeUnsignedInt8 = 10,
        SpvImageChannelDataTypeUnsignedInt16 = 11,
        SpvImageChannelDataTypeUnsignedInt32 = 12,
        SpvImageChannelDataTypeHalfFloat = 13,
        SpvImageChannelDataTypeFloat = 14,
        SpvImageChannelDataTypeUnormInt24 = 15,
        SpvImageChannelDataTypeUnormInt101010_2 = 16,
        SpvImageChannelDataTypeMax = 0x7fffffff,
    }

    public enum SpvImageOperandsShift_
    {
        SpvImageOperandsBiasShift = 0,
        SpvImageOperandsLodShift = 1,
        SpvImageOperandsGradShift = 2,
        SpvImageOperandsConstOffsetShift = 3,
        SpvImageOperandsOffsetShift = 4,
        SpvImageOperandsConstOffsetsShift = 5,
        SpvImageOperandsSampleShift = 6,
        SpvImageOperandsMinLodShift = 7,
        SpvImageOperandsMakeTexelAvailableShift = 8,
        SpvImageOperandsMakeTexelAvailableKHRShift = 8,
        SpvImageOperandsMakeTexelVisibleShift = 9,
        SpvImageOperandsMakeTexelVisibleKHRShift = 9,
        SpvImageOperandsNonPrivateTexelShift = 10,
        SpvImageOperandsNonPrivateTexelKHRShift = 10,
        SpvImageOperandsVolatileTexelShift = 11,
        SpvImageOperandsVolatileTexelKHRShift = 11,
        SpvImageOperandsSignExtendShift = 12,
        SpvImageOperandsZeroExtendShift = 13,
        SpvImageOperandsNontemporalShift = 14,
        SpvImageOperandsOffsetsShift = 16,
        SpvImageOperandsMax = 0x7fffffff,
    }

    public enum SpvImageOperandsMask_
    {
        SpvImageOperandsMaskNone = 0,
        SpvImageOperandsBiasMask = 0x00000001,
        SpvImageOperandsLodMask = 0x00000002,
        SpvImageOperandsGradMask = 0x00000004,
        SpvImageOperandsConstOffsetMask = 0x00000008,
        SpvImageOperandsOffsetMask = 0x00000010,
        SpvImageOperandsConstOffsetsMask = 0x00000020,
        SpvImageOperandsSampleMask = 0x00000040,
        SpvImageOperandsMinLodMask = 0x00000080,
        SpvImageOperandsMakeTexelAvailableMask = 0x00000100,
        SpvImageOperandsMakeTexelAvailableKHRMask = 0x00000100,
        SpvImageOperandsMakeTexelVisibleMask = 0x00000200,
        SpvImageOperandsMakeTexelVisibleKHRMask = 0x00000200,
        SpvImageOperandsNonPrivateTexelMask = 0x00000400,
        SpvImageOperandsNonPrivateTexelKHRMask = 0x00000400,
        SpvImageOperandsVolatileTexelMask = 0x00000800,
        SpvImageOperandsVolatileTexelKHRMask = 0x00000800,
        SpvImageOperandsSignExtendMask = 0x00001000,
        SpvImageOperandsZeroExtendMask = 0x00002000,
        SpvImageOperandsNontemporalMask = 0x00004000,
        SpvImageOperandsOffsetsMask = 0x00010000,
    }

    public enum SpvFPFastMathModeShift_
    {
        SpvFPFastMathModeNotNaNShift = 0,
        SpvFPFastMathModeNotInfShift = 1,
        SpvFPFastMathModeNSZShift = 2,
        SpvFPFastMathModeAllowRecipShift = 3,
        SpvFPFastMathModeFastShift = 4,
        SpvFPFastMathModeAllowContractFastINTELShift = 16,
        SpvFPFastMathModeAllowReassocINTELShift = 17,
        SpvFPFastMathModeMax = 0x7fffffff,
    }

    public enum SpvFPFastMathModeMask_
    {
        SpvFPFastMathModeMaskNone = 0,
        SpvFPFastMathModeNotNaNMask = 0x00000001,
        SpvFPFastMathModeNotInfMask = 0x00000002,
        SpvFPFastMathModeNSZMask = 0x00000004,
        SpvFPFastMathModeAllowRecipMask = 0x00000008,
        SpvFPFastMathModeFastMask = 0x00000010,
        SpvFPFastMathModeAllowContractFastINTELMask = 0x00010000,
        SpvFPFastMathModeAllowReassocINTELMask = 0x00020000,
    }

    public enum SpvFPRoundingMode_
    {
        SpvFPRoundingModeRTE = 0,
        SpvFPRoundingModeRTZ = 1,
        SpvFPRoundingModeRTP = 2,
        SpvFPRoundingModeRTN = 3,
        SpvFPRoundingModeMax = 0x7fffffff,
    }

    public enum SpvLinkageType_
    {
        SpvLinkageTypeExport = 0,
        SpvLinkageTypeImport = 1,
        SpvLinkageTypeLinkOnceODR = 2,
        SpvLinkageTypeMax = 0x7fffffff,
    }

    public enum SpvAccessQualifier_
    {
        SpvAccessQualifierReadOnly = 0,
        SpvAccessQualifierWriteOnly = 1,
        SpvAccessQualifierReadWrite = 2,
        SpvAccessQualifierMax = 0x7fffffff,
    }

    public enum SpvFunctionParameterAttribute_
    {
        SpvFunctionParameterAttributeZext = 0,
        SpvFunctionParameterAttributeSext = 1,
        SpvFunctionParameterAttributeByVal = 2,
        SpvFunctionParameterAttributeSret = 3,
        SpvFunctionParameterAttributeNoAlias = 4,
        SpvFunctionParameterAttributeNoCapture = 5,
        SpvFunctionParameterAttributeNoWrite = 6,
        SpvFunctionParameterAttributeNoReadWrite = 7,
        SpvFunctionParameterAttributeMax = 0x7fffffff,
    }

    public enum SpvDecoration_
    {
        SpvDecorationRelaxedPrecision = 0,
        SpvDecorationSpecId = 1,
        SpvDecorationBlock = 2,
        SpvDecorationBufferBlock = 3,
        SpvDecorationRowMajor = 4,
        SpvDecorationColMajor = 5,
        SpvDecorationArrayStride = 6,
        SpvDecorationMatrixStride = 7,
        SpvDecorationGLSLShared = 8,
        SpvDecorationGLSLPacked = 9,
        SpvDecorationCPacked = 10,
        SpvDecorationBuiltIn = 11,
        SpvDecorationNoPerspective = 13,
        SpvDecorationFlat = 14,
        SpvDecorationPatch = 15,
        SpvDecorationCentroid = 16,
        SpvDecorationSample = 17,
        SpvDecorationInvariant = 18,
        SpvDecorationRestrict = 19,
        SpvDecorationAliased = 20,
        SpvDecorationVolatile = 21,
        SpvDecorationConstant = 22,
        SpvDecorationCoherent = 23,
        SpvDecorationNonWritable = 24,
        SpvDecorationNonReadable = 25,
        SpvDecorationUniform = 26,
        SpvDecorationUniformId = 27,
        SpvDecorationSaturatedConversion = 28,
        SpvDecorationStream = 29,
        SpvDecorationLocation = 30,
        SpvDecorationComponent = 31,
        SpvDecorationIndex = 32,
        SpvDecorationBinding = 33,
        SpvDecorationDescriptorSet = 34,
        SpvDecorationOffset = 35,
        SpvDecorationXfbBuffer = 36,
        SpvDecorationXfbStride = 37,
        SpvDecorationFuncParamAttr = 38,
        SpvDecorationFPRoundingMode = 39,
        SpvDecorationFPFastMathMode = 40,
        SpvDecorationLinkageAttributes = 41,
        SpvDecorationNoContraction = 42,
        SpvDecorationInputAttachmentIndex = 43,
        SpvDecorationAlignment = 44,
        SpvDecorationMaxByteOffset = 45,
        SpvDecorationAlignmentId = 46,
        SpvDecorationMaxByteOffsetId = 47,
        SpvDecorationNoSignedWrap = 4469,
        SpvDecorationNoUnsignedWrap = 4470,
        SpvDecorationExplicitInterpAMD = 4999,
        SpvDecorationOverrideCoverageNV = 5248,
        SpvDecorationPassthroughNV = 5250,
        SpvDecorationViewportRelativeNV = 5252,
        SpvDecorationSecondaryViewportRelativeNV = 5256,
        SpvDecorationPerPrimitiveNV = 5271,
        SpvDecorationPerViewNV = 5272,
        SpvDecorationPerTaskNV = 5273,
        SpvDecorationPerVertexKHR = 5285,
        SpvDecorationPerVertexNV = 5285,
        SpvDecorationNonUniform = 5300,
        SpvDecorationNonUniformEXT = 5300,
        SpvDecorationRestrictPointer = 5355,
        SpvDecorationRestrictPointerEXT = 5355,
        SpvDecorationAliasedPointer = 5356,
        SpvDecorationAliasedPointerEXT = 5356,
        SpvDecorationBindlessSamplerNV = 5398,
        SpvDecorationBindlessImageNV = 5399,
        SpvDecorationBoundSamplerNV = 5400,
        SpvDecorationBoundImageNV = 5401,
        SpvDecorationSIMTCallINTEL = 5599,
        SpvDecorationReferencedIndirectlyINTEL = 5602,
        SpvDecorationClobberINTEL = 5607,
        SpvDecorationSideEffectsINTEL = 5608,
        SpvDecorationVectorComputeVariableINTEL = 5624,
        SpvDecorationFuncParamIOKindINTEL = 5625,
        SpvDecorationVectorComputeFunctionINTEL = 5626,
        SpvDecorationStackCallINTEL = 5627,
        SpvDecorationGlobalVariableOffsetINTEL = 5628,
        SpvDecorationCounterBuffer = 5634,
        SpvDecorationHlslCounterBufferGOOGLE = 5634,
        SpvDecorationHlslSemanticGOOGLE = 5635,
        SpvDecorationUserSemantic = 5635,
        SpvDecorationUserTypeGOOGLE = 5636,
        SpvDecorationFunctionRoundingModeINTEL = 5822,
        SpvDecorationFunctionDenormModeINTEL = 5823,
        SpvDecorationRegisterINTEL = 5825,
        SpvDecorationMemoryINTEL = 5826,
        SpvDecorationNumbanksINTEL = 5827,
        SpvDecorationBankwidthINTEL = 5828,
        SpvDecorationMaxPrivateCopiesINTEL = 5829,
        SpvDecorationSinglepumpINTEL = 5830,
        SpvDecorationDoublepumpINTEL = 5831,
        SpvDecorationMaxReplicatesINTEL = 5832,
        SpvDecorationSimpleDualPortINTEL = 5833,
        SpvDecorationMergeINTEL = 5834,
        SpvDecorationBankBitsINTEL = 5835,
        SpvDecorationForcePow2DepthINTEL = 5836,
        SpvDecorationBurstCoalesceINTEL = 5899,
        SpvDecorationCacheSizeINTEL = 5900,
        SpvDecorationDontStaticallyCoalesceINTEL = 5901,
        SpvDecorationPrefetchINTEL = 5902,
        SpvDecorationStallEnableINTEL = 5905,
        SpvDecorationFuseLoopsInFunctionINTEL = 5907,
        SpvDecorationAliasScopeINTEL = 5914,
        SpvDecorationNoAliasINTEL = 5915,
        SpvDecorationBufferLocationINTEL = 5921,
        SpvDecorationIOPipeStorageINTEL = 5944,
        SpvDecorationFunctionFloatingPointModeINTEL = 6080,
        SpvDecorationSingleElementVectorINTEL = 6085,
        SpvDecorationVectorComputeCallableFunctionINTEL = 6087,
        SpvDecorationMediaBlockIOINTEL = 6140,
        SpvDecorationMax = 0x7fffffff,
    }

    public enum SpvBuiltIn_
    {
        SpvBuiltInPosition = 0,
        SpvBuiltInPointSize = 1,
        SpvBuiltInClipDistance = 3,
        SpvBuiltInCullDistance = 4,
        SpvBuiltInVertexId = 5,
        SpvBuiltInInstanceId = 6,
        SpvBuiltInPrimitiveId = 7,
        SpvBuiltInInvocationId = 8,
        SpvBuiltInLayer = 9,
        SpvBuiltInViewportIndex = 10,
        SpvBuiltInTessLevelOuter = 11,
        SpvBuiltInTessLevelInner = 12,
        SpvBuiltInTessCoord = 13,
        SpvBuiltInPatchVertices = 14,
        SpvBuiltInFragCoord = 15,
        SpvBuiltInPointCoord = 16,
        SpvBuiltInFrontFacing = 17,
        SpvBuiltInSampleId = 18,
        SpvBuiltInSamplePosition = 19,
        SpvBuiltInSampleMask = 20,
        SpvBuiltInFragDepth = 22,
        SpvBuiltInHelperInvocation = 23,
        SpvBuiltInNumWorkgroups = 24,
        SpvBuiltInWorkgroupSize = 25,
        SpvBuiltInWorkgroupId = 26,
        SpvBuiltInLocalInvocationId = 27,
        SpvBuiltInGlobalInvocationId = 28,
        SpvBuiltInLocalInvocationIndex = 29,
        SpvBuiltInWorkDim = 30,
        SpvBuiltInGlobalSize = 31,
        SpvBuiltInEnqueuedWorkgroupSize = 32,
        SpvBuiltInGlobalOffset = 33,
        SpvBuiltInGlobalLinearId = 34,
        SpvBuiltInSubgroupSize = 36,
        SpvBuiltInSubgroupMaxSize = 37,
        SpvBuiltInNumSubgroups = 38,
        SpvBuiltInNumEnqueuedSubgroups = 39,
        SpvBuiltInSubgroupId = 40,
        SpvBuiltInSubgroupLocalInvocationId = 41,
        SpvBuiltInVertexIndex = 42,
        SpvBuiltInInstanceIndex = 43,
        SpvBuiltInSubgroupEqMask = 4416,
        SpvBuiltInSubgroupEqMaskKHR = 4416,
        SpvBuiltInSubgroupGeMask = 4417,
        SpvBuiltInSubgroupGeMaskKHR = 4417,
        SpvBuiltInSubgroupGtMask = 4418,
        SpvBuiltInSubgroupGtMaskKHR = 4418,
        SpvBuiltInSubgroupLeMask = 4419,
        SpvBuiltInSubgroupLeMaskKHR = 4419,
        SpvBuiltInSubgroupLtMask = 4420,
        SpvBuiltInSubgroupLtMaskKHR = 4420,
        SpvBuiltInBaseVertex = 4424,
        SpvBuiltInBaseInstance = 4425,
        SpvBuiltInDrawIndex = 4426,
        SpvBuiltInPrimitiveShadingRateKHR = 4432,
        SpvBuiltInDeviceIndex = 4438,
        SpvBuiltInViewIndex = 4440,
        SpvBuiltInShadingRateKHR = 4444,
        SpvBuiltInBaryCoordNoPerspAMD = 4992,
        SpvBuiltInBaryCoordNoPerspCentroidAMD = 4993,
        SpvBuiltInBaryCoordNoPerspSampleAMD = 4994,
        SpvBuiltInBaryCoordSmoothAMD = 4995,
        SpvBuiltInBaryCoordSmoothCentroidAMD = 4996,
        SpvBuiltInBaryCoordSmoothSampleAMD = 4997,
        SpvBuiltInBaryCoordPullModelAMD = 4998,
        SpvBuiltInFragStencilRefEXT = 5014,
        SpvBuiltInViewportMaskNV = 5253,
        SpvBuiltInSecondaryPositionNV = 5257,
        SpvBuiltInSecondaryViewportMaskNV = 5258,
        SpvBuiltInPositionPerViewNV = 5261,
        SpvBuiltInViewportMaskPerViewNV = 5262,
        SpvBuiltInFullyCoveredEXT = 5264,
        SpvBuiltInTaskCountNV = 5274,
        SpvBuiltInPrimitiveCountNV = 5275,
        SpvBuiltInPrimitiveIndicesNV = 5276,
        SpvBuiltInClipDistancePerViewNV = 5277,
        SpvBuiltInCullDistancePerViewNV = 5278,
        SpvBuiltInLayerPerViewNV = 5279,
        SpvBuiltInMeshViewCountNV = 5280,
        SpvBuiltInMeshViewIndicesNV = 5281,
        SpvBuiltInBaryCoordKHR = 5286,
        SpvBuiltInBaryCoordNV = 5286,
        SpvBuiltInBaryCoordNoPerspKHR = 5287,
        SpvBuiltInBaryCoordNoPerspNV = 5287,
        SpvBuiltInFragSizeEXT = 5292,
        SpvBuiltInFragmentSizeNV = 5292,
        SpvBuiltInFragInvocationCountEXT = 5293,
        SpvBuiltInInvocationsPerPixelNV = 5293,
        SpvBuiltInLaunchIdKHR = 5319,
        SpvBuiltInLaunchIdNV = 5319,
        SpvBuiltInLaunchSizeKHR = 5320,
        SpvBuiltInLaunchSizeNV = 5320,
        SpvBuiltInWorldRayOriginKHR = 5321,
        SpvBuiltInWorldRayOriginNV = 5321,
        SpvBuiltInWorldRayDirectionKHR = 5322,
        SpvBuiltInWorldRayDirectionNV = 5322,
        SpvBuiltInObjectRayOriginKHR = 5323,
        SpvBuiltInObjectRayOriginNV = 5323,
        SpvBuiltInObjectRayDirectionKHR = 5324,
        SpvBuiltInObjectRayDirectionNV = 5324,
        SpvBuiltInRayTminKHR = 5325,
        SpvBuiltInRayTminNV = 5325,
        SpvBuiltInRayTmaxKHR = 5326,
        SpvBuiltInRayTmaxNV = 5326,
        SpvBuiltInInstanceCustomIndexKHR = 5327,
        SpvBuiltInInstanceCustomIndexNV = 5327,
        SpvBuiltInObjectToWorldKHR = 5330,
        SpvBuiltInObjectToWorldNV = 5330,
        SpvBuiltInWorldToObjectKHR = 5331,
        SpvBuiltInWorldToObjectNV = 5331,
        SpvBuiltInHitTNV = 5332,
        SpvBuiltInHitKindKHR = 5333,
        SpvBuiltInHitKindNV = 5333,
        SpvBuiltInCurrentRayTimeNV = 5334,
        SpvBuiltInIncomingRayFlagsKHR = 5351,
        SpvBuiltInIncomingRayFlagsNV = 5351,
        SpvBuiltInRayGeometryIndexKHR = 5352,
        SpvBuiltInWarpsPerSMNV = 5374,
        SpvBuiltInSMCountNV = 5375,
        SpvBuiltInWarpIDNV = 5376,
        SpvBuiltInSMIDNV = 5377,
        SpvBuiltInCullMaskKHR = 6021,
        SpvBuiltInMax = 0x7fffffff,
    }

    public enum SpvSelectionControlShift_
    {
        SpvSelectionControlFlattenShift = 0,
        SpvSelectionControlDontFlattenShift = 1,
        SpvSelectionControlMax = 0x7fffffff,
    }

    public enum SpvSelectionControlMask_
    {
        SpvSelectionControlMaskNone = 0,
        SpvSelectionControlFlattenMask = 0x00000001,
        SpvSelectionControlDontFlattenMask = 0x00000002,
    }

    public enum SpvLoopControlShift_
    {
        SpvLoopControlUnrollShift = 0,
        SpvLoopControlDontUnrollShift = 1,
        SpvLoopControlDependencyInfiniteShift = 2,
        SpvLoopControlDependencyLengthShift = 3,
        SpvLoopControlMinIterationsShift = 4,
        SpvLoopControlMaxIterationsShift = 5,
        SpvLoopControlIterationMultipleShift = 6,
        SpvLoopControlPeelCountShift = 7,
        SpvLoopControlPartialCountShift = 8,
        SpvLoopControlInitiationIntervalINTELShift = 16,
        SpvLoopControlMaxConcurrencyINTELShift = 17,
        SpvLoopControlDependencyArrayINTELShift = 18,
        SpvLoopControlPipelineEnableINTELShift = 19,
        SpvLoopControlLoopCoalesceINTELShift = 20,
        SpvLoopControlMaxInterleavingINTELShift = 21,
        SpvLoopControlSpeculatedIterationsINTELShift = 22,
        SpvLoopControlNoFusionINTELShift = 23,
        SpvLoopControlMax = 0x7fffffff,
    }

    public enum SpvLoopControlMask_
    {
        SpvLoopControlMaskNone = 0,
        SpvLoopControlUnrollMask = 0x00000001,
        SpvLoopControlDontUnrollMask = 0x00000002,
        SpvLoopControlDependencyInfiniteMask = 0x00000004,
        SpvLoopControlDependencyLengthMask = 0x00000008,
        SpvLoopControlMinIterationsMask = 0x00000010,
        SpvLoopControlMaxIterationsMask = 0x00000020,
        SpvLoopControlIterationMultipleMask = 0x00000040,
        SpvLoopControlPeelCountMask = 0x00000080,
        SpvLoopControlPartialCountMask = 0x00000100,
        SpvLoopControlInitiationIntervalINTELMask = 0x00010000,
        SpvLoopControlMaxConcurrencyINTELMask = 0x00020000,
        SpvLoopControlDependencyArrayINTELMask = 0x00040000,
        SpvLoopControlPipelineEnableINTELMask = 0x00080000,
        SpvLoopControlLoopCoalesceINTELMask = 0x00100000,
        SpvLoopControlMaxInterleavingINTELMask = 0x00200000,
        SpvLoopControlSpeculatedIterationsINTELMask = 0x00400000,
        SpvLoopControlNoFusionINTELMask = 0x00800000,
    }

    public enum SpvFunctionControlShift_
    {
        SpvFunctionControlInlineShift = 0,
        SpvFunctionControlDontInlineShift = 1,
        SpvFunctionControlPureShift = 2,
        SpvFunctionControlConstShift = 3,
        SpvFunctionControlOptNoneINTELShift = 16,
        SpvFunctionControlMax = 0x7fffffff,
    }

    public enum SpvFunctionControlMask_
    {
        SpvFunctionControlMaskNone = 0,
        SpvFunctionControlInlineMask = 0x00000001,
        SpvFunctionControlDontInlineMask = 0x00000002,
        SpvFunctionControlPureMask = 0x00000004,
        SpvFunctionControlConstMask = 0x00000008,
        SpvFunctionControlOptNoneINTELMask = 0x00010000,
    }

    public enum SpvMemorySemanticsShift_
    {
        SpvMemorySemanticsAcquireShift = 1,
        SpvMemorySemanticsReleaseShift = 2,
        SpvMemorySemanticsAcquireReleaseShift = 3,
        SpvMemorySemanticsSequentiallyConsistentShift = 4,
        SpvMemorySemanticsUniformMemoryShift = 6,
        SpvMemorySemanticsSubgroupMemoryShift = 7,
        SpvMemorySemanticsWorkgroupMemoryShift = 8,
        SpvMemorySemanticsCrossWorkgroupMemoryShift = 9,
        SpvMemorySemanticsAtomicCounterMemoryShift = 10,
        SpvMemorySemanticsImageMemoryShift = 11,
        SpvMemorySemanticsOutputMemoryShift = 12,
        SpvMemorySemanticsOutputMemoryKHRShift = 12,
        SpvMemorySemanticsMakeAvailableShift = 13,
        SpvMemorySemanticsMakeAvailableKHRShift = 13,
        SpvMemorySemanticsMakeVisibleShift = 14,
        SpvMemorySemanticsMakeVisibleKHRShift = 14,
        SpvMemorySemanticsVolatileShift = 15,
        SpvMemorySemanticsMax = 0x7fffffff,
    }

    public enum SpvMemorySemanticsMask_
    {
        SpvMemorySemanticsMaskNone = 0,
        SpvMemorySemanticsAcquireMask = 0x00000002,
        SpvMemorySemanticsReleaseMask = 0x00000004,
        SpvMemorySemanticsAcquireReleaseMask = 0x00000008,
        SpvMemorySemanticsSequentiallyConsistentMask = 0x00000010,
        SpvMemorySemanticsUniformMemoryMask = 0x00000040,
        SpvMemorySemanticsSubgroupMemoryMask = 0x00000080,
        SpvMemorySemanticsWorkgroupMemoryMask = 0x00000100,
        SpvMemorySemanticsCrossWorkgroupMemoryMask = 0x00000200,
        SpvMemorySemanticsAtomicCounterMemoryMask = 0x00000400,
        SpvMemorySemanticsImageMemoryMask = 0x00000800,
        SpvMemorySemanticsOutputMemoryMask = 0x00001000,
        SpvMemorySemanticsOutputMemoryKHRMask = 0x00001000,
        SpvMemorySemanticsMakeAvailableMask = 0x00002000,
        SpvMemorySemanticsMakeAvailableKHRMask = 0x00002000,
        SpvMemorySemanticsMakeVisibleMask = 0x00004000,
        SpvMemorySemanticsMakeVisibleKHRMask = 0x00004000,
        SpvMemorySemanticsVolatileMask = 0x00008000,
    }

    public enum SpvMemoryAccessShift_
    {
        SpvMemoryAccessVolatileShift = 0,
        SpvMemoryAccessAlignedShift = 1,
        SpvMemoryAccessNontemporalShift = 2,
        SpvMemoryAccessMakePointerAvailableShift = 3,
        SpvMemoryAccessMakePointerAvailableKHRShift = 3,
        SpvMemoryAccessMakePointerVisibleShift = 4,
        SpvMemoryAccessMakePointerVisibleKHRShift = 4,
        SpvMemoryAccessNonPrivatePointerShift = 5,
        SpvMemoryAccessNonPrivatePointerKHRShift = 5,
        SpvMemoryAccessAliasScopeINTELMaskShift = 16,
        SpvMemoryAccessNoAliasINTELMaskShift = 17,
        SpvMemoryAccessMax = 0x7fffffff,
    }

    public enum SpvMemoryAccessMask_
    {
        SpvMemoryAccessMaskNone = 0,
        SpvMemoryAccessVolatileMask = 0x00000001,
        SpvMemoryAccessAlignedMask = 0x00000002,
        SpvMemoryAccessNontemporalMask = 0x00000004,
        SpvMemoryAccessMakePointerAvailableMask = 0x00000008,
        SpvMemoryAccessMakePointerAvailableKHRMask = 0x00000008,
        SpvMemoryAccessMakePointerVisibleMask = 0x00000010,
        SpvMemoryAccessMakePointerVisibleKHRMask = 0x00000010,
        SpvMemoryAccessNonPrivatePointerMask = 0x00000020,
        SpvMemoryAccessNonPrivatePointerKHRMask = 0x00000020,
        SpvMemoryAccessAliasScopeINTELMaskMask = 0x00010000,
        SpvMemoryAccessNoAliasINTELMaskMask = 0x00020000,
    }

    public enum SpvScope_
    {
        SpvScopeCrossDevice = 0,
        SpvScopeDevice = 1,
        SpvScopeWorkgroup = 2,
        SpvScopeSubgroup = 3,
        SpvScopeInvocation = 4,
        SpvScopeQueueFamily = 5,
        SpvScopeQueueFamilyKHR = 5,
        SpvScopeShaderCallKHR = 6,
        SpvScopeMax = 0x7fffffff,
    }

    public enum SpvGroupOperation_
    {
        SpvGroupOperationReduce = 0,
        SpvGroupOperationInclusiveScan = 1,
        SpvGroupOperationExclusiveScan = 2,
        SpvGroupOperationClusteredReduce = 3,
        SpvGroupOperationPartitionedReduceNV = 6,
        SpvGroupOperationPartitionedInclusiveScanNV = 7,
        SpvGroupOperationPartitionedExclusiveScanNV = 8,
        SpvGroupOperationMax = 0x7fffffff,
    }

    public enum SpvKernelEnqueueFlags_
    {
        SpvKernelEnqueueFlagsNoWait = 0,
        SpvKernelEnqueueFlagsWaitKernel = 1,
        SpvKernelEnqueueFlagsWaitWorkGroup = 2,
        SpvKernelEnqueueFlagsMax = 0x7fffffff,
    }

    public enum SpvKernelProfilingInfoShift_
    {
        SpvKernelProfilingInfoCmdExecTimeShift = 0,
        SpvKernelProfilingInfoMax = 0x7fffffff,
    }

    public enum SpvKernelProfilingInfoMask_
    {
        SpvKernelProfilingInfoMaskNone = 0,
        SpvKernelProfilingInfoCmdExecTimeMask = 0x00000001,
    }

    public enum SpvCapability_
    {
        SpvCapabilityMatrix = 0,
        SpvCapabilityShader = 1,
        SpvCapabilityGeometry = 2,
        SpvCapabilityTessellation = 3,
        SpvCapabilityAddresses = 4,
        SpvCapabilityLinkage = 5,
        SpvCapabilityKernel = 6,
        SpvCapabilityVector16 = 7,
        SpvCapabilityFloat16Buffer = 8,
        SpvCapabilityFloat16 = 9,
        SpvCapabilityFloat64 = 10,
        SpvCapabilityInt64 = 11,
        SpvCapabilityInt64Atomics = 12,
        SpvCapabilityImageBasic = 13,
        SpvCapabilityImageReadWrite = 14,
        SpvCapabilityImageMipmap = 15,
        SpvCapabilityPipes = 17,
        SpvCapabilityGroups = 18,
        SpvCapabilityDeviceEnqueue = 19,
        SpvCapabilityLiteralSampler = 20,
        SpvCapabilityAtomicStorage = 21,
        SpvCapabilityInt16 = 22,
        SpvCapabilityTessellationPointSize = 23,
        SpvCapabilityGeometryPointSize = 24,
        SpvCapabilityImageGatherExtended = 25,
        SpvCapabilityStorageImageMultisample = 27,
        SpvCapabilityUniformBufferArrayDynamicIndexing = 28,
        SpvCapabilitySampledImageArrayDynamicIndexing = 29,
        SpvCapabilityStorageBufferArrayDynamicIndexing = 30,
        SpvCapabilityStorageImageArrayDynamicIndexing = 31,
        SpvCapabilityClipDistance = 32,
        SpvCapabilityCullDistance = 33,
        SpvCapabilityImageCubeArray = 34,
        SpvCapabilitySampleRateShading = 35,
        SpvCapabilityImageRect = 36,
        SpvCapabilitySampledRect = 37,
        SpvCapabilityGenericPointer = 38,
        SpvCapabilityInt8 = 39,
        SpvCapabilityInputAttachment = 40,
        SpvCapabilitySparseResidency = 41,
        SpvCapabilityMinLod = 42,
        SpvCapabilitySampled1D = 43,
        SpvCapabilityImage1D = 44,
        SpvCapabilitySampledCubeArray = 45,
        SpvCapabilitySampledBuffer = 46,
        SpvCapabilityImageBuffer = 47,
        SpvCapabilityImageMSArray = 48,
        SpvCapabilityStorageImageExtendedFormats = 49,
        SpvCapabilityImageQuery = 50,
        SpvCapabilityDerivativeControl = 51,
        SpvCapabilityInterpolationFunction = 52,
        SpvCapabilityTransformFeedback = 53,
        SpvCapabilityGeometryStreams = 54,
        SpvCapabilityStorageImageReadWithoutFormat = 55,
        SpvCapabilityStorageImageWriteWithoutFormat = 56,
        SpvCapabilityMultiViewport = 57,
        SpvCapabilitySubgroupDispatch = 58,
        SpvCapabilityNamedBarrier = 59,
        SpvCapabilityPipeStorage = 60,
        SpvCapabilityGroupNonUniform = 61,
        SpvCapabilityGroupNonUniformVote = 62,
        SpvCapabilityGroupNonUniformArithmetic = 63,
        SpvCapabilityGroupNonUniformBallot = 64,
        SpvCapabilityGroupNonUniformShuffle = 65,
        SpvCapabilityGroupNonUniformShuffleRelative = 66,
        SpvCapabilityGroupNonUniformClustered = 67,
        SpvCapabilityGroupNonUniformQuad = 68,
        SpvCapabilityShaderLayer = 69,
        SpvCapabilityShaderViewportIndex = 70,
        SpvCapabilityUniformDecoration = 71,
        SpvCapabilityFragmentShadingRateKHR = 4422,
        SpvCapabilitySubgroupBallotKHR = 4423,
        SpvCapabilityDrawParameters = 4427,
        SpvCapabilityWorkgroupMemoryExplicitLayoutKHR = 4428,
        SpvCapabilityWorkgroupMemoryExplicitLayout8BitAccessKHR = 4429,
        SpvCapabilityWorkgroupMemoryExplicitLayout16BitAccessKHR = 4430,
        SpvCapabilitySubgroupVoteKHR = 4431,
        SpvCapabilityStorageBuffer16BitAccess = 4433,
        SpvCapabilityStorageUniformBufferBlock16 = 4433,
        SpvCapabilityStorageUniform16 = 4434,
        SpvCapabilityUniformAndStorageBuffer16BitAccess = 4434,
        SpvCapabilityStoragePushConstant16 = 4435,
        SpvCapabilityStorageInputOutput16 = 4436,
        SpvCapabilityDeviceGroup = 4437,
        SpvCapabilityMultiView = 4439,
        SpvCapabilityVariablePointersStorageBuffer = 4441,
        SpvCapabilityVariablePointers = 4442,
        SpvCapabilityAtomicStorageOps = 4445,
        SpvCapabilitySampleMaskPostDepthCoverage = 4447,
        SpvCapabilityStorageBuffer8BitAccess = 4448,
        SpvCapabilityUniformAndStorageBuffer8BitAccess = 4449,
        SpvCapabilityStoragePushConstant8 = 4450,
        SpvCapabilityDenormPreserve = 4464,
        SpvCapabilityDenormFlushToZero = 4465,
        SpvCapabilitySignedZeroInfNanPreserve = 4466,
        SpvCapabilityRoundingModeRTE = 4467,
        SpvCapabilityRoundingModeRTZ = 4468,
        SpvCapabilityRayQueryProvisionalKHR = 4471,
        SpvCapabilityRayQueryKHR = 4472,
        SpvCapabilityRayTraversalPrimitiveCullingKHR = 4478,
        SpvCapabilityRayTracingKHR = 4479,
        SpvCapabilityFloat16ImageAMD = 5008,
        SpvCapabilityImageGatherBiasLodAMD = 5009,
        SpvCapabilityFragmentMaskAMD = 5010,
        SpvCapabilityStencilExportEXT = 5013,
        SpvCapabilityImageReadWriteLodAMD = 5015,
        SpvCapabilityInt64ImageEXT = 5016,
        SpvCapabilityShaderClockKHR = 5055,
        SpvCapabilitySampleMaskOverrideCoverageNV = 5249,
        SpvCapabilityGeometryShaderPassthroughNV = 5251,
        SpvCapabilityShaderViewportIndexLayerEXT = 5254,
        SpvCapabilityShaderViewportIndexLayerNV = 5254,
        SpvCapabilityShaderViewportMaskNV = 5255,
        SpvCapabilityShaderStereoViewNV = 5259,
        SpvCapabilityPerViewAttributesNV = 5260,
        SpvCapabilityFragmentFullyCoveredEXT = 5265,
        SpvCapabilityMeshShadingNV = 5266,
        SpvCapabilityImageFootprintNV = 5282,
        SpvCapabilityFragmentBarycentricKHR = 5284,
        SpvCapabilityFragmentBarycentricNV = 5284,
        SpvCapabilityComputeDerivativeGroupQuadsNV = 5288,
        SpvCapabilityFragmentDensityEXT = 5291,
        SpvCapabilityShadingRateNV = 5291,
        SpvCapabilityGroupNonUniformPartitionedNV = 5297,
        SpvCapabilityShaderNonUniform = 5301,
        SpvCapabilityShaderNonUniformEXT = 5301,
        SpvCapabilityRuntimeDescriptorArray = 5302,
        SpvCapabilityRuntimeDescriptorArrayEXT = 5302,
        SpvCapabilityInputAttachmentArrayDynamicIndexing = 5303,
        SpvCapabilityInputAttachmentArrayDynamicIndexingEXT = 5303,
        SpvCapabilityUniformTexelBufferArrayDynamicIndexing = 5304,
        SpvCapabilityUniformTexelBufferArrayDynamicIndexingEXT = 5304,
        SpvCapabilityStorageTexelBufferArrayDynamicIndexing = 5305,
        SpvCapabilityStorageTexelBufferArrayDynamicIndexingEXT = 5305,
        SpvCapabilityUniformBufferArrayNonUniformIndexing = 5306,
        SpvCapabilityUniformBufferArrayNonUniformIndexingEXT = 5306,
        SpvCapabilitySampledImageArrayNonUniformIndexing = 5307,
        SpvCapabilitySampledImageArrayNonUniformIndexingEXT = 5307,
        SpvCapabilityStorageBufferArrayNonUniformIndexing = 5308,
        SpvCapabilityStorageBufferArrayNonUniformIndexingEXT = 5308,
        SpvCapabilityStorageImageArrayNonUniformIndexing = 5309,
        SpvCapabilityStorageImageArrayNonUniformIndexingEXT = 5309,
        SpvCapabilityInputAttachmentArrayNonUniformIndexing = 5310,
        SpvCapabilityInputAttachmentArrayNonUniformIndexingEXT = 5310,
        SpvCapabilityUniformTexelBufferArrayNonUniformIndexing = 5311,
        SpvCapabilityUniformTexelBufferArrayNonUniformIndexingEXT = 5311,
        SpvCapabilityStorageTexelBufferArrayNonUniformIndexing = 5312,
        SpvCapabilityStorageTexelBufferArrayNonUniformIndexingEXT = 5312,
        SpvCapabilityRayTracingNV = 5340,
        SpvCapabilityRayTracingMotionBlurNV = 5341,
        SpvCapabilityVulkanMemoryModel = 5345,
        SpvCapabilityVulkanMemoryModelKHR = 5345,
        SpvCapabilityVulkanMemoryModelDeviceScope = 5346,
        SpvCapabilityVulkanMemoryModelDeviceScopeKHR = 5346,
        SpvCapabilityPhysicalStorageBufferAddresses = 5347,
        SpvCapabilityPhysicalStorageBufferAddressesEXT = 5347,
        SpvCapabilityComputeDerivativeGroupLinearNV = 5350,
        SpvCapabilityRayTracingProvisionalKHR = 5353,
        SpvCapabilityCooperativeMatrixNV = 5357,
        SpvCapabilityFragmentShaderSampleInterlockEXT = 5363,
        SpvCapabilityFragmentShaderShadingRateInterlockEXT = 5372,
        SpvCapabilityShaderSMBuiltinsNV = 5373,
        SpvCapabilityFragmentShaderPixelInterlockEXT = 5378,
        SpvCapabilityDemoteToHelperInvocation = 5379,
        SpvCapabilityDemoteToHelperInvocationEXT = 5379,
        SpvCapabilityBindlessTextureNV = 5390,
        SpvCapabilitySubgroupShuffleINTEL = 5568,
        SpvCapabilitySubgroupBufferBlockIOINTEL = 5569,
        SpvCapabilitySubgroupImageBlockIOINTEL = 5570,
        SpvCapabilitySubgroupImageMediaBlockIOINTEL = 5579,
        SpvCapabilityRoundToInfinityINTEL = 5582,
        SpvCapabilityFloatingPointModeINTEL = 5583,
        SpvCapabilityIntegerFunctions2INTEL = 5584,
        SpvCapabilityFunctionPointersINTEL = 5603,
        SpvCapabilityIndirectReferencesINTEL = 5604,
        SpvCapabilityAsmINTEL = 5606,
        SpvCapabilityAtomicFloat32MinMaxEXT = 5612,
        SpvCapabilityAtomicFloat64MinMaxEXT = 5613,
        SpvCapabilityAtomicFloat16MinMaxEXT = 5616,
        SpvCapabilityVectorComputeINTEL = 5617,
        SpvCapabilityVectorAnyINTEL = 5619,
        SpvCapabilityExpectAssumeKHR = 5629,
        SpvCapabilitySubgroupAvcMotionEstimationINTEL = 5696,
        SpvCapabilitySubgroupAvcMotionEstimationIntraINTEL = 5697,
        SpvCapabilitySubgroupAvcMotionEstimationChromaINTEL = 5698,
        SpvCapabilityVariableLengthArrayINTEL = 5817,
        SpvCapabilityFunctionFloatControlINTEL = 5821,
        SpvCapabilityFPGAMemoryAttributesINTEL = 5824,
        SpvCapabilityFPFastMathModeINTEL = 5837,
        SpvCapabilityArbitraryPrecisionIntegersINTEL = 5844,
        SpvCapabilityArbitraryPrecisionFloatingPointINTEL = 5845,
        SpvCapabilityUnstructuredLoopControlsINTEL = 5886,
        SpvCapabilityFPGALoopControlsINTEL = 5888,
        SpvCapabilityKernelAttributesINTEL = 5892,
        SpvCapabilityFPGAKernelAttributesINTEL = 5897,
        SpvCapabilityFPGAMemoryAccessesINTEL = 5898,
        SpvCapabilityFPGAClusterAttributesINTEL = 5904,
        SpvCapabilityLoopFuseINTEL = 5906,
        SpvCapabilityMemoryAccessAliasingINTEL = 5910,
        SpvCapabilityFPGABufferLocationINTEL = 5920,
        SpvCapabilityArbitraryPrecisionFixedPointINTEL = 5922,
        SpvCapabilityUSMStorageClassesINTEL = 5935,
        SpvCapabilityIOPipesINTEL = 5943,
        SpvCapabilityBlockingPipesINTEL = 5945,
        SpvCapabilityFPGARegINTEL = 5948,
        SpvCapabilityDotProductInputAll = 6016,
        SpvCapabilityDotProductInputAllKHR = 6016,
        SpvCapabilityDotProductInput4x8Bit = 6017,
        SpvCapabilityDotProductInput4x8BitKHR = 6017,
        SpvCapabilityDotProductInput4x8BitPacked = 6018,
        SpvCapabilityDotProductInput4x8BitPackedKHR = 6018,
        SpvCapabilityDotProduct = 6019,
        SpvCapabilityDotProductKHR = 6019,
        SpvCapabilityRayCullMaskKHR = 6020,
        SpvCapabilityBitInstructions = 6025,
        SpvCapabilityGroupNonUniformRotateKHR = 6026,
        SpvCapabilityAtomicFloat32AddEXT = 6033,
        SpvCapabilityAtomicFloat64AddEXT = 6034,
        SpvCapabilityLongConstantCompositeINTEL = 6089,
        SpvCapabilityOptNoneINTEL = 6094,
        SpvCapabilityAtomicFloat16AddEXT = 6095,
        SpvCapabilityDebugInfoModuleINTEL = 6114,
        SpvCapabilitySplitBarrierINTEL = 6141,
        SpvCapabilityGroupUniformArithmeticKHR = 6400,
        SpvCapabilityMax = 0x7fffffff,
    }

    public enum SpvRayFlagsShift_
    {
        SpvRayFlagsOpaqueKHRShift = 0,
        SpvRayFlagsNoOpaqueKHRShift = 1,
        SpvRayFlagsTerminateOnFirstHitKHRShift = 2,
        SpvRayFlagsSkipClosestHitShaderKHRShift = 3,
        SpvRayFlagsCullBackFacingTrianglesKHRShift = 4,
        SpvRayFlagsCullFrontFacingTrianglesKHRShift = 5,
        SpvRayFlagsCullOpaqueKHRShift = 6,
        SpvRayFlagsCullNoOpaqueKHRShift = 7,
        SpvRayFlagsSkipTrianglesKHRShift = 8,
        SpvRayFlagsSkipAABBsKHRShift = 9,
        SpvRayFlagsMax = 0x7fffffff,
    }

    public enum SpvRayFlagsMask_
    {
        SpvRayFlagsMaskNone = 0,
        SpvRayFlagsOpaqueKHRMask = 0x00000001,
        SpvRayFlagsNoOpaqueKHRMask = 0x00000002,
        SpvRayFlagsTerminateOnFirstHitKHRMask = 0x00000004,
        SpvRayFlagsSkipClosestHitShaderKHRMask = 0x00000008,
        SpvRayFlagsCullBackFacingTrianglesKHRMask = 0x00000010,
        SpvRayFlagsCullFrontFacingTrianglesKHRMask = 0x00000020,
        SpvRayFlagsCullOpaqueKHRMask = 0x00000040,
        SpvRayFlagsCullNoOpaqueKHRMask = 0x00000080,
        SpvRayFlagsSkipTrianglesKHRMask = 0x00000100,
        SpvRayFlagsSkipAABBsKHRMask = 0x00000200,
    }

    public enum SpvRayQueryIntersection_
    {
        SpvRayQueryIntersectionRayQueryCandidateIntersectionKHR = 0,
        SpvRayQueryIntersectionRayQueryCommittedIntersectionKHR = 1,
        SpvRayQueryIntersectionMax = 0x7fffffff,
    }

    public enum SpvRayQueryCommittedIntersectionType_
    {
        SpvRayQueryCommittedIntersectionTypeRayQueryCommittedIntersectionNoneKHR = 0,
        SpvRayQueryCommittedIntersectionTypeRayQueryCommittedIntersectionTriangleKHR = 1,
        SpvRayQueryCommittedIntersectionTypeRayQueryCommittedIntersectionGeneratedKHR = 2,
        SpvRayQueryCommittedIntersectionTypeMax = 0x7fffffff,
    }

    public enum SpvRayQueryCandidateIntersectionType_
    {
        SpvRayQueryCandidateIntersectionTypeRayQueryCandidateIntersectionTriangleKHR = 0,
        SpvRayQueryCandidateIntersectionTypeRayQueryCandidateIntersectionAABBKHR = 1,
        SpvRayQueryCandidateIntersectionTypeMax = 0x7fffffff,
    }

    public enum SpvFragmentShadingRateShift_
    {
        SpvFragmentShadingRateVertical2PixelsShift = 0,
        SpvFragmentShadingRateVertical4PixelsShift = 1,
        SpvFragmentShadingRateHorizontal2PixelsShift = 2,
        SpvFragmentShadingRateHorizontal4PixelsShift = 3,
        SpvFragmentShadingRateMax = 0x7fffffff,
    }

    public enum SpvFragmentShadingRateMask_
    {
        SpvFragmentShadingRateMaskNone = 0,
        SpvFragmentShadingRateVertical2PixelsMask = 0x00000001,
        SpvFragmentShadingRateVertical4PixelsMask = 0x00000002,
        SpvFragmentShadingRateHorizontal2PixelsMask = 0x00000004,
        SpvFragmentShadingRateHorizontal4PixelsMask = 0x00000008,
    }

    public enum SpvFPDenormMode_
    {
        SpvFPDenormModePreserve = 0,
        SpvFPDenormModeFlushToZero = 1,
        SpvFPDenormModeMax = 0x7fffffff,
    }

    public enum SpvFPOperationMode_
    {
        SpvFPOperationModeIEEE = 0,
        SpvFPOperationModeALT = 1,
        SpvFPOperationModeMax = 0x7fffffff,
    }

    public enum SpvQuantizationModes_
    {
        SpvQuantizationModesTRN = 0,
        SpvQuantizationModesTRN_ZERO = 1,
        SpvQuantizationModesRND = 2,
        SpvQuantizationModesRND_ZERO = 3,
        SpvQuantizationModesRND_INF = 4,
        SpvQuantizationModesRND_MIN_INF = 5,
        SpvQuantizationModesRND_CONV = 6,
        SpvQuantizationModesRND_CONV_ODD = 7,
        SpvQuantizationModesMax = 0x7fffffff,
    }

    public enum SpvOverflowModes_
    {
        SpvOverflowModesWRAP = 0,
        SpvOverflowModesSAT = 1,
        SpvOverflowModesSAT_ZERO = 2,
        SpvOverflowModesSAT_SYM = 3,
        SpvOverflowModesMax = 0x7fffffff,
    }

    public enum SpvPackedVectorFormat_
    {
        SpvPackedVectorFormatPackedVectorFormat4x8Bit = 0,
        SpvPackedVectorFormatPackedVectorFormat4x8BitKHR = 0,
        SpvPackedVectorFormatMax = 0x7fffffff,
    }

    public enum SpvOp_
    {
        SpvOpNop = 0,
        SpvOpUndef = 1,
        SpvOpSourceContinued = 2,
        SpvOpSource = 3,
        SpvOpSourceExtension = 4,
        SpvOpName = 5,
        SpvOpMemberName = 6,
        SpvOpString = 7,
        SpvOpLine = 8,
        SpvOpExtension = 10,
        SpvOpExtInstImport = 11,
        SpvOpExtInst = 12,
        SpvOpMemoryModel = 14,
        SpvOpEntryPoint = 15,
        SpvOpExecutionMode = 16,
        SpvOpCapability = 17,
        SpvOpTypeVoid = 19,
        SpvOpTypeBool = 20,
        SpvOpTypeInt = 21,
        SpvOpTypeFloat = 22,
        SpvOpTypeVector = 23,
        SpvOpTypeMatrix = 24,
        SpvOpTypeImage = 25,
        SpvOpTypeSampler = 26,
        SpvOpTypeSampledImage = 27,
        SpvOpTypeArray = 28,
        SpvOpTypeRuntimeArray = 29,
        SpvOpTypeStruct = 30,
        SpvOpTypeOpaque = 31,
        SpvOpTypePointer = 32,
        SpvOpTypeFunction = 33,
        SpvOpTypeEvent = 34,
        SpvOpTypeDeviceEvent = 35,
        SpvOpTypeReserveId = 36,
        SpvOpTypeQueue = 37,
        SpvOpTypePipe = 38,
        SpvOpTypeForwardPointer = 39,
        SpvOpConstantTrue = 41,
        SpvOpConstantFalse = 42,
        SpvOpConstant = 43,
        SpvOpConstantComposite = 44,
        SpvOpConstantSampler = 45,
        SpvOpConstantNull = 46,
        SpvOpSpecConstantTrue = 48,
        SpvOpSpecConstantFalse = 49,
        SpvOpSpecConstant = 50,
        SpvOpSpecConstantComposite = 51,
        SpvOpSpecConstantOp = 52,
        SpvOpFunction = 54,
        SpvOpFunctionParameter = 55,
        SpvOpFunctionEnd = 56,
        SpvOpFunctionCall = 57,
        SpvOpVariable = 59,
        SpvOpImageTexelPointer = 60,
        SpvOpLoad = 61,
        SpvOpStore = 62,
        SpvOpCopyMemory = 63,
        SpvOpCopyMemorySized = 64,
        SpvOpAccessChain = 65,
        SpvOpInBoundsAccessChain = 66,
        SpvOpPtrAccessChain = 67,
        SpvOpArrayLength = 68,
        SpvOpGenericPtrMemSemantics = 69,
        SpvOpInBoundsPtrAccessChain = 70,
        SpvOpDecorate = 71,
        SpvOpMemberDecorate = 72,
        SpvOpDecorationGroup = 73,
        SpvOpGroupDecorate = 74,
        SpvOpGroupMemberDecorate = 75,
        SpvOpVectorExtractDynamic = 77,
        SpvOpVectorInsertDynamic = 78,
        SpvOpVectorShuffle = 79,
        SpvOpCompositeConstruct = 80,
        SpvOpCompositeExtract = 81,
        SpvOpCompositeInsert = 82,
        SpvOpCopyObject = 83,
        SpvOpTranspose = 84,
        SpvOpSampledImage = 86,
        SpvOpImageSampleImplicitLod = 87,
        SpvOpImageSampleExplicitLod = 88,
        SpvOpImageSampleDrefImplicitLod = 89,
        SpvOpImageSampleDrefExplicitLod = 90,
        SpvOpImageSampleProjImplicitLod = 91,
        SpvOpImageSampleProjExplicitLod = 92,
        SpvOpImageSampleProjDrefImplicitLod = 93,
        SpvOpImageSampleProjDrefExplicitLod = 94,
        SpvOpImageFetch = 95,
        SpvOpImageGather = 96,
        SpvOpImageDrefGather = 97,
        SpvOpImageRead = 98,
        SpvOpImageWrite = 99,
        SpvOpImage = 100,
        SpvOpImageQueryFormat = 101,
        SpvOpImageQueryOrder = 102,
        SpvOpImageQuerySizeLod = 103,
        SpvOpImageQuerySize = 104,
        SpvOpImageQueryLod = 105,
        SpvOpImageQueryLevels = 106,
        SpvOpImageQuerySamples = 107,
        SpvOpConvertFToU = 109,
        SpvOpConvertFToS = 110,
        SpvOpConvertSToF = 111,
        SpvOpConvertUToF = 112,
        SpvOpUConvert = 113,
        SpvOpSConvert = 114,
        SpvOpFConvert = 115,
        SpvOpQuantizeToF16 = 116,
        SpvOpConvertPtrToU = 117,
        SpvOpSatConvertSToU = 118,
        SpvOpSatConvertUToS = 119,
        SpvOpConvertUToPtr = 120,
        SpvOpPtrCastToGeneric = 121,
        SpvOpGenericCastToPtr = 122,
        SpvOpGenericCastToPtrExplicit = 123,
        SpvOpBitcast = 124,
        SpvOpSNegate = 126,
        SpvOpFNegate = 127,
        SpvOpIAdd = 128,
        SpvOpFAdd = 129,
        SpvOpISub = 130,
        SpvOpFSub = 131,
        SpvOpIMul = 132,
        SpvOpFMul = 133,
        SpvOpUDiv = 134,
        SpvOpSDiv = 135,
        SpvOpFDiv = 136,
        SpvOpUMod = 137,
        SpvOpSRem = 138,
        SpvOpSMod = 139,
        SpvOpFRem = 140,
        SpvOpFMod = 141,
        SpvOpVectorTimesScalar = 142,
        SpvOpMatrixTimesScalar = 143,
        SpvOpVectorTimesMatrix = 144,
        SpvOpMatrixTimesVector = 145,
        SpvOpMatrixTimesMatrix = 146,
        SpvOpOuterProduct = 147,
        SpvOpDot = 148,
        SpvOpIAddCarry = 149,
        SpvOpISubBorrow = 150,
        SpvOpUMulExtended = 151,
        SpvOpSMulExtended = 152,
        SpvOpAny = 154,
        SpvOpAll = 155,
        SpvOpIsNan = 156,
        SpvOpIsInf = 157,
        SpvOpIsFinite = 158,
        SpvOpIsNormal = 159,
        SpvOpSignBitSet = 160,
        SpvOpLessOrGreater = 161,
        SpvOpOrdered = 162,
        SpvOpUnordered = 163,
        SpvOpLogicalEqual = 164,
        SpvOpLogicalNotEqual = 165,
        SpvOpLogicalOr = 166,
        SpvOpLogicalAnd = 167,
        SpvOpLogicalNot = 168,
        SpvOpSelect = 169,
        SpvOpIEqual = 170,
        SpvOpINotEqual = 171,
        SpvOpUGreaterThan = 172,
        SpvOpSGreaterThan = 173,
        SpvOpUGreaterThanEqual = 174,
        SpvOpSGreaterThanEqual = 175,
        SpvOpULessThan = 176,
        SpvOpSLessThan = 177,
        SpvOpULessThanEqual = 178,
        SpvOpSLessThanEqual = 179,
        SpvOpFOrdEqual = 180,
        SpvOpFUnordEqual = 181,
        SpvOpFOrdNotEqual = 182,
        SpvOpFUnordNotEqual = 183,
        SpvOpFOrdLessThan = 184,
        SpvOpFUnordLessThan = 185,
        SpvOpFOrdGreaterThan = 186,
        SpvOpFUnordGreaterThan = 187,
        SpvOpFOrdLessThanEqual = 188,
        SpvOpFUnordLessThanEqual = 189,
        SpvOpFOrdGreaterThanEqual = 190,
        SpvOpFUnordGreaterThanEqual = 191,
        SpvOpShiftRightLogical = 194,
        SpvOpShiftRightArithmetic = 195,
        SpvOpShiftLeftLogical = 196,
        SpvOpBitwiseOr = 197,
        SpvOpBitwiseXor = 198,
        SpvOpBitwiseAnd = 199,
        SpvOpNot = 200,
        SpvOpBitFieldInsert = 201,
        SpvOpBitFieldSExtract = 202,
        SpvOpBitFieldUExtract = 203,
        SpvOpBitReverse = 204,
        SpvOpBitCount = 205,
        SpvOpDPdx = 207,
        SpvOpDPdy = 208,
        SpvOpFwidth = 209,
        SpvOpDPdxFine = 210,
        SpvOpDPdyFine = 211,
        SpvOpFwidthFine = 212,
        SpvOpDPdxCoarse = 213,
        SpvOpDPdyCoarse = 214,
        SpvOpFwidthCoarse = 215,
        SpvOpEmitVertex = 218,
        SpvOpEndPrimitive = 219,
        SpvOpEmitStreamVertex = 220,
        SpvOpEndStreamPrimitive = 221,
        SpvOpControlBarrier = 224,
        SpvOpMemoryBarrier = 225,
        SpvOpAtomicLoad = 227,
        SpvOpAtomicStore = 228,
        SpvOpAtomicExchange = 229,
        SpvOpAtomicCompareExchange = 230,
        SpvOpAtomicCompareExchangeWeak = 231,
        SpvOpAtomicIIncrement = 232,
        SpvOpAtomicIDecrement = 233,
        SpvOpAtomicIAdd = 234,
        SpvOpAtomicISub = 235,
        SpvOpAtomicSMin = 236,
        SpvOpAtomicUMin = 237,
        SpvOpAtomicSMax = 238,
        SpvOpAtomicUMax = 239,
        SpvOpAtomicAnd = 240,
        SpvOpAtomicOr = 241,
        SpvOpAtomicXor = 242,
        SpvOpPhi = 245,
        SpvOpLoopMerge = 246,
        SpvOpSelectionMerge = 247,
        SpvOpLabel = 248,
        SpvOpBranch = 249,
        SpvOpBranchConditional = 250,
        SpvOpSwitch = 251,
        SpvOpKill = 252,
        SpvOpReturn = 253,
        SpvOpReturnValue = 254,
        SpvOpUnreachable = 255,
        SpvOpLifetimeStart = 256,
        SpvOpLifetimeStop = 257,
        SpvOpGroupAsyncCopy = 259,
        SpvOpGroupWaitEvents = 260,
        SpvOpGroupAll = 261,
        SpvOpGroupAny = 262,
        SpvOpGroupBroadcast = 263,
        SpvOpGroupIAdd = 264,
        SpvOpGroupFAdd = 265,
        SpvOpGroupFMin = 266,
        SpvOpGroupUMin = 267,
        SpvOpGroupSMin = 268,
        SpvOpGroupFMax = 269,
        SpvOpGroupUMax = 270,
        SpvOpGroupSMax = 271,
        SpvOpReadPipe = 274,
        SpvOpWritePipe = 275,
        SpvOpReservedReadPipe = 276,
        SpvOpReservedWritePipe = 277,
        SpvOpReserveReadPipePackets = 278,
        SpvOpReserveWritePipePackets = 279,
        SpvOpCommitReadPipe = 280,
        SpvOpCommitWritePipe = 281,
        SpvOpIsValidReserveId = 282,
        SpvOpGetNumPipePackets = 283,
        SpvOpGetMaxPipePackets = 284,
        SpvOpGroupReserveReadPipePackets = 285,
        SpvOpGroupReserveWritePipePackets = 286,
        SpvOpGroupCommitReadPipe = 287,
        SpvOpGroupCommitWritePipe = 288,
        SpvOpEnqueueMarker = 291,
        SpvOpEnqueueKernel = 292,
        SpvOpGetKernelNDrangeSubGroupCount = 293,
        SpvOpGetKernelNDrangeMaxSubGroupSize = 294,
        SpvOpGetKernelWorkGroupSize = 295,
        SpvOpGetKernelPreferredWorkGroupSizeMultiple = 296,
        SpvOpRetainEvent = 297,
        SpvOpReleaseEvent = 298,
        SpvOpCreateUserEvent = 299,
        SpvOpIsValidEvent = 300,
        SpvOpSetUserEventStatus = 301,
        SpvOpCaptureEventProfilingInfo = 302,
        SpvOpGetDefaultQueue = 303,
        SpvOpBuildNDRange = 304,
        SpvOpImageSparseSampleImplicitLod = 305,
        SpvOpImageSparseSampleExplicitLod = 306,
        SpvOpImageSparseSampleDrefImplicitLod = 307,
        SpvOpImageSparseSampleDrefExplicitLod = 308,
        SpvOpImageSparseSampleProjImplicitLod = 309,
        SpvOpImageSparseSampleProjExplicitLod = 310,
        SpvOpImageSparseSampleProjDrefImplicitLod = 311,
        SpvOpImageSparseSampleProjDrefExplicitLod = 312,
        SpvOpImageSparseFetch = 313,
        SpvOpImageSparseGather = 314,
        SpvOpImageSparseDrefGather = 315,
        SpvOpImageSparseTexelsResident = 316,
        SpvOpNoLine = 317,
        SpvOpAtomicFlagTestAndSet = 318,
        SpvOpAtomicFlagClear = 319,
        SpvOpImageSparseRead = 320,
        SpvOpSizeOf = 321,
        SpvOpTypePipeStorage = 322,
        SpvOpConstantPipeStorage = 323,
        SpvOpCreatePipeFromPipeStorage = 324,
        SpvOpGetKernelLocalSizeForSubgroupCount = 325,
        SpvOpGetKernelMaxNumSubgroups = 326,
        SpvOpTypeNamedBarrier = 327,
        SpvOpNamedBarrierInitialize = 328,
        SpvOpMemoryNamedBarrier = 329,
        SpvOpModuleProcessed = 330,
        SpvOpExecutionModeId = 331,
        SpvOpDecorateId = 332,
        SpvOpGroupNonUniformElect = 333,
        SpvOpGroupNonUniformAll = 334,
        SpvOpGroupNonUniformAny = 335,
        SpvOpGroupNonUniformAllEqual = 336,
        SpvOpGroupNonUniformBroadcast = 337,
        SpvOpGroupNonUniformBroadcastFirst = 338,
        SpvOpGroupNonUniformBallot = 339,
        SpvOpGroupNonUniformInverseBallot = 340,
        SpvOpGroupNonUniformBallotBitExtract = 341,
        SpvOpGroupNonUniformBallotBitCount = 342,
        SpvOpGroupNonUniformBallotFindLSB = 343,
        SpvOpGroupNonUniformBallotFindMSB = 344,
        SpvOpGroupNonUniformShuffle = 345,
        SpvOpGroupNonUniformShuffleXor = 346,
        SpvOpGroupNonUniformShuffleUp = 347,
        SpvOpGroupNonUniformShuffleDown = 348,
        SpvOpGroupNonUniformIAdd = 349,
        SpvOpGroupNonUniformFAdd = 350,
        SpvOpGroupNonUniformIMul = 351,
        SpvOpGroupNonUniformFMul = 352,
        SpvOpGroupNonUniformSMin = 353,
        SpvOpGroupNonUniformUMin = 354,
        SpvOpGroupNonUniformFMin = 355,
        SpvOpGroupNonUniformSMax = 356,
        SpvOpGroupNonUniformUMax = 357,
        SpvOpGroupNonUniformFMax = 358,
        SpvOpGroupNonUniformBitwiseAnd = 359,
        SpvOpGroupNonUniformBitwiseOr = 360,
        SpvOpGroupNonUniformBitwiseXor = 361,
        SpvOpGroupNonUniformLogicalAnd = 362,
        SpvOpGroupNonUniformLogicalOr = 363,
        SpvOpGroupNonUniformLogicalXor = 364,
        SpvOpGroupNonUniformQuadBroadcast = 365,
        SpvOpGroupNonUniformQuadSwap = 366,
        SpvOpCopyLogical = 400,
        SpvOpPtrEqual = 401,
        SpvOpPtrNotEqual = 402,
        SpvOpPtrDiff = 403,
        SpvOpTerminateInvocation = 4416,
        SpvOpSubgroupBallotKHR = 4421,
        SpvOpSubgroupFirstInvocationKHR = 4422,
        SpvOpSubgroupAllKHR = 4428,
        SpvOpSubgroupAnyKHR = 4429,
        SpvOpSubgroupAllEqualKHR = 4430,
        SpvOpGroupNonUniformRotateKHR = 4431,
        SpvOpSubgroupReadInvocationKHR = 4432,
        SpvOpTraceRayKHR = 4445,
        SpvOpExecuteCallableKHR = 4446,
        SpvOpConvertUToAccelerationStructureKHR = 4447,
        SpvOpIgnoreIntersectionKHR = 4448,
        SpvOpTerminateRayKHR = 4449,
        SpvOpSDot = 4450,
        SpvOpSDotKHR = 4450,
        SpvOpUDot = 4451,
        SpvOpUDotKHR = 4451,
        SpvOpSUDot = 4452,
        SpvOpSUDotKHR = 4452,
        SpvOpSDotAccSat = 4453,
        SpvOpSDotAccSatKHR = 4453,
        SpvOpUDotAccSat = 4454,
        SpvOpUDotAccSatKHR = 4454,
        SpvOpSUDotAccSat = 4455,
        SpvOpSUDotAccSatKHR = 4455,
        SpvOpTypeRayQueryKHR = 4472,
        SpvOpRayQueryInitializeKHR = 4473,
        SpvOpRayQueryTerminateKHR = 4474,
        SpvOpRayQueryGenerateIntersectionKHR = 4475,
        SpvOpRayQueryConfirmIntersectionKHR = 4476,
        SpvOpRayQueryProceedKHR = 4477,
        SpvOpRayQueryGetIntersectionTypeKHR = 4479,
        SpvOpGroupIAddNonUniformAMD = 5000,
        SpvOpGroupFAddNonUniformAMD = 5001,
        SpvOpGroupFMinNonUniformAMD = 5002,
        SpvOpGroupUMinNonUniformAMD = 5003,
        SpvOpGroupSMinNonUniformAMD = 5004,
        SpvOpGroupFMaxNonUniformAMD = 5005,
        SpvOpGroupUMaxNonUniformAMD = 5006,
        SpvOpGroupSMaxNonUniformAMD = 5007,
        SpvOpFragmentMaskFetchAMD = 5011,
        SpvOpFragmentFetchAMD = 5012,
        SpvOpReadClockKHR = 5056,
        SpvOpImageSampleFootprintNV = 5283,
        SpvOpGroupNonUniformPartitionNV = 5296,
        SpvOpWritePackedPrimitiveIndices4x8NV = 5299,
        SpvOpReportIntersectionKHR = 5334,
        SpvOpReportIntersectionNV = 5334,
        SpvOpIgnoreIntersectionNV = 5335,
        SpvOpTerminateRayNV = 5336,
        SpvOpTraceNV = 5337,
        SpvOpTraceMotionNV = 5338,
        SpvOpTraceRayMotionNV = 5339,
        SpvOpTypeAccelerationStructureKHR = 5341,
        SpvOpTypeAccelerationStructureNV = 5341,
        SpvOpExecuteCallableNV = 5344,
        SpvOpTypeCooperativeMatrixNV = 5358,
        SpvOpCooperativeMatrixLoadNV = 5359,
        SpvOpCooperativeMatrixStoreNV = 5360,
        SpvOpCooperativeMatrixMulAddNV = 5361,
        SpvOpCooperativeMatrixLengthNV = 5362,
        SpvOpBeginInvocationInterlockEXT = 5364,
        SpvOpEndInvocationInterlockEXT = 5365,
        SpvOpDemoteToHelperInvocation = 5380,
        SpvOpDemoteToHelperInvocationEXT = 5380,
        SpvOpIsHelperInvocationEXT = 5381,
        SpvOpConvertUToImageNV = 5391,
        SpvOpConvertUToSamplerNV = 5392,
        SpvOpConvertImageToUNV = 5393,
        SpvOpConvertSamplerToUNV = 5394,
        SpvOpConvertUToSampledImageNV = 5395,
        SpvOpConvertSampledImageToUNV = 5396,
        SpvOpSamplerImageAddressingModeNV = 5397,
        SpvOpSubgroupShuffleINTEL = 5571,
        SpvOpSubgroupShuffleDownINTEL = 5572,
        SpvOpSubgroupShuffleUpINTEL = 5573,
        SpvOpSubgroupShuffleXorINTEL = 5574,
        SpvOpSubgroupBlockReadINTEL = 5575,
        SpvOpSubgroupBlockWriteINTEL = 5576,
        SpvOpSubgroupImageBlockReadINTEL = 5577,
        SpvOpSubgroupImageBlockWriteINTEL = 5578,
        SpvOpSubgroupImageMediaBlockReadINTEL = 5580,
        SpvOpSubgroupImageMediaBlockWriteINTEL = 5581,
        SpvOpUCountLeadingZerosINTEL = 5585,
        SpvOpUCountTrailingZerosINTEL = 5586,
        SpvOpAbsISubINTEL = 5587,
        SpvOpAbsUSubINTEL = 5588,
        SpvOpIAddSatINTEL = 5589,
        SpvOpUAddSatINTEL = 5590,
        SpvOpIAverageINTEL = 5591,
        SpvOpUAverageINTEL = 5592,
        SpvOpIAverageRoundedINTEL = 5593,
        SpvOpUAverageRoundedINTEL = 5594,
        SpvOpISubSatINTEL = 5595,
        SpvOpUSubSatINTEL = 5596,
        SpvOpIMul32x16INTEL = 5597,
        SpvOpUMul32x16INTEL = 5598,
        SpvOpConstantFunctionPointerINTEL = 5600,
        SpvOpFunctionPointerCallINTEL = 5601,
        SpvOpAsmTargetINTEL = 5609,
        SpvOpAsmINTEL = 5610,
        SpvOpAsmCallINTEL = 5611,
        SpvOpAtomicFMinEXT = 5614,
        SpvOpAtomicFMaxEXT = 5615,
        SpvOpAssumeTrueKHR = 5630,
        SpvOpExpectKHR = 5631,
        SpvOpDecorateString = 5632,
        SpvOpDecorateStringGOOGLE = 5632,
        SpvOpMemberDecorateString = 5633,
        SpvOpMemberDecorateStringGOOGLE = 5633,
        SpvOpVmeImageINTEL = 5699,
        SpvOpTypeVmeImageINTEL = 5700,
        SpvOpTypeAvcImePayloadINTEL = 5701,
        SpvOpTypeAvcRefPayloadINTEL = 5702,
        SpvOpTypeAvcSicPayloadINTEL = 5703,
        SpvOpTypeAvcMcePayloadINTEL = 5704,
        SpvOpTypeAvcMceResultINTEL = 5705,
        SpvOpTypeAvcImeResultINTEL = 5706,
        SpvOpTypeAvcImeResultSingleReferenceStreamoutINTEL = 5707,
        SpvOpTypeAvcImeResultDualReferenceStreamoutINTEL = 5708,
        SpvOpTypeAvcImeSingleReferenceStreaminINTEL = 5709,
        SpvOpTypeAvcImeDualReferenceStreaminINTEL = 5710,
        SpvOpTypeAvcRefResultINTEL = 5711,
        SpvOpTypeAvcSicResultINTEL = 5712,
        SpvOpSubgroupAvcMceGetDefaultInterBaseMultiReferencePenaltyINTEL = 5713,
        SpvOpSubgroupAvcMceSetInterBaseMultiReferencePenaltyINTEL = 5714,
        SpvOpSubgroupAvcMceGetDefaultInterShapePenaltyINTEL = 5715,
        SpvOpSubgroupAvcMceSetInterShapePenaltyINTEL = 5716,
        SpvOpSubgroupAvcMceGetDefaultInterDirectionPenaltyINTEL = 5717,
        SpvOpSubgroupAvcMceSetInterDirectionPenaltyINTEL = 5718,
        SpvOpSubgroupAvcMceGetDefaultIntraLumaShapePenaltyINTEL = 5719,
        SpvOpSubgroupAvcMceGetDefaultInterMotionVectorCostTableINTEL = 5720,
        SpvOpSubgroupAvcMceGetDefaultHighPenaltyCostTableINTEL = 5721,
        SpvOpSubgroupAvcMceGetDefaultMediumPenaltyCostTableINTEL = 5722,
        SpvOpSubgroupAvcMceGetDefaultLowPenaltyCostTableINTEL = 5723,
        SpvOpSubgroupAvcMceSetMotionVectorCostFunctionINTEL = 5724,
        SpvOpSubgroupAvcMceGetDefaultIntraLumaModePenaltyINTEL = 5725,
        SpvOpSubgroupAvcMceGetDefaultNonDcLumaIntraPenaltyINTEL = 5726,
        SpvOpSubgroupAvcMceGetDefaultIntraChromaModeBasePenaltyINTEL = 5727,
        SpvOpSubgroupAvcMceSetAcOnlyHaarINTEL = 5728,
        SpvOpSubgroupAvcMceSetSourceInterlacedFieldPolarityINTEL = 5729,
        SpvOpSubgroupAvcMceSetSingleReferenceInterlacedFieldPolarityINTEL = 5730,
        SpvOpSubgroupAvcMceSetDualReferenceInterlacedFieldPolaritiesINTEL = 5731,
        SpvOpSubgroupAvcMceConvertToImePayloadINTEL = 5732,
        SpvOpSubgroupAvcMceConvertToImeResultINTEL = 5733,
        SpvOpSubgroupAvcMceConvertToRefPayloadINTEL = 5734,
        SpvOpSubgroupAvcMceConvertToRefResultINTEL = 5735,
        SpvOpSubgroupAvcMceConvertToSicPayloadINTEL = 5736,
        SpvOpSubgroupAvcMceConvertToSicResultINTEL = 5737,
        SpvOpSubgroupAvcMceGetMotionVectorsINTEL = 5738,
        SpvOpSubgroupAvcMceGetInterDistortionsINTEL = 5739,
        SpvOpSubgroupAvcMceGetBestInterDistortionsINTEL = 5740,
        SpvOpSubgroupAvcMceGetInterMajorShapeINTEL = 5741,
        SpvOpSubgroupAvcMceGetInterMinorShapeINTEL = 5742,
        SpvOpSubgroupAvcMceGetInterDirectionsINTEL = 5743,
        SpvOpSubgroupAvcMceGetInterMotionVectorCountINTEL = 5744,
        SpvOpSubgroupAvcMceGetInterReferenceIdsINTEL = 5745,
        SpvOpSubgroupAvcMceGetInterReferenceInterlacedFieldPolaritiesINTEL = 5746,
        SpvOpSubgroupAvcImeInitializeINTEL = 5747,
        SpvOpSubgroupAvcImeSetSingleReferenceINTEL = 5748,
        SpvOpSubgroupAvcImeSetDualReferenceINTEL = 5749,
        SpvOpSubgroupAvcImeRefWindowSizeINTEL = 5750,
        SpvOpSubgroupAvcImeAdjustRefOffsetINTEL = 5751,
        SpvOpSubgroupAvcImeConvertToMcePayloadINTEL = 5752,
        SpvOpSubgroupAvcImeSetMaxMotionVectorCountINTEL = 5753,
        SpvOpSubgroupAvcImeSetUnidirectionalMixDisableINTEL = 5754,
        SpvOpSubgroupAvcImeSetEarlySearchTerminationThresholdINTEL = 5755,
        SpvOpSubgroupAvcImeSetWeightedSadINTEL = 5756,
        SpvOpSubgroupAvcImeEvaluateWithSingleReferenceINTEL = 5757,
        SpvOpSubgroupAvcImeEvaluateWithDualReferenceINTEL = 5758,
        SpvOpSubgroupAvcImeEvaluateWithSingleReferenceStreaminINTEL = 5759,
        SpvOpSubgroupAvcImeEvaluateWithDualReferenceStreaminINTEL = 5760,
        SpvOpSubgroupAvcImeEvaluateWithSingleReferenceStreamoutINTEL = 5761,
        SpvOpSubgroupAvcImeEvaluateWithDualReferenceStreamoutINTEL = 5762,
        SpvOpSubgroupAvcImeEvaluateWithSingleReferenceStreaminoutINTEL = 5763,
        SpvOpSubgroupAvcImeEvaluateWithDualReferenceStreaminoutINTEL = 5764,
        SpvOpSubgroupAvcImeConvertToMceResultINTEL = 5765,
        SpvOpSubgroupAvcImeGetSingleReferenceStreaminINTEL = 5766,
        SpvOpSubgroupAvcImeGetDualReferenceStreaminINTEL = 5767,
        SpvOpSubgroupAvcImeStripSingleReferenceStreamoutINTEL = 5768,
        SpvOpSubgroupAvcImeStripDualReferenceStreamoutINTEL = 5769,
        SpvOpSubgroupAvcImeGetStreamoutSingleReferenceMajorShapeMotionVectorsINTEL = 5770,
        SpvOpSubgroupAvcImeGetStreamoutSingleReferenceMajorShapeDistortionsINTEL = 5771,
        SpvOpSubgroupAvcImeGetStreamoutSingleReferenceMajorShapeReferenceIdsINTEL = 5772,
        SpvOpSubgroupAvcImeGetStreamoutDualReferenceMajorShapeMotionVectorsINTEL = 5773,
        SpvOpSubgroupAvcImeGetStreamoutDualReferenceMajorShapeDistortionsINTEL = 5774,
        SpvOpSubgroupAvcImeGetStreamoutDualReferenceMajorShapeReferenceIdsINTEL = 5775,
        SpvOpSubgroupAvcImeGetBorderReachedINTEL = 5776,
        SpvOpSubgroupAvcImeGetTruncatedSearchIndicationINTEL = 5777,
        SpvOpSubgroupAvcImeGetUnidirectionalEarlySearchTerminationINTEL = 5778,
        SpvOpSubgroupAvcImeGetWeightingPatternMinimumMotionVectorINTEL = 5779,
        SpvOpSubgroupAvcImeGetWeightingPatternMinimumDistortionINTEL = 5780,
        SpvOpSubgroupAvcFmeInitializeINTEL = 5781,
        SpvOpSubgroupAvcBmeInitializeINTEL = 5782,
        SpvOpSubgroupAvcRefConvertToMcePayloadINTEL = 5783,
        SpvOpSubgroupAvcRefSetBidirectionalMixDisableINTEL = 5784,
        SpvOpSubgroupAvcRefSetBilinearFilterEnableINTEL = 5785,
        SpvOpSubgroupAvcRefEvaluateWithSingleReferenceINTEL = 5786,
        SpvOpSubgroupAvcRefEvaluateWithDualReferenceINTEL = 5787,
        SpvOpSubgroupAvcRefEvaluateWithMultiReferenceINTEL = 5788,
        SpvOpSubgroupAvcRefEvaluateWithMultiReferenceInterlacedINTEL = 5789,
        SpvOpSubgroupAvcRefConvertToMceResultINTEL = 5790,
        SpvOpSubgroupAvcSicInitializeINTEL = 5791,
        SpvOpSubgroupAvcSicConfigureSkcINTEL = 5792,
        SpvOpSubgroupAvcSicConfigureIpeLumaINTEL = 5793,
        SpvOpSubgroupAvcSicConfigureIpeLumaChromaINTEL = 5794,
        SpvOpSubgroupAvcSicGetMotionVectorMaskINTEL = 5795,
        SpvOpSubgroupAvcSicConvertToMcePayloadINTEL = 5796,
        SpvOpSubgroupAvcSicSetIntraLumaShapePenaltyINTEL = 5797,
        SpvOpSubgroupAvcSicSetIntraLumaModeCostFunctionINTEL = 5798,
        SpvOpSubgroupAvcSicSetIntraChromaModeCostFunctionINTEL = 5799,
        SpvOpSubgroupAvcSicSetBilinearFilterEnableINTEL = 5800,
        SpvOpSubgroupAvcSicSetSkcForwardTransformEnableINTEL = 5801,
        SpvOpSubgroupAvcSicSetBlockBasedRawSkipSadINTEL = 5802,
        SpvOpSubgroupAvcSicEvaluateIpeINTEL = 5803,
        SpvOpSubgroupAvcSicEvaluateWithSingleReferenceINTEL = 5804,
        SpvOpSubgroupAvcSicEvaluateWithDualReferenceINTEL = 5805,
        SpvOpSubgroupAvcSicEvaluateWithMultiReferenceINTEL = 5806,
        SpvOpSubgroupAvcSicEvaluateWithMultiReferenceInterlacedINTEL = 5807,
        SpvOpSubgroupAvcSicConvertToMceResultINTEL = 5808,
        SpvOpSubgroupAvcSicGetIpeLumaShapeINTEL = 5809,
        SpvOpSubgroupAvcSicGetBestIpeLumaDistortionINTEL = 5810,
        SpvOpSubgroupAvcSicGetBestIpeChromaDistortionINTEL = 5811,
        SpvOpSubgroupAvcSicGetPackedIpeLumaModesINTEL = 5812,
        SpvOpSubgroupAvcSicGetIpeChromaModeINTEL = 5813,
        SpvOpSubgroupAvcSicGetPackedSkcLumaCountThresholdINTEL = 5814,
        SpvOpSubgroupAvcSicGetPackedSkcLumaSumThresholdINTEL = 5815,
        SpvOpSubgroupAvcSicGetInterRawSadsINTEL = 5816,
        SpvOpVariableLengthArrayINTEL = 5818,
        SpvOpSaveMemoryINTEL = 5819,
        SpvOpRestoreMemoryINTEL = 5820,
        SpvOpArbitraryFloatSinCosPiINTEL = 5840,
        SpvOpArbitraryFloatCastINTEL = 5841,
        SpvOpArbitraryFloatCastFromIntINTEL = 5842,
        SpvOpArbitraryFloatCastToIntINTEL = 5843,
        SpvOpArbitraryFloatAddINTEL = 5846,
        SpvOpArbitraryFloatSubINTEL = 5847,
        SpvOpArbitraryFloatMulINTEL = 5848,
        SpvOpArbitraryFloatDivINTEL = 5849,
        SpvOpArbitraryFloatGTINTEL = 5850,
        SpvOpArbitraryFloatGEINTEL = 5851,
        SpvOpArbitraryFloatLTINTEL = 5852,
        SpvOpArbitraryFloatLEINTEL = 5853,
        SpvOpArbitraryFloatEQINTEL = 5854,
        SpvOpArbitraryFloatRecipINTEL = 5855,
        SpvOpArbitraryFloatRSqrtINTEL = 5856,
        SpvOpArbitraryFloatCbrtINTEL = 5857,
        SpvOpArbitraryFloatHypotINTEL = 5858,
        SpvOpArbitraryFloatSqrtINTEL = 5859,
        SpvOpArbitraryFloatLogINTEL = 5860,
        SpvOpArbitraryFloatLog2INTEL = 5861,
        SpvOpArbitraryFloatLog10INTEL = 5862,
        SpvOpArbitraryFloatLog1pINTEL = 5863,
        SpvOpArbitraryFloatExpINTEL = 5864,
        SpvOpArbitraryFloatExp2INTEL = 5865,
        SpvOpArbitraryFloatExp10INTEL = 5866,
        SpvOpArbitraryFloatExpm1INTEL = 5867,
        SpvOpArbitraryFloatSinINTEL = 5868,
        SpvOpArbitraryFloatCosINTEL = 5869,
        SpvOpArbitraryFloatSinCosINTEL = 5870,
        SpvOpArbitraryFloatSinPiINTEL = 5871,
        SpvOpArbitraryFloatCosPiINTEL = 5872,
        SpvOpArbitraryFloatASinINTEL = 5873,
        SpvOpArbitraryFloatASinPiINTEL = 5874,
        SpvOpArbitraryFloatACosINTEL = 5875,
        SpvOpArbitraryFloatACosPiINTEL = 5876,
        SpvOpArbitraryFloatATanINTEL = 5877,
        SpvOpArbitraryFloatATanPiINTEL = 5878,
        SpvOpArbitraryFloatATan2INTEL = 5879,
        SpvOpArbitraryFloatPowINTEL = 5880,
        SpvOpArbitraryFloatPowRINTEL = 5881,
        SpvOpArbitraryFloatPowNINTEL = 5882,
        SpvOpLoopControlINTEL = 5887,
        SpvOpAliasDomainDeclINTEL = 5911,
        SpvOpAliasScopeDeclINTEL = 5912,
        SpvOpAliasScopeListDeclINTEL = 5913,
        SpvOpFixedSqrtINTEL = 5923,
        SpvOpFixedRecipINTEL = 5924,
        SpvOpFixedRsqrtINTEL = 5925,
        SpvOpFixedSinINTEL = 5926,
        SpvOpFixedCosINTEL = 5927,
        SpvOpFixedSinCosINTEL = 5928,
        SpvOpFixedSinPiINTEL = 5929,
        SpvOpFixedCosPiINTEL = 5930,
        SpvOpFixedSinCosPiINTEL = 5931,
        SpvOpFixedLogINTEL = 5932,
        SpvOpFixedExpINTEL = 5933,
        SpvOpPtrCastToCrossWorkgroupINTEL = 5934,
        SpvOpCrossWorkgroupCastToPtrINTEL = 5938,
        SpvOpReadPipeBlockingINTEL = 5946,
        SpvOpWritePipeBlockingINTEL = 5947,
        SpvOpFPGARegINTEL = 5949,
        SpvOpRayQueryGetRayTMinKHR = 6016,
        SpvOpRayQueryGetRayFlagsKHR = 6017,
        SpvOpRayQueryGetIntersectionTKHR = 6018,
        SpvOpRayQueryGetIntersectionInstanceCustomIndexKHR = 6019,
        SpvOpRayQueryGetIntersectionInstanceIdKHR = 6020,
        SpvOpRayQueryGetIntersectionInstanceShaderBindingTableRecordOffsetKHR = 6021,
        SpvOpRayQueryGetIntersectionGeometryIndexKHR = 6022,
        SpvOpRayQueryGetIntersectionPrimitiveIndexKHR = 6023,
        SpvOpRayQueryGetIntersectionBarycentricsKHR = 6024,
        SpvOpRayQueryGetIntersectionFrontFaceKHR = 6025,
        SpvOpRayQueryGetIntersectionCandidateAABBOpaqueKHR = 6026,
        SpvOpRayQueryGetIntersectionObjectRayDirectionKHR = 6027,
        SpvOpRayQueryGetIntersectionObjectRayOriginKHR = 6028,
        SpvOpRayQueryGetWorldRayDirectionKHR = 6029,
        SpvOpRayQueryGetWorldRayOriginKHR = 6030,
        SpvOpRayQueryGetIntersectionObjectToWorldKHR = 6031,
        SpvOpRayQueryGetIntersectionWorldToObjectKHR = 6032,
        SpvOpAtomicFAddEXT = 6035,
        SpvOpTypeBufferSurfaceINTEL = 6086,
        SpvOpTypeStructContinuedINTEL = 6090,
        SpvOpConstantCompositeContinuedINTEL = 6091,
        SpvOpSpecConstantCompositeContinuedINTEL = 6092,
        SpvOpControlBarrierArriveINTEL = 6142,
        SpvOpControlBarrierWaitINTEL = 6143,
        SpvOpGroupIMulKHR = 6401,
        SpvOpGroupFMulKHR = 6402,
        SpvOpGroupBitwiseAndKHR = 6403,
        SpvOpGroupBitwiseOrKHR = 6404,
        SpvOpGroupBitwiseXorKHR = 6405,
        SpvOpGroupLogicalAndKHR = 6406,
        SpvOpGroupLogicalOrKHR = 6407,
        SpvOpGroupLogicalXorKHR = 6408,
        SpvOpMax = 0x7fffffff,
    }

    public static unsafe partial class SpirvReflect
    {
        private const string importLibName = "./SpirvReflect.dll";

        public const int SPV_REFLECT_MAX_ARRAY_DIMS = 32;
        public const int SPV_REFLECT_MAX_DESCRIPTOR_SETS = 64;
        public const int SPV_REFLECT_BINDING_NUMBER_DONT_CHANGE = ~0;
        public const int SPV_REFLECT_SET_NUMBER_DONT_CHANGE = ~0;

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectResult spvReflectCreateShaderModule(nuint size, void* p_code, SpvReflectShaderModule* p_module);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void spvReflectDestroyShaderModule(SpvReflectShaderModule* p_module);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint spvReflectGetCodeSize(SpvReflectShaderModule* p_module);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint* spvReflectGetCode(SpvReflectShaderModule* p_module);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectEntryPoint* spvReflectGetEntryPoint(SpvReflectShaderModule* p_module, sbyte* entry_point);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectResult spvReflectEnumerateDescriptorBindings(SpvReflectShaderModule* p_module, uint* p_count, SpvReflectDescriptorBinding** pp_bindings);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectResult spvReflectEnumerateEntryPointDescriptorBindings(SpvReflectShaderModule* p_module, sbyte* entry_point, uint* p_count, SpvReflectDescriptorBinding** pp_bindings);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectResult spvReflectEnumerateDescriptorSets(SpvReflectShaderModule* p_module, uint* p_count, SpvReflectDescriptorSet** pp_sets);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectResult spvReflectEnumerateEntryPointDescriptorSets(SpvReflectShaderModule* p_module, sbyte* entry_point, uint* p_count, SpvReflectDescriptorSet** pp_sets);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectResult spvReflectEnumerateInterfaceVariables(SpvReflectShaderModule* p_module, uint* p_count, SpvReflectInterfaceVariable** pp_variables);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectResult spvReflectEnumerateEntryPointInterfaceVariables(SpvReflectShaderModule* p_module, sbyte* entry_point, uint* p_count, SpvReflectInterfaceVariable** pp_variables);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectResult spvReflectEnumerateInputVariables(SpvReflectShaderModule* p_module, uint* p_count, SpvReflectInterfaceVariable** pp_variables);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectResult spvReflectEnumerateEntryPointInputVariables(SpvReflectShaderModule* p_module, sbyte* entry_point, uint* p_count, SpvReflectInterfaceVariable** pp_variables);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectResult spvReflectEnumerateOutputVariables(SpvReflectShaderModule* p_module, uint* p_count, SpvReflectInterfaceVariable** pp_variables);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectResult spvReflectEnumerateEntryPointOutputVariables(SpvReflectShaderModule* p_module, sbyte* entry_point, uint* p_count, SpvReflectInterfaceVariable** pp_variables);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectResult spvReflectEnumeratePushConstantBlocks(SpvReflectShaderModule* p_module, uint* p_count, SpvReflectBlockVariable** pp_blocks);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectResult spvReflectEnumerateEntryPointPushConstantBlocks(SpvReflectShaderModule* p_module, sbyte* entry_point, uint* p_count, SpvReflectBlockVariable** pp_blocks);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectDescriptorBinding* spvReflectGetDescriptorBinding(SpvReflectShaderModule* p_module, uint binding_number, uint set_number, SpvReflectResult* p_result);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectDescriptorBinding* spvReflectGetEntryPointDescriptorBinding(SpvReflectShaderModule* p_module, sbyte* entry_point, uint binding_number, uint set_number, SpvReflectResult* p_result);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectDescriptorSet* spvReflectGetDescriptorSet(SpvReflectShaderModule* p_module, uint set_number, SpvReflectResult* p_result);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectDescriptorSet* spvReflectGetEntryPointDescriptorSet(SpvReflectShaderModule* p_module, sbyte* entry_point, uint set_number, SpvReflectResult* p_result);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectInterfaceVariable* spvReflectGetInputVariableByLocation(SpvReflectShaderModule* p_module, uint location, SpvReflectResult* p_result);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectInterfaceVariable* spvReflectGetEntryPointInputVariableByLocation(SpvReflectShaderModule* p_module, sbyte* entry_point, uint location, SpvReflectResult* p_result);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectInterfaceVariable* spvReflectGetInputVariableBySemantic(SpvReflectShaderModule* p_module, sbyte* semantic, SpvReflectResult* p_result);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectInterfaceVariable* spvReflectGetEntryPointInputVariableBySemantic(SpvReflectShaderModule* p_module, sbyte* entry_point, sbyte* semantic, SpvReflectResult* p_result);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectInterfaceVariable* spvReflectGetOutputVariableByLocation(SpvReflectShaderModule* p_module, uint location, SpvReflectResult* p_result);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectInterfaceVariable* spvReflectGetEntryPointOutputVariableByLocation(SpvReflectShaderModule* p_module, sbyte* entry_point, uint location, SpvReflectResult* p_result);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectInterfaceVariable* spvReflectGetOutputVariableBySemantic(SpvReflectShaderModule* p_module, sbyte* semantic, SpvReflectResult* p_result);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectInterfaceVariable* spvReflectGetEntryPointOutputVariableBySemantic(SpvReflectShaderModule* p_module, sbyte* entry_point, sbyte* semantic, SpvReflectResult* p_result);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectBlockVariable* spvReflectGetPushConstantBlock(SpvReflectShaderModule* p_module, uint index, SpvReflectResult* p_result);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectBlockVariable* spvReflectGetEntryPointPushConstantBlock(SpvReflectShaderModule* p_module, sbyte* entry_point, SpvReflectResult* p_result);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectResult spvReflectChangeDescriptorBindingNumbers(SpvReflectShaderModule* p_module, SpvReflectDescriptorBinding* p_binding, uint new_binding_number, uint new_set_number);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectResult spvReflectChangeDescriptorSetNumber(SpvReflectShaderModule* p_module, SpvReflectDescriptorSet* p_set, uint new_set_number);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectResult spvReflectChangeInputVariableLocation(SpvReflectShaderModule* p_module, SpvReflectInterfaceVariable* p_input_variable, uint new_location);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern SpvReflectResult spvReflectChangeOutputVariableLocation(SpvReflectShaderModule* p_module, SpvReflectInterfaceVariable* p_output_variable, uint new_location);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern sbyte* spvReflectSourceLanguage(SpvSourceLanguage_ source_lang);

        [DllImport(importLibName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern sbyte* spvReflectBlockVariableTypeName(SpvReflectBlockVariable* p_var);

        public const uint SpvMagicNumber = 0x07230203;
        public const uint SpvVersion = 0x00010600;
        public const uint SpvRevision = 1;
        public const uint SpvOpCodeMask = 0xffff;
        public const uint SpvWordCountShift = 16;
    }
}
