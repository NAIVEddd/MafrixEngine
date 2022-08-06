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
using glTFLoader;
using glTFLoader.Schema;
using Image = SixLabors.ImageSharp.Image;

namespace MafrixEngine.ModelLoaders
{
    using Vec2 = Vector2D<float>;
    using Vec3 = Vector3D<float>;
    using Mat4 = Matrix4X4<float>;
    using BufferView = glTFLoader.Schema.BufferView;

    public struct Vertex
    {
        public Vec3 pos;
        public Vec3 color;
        public Vec2 texCoord;
        public Vertex(Vec3 p, Vec3 c, Vec2 t) => (pos, color, texCoord) = (p, c, t);
        public unsafe static VertexInputBindingDescription GetBindingDescription()
        {
            var bindingDescription = new VertexInputBindingDescription();
            bindingDescription.Binding = 0;
            bindingDescription.Stride = (uint)sizeof(Vertex);
            bindingDescription.InputRate = VertexInputRate.Vertex;

            return bindingDescription;
        }
        public unsafe static VertexInputAttributeDescription[] GetAttributeDescriptions()
        {
            var attributeDescriptions = new VertexInputAttributeDescription[3];
            attributeDescriptions[0].Binding = 0;
            attributeDescriptions[0].Location = 0;
            attributeDescriptions[0].Format = Format.R32G32B32Sfloat;
            attributeDescriptions[0].Offset = (uint)Marshal.OffsetOf<Vertex>("pos").ToInt32();
            attributeDescriptions[1].Binding = 0;
            attributeDescriptions[1].Location = 1;
            attributeDescriptions[1].Format = Format.R32G32B32Sfloat;
            attributeDescriptions[1].Offset = (uint)Marshal.OffsetOf<Vertex>("color").ToInt32();
            attributeDescriptions[2].Binding = 0;
            attributeDescriptions[2].Location = 2;
            attributeDescriptions[2].Format = Format.R32G32Sfloat;
            attributeDescriptions[2].Offset = (uint)Marshal.OffsetOf<Vertex>("texCoord").ToInt32();

            return attributeDescriptions;
        }
    }
    public class GltfNode
    {
        public Matrix4X4<float>? matrix;
        // must be converted to matrices and
        //   postmultiplied in the T*R*S order
        //   which is first the scale, then the rotation,
        //      and then the translation
        public Vector3D<float>? translation;
        public Quaternion<float>? rotation;
        public Vector3D<float>? scale;
    }

    public class GltfBuffer
    {
        public UInt64 length;
        public byte[] buffer;
    }

    public enum GlefBufferViewTarget
    {

    }

    public class GltfBufferView
    {
        //public GltfBufferView
        public GltfBuffer buffer;
        public UInt64 length;
        public int offset;
        public uint? byteStride; // size of element(target)
        public int target;
    }

    public class GltfMesh
    {
        public Vertex[] verticesBuffer;
        public uint[] indicesBuffer;
        public Image texture;
    }

    public class GltfLoader //: IDisposable
    {
        public Vertex[] verticesBuffer;
        public uint[] indicesBuffer;

        public unsafe GltfLoader(string filename)
        {
            var path = Directory.GetCurrentDirectory();
            var name = Path.Combine(path, filename);
            var gltf = Interface.LoadModel(name);

            var vertexMap = new Dictionary<Vertex, uint>();
            var vertices = new List<Vertex>();
            var indices = new List<uint>();
            
            var mesh = gltf.Meshes[0];
            var accessors = gltf.Accessors;
            BufferView[] bufferViews = gltf.BufferViews;
            var buffers = gltf.Buffers;
            byte[][] bytesBuffers = new byte[buffers.Length][];
            for(var j = 0; j < buffers.Length; j++)
            {
                byte[] buffer = gltf.LoadBinaryBuffer(j, name);
                bytesBuffers[j] = buffer;
            }
            //mesh
            //mesh[] ->
            //    primitives[] ->
            //      Attributes {string, int}[] (NORMAL,POSITION,TEXCOORD_n)
            //      indices int
            //      material int
            //      mode int|enum
            //Accessors[] ->
            //    BufferView int (index of view)
            //    Byteoffset int
            //    Count int
            //BufferViews[] ->
            //    Buffer int (index of buffer)
            //    ByteLength int
            //    ByteOffset int
            //Buffer[]
            //    ByteLength int
            //    Uri string
            for (var i = 0; i < mesh.Primitives.Length; i++)
            {
                int idx = mesh.Primitives[i].Indices!.Value;
                var primAccessor = accessors[idx];
                var primBufferView = bufferViews[primAccessor.BufferView!.Value];
                var primBuffer = buffers[primBufferView.Buffer];
                var primOffset = primAccessor.ByteOffset + primBufferView.ByteOffset;

                Accessor? normalAccessor = null;
                BufferView? normalBufferView = null;
                int normalBufferIdx = 0;
                Accessor? texCoordsAccessor = null;
                BufferView? texCoordsBufferView = null;
                int texCoordsBufferIdx = 0;
                Accessor? positionAcccessor = null;
                BufferView? positionBufferView = null;
                int positionBufferIdx = 0;
                foreach (var attr in mesh.Primitives[i].Attributes)
                {
                    if(attr.Key == "NORMAL")
                    {
                        normalAccessor = accessors[attr.Value];
                        normalBufferView = bufferViews[normalAccessor.BufferView!.Value];
                        normalBufferIdx = normalBufferView.Buffer;
                    } else if(attr.Key == "POSITION")
                    {
                        positionAcccessor = accessors[attr.Value];
                        positionBufferView = bufferViews[positionAcccessor.BufferView!.Value];
                        positionBufferIdx = positionBufferView.Buffer;
                    } else if(attr.Key == "TEXCOORD_0")
                    {
                        texCoordsAccessor = accessors[attr.Value];
                        texCoordsBufferView = bufferViews[texCoordsAccessor.BufferView!.Value];
                        texCoordsBufferIdx = texCoordsBufferView.Buffer;
                    }
                }

                Vec3 position;
                int posOffset = positionAcccessor!.ByteOffset + positionBufferView!.ByteOffset;
                Vec3 normal;
                int normalOffset = normalAccessor!.ByteOffset + normalBufferView!.ByteOffset;
                Vec2 texCoord;
                int texCoordOffset = texCoordsAccessor!.ByteOffset + texCoordsBufferView!.ByteOffset;

                byte[] buffer = bytesBuffers[primBufferView.Buffer];
                if(!(positionBufferIdx == normalBufferIdx && normalBufferIdx == texCoordsBufferIdx))
                {
                    throw new Exception("Bug: gltf buffer index not equal.");
                }

                fixed (byte* p = &buffer[primOffset])
                {
                    uint* iPtr = (uint*)p;
                    float* ptr = (float*)(p + posOffset);
                    Vec3* pPtr = (Vec3*)ptr;
                    ptr = (float*)(p + normalOffset);
                    Vec3* nPtr = (Vec3*)ptr;
                    ptr = (float*)(p + texCoordOffset);
                    Vec2* tPtr = (Vec2*)ptr;

                    for (var j = 0; j < primAccessor.Count; j++)
                    {
                        uint primIdx = iPtr[j];
                        position = pPtr[primIdx];
                        normal = nPtr[primIdx];
                        texCoord = tPtr[primIdx];
                        //texCoord = new Vec2(texCoord.X, 1.0f - texCoord.Y);
                        
                        var vertex = new Vertex(position, normal, texCoord);
                        if (vertexMap.TryGetValue(vertex, out var meshIndex))
                        {
                            indices.Add(meshIndex);
                        }
                        else
                        {
                            indices.Add((uint)vertices.Count);
                            vertexMap[vertex] = (uint)vertices.Count;
                            vertices.Add(vertex);
                        }
                    }
                }
            }

            verticesBuffer = vertices.ToArray();
            indicesBuffer = indices.ToArray();
        }


    }
}
