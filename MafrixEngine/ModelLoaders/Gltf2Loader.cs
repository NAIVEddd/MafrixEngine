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

        public Gltf2Accessor(Gltf2Loader loader, Gltf gltf, int index)
        {
            var accessor = gltf.Accessors[index];
            bufferView = new Gltf2BufferView(loader, gltf, accessor.BufferView!.Value);
            type = accessor.Type;
            componentType = accessor.ComponentType;
            count = accessor.Count;
        }

        public unsafe ReadOnlyMemory<T> GetMemory<T>() where T : struct
        {
            TypeCheck<T>();

            var totalOffset = offset + bufferView.offset;
            var buffer = bufferView.buffer.buffer;
            var source = new ReadOnlySpan<byte>(buffer, totalOffset, bufferView.length);
            var targ = MemoryMarshal.Cast<byte, T>(source);
            return new ReadOnlyMemory<T>(targ.ToArray(), 0, count);
        }

        private unsafe void TypeCheck<T>() where T : struct
        {
            Debug.Assert(true);
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

    public class Gltf2Primitive
    {
        public uint vertexStart;
        public uint indexStart;
        public uint indexCount;
        public int materialIndex;

        public Gltf2Primitive(Gltf2Loader loader, Gltf gltf, int index, Mesh mesh, bool startZero = false)
        {
            if(startZero)
            {
                var primitive = mesh.Primitives[index];
                materialIndex = primitive.Material!.Value;
                vertexStart = loader.VertexCount;
                indexStart = loader.IndexCount;
                var vertexs = new VertexBuffer(loader, gltf, primitive);
                indexCount = (uint)vertexs.Count;

                loader.vertexMap.Clear();
                foreach (var vertex in vertexs.GetVertex())
                {
                    if (loader.vertexMap.TryGetValue(vertex, out var meshIndex))
                    {
                        loader.indices.Add(meshIndex);
                    }
                    else
                    {
                        loader.indices.Add((uint)loader.vertices.Count);
                        loader.vertexMap[vertex] = (uint)loader.vertexMap.Count;
                        loader.vertices.Add(vertex);
                    }
                }
                loader.VertexCount += (uint)loader.vertexMap.Count;
                loader.IndexCount = (uint)loader.indices.Count;
            }
            else
            {
                var primitive = mesh.Primitives[index];
                materialIndex = primitive.Material!.Value;
                vertexStart = (uint)loader.VertexCount;
                indexStart = (uint)loader.IndexCount;
                var vertexs = new VertexBuffer(loader, gltf, primitive);
                loader.vertexMap.Clear();
                var verticesList = new List<Vertex>();
                var indicesList = new List<uint>();
                foreach (var vertex in vertexs.GetVertex())
                {
                    if (loader.vertexMap.TryGetValue(vertex, out var meshIndex))
                    {
                        indicesList.Add(meshIndex);
                    }
                    else
                    {
                        indicesList.Add((uint)verticesList.Count);
                        loader.vertexMap[vertex] = (uint)verticesList.Count;
                        verticesList.Add(vertex);
                    }
                }
                loader.vertices.AddRange(verticesList);
                loader.indices.AddRange(indicesList);
                loader.VertexCount = (uint)loader.vertices.Count;
                loader.IndexCount = (uint)loader.indices.Count;
                indexCount = loader.IndexCount - indexStart;
            }
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

    public class Gltf2Node
    {
        // localTransform = translationMatrix * rotationMatrix * scaleMatrix
        public Vector3D<float> translation;
        public Vector4D<float> rotation;
        public Vector3D<float> scale;
        public Matrix4X4<float> matrix;

        public int? mesh;
        public int[] childrens;

        public Gltf2Node(Gltf2Loader loader, Gltf gltf, int index)
        {
            var node = gltf.Nodes[index];
            mesh = node.Mesh;
            childrens = node.Children;
            var m = node.Matrix;
            var tmp = new Matrix4X4<float>(
                    m[0], m[1], m[2], m[3],
                    m[4], m[5], m[6], m[7],
                    m[8], m[9], m[10], m[11],
                    m[12], m[13], m[14], m[15]
                );
            matrix = Matrix4X4.Transpose(tmp);
            translation = new Vector3D<float>(node.Translation[0], node.Translation[1], node.Translation[2]);
            rotation = new Vector4D<float>(node.Rotation[0], node.Rotation[1], node.Rotation[2], node.Rotation[3]);
            scale = new Vector3D<float>(node.Scale[0], node.Scale[1], node.Scale[2]);
        }
    }

    public class Gltf2RootNode : IDisposable
    {
        public Vertex[] vertices;
        public uint[] indices;
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
        }

        public void BindCommand(Vk vk, CommandBuffer commandBuffer,
                VulkanBuffer vertices, VulkanBuffer indices, Action<int> bindDescriptorSet)
        {
            vk.CmdBindVertexBuffers(commandBuffer, 0, 1, vertices, 0);
            vk.CmdBindIndexBuffer(commandBuffer, indices, 0, IndexType.Uint32);
            var offset = 0;
            foreach (var node in nodes)
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
                        vk.CmdDrawIndexed(commandBuffer, prim.indexCount, 1, prim.indexStart, (int)prim.vertexStart, 0);// (uint)m);
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

        public unsafe void UpdateDescriptorSets(Vk vk, Device device,
            WriteDescriptorSet[] descriptorWrites,
            DescriptorImageInfo[] imageInfo,
            DescriptorBufferInfo bufferInfo,
            DescriptorSet[] descriptorSets, VulkanBuffer[] buffer, int start)
        {
            descriptorWrites[0].PBufferInfo = &bufferInfo;
            fixed(DescriptorImageInfo* pimageInfo = imageInfo)
            {
                descriptorWrites[1].PImageInfo = pimageInfo;
                foreach (var node in nodes)
                {
                    UpdateNodeDescriptorSets(vk, device, node,
                            descriptorWrites, imageInfo, ref bufferInfo,
                            descriptorSets, buffer, ref start);
                }
            }
            
        }

        private unsafe void UpdateNodeDescriptorSets(Vk vk, Device device,
            Gltf2Node node,
            WriteDescriptorSet[] descriptorWrites,
            DescriptorImageInfo[] imageInfo,
            ref DescriptorBufferInfo bufferInfo,
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

                    bufferInfo.Buffer = buffer[offset + i];
                    imageInfo[0].ImageView = material.baseTexture.imageView;
                    imageInfo[1].ImageView = material.metallicTexture.imageView;
                    imageInfo[2].ImageView = material.normalTexture.imageView;
                    descriptorWrites[0].DstSet = descriptorSets[offset + i];
                    descriptorWrites[1].DstSet = descriptorSets[offset + i];
                    fixed (WriteDescriptorSet* descPtr = descriptorWrites)
                    {
                        vk.UpdateDescriptorSets(device, 2, descPtr, 0, null);
                    }
                }
                offset += len;
            }

            if (node.childrens != null)
            {
                foreach (var child in node.childrens)
                {
                    UpdateNodeDescriptorSets(vk, device, nodes[child],
                        descriptorWrites, imageInfo, ref bufferInfo,
                        descriptorSets, buffer, ref offset);
                }
            }
        }

            public void UpdateUniformBuffer(out Matrix4X4<float>[] modelMatrix)
        {
            modelMatrix = new Matrix4X4<float>[DescriptorSetCount];
            var offset = 0;
            foreach (var node in nodes)
            {
                UpdateNodeUniformBuffer(node, ref modelMatrix, Matrix4X4<float>.Identity, ref offset);
            }

            void UpdateNodeUniformBuffer(Gltf2Node node, ref Matrix4X4<float>[] modelMatrix, Matrix4X4<float> matrix, ref int offset)
            {
                var mat = node.matrix * matrix;

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

    public class Gltf2Loader
    {
        private string absPath;
        private string path;
        private string name;
        private Gltf gltf;
        public byte[][] buffers;
        public Dictionary<Vertex, uint> vertexMap;
        public List<Vertex> vertices;
        public List<uint> indices;
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

            vertexMap = new Dictionary<Vertex, uint>();
            vertices = new List<Vertex>();
            indices = new List<uint>();
        }

        public VkContext vkContext;
        public SingleTimeCommand stCommand;
        public StagingBuffer stagingBuffer;
        public uint VertexCount { get; set; }
        public uint IndexCount { get; set; }
        public Gltf2RootNode Parse(VkContext vkContext, SingleTimeCommand stCommand, StagingBuffer staging)
        {
            this.vkContext = vkContext;
            this.stCommand = stCommand;
            this.stagingBuffer = staging;
            VertexCount = 0;
            IndexCount = 0;

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
            rootNode.vertices = vertices.ToArray();
            rootNode.textures = textures;
            rootNode.indices = indices.ToArray();
            rootNode.materials = materials;
            rootNode.meshes = meshes;
            rootNode.nodes = new Gltf2Node[scene.Nodes.Length];
            for (int i = 0; i < rootNode.nodes.Length; i++)
            {
                rootNode.nodes[i] = nodes[scene.Nodes[i]];
            }
            return rootNode;
        }

        public Image<Rgba32> LoadImage(string uri)
        {
            var imName = Path.Combine(absPath, Path.Combine(path, uri));
            return Image.Load<Rgba32>(imName);
        }
    }
}
