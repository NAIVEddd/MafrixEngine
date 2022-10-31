using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MafrixEngine.Source.Interface;
using Silk.NET.Windowing;

namespace MafrixEngine.Source
{
    public class Engine : IDisposable
    {
        public Engine()
        {
        }

        public IWindow window;
        public CameraSystem cameraSys;
        public IRender renderSys;
        public IStaticScene staticScene;

        // init window|render|scene|charactor
        public void Init(string configFile)
        {
            renderSys.Window = window;
            //staticScene.render = renderSys;

            renderSys.StartUp();
            //staticScene.StartUp();

        }

        public void Run()
        {
            window.Update += TickOneFrame;
            window.Run();
            Dispose();
        }
        
        public void TickOneFrame(double delta)
        {
            TickLogic();
            TickRender();
        }

        private void TickLogic()
        {
            // fetch input event
            // do some process on models or levelss
        }
        private void TickRender()
        {
            // draw the scene to screen/window
            renderSys.Draw(0);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class EngineSubsystems
    {
        public EngineSubsystems()
        {
        }
    }

    public class CameraSystem : ISubsystem
    {
        public void ShutDown()
        {
            throw new NotImplementedException();
        }

        public void StartUp()
        {
            throw new NotImplementedException();
        }
    }
}
