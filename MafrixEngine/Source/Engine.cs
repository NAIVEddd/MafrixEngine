using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MafrixEngine.Source
{
    public class Engine
    {
        public Engine()
        {
        }

        // init window|render|scene|charactor
        public void Init(string configFile)
        {

        }

        public void TickOneFrame()
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
        }
    }

    public class EngineSubsystems
    {
        public EngineSubsystems()
        {
        }
    }
}
