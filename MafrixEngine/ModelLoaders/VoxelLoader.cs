using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using Buffer = Silk.NET.Vulkan.Buffer;
using MafrixEngine.GraphicsWrapper;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using Image = SixLabors.ImageSharp.Image;
using Serilog.Core;
using ThirdPartyLib;
using System.Drawing;
using MafrixEngine.Source.Interface;
using MafrixEngine.Source.DataStruct;

namespace MafrixEngine.ModelLoaders
{
    public enum VoxelType : byte
    {
        Air = 0,
        Dirt,
        Land,
        Water,
    }

    public class VoxelLoader : IDescriptor, IDisposable
    {
        private Cube cube;
        public Vertex[] vertices;
        public uint[] indices;
        public VkTexture texture;

        public VoxelLoader()
        {
            var vList = new VertexList<Vertex>();
            cube = new Cube(vList);
        }

        public int DescriptorSetCount { get; set; } = 1;
        public void BindCommand(Vk vk, CommandBuffer commandBuffer,
                Buffer vertices, Buffer indices, Action<int> bindDescriptorSet)
        {
            vk.CmdBindVertexBuffers(commandBuffer, 0, 1, vertices, 0);
            vk.CmdBindIndexBuffer(commandBuffer, indices, 0, IndexType.Uint32);
            bindDescriptorSet(0);
            vk.CmdDrawIndexed(commandBuffer, (uint)this.indices.Length, 1, 0, 0, 0);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Build(VkContext vkContext, SingleTimeCommand stCommand, StagingBuffer stagingBuffer)
        {
            var block = new TerrainChunkWithType(new Vector3D<int>(0, 0, 0));
            vertices = block.vertices;
            indices = block.indices;

            Image<Rgba32> image = Image.Load<Rgba32>("./Textures/lim.PNG");
            this.texture = new VkTexture(vkContext, stCommand, stagingBuffer, image);
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

            fixed(DescriptorPoolSize* ptr = tmpPoolSizes)
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

            var layouts = new DescriptorSetLayout[setLayouts.Length];
            for (var i = 0; i < layouts.Length; i++)
            {
                layouts[i] = setLayouts[i];
            }

            var descriptorSetCount = (layouts.Length * DescriptorSetCount);
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
                    if(vkContext.vk.AllocateDescriptorSets(vkContext.device, &allocInfo, tmpSetLayout) != Result.Success)
                    {
                        throw new Exception("failed to allocate descriptor sets.");
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
            Sampler sampler,
            DescriptorSet[] descriptorSets, Buffer[] buffer, int start)
        {
            var writer = new VkOldDescriptorWriter(vkContext, 1, 3);
            writer.WriteBuffer(0, new DescriptorBufferInfo(buffer[start], 0, (ulong)Unsafe.SizeOf<UniformBufferObject>()));
            var imageInfos = new DescriptorImageInfo[]
            {
                new DescriptorImageInfo(sampler, texture.imageView, ImageLayout.ShaderReadOnlyOptimal),
                new DescriptorImageInfo(sampler, texture.imageView, ImageLayout.ShaderReadOnlyOptimal),
                new DescriptorImageInfo(sampler, texture.imageView, ImageLayout.ShaderReadOnlyOptimal),
            };
            writer.WriteImage(1, imageInfos);
            writer.Write(descriptorSets[start]);
        }

        public void UpdateUniformBuffer(out Matrix4X4<float>[] modelMatrix)
        {
            modelMatrix = new Matrix4X4<float>[DescriptorSetCount];
            modelMatrix[0] = Matrix4X4<float>.Identity;
        }
    }

    public class TerrainChunk
    {
        static public uint SideCount { get; set; } = 16;
        static public uint HeightCount { get; set; } = 1;
        static public uint CubeCount { get { return SideCount * SideCount * HeightCount; } }
        static public float SideLength { get; set; } = SideCount * Cube.SideLength;
        static public float HeightLength { get; set; } = HeightCount * Cube.SideLength;
        public Vertex[] vertices { get; set; } = Array.Empty<Vertex>();
        public uint[] indices { get; set; } = Array.Empty<uint>();
        public uint indexCount = SideCount * SideCount * HeightCount * 36u;
        //private Vector3D<float>[,,] voxelPositions = new Vector3D<float>[HeightCount, SideCount, SideCount];
        private Cube cube;

        public TerrainChunk(Vector3D<int> blockIndex)
        {
            var position = new Vector3D<float>(blockIndex.X * SideLength,
                                                blockIndex.Y * HeightLength,
                                                blockIndex.Z * SideLength);
            var vList = new VertexList<Vertex>();
            cube = new Cube(vList);
            vertices = new Vertex[CubeCount * 24];
            indices = new uint[CubeCount * 36];
            
            var curPos = position;
            var vertexOffset = 0;
            uint indexOffset = 0;
            for (int y = 0; y < HeightCount; y++)
            {
                curPos.Z = position.Z;
                for (int z = 0; z < SideCount; z++)
                {
                    curPos.X = position.X;
                    for (int x = 0; x < SideCount; x++)
                    {
                        cube.GetVertex(curPos, out var vert, out var indi);
                        for (int k = 0; k < vert.Length; k++)
                        {
                            vertices[vertexOffset + k] = vert[k];
                        }
                        for (int k = 0; k < indi.Length; k++)
                        {
                            indices[indexOffset + k] = indi[k] + (uint)vertexOffset;
                        }
                        vertexOffset += vert.Length;
                        indexOffset += (uint)indi.Length;
                        curPos.X += Cube.SideLength;
                    }
                    curPos.Z += Cube.SideLength;
                }
                curPos.Y += Cube.SideLength;
            }
        }
    }

    public class TerrainChunkWithType
    {
        static public uint SideCount { get; set; } = 64;
        static public uint HeightCount { get; set; } = 100;
        static public uint CubeCount { get { return SideCount * SideCount * HeightCount; } }
        static public float SideLength { get; set; } = SideCount * Cube.SideLength;
        static public float HeightLength { get; set; } = HeightCount * Cube.SideLength;
        public Vertex[] vertices { get; set; } = Array.Empty<Vertex>();
        public uint[] indices { get; set; } = Array.Empty<uint>();
        public uint indexCount = SideCount * SideCount * HeightCount * 36u;
        private Vector3D<float>[,,] voxelPositions = new Vector3D<float>[HeightCount + 2, SideCount + 2, SideCount + 2];
        private CubeFace[,,] voxelNeighbor = new CubeFace[HeightCount + 2, SideCount + 2, SideCount + 2];
        private VoxelType[,,] voxelTypes = new VoxelType[HeightCount + 2, SideCount + 2, SideCount + 2];
        private Vector3D<int>[,,] voxelIndex = new Vector3D<int> [HeightCount + 2, SideCount + 2, SideCount + 2];
        private Cube cube;
        private FastNoise fastNoise;
        private float[] noiseData;

        public TerrainChunkWithType(Vector3D<int> blockIndex)
        {
            var position = new Vector3D<float>(blockIndex.X * SideLength,
                                                blockIndex.Y * HeightLength,
                                                blockIndex.Z * SideLength);
            var cubeIndex = new Vector3D<int>(blockIndex.X * (int)SideCount,
                                                blockIndex.Y * (int)SideCount,
                                                blockIndex.Z * (int)HeightCount);
            var curCubeIndex = cubeIndex;

            {   // Generate noise data
                var frequency = 0.08f;
                var amplitude = 3.0f;

                var perlin = new FastNoise("Perlin");
                var gradient = new FastNoise("DomainWarpGradient");
                var fractal = new FastNoise("DomainWarpFractalProgressive");

                gradient.Set("Source", perlin);
                gradient.Set("WarpAmplitude", amplitude);
                gradient.Set("WarpFrequency", frequency);
                fractal.Set("Domain Warp Source", gradient);
                fractal.Set("Octaves", 4);

                fastNoise = fractal;
                noiseData = new float[SideCount * SideCount];
                var minMax = fastNoise.GenUniformGrid2D(noiseData, cubeIndex.X, cubeIndex.Z, (int)SideCount, (int)SideCount, 0.2f, 1337);
            }

            var vList = new VertexList<Vertex>();
            cube = new Cube(vList);

            var curPos = position;
            for (int y = 1; y <= HeightCount; y++)
            {
                curPos.Z = position.Z;
                curCubeIndex.Y = cubeIndex.Y + y;
                for (int z = 1; z <= SideCount; z++)
                {
                    curPos.X = position.X;
                    curCubeIndex.Z = cubeIndex.Z + z;
                    for (int x = 1; x <= SideCount; x++)
                    {
                        curCubeIndex.X = cubeIndex.X + x;

                        voxelPositions[y, z, x] = curPos;
                        voxelTypes[y, z, x] = GetBlock(curCubeIndex);
                        voxelIndex[y, z, x] = curCubeIndex;

                        curPos.X += Cube.SideLength;
                    }
                    curPos.Z += Cube.SideLength;
                }
                curPos.Y += Cube.SideLength;
            }
            var l = new ValueTuple<Vector3D<int>, CubeFace>[]
            {
                (new Vector3D<int>(0, -1, 0), CubeFace.Bottom),
                (new Vector3D<int>(0, 0, -1), CubeFace.Back),
                (new Vector3D<int>(1, 0, 0), CubeFace.Right),
                (new Vector3D<int>(0, 0, 1), CubeFace.Front),
                (new Vector3D<int>(-1, 0, 0), CubeFace.Left),
                (new Vector3D<int>(0, 1, 0), CubeFace.Top),
            };
            for (int y = 1; y <= HeightCount; y++)
            {
                for (int z = 1; z <= SideCount; z++)
                {
                    for (int x = 1; x <= SideCount; x++)
                    {
                        var p = voxelIndex[y, z, x];
                        if(voxelTypes[p.Y, p.Z, p.X] != VoxelType.Air)
                        {
                            var face = CubeFace.None;
                            foreach (var (v, f) in l)
                            {
                                var idx = p + v;
                                if (voxelTypes[idx.Y, idx.Z, idx.X] == VoxelType.Air)
                                {
                                    face |= f;
                                }
                            }
                            cube.GetVertex(voxelPositions[y, z, x], face);
                        }

                    }
                }
            }
            //cube.GetVertex(default, CubeFace.Bottom | CubeFace.Left | CubeFace.Right
            //    | CubeFace.Front | CubeFace.Top | CubeFace.Back);
            vertices = vList.GetVertices;
            indices = vList.GetIndices;
        }
        private VoxelType GetBlock(Vector3D<int> index)
        {
            index += new Vector3D<int>(-1, -1, -1);
            var noise = noiseData[index.Z * SideCount + index.X];
            var surfaceY = 5 + noise * 4;

            var seaLevel = 5;
            if (index.Y < surfaceY)
            {
                return VoxelType.Dirt;
            } else if(index.Y < seaLevel)
            {
                return VoxelType.Water;
            }
            return VoxelType.Air;
        }
    }

    [Flags]
    public enum CubeFace
    {
        None = 0,
        Bottom = 1,
        Back = 2,
        Right = 4,
        Front = 8,
        Left = 16,
        Top = 32,
    }

    public class Cube
    {
        private VertexList<Vertex> vList;
        public Cube(VertexList<Vertex> vertexList)
        {
            this.vList = vertexList;
        }

        static Vector3D<float>[] Position = new Vector3D<float>[8]
            {
                new Vector3D<float>(-1, -1, -1),
                new Vector3D<float>(1, -1, -1),
                new Vector3D<float>(-1, -1, 1),
                new Vector3D<float>(1, -1, 1),
                new Vector3D<float>(-1, 1, -1),
                new Vector3D<float>(1, 1, -1),
                new Vector3D<float>(-1, 1, 1),
                new Vector3D<float>(1, 1, 1),
            };
        static Vector3D<float>[] Normals = new Vector3D<float>[6]
            {
                new Vector3D<float>(0, -1, 0),  // bottom
                new Vector3D<float>(0, 0, -1),  // back
                new Vector3D<float>(1, 0, 0),   // right
                new Vector3D<float>(0, 0, 1),   // front
                new Vector3D<float>(-1, 0, 0),  // left
                new Vector3D<float>(0, 1, 0)    // top
            };
        static public float SideLength { get; set; } = 2.0f;
        public void GetVertex(Vector3D<float> pos, out Vertex[] vertex, out uint[] index)
        {
            var positions = new Vector3D<float>[8];
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = Position[i] + pos;
            }
            var texs = new Vector2D<float>[4]
            {
                new Vector2D<float>(1, 1),
                new Vector2D<float>(1, 0),
                new Vector2D<float>(0, 1),
                new Vector2D<float>(0, 0),
            };

            vertex = new Vertex[24]
            {
                new Vertex(positions[1], Normals[0], texs[3]), // bottom
                new Vertex(positions[2], Normals[0], texs[0]),
                new Vertex(positions[3], Normals[0], texs[2]),
                new Vertex(positions[0], Normals[0], texs[1]),
                new Vertex(positions[5], Normals[1], texs[3]), // back
                new Vertex(positions[0], Normals[1], texs[0]),
                new Vertex(positions[1], Normals[1], texs[2]),
                new Vertex(positions[4], Normals[1], texs[1]),
                new Vertex(positions[7], Normals[2], texs[3]), // right
                new Vertex(positions[1], Normals[2], texs[0]),
                new Vertex(positions[3], Normals[2], texs[2]),
                new Vertex(positions[5], Normals[2], texs[1]),
                new Vertex(positions[6], Normals[3], texs[3]), // front
                new Vertex(positions[3], Normals[3], texs[0]),
                new Vertex(positions[2], Normals[3], texs[2]),
                new Vertex(positions[7], Normals[3], texs[1]),
                new Vertex(positions[4], Normals[4], texs[3]),  // left
                new Vertex(positions[2], Normals[4], texs[0]),
                new Vertex(positions[0], Normals[4], texs[2]),
                new Vertex(positions[6], Normals[4], texs[1]),
                new Vertex(positions[7], Normals[5], texs[3]),  // top
                new Vertex(positions[4], Normals[5], texs[0]),
                new Vertex(positions[5], Normals[5], texs[2]),
                new Vertex(positions[6], Normals[5], texs[1]),
            };
            index = new uint[36]
            {
                0,1,2, 3,1,0,
                4,5,6, 7,5,4,
                8,9,10, 11,9,8,
                12,13,14, 15,13,12,
                16,17,18, 19,17,16,
                20,21,22, 23,21,20
            };
        }

        public void GetVertex(Vector3D<float> pos, CubeFace face)
        {
            var positions = new Vector3D<float>[8];
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = Position[i] + pos;
            }
            var texs = new Vector2D<float>[4]
            {
                new Vector2D<float>(1, 1),
                new Vector2D<float>(1, 0),
                new Vector2D<float>(0, 1),
                new Vector2D<float>(0, 0),
            };

            var vertex = new Vertex[24]
            {
                new Vertex(positions[1], Normals[0], texs[3]), // bottom
                new Vertex(positions[2], Normals[0], texs[0]),
                new Vertex(positions[3], Normals[0], texs[2]),
                new Vertex(positions[0], Normals[0], texs[1]),
                new Vertex(positions[5], Normals[1], texs[3]), // back
                new Vertex(positions[0], Normals[1], texs[0]),
                new Vertex(positions[1], Normals[1], texs[2]),
                new Vertex(positions[4], Normals[1], texs[1]),
                new Vertex(positions[7], Normals[2], texs[3]), // right
                new Vertex(positions[1], Normals[2], texs[0]),
                new Vertex(positions[3], Normals[2], texs[2]),
                new Vertex(positions[5], Normals[2], texs[1]),
                new Vertex(positions[6], Normals[3], texs[3]), // front
                new Vertex(positions[3], Normals[3], texs[0]),
                new Vertex(positions[2], Normals[3], texs[2]),
                new Vertex(positions[7], Normals[3], texs[1]),
                new Vertex(positions[4], Normals[4], texs[3]),  // left
                new Vertex(positions[2], Normals[4], texs[0]),
                new Vertex(positions[0], Normals[4], texs[2]),
                new Vertex(positions[6], Normals[4], texs[1]),
                new Vertex(positions[7], Normals[5], texs[3]),  // top
                new Vertex(positions[4], Normals[5], texs[0]),
                new Vertex(positions[5], Normals[5], texs[2]),
                new Vertex(positions[6], Normals[5], texs[1]),
            };
            var index = new uint[36]
            {
                0,1,2, 3,1,0,
                4,5,6, 7,5,4,
                8,9,10, 11,9,8,
                12,13,14, 15,13,12,
                16,17,18, 19,17,16,
                20,21,22, 23,21,20
            };

            var vertexOffset = 0;
            var flag = 1;
            for (int i = 0; i < 6; i++)
            {
                if(((CubeFace)(flag << i) & face) != CubeFace.None)
                {
                    for (int v = vertexOffset; v < vertexOffset + 6; v++)
                    {
                        var idx = index[v];
                        vList.Add(vertex[idx]);
                    }
                }
                vertexOffset += 6;
            }
        }
    }
}
