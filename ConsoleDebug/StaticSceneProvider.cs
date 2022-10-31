using MafrixEngine.GraphicsWrapper;
using MafrixEngine.ModelLoaders;
using MafrixEngine.Source.DataStruct;
using MafrixEngine.Source.Interface;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleDebug
{
    public class StaticSceneProvider : IStaticScene, ISubsystem
    {

        private IRender _render;
        public IRender render
        {
            get => _render;
            set
            {
                _render = value;
                value.AddStaticDraw(Draw);
            }
        }
        private Vk vk { get => render.vkContext.vk; }
        private Device device { get => render.vkContext.device; }
        private VkContext vkContext { get => render.vkContext; }
        private Mesh models;
        public string SceneConfigPath { get; set; }
        private ValueTuple<string, string> modelsPath;
        public StaticSceneProvider(ValueTuple<string, string, int> models)
        {
            modelsPath = (models.Item1, models.Item2);
        }
        public void Draw(CommandBuffer commandBuffer)
        {
            models.BindCommand(vk, commandBuffer, BindDescriptorSets);
            void BindDescriptorSets(int nodeIndex)
            {
            }
            //vk.CmdBindVertexBuffers(commandBuffer, 0, 0, models.vertexBuffer.buffer, 0);
        }

        public void ShutDown()
        {
            throw new NotImplementedException();
        }

        public unsafe void StartUp()
        {
            var stCommand = render.SingleCommand;
            var staging = render.Staging;
            var meshes = new Mesh(vkContext);

            /// gltf load model
            var (path, name) = modelsPath;
            var loader = new Gltf2Loader(path, name);
            var gltf2 = loader.Parse(vkContext, stCommand, staging);
            meshes.vertices = gltf2.vertices;
            meshes.indices = gltf2.indices;
            meshes.model = gltf2;
            ulong bufferSize = (ulong)(sizeof(Vertex) * meshes.vertices.Length);
            meshes.vertexBuffer.Init(bufferSize,
                BufferUsageFlags.TransferDstBit |
                BufferUsageFlags.VertexBufferBit);
            meshes.vertexBuffer.UpdateData(meshes.vertices, stCommand, staging);
            bufferSize = (ulong)(sizeof(uint) * meshes.indices.Length);
            meshes.indicesBuffer.Init(bufferSize,
                BufferUsageFlags.TransferDstBit |
                BufferUsageFlags.IndexBufferBit);
            meshes.indicesBuffer.UpdateData(meshes.indices, stCommand, staging);

            /// voxel load model
            //var voxel = new VoxelLoader();
            //voxel.Build(vkContext, stCommand, staging);
            //meshes[i].vertices = voxel.vertices;
            //meshes[i].indices = voxel.indices;
            //meshes[i].model = voxel;
            models = meshes;
        }
    }
}
