using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MafrixEngine.Source.Interface
{
    public interface IRender
    {
        public void SetCamera();
        public void UpdateModels();
        public void Draw();
    }

    public interface IRenderable
    {
        public void BindCommand(Vk vk, CommandBuffer commandBuffer, Action<int> action);
    }
}
