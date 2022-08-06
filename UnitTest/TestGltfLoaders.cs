using System;
using System.IO;
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
using glTFLoader;
using glTFLoader.Schema;
using SharpGLTF;
using SharpGLTF.Schema2;
using SharpGLTF.Transforms;
using Vertex = MafrixEngine.ModelLoaders.Vertex;
//using MafrixEngine.ModelLoaders;
using Image = SixLabors.ImageSharp.Image;
using Node = Silk.NET.Assimp.Node;

namespace UnitTest
{
    public class TestGltfLoaders
    {
        [Fact]
        public unsafe void TestGltfLoader()
        {
            using var assimp = Assimp.GetApi();
            var scene = assimp.ImportFile("Asserts/viking_room/scene.gltf", (uint)PostProcessPreset.TargetRealTimeMaximumQuality);

            Assert.NotNull(scene->ToString());
            Assert.Equal(1, (int)scene->MNumMeshes);
            Assert.Equal(1, (int)scene->MNumMaterials);
            Assert.NotEqual(0u, (uint)scene->MMeshes);
            Assert.NotEqual(0u, (uint)scene->MMaterials);

            var vertexMap = new Dictionary<Vertex, uint>();
            var vertices = new List<Vertex>();
            var indices = new List<uint>();

            VisitSceneNode(scene->MRootNode);
            assimp.ReleaseImport(scene);

            void VisitSceneNode(Node* node)
            {
                for (int m = 0; m < node->MNumMeshes; m++)
                {
                    var mesh = scene->MMeshes[node->MMeshes[m]];

                    for (int f = 0; f < mesh->MNumFaces; f++)
                    {
                        var face = mesh->MFaces[f];

                        for (int i = 0; i < face.MNumIndices; i++)
                        {
                            uint index = face.MIndices[i];

                            var position = mesh->MVertices[index];
                            var texture = mesh->MTextureCoords[0][(int)index];

                            Vertex vertex = new Vertex
                            {
                                pos = new Vector3D<float>(position.X, position.Y, position.Z),
                                color = new Vector3D<float>(1, 1, 1),
                                //Flip Y for OBJ in Vulkan
                                texCoord = new Vector2D<float>(texture.X, 1.0f - texture.Y)
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
                }

                for (int c = 0; c < node->MNumChildren; c++)
                {
                    VisitSceneNode(node->MChildren[c]);
                }
            }
        }

        private string gltfName = @"Asserts/viking_room/scene.gltf";
        [Fact]
        public unsafe void TestGltf2Loader()
        {
            var path = Directory.GetCurrentDirectory();
            var filename = Path.Combine(path, gltfName);
            Assert.True(Directory.Exists(path));

            var gltf = Interface.LoadModel(filename);
            Assert.NotNull(gltf);

            // read all buffers
            for(var i = 0; i < gltf.Buffers?.Length; i++)
            {
                var expectedLength = gltf.Buffers[i].ByteLength;
                var bufferBytes = gltf.LoadBinaryBuffer(i, filename);

                Assert.NotEqual(0, expectedLength);
                Assert.NotNull(bufferBytes);
            }

            // open all images
            for(int i = 0; i < gltf.Images?.Length; i++)
            {
                using (var s = gltf.OpenImageFile(i, filename))
                {
                    Assert.NotNull(s);

                    var image = Image.Load<Rgba32>(s);
                    Assert.NotNull(image);
                    Assert.NotEqual(0, image.Width);
                    Assert.NotEqual(0, image.Height);

                    s.Seek(0, SeekOrigin.Begin);
                    using (var rb = new BinaryReader(s))
                    {
                        uint header = rb.ReadUInt32();

                        var isPngOrJpeg = header == 0x474e5089 || header == 0xd8ff;
                        Assert.True(isPngOrJpeg);
                    }
                }
            }
        }

        [Fact]
        public unsafe void TestSharpGltf()
        {
            Assert.True(true);
        }
    }
}
