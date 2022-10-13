using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Image = SixLabors.ImageSharp.Image;
using glTFLoader;
using glTFLoader.Schema;
using System.Security.Cryptography.X509Certificates;
using Buffer = glTFLoader.Schema.Buffer;
using VulkanBuffer = Silk.NET.Vulkan.Buffer;
using System.Diagnostics;
using Sampler = glTFLoader.Schema.Sampler;
using VulkanSampler = Silk.NET.Vulkan.Sampler;
using MafrixEngine.GraphicsWrapper;
using Mesh = glTFLoader.Schema.Mesh;
using Silk.NET.Input;
using System.Collections;
using MafrixEngine.Source.Interface;
using static glTFLoader.Schema.AnimationChannelTarget;

namespace MafrixEngine.ModelLoaders
{
    public class Gltf2Buffer
    {
        public byte[] buffer;
        public Gltf2Buffer(Gltf2Loader loader, Gltf gltf, int bufferIndex)
        {
            buffer = loader.buffers[bufferIndex];
        }
    }

    public class Gltf2BufferView
    {
        public Gltf2Buffer buffer;
        public int offset;
        public int length;
        public Gltf2BufferView(Gltf2Loader loader, Gltf gltf, int viewIndex)
        {
            var view = gltf.BufferViews[viewIndex];
            buffer = new Gltf2Buffer(loader, gltf, view.Buffer);
            offset = view.ByteOffset;
            length = view.ByteLength;
        }
    }

    public class Gltf2Accessor
    {
        private Gltf2BufferView bufferView;
        private int offset;
        private int count;
        private Accessor.TypeEnum type;
        private Accessor.ComponentTypeEnum componentType;

        public Accessor.ComponentTypeEnum RawType { get => componentType; }
        public Type ComponentType { get 
            { 
                switch(componentType)
                {
                    case Accessor.ComponentTypeEnum.BYTE: return typeof(SByte);
                    case Accessor.ComponentTypeEnum.UNSIGNED_BYTE: return typeof(Byte);
                    case Accessor.ComponentTypeEnum.SHORT: return typeof(Int16);
                    case Accessor.ComponentTypeEnum.UNSIGNED_SHORT: return typeof(UInt16);
                    case Accessor.ComponentTypeEnum.UNSIGNED_INT: return typeof(UInt32);
                    case Accessor.ComponentTypeEnum.FLOAT: return typeof(float);
                }
                throw new NotSupportedException("Wrong component type found in gltf accessor.");
            } 
        }
        public int ComponentTypeCount
        {
            get
            {
                switch (componentType)
                {
                    case Accessor.ComponentTypeEnum.BYTE: return sizeof(SByte);
                    case Accessor.ComponentTypeEnum.UNSIGNED_BYTE: return sizeof(Byte);
                    case Accessor.ComponentTypeEnum.SHORT: return sizeof(Int16);
                    case Accessor.ComponentTypeEnum.UNSIGNED_SHORT: return sizeof(UInt16);
                    case Accessor.ComponentTypeEnum.UNSIGNED_INT: return sizeof(UInt32);
                    case Accessor.ComponentTypeEnum.FLOAT: return sizeof(float);
                }
                throw new NotSupportedException("Wrong component type found in gltf accessor.");
            }
        }
        public int ComponentCount { get
            {
                switch(type)
                {
                    case Accessor.TypeEnum.SCALAR: return 1;
                    case Accessor.TypeEnum.VEC2: return 2;
                    case Accessor.TypeEnum.VEC3: return 3;
                    case Accessor.TypeEnum.VEC4: return 4;
                    case Accessor.TypeEnum.MAT2: return 4;
                    case Accessor.TypeEnum.MAT3: return 9;
                    case Accessor.TypeEnum.MAT4: return 16;
                }
                throw new NotSupportedException("Wrong type found in gltf accessor.");
            }
        }

        public int Count { get => count; }
        public float[]? min;
        public float[]? max;

        public Gltf2Accessor(Gltf2Loader loader, Gltf gltf, int index)
        {
            var accessor = gltf.Accessors[index];
            bufferView = new Gltf2BufferView(loader, gltf, accessor.BufferView!.Value);
            type = accessor.Type;
            componentType = accessor.ComponentType;
            count = accessor.Count;
            offset = accessor.ByteOffset;
            if(accessor.Min != null && accessor.Max != null)
            {
                min = new float[ComponentCount];
                max = new float[ComponentCount];
                for (int i = 0; i < ComponentCount; i++)
                {
                    min[i] = accessor.Min[i];
                    max[i] = accessor.Max[i];
                }
            }
        }

        public unsafe ReadOnlyMemory<T> GetMemory<T>() where T : struct
        {
#if DEBUG
            TypeCheck<T>();
#endif

            var totalOffset = offset + bufferView.offset;
            byte[] buffer = bufferView.buffer.buffer;
            //var source = new ReadOnlySpan<byte>(buffer, totalOffset, bufferView.length);
            var source = MemoryMarshal.CreateReadOnlySpan<byte>(ref buffer[totalOffset], bufferView.length);
            var targ = MemoryMarshal.Cast<byte, T>(source);
            return new ReadOnlyMemory<T>(targ.ToArray(), 0, count);
        }

        private unsafe void TypeCheck<T>() where T : struct
        {
            var b = Unsafe.SizeOf<T>() == ComponentTypeCount * ComponentCount;
            Debug.Assert(b);
        }
    }
    
    public class VertexBuffer
    {
        private int indicesCount;
        private Gltf2Accessor positionAccessor;
        public ReadOnlyMemory<Vector3D<float>> positions;
        private Gltf2Accessor normalAccessor;
        public ReadOnlyMemory<Vector3D<float>> normals;
        private Gltf2Accessor texAccessor;
        public ReadOnlyMemory<Vector2D<float>> texs;

        private Gltf2Accessor indicesAccessor;

        public int Count { get => indicesCount; }
        public VertexBuffer(Gltf2Loader loader, Gltf gltf, MeshPrimitive primitive)
        {
            var info = primitive.Attributes;
            
            positionAccessor = new Gltf2Accessor(loader, gltf, info["POSITION"]);
            positions = positionAccessor.GetMemory<Vector3D<float>>();
            normalAccessor = new Gltf2Accessor(loader, gltf, info["NORMAL"]);
            normals = normalAccessor.GetMemory<Vector3D<float>>();
            texAccessor = new Gltf2Accessor(loader, gltf, info["TEXCOORD_0"]);
            texs = texAccessor.GetMemory<Vector2D<float>>();
            indicesAccessor = new Gltf2Accessor(loader, gltf, primitive.Indices!.Value);
            indicesCount = indicesAccessor.Count;
        }

        public Vertex[] GetVertex()
        {
            var vertex = new Vertex[indicesCount];
            var indices = new uint[indicesCount];
            switch(indicesAccessor.RawType)
            {
                case Accessor.ComponentTypeEnum.BYTE:
                    {
                        var source = indicesAccessor.GetMemory<SByte>().Span;
                        for (int i = 0; i < source.Length; i++)
                        {
                            indices[i] = (uint)source[i];
                        }
                    }
                    break;
                case Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
                    {
                        var source = indicesAccessor.GetMemory<Byte>().Span;
                        for (int i = 0; i < source.Length; i++)
                        {
                            indices[i] = (uint)source[i];
                        }
                    }
                    break;
                case Accessor.ComponentTypeEnum.SHORT:
                    {
                        var source = indicesAccessor.GetMemory<Int16>().Span;
                        for (int i = 0; i < source.Length; i++)
                        {
                            indices[i] = (uint)source[i];
                        }
                    }
                    break;
                case Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
                    {
                        var source = indicesAccessor.GetMemory<UInt16>().Span;
                        for (int i = 0; i < source.Length; i++)
                        {
                            indices[i] = (uint)source[i];
                        }
                    }
                    break;
                case Accessor.ComponentTypeEnum.UNSIGNED_INT:
                    {
                        var source = indicesAccessor.GetMemory<UInt32>().Span;
                        for (int i = 0; i < source.Length; i++)
                        {
                            indices[i] = source[i];
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException("Wrong Indicies type.");
            }

            var positionSpan = positions.Span;
            var normalSpan = normals.Span;
            var texSpan = texs.Span;
            for (int i = 0; i < indicesCount; i++)
            {
                var idx = (int)indices[i];
                var pos = positionSpan[idx];
                var normal = normalSpan[idx];
                var tex = texSpan[idx];
                vertex[i] = new Vertex(pos, normal, tex);
            }
            return vertex;
        }
    }

    public struct AnimatedVertex : IVertexData
    {
        public Vector3D<float> pos;
        public Vector3D<float> normal;
        public Vector2D<float> texCoord;
        public Vector4D<uint> joints;
        public Vector4D<float> weights;

        public VertexInputBindingDescription BindingDescription => GetBindingDescription();

        public VertexInputAttributeDescription[] AttributeDescriptions => GetAttributeDescriptions();

        public AnimatedVertex(Vector3D<float> p, Vector3D<float> n, Vector2D<float> t, Vector4D<uint> j, Vector4D<float> w) =>
            (pos, normal, texCoord, joints, weights) = (p, n, t, j, w);
        public unsafe static VertexInputBindingDescription GetBindingDescription()
        {
            var bindingDescription = new VertexInputBindingDescription();
            bindingDescription.Binding = 0;
            bindingDescription.Stride = (uint)sizeof(AnimatedVertex);
            bindingDescription.InputRate = VertexInputRate.Vertex;

            return bindingDescription;
        }
        public unsafe static VertexInputAttributeDescription[] GetAttributeDescriptions()
        {
            var attributeDescriptions = new VertexInputAttributeDescription[5];
            attributeDescriptions[0].Binding = 0;
            attributeDescriptions[0].Location = 0;
            attributeDescriptions[0].Format = Format.R32G32B32Sfloat;
            attributeDescriptions[0].Offset = (uint)Marshal.OffsetOf<AnimatedVertex>("pos").ToInt32();
            attributeDescriptions[1].Binding = 0;
            attributeDescriptions[1].Location = 1;
            attributeDescriptions[1].Format = Format.R32G32B32Sfloat;
            attributeDescriptions[1].Offset = (uint)Marshal.OffsetOf<AnimatedVertex>("normal").ToInt32();
            attributeDescriptions[2].Binding = 0;
            attributeDescriptions[2].Location = 2;
            attributeDescriptions[2].Format = Format.R32G32Sfloat;
            attributeDescriptions[2].Offset = (uint)Marshal.OffsetOf<AnimatedVertex>("texCoord").ToInt32();
            attributeDescriptions[3].Binding = 0;
            attributeDescriptions[3].Location = 3;
            attributeDescriptions[3].Format = Format.R32G32B32A32Uint;
            attributeDescriptions[3].Offset = (uint)Marshal.OffsetOf<AnimatedVertex>("joints").ToInt32();
            attributeDescriptions[4].Binding = 0;
            attributeDescriptions[4].Location = 4;
            attributeDescriptions[4].Format = Format.R32G32B32A32Sfloat;
            attributeDescriptions[4].Offset = (uint)Marshal.OffsetOf<AnimatedVertex>("weights").ToInt32();

            return attributeDescriptions;
        }
    }

    public class AnimatedVertexBuffer
    {
        private int indicesCount;
        private Gltf2Accessor positionAccessor;
        public ReadOnlyMemory<Vector3D<float>> positions;
        private Gltf2Accessor normalAccessor;
        public ReadOnlyMemory<Vector3D<float>> normals;
        private Gltf2Accessor texAccessor;
        public ReadOnlyMemory<Vector2D<float>> texs;
        private Gltf2Accessor jointsAccessor;
        public ReadOnlyMemory<Vector4D<ushort>> joints;
        private Gltf2Accessor weightsAccessor;
        public ReadOnlyMemory<Vector4D<float>> weights;

        private Gltf2Accessor indicesAccessor;

        public int Count { get => indicesCount; }
        public AnimatedVertexBuffer(Gltf2Loader loader, Gltf gltf, MeshPrimitive primitive)
        {
            var info = primitive.Attributes;

            positionAccessor = new Gltf2Accessor(loader, gltf, info["POSITION"]);
            positions = positionAccessor.GetMemory<Vector3D<float>>();
            normalAccessor = new Gltf2Accessor(loader, gltf, info["NORMAL"]);
            normals = normalAccessor.GetMemory<Vector3D<float>>();
            texAccessor = new Gltf2Accessor(loader, gltf, info["TEXCOORD_0"]);
            texs = texAccessor.GetMemory<Vector2D<float>>();
            jointsAccessor = new Gltf2Accessor(loader, gltf, info["JOINTS_0"]);
            joints = jointsAccessor.GetMemory<Vector4D<ushort>>();
            weightsAccessor = new Gltf2Accessor(loader, gltf, info["WEIGHTS_0"]);
            weights = weightsAccessor.GetMemory<Vector4D<float>>();
            indicesAccessor = new Gltf2Accessor(loader, gltf, primitive.Indices!.Value);
            indicesCount = indicesAccessor.Count;
        }

        public AnimatedVertex[] GetVertex()
        {
            var vertex = new AnimatedVertex[indicesCount];
            var indices = new uint[indicesCount];
            switch (indicesAccessor.RawType)
            {
                case Accessor.ComponentTypeEnum.BYTE:
                    {
                        var source = indicesAccessor.GetMemory<SByte>().Span;
                        for (int i = 0; i < source.Length; i++)
                        {
                            indices[i] = (uint)source[i];
                        }
                    }
                    break;
                case Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
                    {
                        var source = indicesAccessor.GetMemory<Byte>().Span;
                        for (int i = 0; i < source.Length; i++)
                        {
                            indices[i] = (uint)source[i];
                        }
                    }
                    break;
                case Accessor.ComponentTypeEnum.SHORT:
                    {
                        var source = indicesAccessor.GetMemory<Int16>().Span;
                        for (int i = 0; i < source.Length; i++)
                        {
                            indices[i] = (uint)source[i];
                        }
                    }
                    break;
                case Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
                    {
                        var source = indicesAccessor.GetMemory<UInt16>().Span;
                        for (int i = 0; i < source.Length; i++)
                        {
                            indices[i] = (uint)source[i];
                        }
                    }
                    break;
                case Accessor.ComponentTypeEnum.UNSIGNED_INT:
                    {
                        var source = indicesAccessor.GetMemory<UInt32>().Span;
                        for (int i = 0; i < source.Length; i++)
                        {
                            indices[i] = source[i];
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException("Wrong Indicies type.");
            }

            var positionSpan = positions.Span;
            var normalSpan = normals.Span;
            var texSpan = texs.Span;
            var jointSpan = joints.Span;
            var weightSpan = weights.Span;
            for (int i = 0; i < indicesCount; i++)
            {
                var idx = (int)indices[i];
                var pos = positionSpan[idx];
                var normal = normalSpan[idx];
                var tex = texSpan[idx];
                var j = jointSpan[idx];
                var joint = new Vector4D<uint>(j.X, j.Y, j.Z, j.W);
                var weight = weightSpan[idx];
                vertex[i] = new AnimatedVertex(pos, normal, tex, joint, weight);
            }
            return vertex;
        }
    }

    public struct JointInfo
    {
        public Vector4D<ushort> joints;
        public Vector4D<float> weights;
        public JointInfo(Vector4D<ushort> j, Vector4D<float> w) { joints = j; weights = w; }
    }
    public class AnimationVertexBuffer
    {
        private int indicesCount;
        private Gltf2Accessor jointAccessor;
        public ReadOnlyMemory<Vector4D<ushort>> joints;
        private Gltf2Accessor weightAccessor;
        public ReadOnlyMemory<Vector4D<float>> weights;

        private Gltf2Accessor indicesAccessor;

        public int Count { get => indicesCount; }
        public AnimationVertexBuffer(Gltf2Loader loader, Gltf gltf, MeshPrimitive primitive)
        {
            var info = primitive.Attributes;

            jointAccessor = new Gltf2Accessor(loader, gltf, info["JOINTS_0"]);
            joints = jointAccessor.GetMemory<Vector4D<ushort>>();
            weightAccessor = new Gltf2Accessor(loader, gltf, info["WEIGHTS_0"]);
            weights = weightAccessor.GetMemory<Vector4D<float>>();
            indicesAccessor = new Gltf2Accessor(loader, gltf, primitive.Indices!.Value);
            indicesCount = indicesAccessor.Count;
        }

        public JointInfo[] GetJointInfo()
        {
            var jointArray = new JointInfo[indicesCount];
            var indices = new uint[indicesCount];
            switch (indicesAccessor.RawType)
            {
                case Accessor.ComponentTypeEnum.BYTE:
                    {
                        var source = indicesAccessor.GetMemory<SByte>().Span;
                        for (int i = 0; i < source.Length; i++)
                        {
                            indices[i] = (uint)source[i];
                        }
                    }
                    break;
                case Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
                    {
                        var source = indicesAccessor.GetMemory<Byte>().Span;
                        for (int i = 0; i < source.Length; i++)
                        {
                            indices[i] = (uint)source[i];
                        }
                    }
                    break;
                case Accessor.ComponentTypeEnum.SHORT:
                    {
                        var source = indicesAccessor.GetMemory<Int16>().Span;
                        for (int i = 0; i < source.Length; i++)
                        {
                            indices[i] = (uint)source[i];
                        }
                    }
                    break;
                case Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
                    {
                        var source = indicesAccessor.GetMemory<UInt16>().Span;
                        for (int i = 0; i < source.Length; i++)
                        {
                            indices[i] = (uint)source[i];
                        }
                    }
                    break;
                case Accessor.ComponentTypeEnum.UNSIGNED_INT:
                    {
                        var source = indicesAccessor.GetMemory<UInt32>().Span;
                        for (int i = 0; i < source.Length; i++)
                        {
                            indices[i] = source[i];
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException("Wrong Indicies type.");
            }

            var jointSpan = joints.Span;
            var weightSpan = weights.Span;
            for (int i = 0; i < indicesCount; i++)
            {
                var idx = (int)indices[i];
                var joint = jointSpan[idx];
                var weight = weightSpan[idx];
                jointArray[i] = new JointInfo(joint, weight);
            }
            return jointArray;
        }
    }


    public class Gltf2Image : IDisposable
    {
        public Image<Rgba32> image;
        public Gltf2Image(Gltf2Loader loader, Gltf gltf, int index)
        {
            var glImage = gltf.Images[index];
            image = loader.LoadImage(glImage.Uri);
        }

        public void Dispose()
        {
            image.Dispose();
        }
    }

    public class Gltf2Sampler : IDisposable
    {
        VulkanSampler sampler;
        private Sampler.MinFilterEnum minFilter;
        private Sampler.MagFilterEnum magFilter;
        private Sampler.WrapSEnum wrapS;
        private Sampler.WrapTEnum wrapT;
        public Gltf2Sampler(Gltf2Loader loader, Gltf gltf, int index)
        {
            var samp = gltf.Samplers[index];
            minFilter = samp.MinFilter!.Value;
            magFilter = samp.MagFilter!.Value;
            wrapS = samp.WrapS;
            wrapT = samp.WrapT;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class Gltf2Texture : IDisposable
    {
        //public Gltf2Image image;
        public VkTexture texture;
        public Gltf2Sampler sampler;
        public ImageView imageView { get => texture.imageView; }

        public Gltf2Texture(Gltf2Loader loader, Gltf gltf, int index)
        {
            var texture = gltf.Textures[index];
            using var image = new Gltf2Image(loader, gltf, texture.Source!.Value);
            sampler = new Gltf2Sampler(loader, gltf, texture.Sampler!.Value);
            this.texture = new VkTexture(loader.vkContext, loader.stCommand, loader.stagingBuffer, image.image);
        }

        public void Dispose()
        {
            //sampler.Dispose();
            texture.Dispose();
        }
    }

    public class Gltf2Material
    {
        public Gltf2Texture baseTexture;
        public Gltf2Texture metallicTexture;
        public Gltf2Texture normalTexture;

        public Gltf2Material(Gltf2Loader loader, Gltf gltf, int index, Gltf2Texture[] textures)
        {
            var material = gltf.Materials[index];
            var textureinfo = material.PbrMetallicRoughness.BaseColorTexture;
            baseTexture = textures[textureinfo!.Index];
            var metallicInfo = material.PbrMetallicRoughness.MetallicRoughnessTexture;
            metallicTexture = metallicInfo != null? textures[metallicInfo!.Index] : baseTexture;
            var normalInfo = material.NormalTexture;
            normalTexture = normalInfo != null? textures[normalInfo!.Index] : baseTexture;
        }
    }

    public class Gltf2AnimationSampler
    {
        public enum EInterpolation
        {
            Step,
            Linear,
            Cubicspline,
        }
        public EInterpolation interpolation;
        public Gltf2Accessor input;
        public Gltf2Accessor output;
        public float timeMax;
        public float[] timeInput;
        public Gltf2AnimationSampler(Gltf2Loader loader, Gltf gltf, Animation animation, int index)
        {
            var sampler = animation.Samplers[index];
            input = new Gltf2Accessor(loader, gltf, sampler.Input);
            output = new Gltf2Accessor(loader, gltf, sampler.Output);
            interpolation = sampler.Interpolation == AnimationSampler.InterpolationEnum.STEP ? EInterpolation.Step :
                            sampler.Interpolation == AnimationSampler.InterpolationEnum.LINEAR ? EInterpolation.Linear :
                            EInterpolation.Cubicspline;
            timeInput = input.GetMemory<float>().ToArray();
            timeMax = timeInput[timeInput.Length-1];
        }

        public (int,float) GetIndex(float time)
        {
            if (time > timeMax) return (-1, 0.0f);
            for (int i = 0; i < timeInput.Length-1; i++)
            {
                if (time >= timeInput[i] && time <= timeInput[i + 1])
                    return (i, (time - timeInput[i]) / (timeInput[i+1] - timeInput[i]));
            }
            return (-1, 0.0f);
        }
    }

    public class Gltf2AnimationChannel
    {
        public PathEnum path;
        public Gltf2AnimationSampler sampler;
        public Gltf2AnimatedRootNode rootNode;
        //public Gltf2RootNode rootNode;
        public int nodeIndex;
        public Gltf2Node node;
        private Vector3D<float>[]? translations;
        private Quaternion<float>[]? rotations;
        private Vector3D<float>[]? scales;
        private float[]? weights;
        public Gltf2AnimationChannel(Gltf2Loader loader, Gltf gltf, Animation animation, Gltf2AnimationSampler[] samplers, int index)
        {
            var channel = animation.Channels[index];
            sampler = samplers[channel.Sampler];
            var target = channel.Target;
            path = target.Path;
            Debug.Assert(target.Node != null);
            rootNode = loader.animatedRootNode;
            nodeIndex = target.Node.Value;
            node = new Gltf2Node(loader, gltf, target.Node.Value);

            switch(path)
            {
                case PathEnum.translation:
                    var trans = sampler.output.GetMemory<Vector3D<float>>();
                    translations = trans.ToArray();
                    break;
                case PathEnum.rotation:
                    var rotation = sampler.output.GetMemory<Quaternion<float>>();
                    rotations = rotation.ToArray();
                    break;
                case PathEnum.scale:
                    var scale = sampler.output.GetMemory<Vector3D<float>>();
                    scales = scale.ToArray();
                    break;
                case PathEnum.weights:
                    var weight = sampler.output.GetMemory<float>();
                    weights = weight.ToArray();
                    break;
            }
        }

        public void Update(float timeStamp)
        {
            var (idx, a) = sampler.GetIndex(timeStamp);
            if (idx < 0) return;
            switch(path)
            {
                case PathEnum.translation:
                    var vec1 = translations![idx];
                    var vec2 = translations![idx + 1];
                    rootNode.nodes[nodeIndex].translation = Vector3D.Lerp(vec1, vec2, a);
                    break;
                case PathEnum.rotation:
                    var rot1 = rotations![idx];
                    var rot2 = rotations![idx + 1];
                    rootNode.nodes[nodeIndex].rotation = Quaternion<float>.Slerp(rot1, rot2, a);
                    break;
                case PathEnum.scale:
                    var sca1 = scales![idx];
                    var sca2 = scales![idx + 1];
                    rootNode.nodes[nodeIndex].scale = Vector3D.Lerp(sca1, sca2, a);
                    break;
                case PathEnum.weights:
                    var weight = weights![idx];
                    //rootNode.nodes[nodeIndex]
                    break;
            }
        }
    }

    // animation using TRS matrix
    public class Gltf2Animation : IDescriptor, IDisposable
    {
        private VkContext vkContext;
        public Gltf2AnimatedRootNode rootNode;
        public Gltf2AnimationChannel[] channels;
        //public Vertex[] vertices;
        public AnimatedVertex[] vertexBuffer;
        public uint[] indexBuffer;
        public float timeMax;
        public float currentTime;
        public Matrix4X4<float>[] jointsInverseMatrix;
        public VkBuffer[] buffers;
        //public VulkanBuffer[] skinBuffer;
        //public DeviceMemory[] skinMemory;
        public Action<Matrix4X4<float>[], VkBuffer> UpdateBuffer;
        public Gltf2Animation(Gltf2Loader loader, Gltf gltf, int index)
        {
            vkContext = loader.vkContext;
            var animation = gltf.Animations[index];
            var samplers = new Gltf2AnimationSampler[animation.Samplers.Length];
            for(var i = 0; i < animation.Samplers.Length; i++)
            {
                samplers[i] = new Gltf2AnimationSampler(loader, gltf, animation, i);
            }
            timeMax = samplers[0].timeMax;
            channels = new Gltf2AnimationChannel[animation.Channels.Length];
            for(var i = 0; i < animation.Channels.Length; i++)
            {
                var channel = new Gltf2AnimationChannel(loader, gltf, animation, samplers, i);
                channels[i] = channel;
            }
            rootNode = loader.animatedRootNode;
            jointsInverseMatrix = new Matrix4X4<float>[rootNode.skins[0].joints.Length];
            //vertices = new Vertex[rootNode.vertices.Length];
        }

        public int DescriptorSetCount { get => rootNode.DescriptorSetCount; set { } }

        public unsafe DescriptorSet[] AllocateDescriptorSets(VkContext vkContext, DescriptorPool pool, DescriptorSetLayout[] setLayouts, int frames)
        {
            var setCount = frames * DescriptorSetCount * setLayouts.Length;
            var sets = new DescriptorSet[setCount];

            var layouts = new DescriptorSetLayout[DescriptorSetCount * setLayouts.Length];
            var copyOffset = 0;
            for (int j = 0; j < DescriptorSetCount * setLayouts.Length; j++)
            {
                for (var i = 0; i < setLayouts.Length; i++)
                {
                    layouts[copyOffset + i] = setLayouts[i];
                }
                copyOffset += setLayouts.Length;
            }

            var descriptorSetCount = layouts.Length;
            var allocInfo = new DescriptorSetAllocateInfo(StructureType.DescriptorSetAllocateInfo);
            allocInfo.DescriptorPool = pool;
            allocInfo.DescriptorSetCount = (uint)descriptorSetCount;
            fixed (DescriptorSetLayout* ptr = layouts)
            {
                allocInfo.PSetLayouts = ptr;
                var offset = 0;
                for (int i = 0; i < frames; i++)
                {
                    var tmpSetLayout = new DescriptorSet[descriptorSetCount];
                    fixed (DescriptorSet* setPtr = tmpSetLayout)
                    {
                        if (vkContext.vk.AllocateDescriptorSets(vkContext.device, in allocInfo, setPtr) != Result.Success)
                        {
                            throw new Exception("failed to allocate descriptor sets.");
                        }
                    }
                    for (int j = 0; j < descriptorSetCount; j++)
                    {
                        sets[offset + j] = tmpSetLayout[j];
                    }

                    offset += descriptorSetCount;
                }
            }

            return sets;
        }

        public void BindCommand(Vk vk, CommandBuffer commandBuffer, VulkanBuffer vertices, VulkanBuffer indices, Action<int> bindDescriptorSet)
        {
            rootNode.BindCommand(vk, commandBuffer, vertices, indices, bindDescriptorSet);
        }

        public unsafe DescriptorPool CreateDescriptorPool(VkContext vkContext, DescriptorSetLayout[] setLayouts, DescriptorPoolSize[] poolSizes, int frames)
        {
            DescriptorPool pool;
            var setCount = setLayouts.Length * frames * DescriptorSetCount;
            var tmpPoolSizes = new DescriptorPoolSize[poolSizes.Length];
            for (int i = 0; i < poolSizes.Length; i++)
            {
                tmpPoolSizes[i] = poolSizes[i];
                tmpPoolSizes[i].DescriptorCount *= (uint)(frames * DescriptorSetCount);
            }

            fixed (DescriptorPoolSize* ptr = tmpPoolSizes)
            {
                var createInfo = new DescriptorPoolCreateInfo(StructureType.DescriptorPoolCreateInfo);
                createInfo.MaxSets = (uint)setCount;
                createInfo.PoolSizeCount = (uint)tmpPoolSizes.Length;
                createInfo.PPoolSizes = ptr;
                if (vkContext.vk.CreateDescriptorPool(vkContext.device, in createInfo, null, out pool) != Result.Success)
                {
                    throw new Exception("failed to create descriptor pool.");
                }
            }

            return pool;
        }

        public unsafe void Dispose()
        {
            rootNode?.Dispose();
            foreach(var item in buffers)
            {
                item.Dispose();
            }
        }

        public void Update(double delta)
        {
            currentTime += (float)delta;
            if(currentTime > timeMax)
            {
                currentTime -= timeMax;
            }
            foreach (var item in channels)
            {
                item.Update(currentTime);
            }
            var matrix = new Matrix4X4<float>[rootNode.nodes.Length];
            for (int i = 0; i < matrix.Length; i++)
            {
                matrix[i] = Matrix4X4<float>.Identity;
            }
            UpdateNodeMatrix(matrix, Matrix4X4<float>.Identity, 0);
            var inverseTransform = Matrix4X4<float>.Identity;
            for (int i = 0; i < rootNode.nodes.Length; i++)
            {
                if (rootNode.nodes[i].skin != null)
                {
                    Matrix4X4.Invert(matrix[i], out inverseTransform);
                    break;
                }
            }
            for (int i = 0; i < rootNode.skins[0].joints.Length; i++)
            {
                var idx = rootNode.skins[0].joints[i];
                var mat = rootNode.skins[0].inverseBindMatrices[i] * matrix[idx] * inverseTransform;
                jointsInverseMatrix[i] = mat;
            }

            for (int i = 0; i < buffers.Length; i++)
            {
                UpdateBuffer(jointsInverseMatrix, buffers[i]);
            }
            for (int i = 0; i < rootNode.meshes.Length; i++)
            {
                var mesh = rootNode.meshes[i];
                for (int p = 0; p < mesh.primitives.Length; p++)
                {
                    //var primitive = mesh.primitives[p];
                    //primitive.Update(rootNode.vertices, vertices, rootNode.indices, jointMatrices);
                }
            }

            void UpdateNodeMatrix(Matrix4X4<float>[] matrix, Matrix4X4<float> parentMatrix, int index)
            {
                var node = rootNode.nodes[index];
                var res = Matrix4X4.CreateScale(node.scale) *
                            Matrix4X4.CreateFromQuaternion(node.rotation) *
                            Matrix4X4.CreateTranslation(node.translation);
                var mat = node.matrix * res * parentMatrix;
                matrix[index] = mat;

                if (node.childrens != null)
                {
                    foreach (var child in node.childrens)
                    {
                        UpdateNodeMatrix(matrix, mat, child);
                    }
                }
            }
        }

        public unsafe void UpdateDescriptorSets(VkContext vkContext, VulkanSampler sampler, DescriptorSet[] descriptorSets, VulkanBuffer[] buffer, int start)
        {
            rootNode.UpdateDescriptorSets(vkContext, sampler, descriptorSets, buffer, start);
            var writer = new VkDescriptorWriter(vkContext, 1, 0);
            writer.WriteBuffer(4, new DescriptorBufferInfo(buffers[start].buffer, 0, (ulong)(Unsafe.SizeOf<Matrix4X4<float>>() * jointsInverseMatrix.Length)), DescriptorType.StorageBuffer);
            writer.Write(descriptorSets[start]);
        }

        public void UpdateUniformBuffer(out Matrix4X4<float>[] modelMatrix)
        {
            rootNode.UpdateUniformBuffer(out modelMatrix);
        }
    }

    public class Gltf2Skin
    {
        public Matrix4X4<float>[] inverseBindMatrices;
        // the index of the node used as a skeleton root
        public int? skeleton;
        // indices of skeleton nodes, used as joints in this skin
        public int[] joints;
        public string? name;
        public Gltf2Skin(Gltf2Loader loader, Gltf gltf, int index)
        {
            var skin = gltf.Skins[index];
            joints = new int[skin.Joints.Length];
            skin.Joints.CopyTo(joints, 0);
            skeleton = skin.Skeleton;
            name = skin.Name;
            var ibmAccessor = new Gltf2Accessor(loader, gltf, skin.InverseBindMatrices!.Value);
            var ibmArr = ibmAccessor.GetMemory<Matrix4X4<float>>().ToArray();
            inverseBindMatrices = ibmArr;
        }
    }

    public class Gltf2Primitive
    {
        public uint vertexStart;
        public uint indexStart;
        public uint indexCount;
        public int materialIndex;

        public Gltf2Primitive(Gltf2Loader loader, Gltf gltf, int index, Mesh mesh)
        {
            var primitive = mesh.Primitives[index];
            materialIndex = primitive.Material!.Value;
            vertexStart = loader.VertexCount;
            indexStart = loader.IndexCount;

            var vList = new VertexList<Vertex>();
            var vertexs = new VertexBuffer(loader, gltf, primitive);
            var vertexArray = vertexs.GetVertex();
            for (int i = 0; i < vertexArray.Length; i++)
            {
                vList.Add(vertexArray[i]);
            }
            loader.vertexList.Add(vList);

            indexCount = loader.IndexCount - indexStart;
        }
    }

    public class Gltf2AnimatedPrimitive
    {
        public uint vertexStart;
        public uint indexStart;
        public uint indexCount;
        public int materialIndex;

        public Gltf2AnimatedPrimitive(Gltf2Loader loader, Gltf gltf, int index, Mesh mesh)
        {
            var primitive = mesh.Primitives[index];
            materialIndex = primitive.Material!.Value;
            vertexStart = loader.AnimatedVertexCount;
            indexStart = loader.AnimatedIndexCount;

            var vList = new VertexList<AnimatedVertex>();
            var vertexs = new AnimatedVertexBuffer(loader, gltf, primitive);
            var vertexArray = vertexs.GetVertex();
            for (int i = 0; i < vertexArray.Length; i++)
            {
                vList.Add(vertexArray[i]);
            }
            loader.animatedVertexList.Add(vList);

            indexCount = loader.AnimatedIndexCount - indexStart;
        }
    }

    public class Gltf2Mesh
    {
        public Gltf2Primitive[] primitives;

        public Gltf2Mesh(Gltf2Loader loader, Gltf gltf, int index)
        {
            var mesh = gltf.Meshes[index];
            primitives = new Gltf2Primitive[mesh.Primitives.Length];
            for (int i = 0; i < primitives.Length; i++)
            {
                primitives[i] = new Gltf2Primitive(loader, gltf, i, mesh);
            }
        }
    }

    public class Gltf2AnimatedMesh
    {
        public Gltf2AnimatedPrimitive[] primitives;

        public Gltf2AnimatedMesh(Gltf2Loader loader, Gltf gltf, int index)
        {
            var mesh = gltf.Meshes[index];
            primitives = new Gltf2AnimatedPrimitive[mesh.Primitives.Length];
            for (int i = 0; i < primitives.Length; i++)
            {
                primitives[i] = new Gltf2AnimatedPrimitive(loader, gltf, i, mesh);
            }
        }
    }

    public class Gltf2Node
    {
        // localTransform = translationMatrix * rotationMatrix * scaleMatrix
        public Vector3D<float> translation;
        public Quaternion<float> rotation;
        public Vector3D<float> scale;
        public Matrix4X4<float> matrix;

        public int? mesh;
        public int? skin;
        public int[] childrens;

        public Gltf2Node(Gltf2Loader loader, Gltf gltf, int index)
        {
            var node = gltf.Nodes[index];
            mesh = node.Mesh;
            skin = node.Skin;
            childrens = node.Children;
            var m = node.Matrix;
            var tmp = new Matrix4X4<float>(
                    m[0], m[1], m[2], m[3],
                    m[4], m[5], m[6], m[7],
                    m[8], m[9], m[10], m[11],
                    m[12], m[13], m[14], m[15]
                );
            matrix = tmp;
            translation = new Vector3D<float>(node.Translation[0], node.Translation[1], node.Translation[2]);
            rotation = new Quaternion<float>(node.Rotation[0], node.Rotation[1], node.Rotation[2], node.Rotation[3]);
            scale = new Vector3D<float>(node.Scale[0], node.Scale[1], node.Scale[2]);
        }
    }

    public class Gltf2RootNode : IDescriptor, IDisposable
    {
        public Vertex[] vertices;
        public uint[] indices;
        public int[] sceneNodesIdx;
        public Gltf2Node[] sceneNodes;
        public Gltf2Node[] nodes;
        public Gltf2Texture[] textures;
        public Gltf2Material[] materials;
        public Gltf2Mesh[] meshes;

        public int DescriptorSetCount { get
            {
                var count = 0;
                for (var i = 0; i < this.meshes.Length; i++)
                {
                    count += this.meshes[i].primitives.Length;
                }
                return count;
            }
            set { }
        }

        public void BindCommand(Vk vk, CommandBuffer commandBuffer,
                VulkanBuffer vertices, VulkanBuffer indices, Action<int> bindDescriptorSet)
        {
            vk.CmdBindVertexBuffers(commandBuffer, 0, 1, vertices, 0);
            vk.CmdBindIndexBuffer(commandBuffer, indices, 0, IndexType.Uint32);
            var offset = 0;
            foreach (var node in sceneNodes)
            {
                RecordNode(node, Matrix4X4<float>.Identity, ref offset);
            }

            void RecordNode(Gltf2Node node, Matrix4X4<float> matrix, ref int offset)
            {
                var nodeMatrix = matrix * node.matrix;
                
                if(node.mesh.HasValue)
                {
                    var mesh = meshes[node.mesh.Value];
                    var len = mesh.primitives.Length;
                    for (int i = 0; i < len; i++)
                    {
                        var prim = mesh.primitives[i];
                        bindDescriptorSet(i + offset);
                        vk.CmdDrawIndexed(commandBuffer, prim.indexCount, 1, prim.indexStart, (int)prim.vertexStart, 0);
                    }
                    offset += len;
                }
                
                if(node.childrens != null)
                {
                    foreach (var child in node.childrens)
                    {
                        RecordNode(nodes[child], nodeMatrix, ref offset);
                    }
                }
            }
        }

        public unsafe DescriptorPool CreateDescriptorPool(VkContext vkContext, DescriptorSetLayout[] setLayouts, DescriptorPoolSize[] poolSizes, int frames)
        {
            DescriptorPool pool;
            var setCount = setLayouts.Length * frames * DescriptorSetCount;
            var tmpPoolSizes = new DescriptorPoolSize[poolSizes.Length];
            for (int i = 0; i < poolSizes.Length; i++)
            {
                tmpPoolSizes[i] = poolSizes[i];
                tmpPoolSizes[i].DescriptorCount *= (uint)(frames * DescriptorSetCount);
            }

            fixed (DescriptorPoolSize* ptr = tmpPoolSizes)
            {
                var createInfo = new DescriptorPoolCreateInfo(StructureType.DescriptorPoolCreateInfo);
                createInfo.MaxSets = (uint)setCount;
                createInfo.PoolSizeCount = (uint)tmpPoolSizes.Length;
                createInfo.PPoolSizes = ptr;
                if (vkContext.vk.CreateDescriptorPool(vkContext.device, in createInfo, null, out pool) != Result.Success)
                {
                    throw new Exception("failed to create descriptor pool.");
                }
            }

            return pool;
        }

        public unsafe DescriptorSet[] AllocateDescriptorSets(VkContext vkContext, DescriptorPool pool, DescriptorSetLayout[] setLayouts, int frames)
        {
            var setCount = frames * DescriptorSetCount * setLayouts.Length;
            var sets = new DescriptorSet[setCount];

            var layouts = new DescriptorSetLayout[DescriptorSetCount * setLayouts.Length];
            var copyOffset = 0;
            for (int j = 0; j < DescriptorSetCount * setLayouts.Length; j++)
            {
                for (var i = 0; i < setLayouts.Length; i++)
                {
                    layouts[copyOffset + i] = setLayouts[i];
                }
                copyOffset += setLayouts.Length;
            }
            

            var descriptorSetCount = layouts.Length;
            var allocInfo = new DescriptorSetAllocateInfo(StructureType.DescriptorSetAllocateInfo);
            allocInfo.DescriptorPool = pool;
            allocInfo.DescriptorSetCount = (uint)descriptorSetCount;
            fixed (DescriptorSetLayout* ptr = layouts)
            {
                allocInfo.PSetLayouts = ptr;
                var offset = 0;
                for (int i = 0; i < frames; i++)
                {
                    var tmpSetLayout = new DescriptorSet[descriptorSetCount];
                    fixed (DescriptorSet* setPtr = tmpSetLayout)
                    {
                        if (vkContext.vk.AllocateDescriptorSets(vkContext.device, in allocInfo, setPtr) != Result.Success)
                        {
                            throw new Exception("failed to allocate descriptor sets.");
                        }
                    }
                    for (int j = 0; j < descriptorSetCount; j++)
                    {
                        sets[offset + j] = tmpSetLayout[j];
                    }

                    offset += descriptorSetCount;
                }
            }

            return sets;
        }

        public unsafe void UpdateDescriptorSets(VkContext vkContext,
            VulkanSampler sampler,
            DescriptorSet[] descriptorSets, VulkanBuffer[] buffer, int start)
        {
            foreach (var node in sceneNodes)
            {
                UpdateNodeDescriptorSets(vkContext, node, sampler,
                        descriptorSets, buffer, ref start);
            }
        }

        private unsafe void UpdateNodeDescriptorSets(VkContext vkContext,
            Gltf2Node node,
            VulkanSampler sampler,
            DescriptorSet[] descriptorSets, VulkanBuffer[] buffer, ref int offset)
        {
            if (node.mesh.HasValue)
            {
                var mesh = meshes[node.mesh.Value];
                var len = mesh.primitives.Length;
                for (int i = 0; i < len; i++)
                {
                    var prim = mesh.primitives[i];
                    var material = materials[prim.materialIndex];

                    var writer = new VkDescriptorWriter(vkContext, 1, 3);
                    writer.WriteBuffer(0, new DescriptorBufferInfo(buffer[offset + i], 0, (ulong)Unsafe.SizeOf<UniformBufferObject>()));
                    var imageInfos = new DescriptorImageInfo[]
                    {
                        new DescriptorImageInfo(sampler, material.baseTexture.imageView, ImageLayout.ShaderReadOnlyOptimal),
                        new DescriptorImageInfo(sampler, material.metallicTexture.imageView, ImageLayout.ShaderReadOnlyOptimal),
                        new DescriptorImageInfo(sampler, material.normalTexture.imageView, ImageLayout.ShaderReadOnlyOptimal),
                    };
                    writer.WriteImage(1, imageInfos);
                    writer.Write(descriptorSets[offset + i]);
                }
                offset += len;
            }

            if (node.childrens != null)
            {
                foreach (var child in node.childrens)
                {
                    UpdateNodeDescriptorSets(vkContext, nodes[child], sampler,
                        descriptorSets, buffer, ref offset);
                }
            }
        }

        public void UpdateUniformBuffer(out Matrix4X4<float>[] modelMatrix)
        {
            modelMatrix = new Matrix4X4<float>[DescriptorSetCount];
            var offset = 0;
            foreach (var node in sceneNodes)
            {
                UpdateNodeUniformBuffer(node, ref modelMatrix, Matrix4X4<float>.Identity, ref offset);
            }

            void UpdateNodeUniformBuffer(Gltf2Node node, ref Matrix4X4<float>[] modelMatrix, Matrix4X4<float> matrix, ref int offset)
            {
                var res = Matrix4X4.CreateTranslation(node.translation) *
                            Matrix4X4.CreateFromQuaternion(node.rotation) *
                            Matrix4X4.CreateScale(node.scale);
                var mat = node.matrix * res * matrix;

                if (node.mesh.HasValue)
                {
                    var len = meshes[node.mesh.Value].primitives.Length;
                    for (int i = 0; i < len; i++)
                    {
                        modelMatrix[offset + i] = mat;
                    }
                    offset += len;
                }
                
                if(node.childrens != null)
                {
                    foreach (var child in node.childrens)
                    {
                        UpdateNodeUniformBuffer(nodes[child], ref modelMatrix, mat, ref offset);
                    }
                }
            }
        }

        public void Dispose()
        {
            foreach(var tex in textures)
            {
                tex.Dispose();
            }
        }
    }

    public class Gltf2AnimatedRootNode : IDescriptor, IDisposable
    {
        public AnimatedVertex[] vertices;
        public uint[] indices;
        public int[] sceneNodesIdx;
        public Gltf2Node[] sceneNodes;
        public Gltf2Node[] nodes;
        public Gltf2Texture[] textures;
        public Gltf2Material[] materials;
        public Gltf2AnimatedMesh[] meshes;
        public Gltf2Skin[] skins;

        public int DescriptorSetCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < this.meshes.Length; i++)
                {
                    count += this.meshes[i].primitives.Length;
                }
                return count;
            }
            set { }
        }

        public void BindCommand(Vk vk, CommandBuffer commandBuffer,
                VulkanBuffer vertices, VulkanBuffer indices, Action<int> bindDescriptorSet)
        {
            vk.CmdBindVertexBuffers(commandBuffer, 0, 1, vertices, 0);
            vk.CmdBindIndexBuffer(commandBuffer, indices, 0, IndexType.Uint32);
            var offset = 0;
            foreach (var node in sceneNodes)
            {
                RecordNode(node, Matrix4X4<float>.Identity, ref offset);
            }

            void RecordNode(Gltf2Node node, Matrix4X4<float> matrix, ref int offset)
            {
                var nodeMatrix = matrix * node.matrix;

                if (node.mesh.HasValue)
                {
                    var mesh = meshes[node.mesh.Value];
                    var len = mesh.primitives.Length;
                    for (int i = 0; i < len; i++)
                    {
                        var prim = mesh.primitives[i];
                        bindDescriptorSet(i + offset);
                        vk.CmdDrawIndexed(commandBuffer, prim.indexCount, 1, prim.indexStart, (int)prim.vertexStart, 0);
                    }
                    offset += len;
                }

                if (node.childrens != null)
                {
                    foreach (var child in node.childrens)
                    {
                        RecordNode(nodes[child], nodeMatrix, ref offset);
                    }
                }
            }
        }

        public unsafe DescriptorPool CreateDescriptorPool(VkContext vkContext, DescriptorSetLayout[] setLayouts, DescriptorPoolSize[] poolSizes, int frames)
        {
            DescriptorPool pool;
            var setCount = setLayouts.Length * frames * DescriptorSetCount;
            var tmpPoolSizes = new DescriptorPoolSize[poolSizes.Length];
            for (int i = 0; i < poolSizes.Length; i++)
            {
                tmpPoolSizes[i] = poolSizes[i];
                tmpPoolSizes[i].DescriptorCount *= (uint)(frames * DescriptorSetCount);
            }

            fixed (DescriptorPoolSize* ptr = tmpPoolSizes)
            {
                var createInfo = new DescriptorPoolCreateInfo(StructureType.DescriptorPoolCreateInfo);
                createInfo.MaxSets = (uint)setCount;
                createInfo.PoolSizeCount = (uint)tmpPoolSizes.Length;
                createInfo.PPoolSizes = ptr;
                if (vkContext.vk.CreateDescriptorPool(vkContext.device, in createInfo, null, out pool) != Result.Success)
                {
                    throw new Exception("failed to create descriptor pool.");
                }
            }

            return pool;
        }

        public unsafe DescriptorSet[] AllocateDescriptorSets(VkContext vkContext, DescriptorPool pool, DescriptorSetLayout[] setLayouts, int frames)
        {
            var setCount = frames * DescriptorSetCount * setLayouts.Length;
            var sets = new DescriptorSet[setCount];

            var layouts = new DescriptorSetLayout[DescriptorSetCount * setLayouts.Length];
            var copyOffset = 0;
            for (int j = 0; j < DescriptorSetCount * setLayouts.Length; j++)
            {
                for (var i = 0; i < setLayouts.Length; i++)
                {
                    layouts[copyOffset + i] = setLayouts[i];
                }
                copyOffset += setLayouts.Length;
            }


            var descriptorSetCount = layouts.Length;
            var allocInfo = new DescriptorSetAllocateInfo(StructureType.DescriptorSetAllocateInfo);
            allocInfo.DescriptorPool = pool;
            allocInfo.DescriptorSetCount = (uint)descriptorSetCount;
            fixed (DescriptorSetLayout* ptr = layouts)
            {
                allocInfo.PSetLayouts = ptr;
                var offset = 0;
                for (int i = 0; i < frames; i++)
                {
                    var tmpSetLayout = new DescriptorSet[descriptorSetCount];
                    fixed (DescriptorSet* setPtr = tmpSetLayout)
                    {
                        if (vkContext.vk.AllocateDescriptorSets(vkContext.device, in allocInfo, setPtr) != Result.Success)
                        {
                            throw new Exception("failed to allocate descriptor sets.");
                        }
                    }
                    for (int j = 0; j < descriptorSetCount; j++)
                    {
                        sets[offset + j] = tmpSetLayout[j];
                    }

                    offset += descriptorSetCount;
                }
            }

            return sets;
        }

        public unsafe void UpdateDescriptorSets(VkContext vkContext,
            VulkanSampler sampler,
            DescriptorSet[] descriptorSets, VulkanBuffer[] buffer, int start)
        {
            foreach (var node in sceneNodes)
            {
                UpdateNodeDescriptorSets(vkContext, node, sampler,
                        descriptorSets, buffer, ref start);
            }
        }

        private unsafe void UpdateNodeDescriptorSets(VkContext vkContext,
            Gltf2Node node,
            VulkanSampler sampler,
            DescriptorSet[] descriptorSets, VulkanBuffer[] buffer, ref int offset)
        {
            if (node.mesh.HasValue)
            {
                var mesh = meshes[node.mesh.Value];
                var len = mesh.primitives.Length;
                for (int i = 0; i < len; i++)
                {
                    var prim = mesh.primitives[i];
                    var material = materials[prim.materialIndex];

                    var writer = new VkDescriptorWriter(vkContext, 1, 3);
                    writer.WriteBuffer(0, new DescriptorBufferInfo(buffer[offset + i], 0, (ulong)Unsafe.SizeOf<UniformBufferObject>()));
                    var imageInfos = new DescriptorImageInfo[]
                    {
                        new DescriptorImageInfo(sampler, material.baseTexture.imageView, ImageLayout.ShaderReadOnlyOptimal),
                        new DescriptorImageInfo(sampler, material.metallicTexture.imageView, ImageLayout.ShaderReadOnlyOptimal),
                        new DescriptorImageInfo(sampler, material.normalTexture.imageView, ImageLayout.ShaderReadOnlyOptimal),
                    };
                    writer.WriteImage(1, imageInfos);
                    writer.Write(descriptorSets[offset + i]);
                }
                offset += len;
            }

            if (node.childrens != null)
            {
                foreach (var child in node.childrens)
                {
                    UpdateNodeDescriptorSets(vkContext, nodes[child], sampler,
                        descriptorSets, buffer, ref offset);
                }
            }
        }

        public void UpdateUniformBuffer(out Matrix4X4<float>[] modelMatrix)
        {
            modelMatrix = new Matrix4X4<float>[DescriptorSetCount];
            var offset = 0;
            foreach (var node in sceneNodes)
            {
                UpdateNodeUniformBuffer(node, ref modelMatrix, Matrix4X4<float>.Identity, ref offset);
            }

            void UpdateNodeUniformBuffer(Gltf2Node node, ref Matrix4X4<float>[] modelMatrix, Matrix4X4<float> matrix, ref int offset)
            {
                var res = Matrix4X4.CreateTranslation(node.translation) *
                            Matrix4X4.CreateFromQuaternion(node.rotation) *
                            Matrix4X4.CreateScale(node.scale);
                var mat = node.matrix * res * matrix;

                if (node.mesh.HasValue)
                {
                    var len = meshes[node.mesh.Value].primitives.Length;
                    for (int i = 0; i < len; i++)
                    {
                        modelMatrix[offset + i] = mat;
                    }
                    offset += len;
                }

                if (node.childrens != null)
                {
                    foreach (var child in node.childrens)
                    {
                        UpdateNodeUniformBuffer(nodes[child], ref modelMatrix, mat, ref offset);
                    }
                }
            }
        }

        public void Dispose()
        {
            foreach (var tex in textures)
            {
                tex.Dispose();
            }
        }
    }

    public class Gltf2Loader
    {
        private string absPath;
        private string path;
        private string name;
        private Gltf gltf;
        public Gltf2RootNode rootNode;
        public Gltf2AnimatedRootNode animatedRootNode;
        public byte[][] buffers;
        public VertexList<Vertex> vertexList;
        public VertexList<AnimatedVertex> animatedVertexList;
        public Gltf2Loader(string path, string name)
        {
            this.path = path;
            this.name = name;
            absPath = Directory.GetCurrentDirectory();

            var realName = Path.Combine(path, name);
            gltf = Interface.LoadModel(realName);
            buffers = new byte[gltf.Buffers.Length][];
            for(int i = 0; i < buffers.Length; i++)
            {
                var bufferPath = Path.Combine(absPath, Path.Combine(path, gltf.Buffers[i].Uri));
                buffers[i] = gltf.LoadBinaryBuffer(i, bufferPath);
            }

            vertexList = new VertexList<Vertex>();
            animatedVertexList = new VertexList<AnimatedVertex>();
        }

        public VkContext vkContext;
        public SingleTimeCommand stCommand;
        public StagingBuffer stagingBuffer;
        public uint AnimatedVertexCount { get => animatedVertexList.GetVertexCount; }
        public uint AnimatedIndexCount { get => animatedVertexList.GetIndexCount; }
        public uint VertexCount { get => vertexList.GetVertexCount; }
        public uint IndexCount { get => vertexList.GetIndexCount; }
        public Gltf2RootNode Parse(VkContext vkContext, SingleTimeCommand stCommand, StagingBuffer staging)
        {
            this.vkContext = vkContext;
            this.stCommand = stCommand;
            this.stagingBuffer = staging;

            var textures = new Gltf2Texture[gltf.Textures.Length];
            for (int i = 0; i < textures.Length; i++)
            {
                textures[i] = new Gltf2Texture(this, gltf, i);
            }

            var materials = new Gltf2Material[gltf.Materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = new Gltf2Material(this, gltf, i, textures);
            }

            var meshes = new Gltf2Mesh[gltf.Meshes.Length];
            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i] = new Gltf2Mesh(this, gltf, i);
            }

            var nodes = new Gltf2Node[gltf.Nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new Gltf2Node(this, gltf, i);
            }

            var scene = gltf.Scenes[gltf.Scene!.Value];

            var rootNode = new Gltf2RootNode();
            rootNode.vertices = vertexList.GetVertices;
            rootNode.textures = textures;
            rootNode.indices = vertexList.GetIndices;
            rootNode.materials = materials;
            rootNode.meshes = meshes;
            rootNode.nodes = nodes;
            rootNode.sceneNodes = new Gltf2Node[scene.Nodes.Length];
            for (int i = 0; i < scene.Nodes.Length; i++)
            {
                rootNode.sceneNodes[i] = nodes[scene.Nodes[i]];
            }
            rootNode.sceneNodesIdx = scene.Nodes;

            //for (int i = 0; i < rootNode.sceneNodes.Length; i++)
            //{
            //    DumpSceneNodes("", i);
            //}
            void DumpSceneNodes(string prefix, int index)
            {
                var node = nodes[index];
                var sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(index.ToString() + " [");
                if (node.childrens != null)
                {
                    foreach (var item in node.childrens)
                    {
                        sb.Append(item.ToString() + ", ");
                    }
                }
                sb.Append(" ]");
                Console.WriteLine(sb);
                if(node.childrens != null)
                {
                    foreach (var item in node.childrens)
                    {
                        DumpSceneNodes(prefix + "-", item);
                    }
                }
            }
            
            this.rootNode = rootNode;
            return rootNode;
        }

        public Gltf2Animation[] ParseAnimation(VkContext vkContext, SingleTimeCommand stCommand, StagingBuffer staging)
        {
            this.vkContext = vkContext;
            this.stCommand = stCommand;
            this.stagingBuffer = staging;

            var textures = new Gltf2Texture[gltf.Textures.Length];
            for (int i = 0; i < textures.Length; i++)
            {
                textures[i] = new Gltf2Texture(this, gltf, i);
            }

            var materials = new Gltf2Material[gltf.Materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = new Gltf2Material(this, gltf, i, textures);
            }

            var meshes = new Gltf2AnimatedMesh[gltf.Meshes.Length];
            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i] = new Gltf2AnimatedMesh(this, gltf, i);
            }

            var nodes = new Gltf2Node[gltf.Nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new Gltf2Node(this, gltf, i);
            }

            var scene = gltf.Scenes[gltf.Scene!.Value];

            var rootNode = new Gltf2AnimatedRootNode();
            rootNode.vertices = animatedVertexList.GetVertices;
            rootNode.textures = textures;
            rootNode.indices = animatedVertexList.GetIndices;
            rootNode.materials = materials;
            rootNode.meshes = meshes;
            rootNode.nodes = nodes;
            rootNode.sceneNodes = new Gltf2Node[scene.Nodes.Length];
            for (int i = 0; i < scene.Nodes.Length; i++)
            {
                rootNode.sceneNodes[i] = nodes[scene.Nodes[i]];
            }
            rootNode.sceneNodesIdx = scene.Nodes;

            if (gltf.Skins.Length > 0)
            {
                var skins = new Gltf2Skin[gltf.Skins.Length];
                for (int i = 0; i < skins.Length; i++)
                {
                    var skin = new Gltf2Skin(this, gltf, i);
                    skins[i] = skin;
                }
                rootNode.skins = skins;
            }
            this.animatedRootNode = rootNode;

            var animations = new Gltf2Animation[gltf.Animations.Length];
            for (int i = 0; i < animations.Length; i++)
            {
                animations[i] = new Gltf2Animation(this, gltf, i);
                animations[i].vertexBuffer = rootNode.vertices;
                animations[i].indexBuffer = rootNode.indices;
            }
            return animations;
        }

        public Image<Rgba32> LoadImage(string uri)
        {
            var imName = Path.Combine(absPath, Path.Combine(path, uri));
            return Image.Load<Rgba32>(imName);
        }
    }
}
