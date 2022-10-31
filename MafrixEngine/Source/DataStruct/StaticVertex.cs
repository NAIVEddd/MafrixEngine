using MafrixEngine.Source.Interface;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MafrixEngine.Source.DataStruct
{
    using Vec2 = Vector2D<float>;
    using Vec3 = Vector3D<float>;
    using Mat4 = Matrix4X4<float>;
    using Buffer = Silk.NET.Vulkan.Buffer;

    public struct UniformBufferObject
    {
        public Mat4 model;
        public Mat4 view;
        public Mat4 proj;
        public UniformBufferObject(Mat4 m, Mat4 v, Mat4 p) => (model, view, proj) = (m, v, p);
    }

    public struct Vertex : IVertexData
    {
        public Vec3 pos;
        public Vec3 normal;
        public Vec2 texCoord;

        public VertexInputBindingDescription BindingDescription => GetBindingDescription();

        public VertexInputAttributeDescription[] AttributeDescriptions => GetAttributeDescriptions();

        public Vertex(Vec3 p, Vec3 n, Vec2 t) => (pos, normal, texCoord) = (p, n, t);
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
            attributeDescriptions[1].Offset = (uint)Marshal.OffsetOf<Vertex>("normal").ToInt32();
            attributeDescriptions[2].Binding = 0;
            attributeDescriptions[2].Location = 2;
            attributeDescriptions[2].Format = Format.R32G32Sfloat;
            attributeDescriptions[2].Offset = (uint)Marshal.OffsetOf<Vertex>("texCoord").ToInt32();

            return attributeDescriptions;
        }

        public override string ToString()
        {
            return pos.ToString() + normal.ToString() + texCoord.ToString();
        }
    }

    public class VertexList<T> where T : IVertexData
    {
        Dictionary<T, uint> vertexMap = new Dictionary<T, uint>();
        List<T> verticesList = new List<T>();
        List<uint> indicesList = new List<uint>();

        public T[] GetVertices { get { return verticesList.ToArray(); } }
        public uint[] GetIndices { get { return indicesList.ToArray(); } }
        public uint GetVertexCount { get { return (uint)verticesList.Count; } }
        public uint GetIndexCount { get { return (uint)indicesList.Count; } }
        public uint Add(T vertex)
        {
            if (vertexMap.TryGetValue(vertex, out var meshIndex))
            {
                indicesList.Add(meshIndex);
                return meshIndex;
            }
            else
            {
                var newIndex = (uint)verticesList.Count;
                indicesList.Add((uint)newIndex);
                vertexMap[vertex] = (uint)verticesList.Count;
                verticesList.Add(vertex);
                return newIndex;
            }
        }

        public void Add(VertexList<T> vertexList)
        {
            verticesList.AddRange(vertexList.verticesList);
            indicesList.AddRange(vertexList.indicesList);
        }
    }
    internal class StaticVertex
    {
    }
}
