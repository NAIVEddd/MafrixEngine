using MafrixEngine.GraphicsWrapper;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MafrixEngine.Source.Interface
{
    public interface IRender : ISubsystem
    {
        public VkContext vkContext { get; }
        public StagingBuffer Staging { get; }
        public SingleTimeCommand SingleCommand { get; }
        public IWindow Window { get; set; }
        public void SetCamera(Cameras.Camera camera);
        public void UpdateModels();
        public void AddStaticDraw(Action<CommandBuffer> action);
        public void Draw(double delta);
    }
}
