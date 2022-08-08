using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Assimp;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

    public class GltfTexture
    {

    }

    public class GltfMaterial
    {
        public unsafe void LoadMaterial(Material* material)
        {
            for(var i = 0; i < material->MNumProperties; i++)
            {
                var property = material->MProperties[i];
                //property->
            }
        }
    }

    public class GltfMesh
    {
        public Vertex[] vertices;
        public uint[] indices;
        public GltfMaterial material;
        public unsafe void LoadMesh(Mesh* mesh, GltfMaterial[] materials)
        {
            var vertexMap = new Dictionary<Vertex, uint>();
            var vertices = new List<Vertex>();
            var indices = new List<uint>();

            var texIdx = mesh->MMaterialIndex;

            for (var i = 0; i < mesh->MNumFaces; i++)
            {
                var face = mesh->MFaces[i];
                for(var j = 0; j < face.MNumIndices; j++)
                {
                    var index = face.MIndices[j];
                    var position = mesh->MVertices[index];
                    var normal = mesh->MNormals[index];
                    var texture = mesh->MTextureCoords[(int)texIdx][index];

                    var vertex = new Vertex
                    {
                        pos = new Vec3(position.X, position.Y, position.Z),
                        color = new Vec3(normal.X, normal.Y, normal.Z),
                        //Flip Y for OBJ in Vulkan
                        texCoord = new Vec2(texture.X, 1.0f - texture.Y)
                    };
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

            this.vertices = vertices.ToArray();
            this.indices = indices.ToArray();
            material = materials[texIdx];
        }
    }

    public class GltfNode
    {
        public System.Numerics.Matrix4x4 matrix;
        // must be converted to matrices and
        //   postmultiplied in the T*R*S order
        //   which is first the scale, then the rotation,
        //      and then the translation
        //public Vector3D<float>? translation;
        //public Quaternion<float>? rotation;
        //public Vector3D<float>? scale;

        public GltfMesh[] meshes;
        public GltfNode[] childs;

        public unsafe void LoadNode(Node* node, GltfMesh[] meshes)
        {
            var meshNum = node->MNumMeshes;
            var childNum = node->MNumChildren;
            this.meshes = new GltfMesh[meshNum];
            this.childs = new GltfNode[childNum];
            for(var m = 0; m < meshNum; m++)
            {
                var idx = node->MMeshes[m];
                this.meshes[m] = meshes[idx];
            }
            for(var i = 0; i < childNum; i++)
            {
                this.childs[i] = new GltfNode();
                var cNode = node->MChildren[i];
                this.childs[i].LoadNode(cNode, meshes);
            }

            matrix = node->MTransformation;            
        }
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

    public class GltfModel
    {
        public Vertex[] verticesBuffer;
        public uint[] indicesBuffer;
        public Image texture;
        public GltfNode[] nodes;
        public Texture[] textures;

        public unsafe void LoadNode(List<Node> totalNodes, Node* parent)
        {

        }
    }

    public class GltfLoader //: IDisposable
    {
        public Vertex[] verticesBuffer;
        public uint[] indicesBuffer;
        public GltfNode rootNode;
        public Image image;

        public unsafe GltfLoader(string filename)
        {
            var path = Directory.GetCurrentDirectory();
            var name = Path.Combine(path, filename);
            using var assimp = Assimp.GetApi();
            var gltf = assimp.ImportFile(name, (uint)PostProcessPreset.TargetRealTimeMaximumQuality);
            
            var vertexMap = new Dictionary<Vertex, uint>();
            var vertices = new List<Vertex>();
            var indices = new List<uint>();

            //var imageName = gltf->MTextures[0]->MFilename.AsString;
            //image = Image.Load(Path.Combine(path, imageName));

            var materials = new GltfMaterial[gltf->MNumMaterials];
            for(var i = 0; i < gltf->MNumMaterials; i++)
            {
                materials[i] = new GltfMaterial();
                materials[i].LoadMaterial(gltf->MMaterials[i]);
            }

            var meshes = new GltfMesh[gltf->MNumMeshes];
            var verticesCount = 0;
            var indicesCount = 0;
            for(var i = 0; i < gltf->MNumMeshes; i++)
            {
                meshes[i] = new GltfMesh();
                meshes[i].LoadMesh(gltf->MMeshes[i], materials);
                verticesCount += meshes[i].vertices.Length;
                indicesCount += meshes[i].indices.Length;
            }

            rootNode = new GltfNode();
            rootNode.LoadNode(gltf->MRootNode, meshes);

            var verticesOffset = 0;
            var indicesOffset = 0;
            verticesBuffer = new Vertex[verticesCount];
            indicesBuffer = new uint[indicesCount];
            var vertexTarg = new Memory<Vertex>(verticesBuffer);
            var indexTarg = new Memory<uint>(indicesBuffer);
            foreach(var mesh in meshes)
            {
                vertexTarg.Slice(verticesOffset, mesh.vertices.Length);
                var src = new Memory<Vertex>(mesh.vertices);
                src.CopyTo(vertexTarg);

                indexTarg.Slice(indicesOffset, mesh.indices.Length);
                var indexSrc = new Memory<uint>(mesh.indices);
                indexSrc.CopyTo(indexTarg);
            }
            //verticesBuffer = meshes[0].vertices;
            //indicesBuffer = meshes[0].indices;
        }

        public Mat4 GetMatrix()
        {
            var mat = System.Numerics.Matrix4x4.Identity;
            for(var n = rootNode; n != null; n = n.childs[0])
            {
                mat = mat * n.matrix;
            }
            return new Mat4(
                mat.M11, mat.M12, mat.M13, mat.M14,
                mat.M21, mat.M22, mat.M23, mat.M24,
                mat.M31, mat.M32, mat.M33, mat.M34,
                mat.M41, mat.M42, mat.M43, mat.M44
                );
        }
    }
}
