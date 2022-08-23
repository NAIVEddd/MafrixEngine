using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MafrixEngine.GraphicsWrapper
{
    public interface IVkLayer : IDisposable
    {
        public string[] LayerNames { get; }
    }

    public class BasicLayer : IVkLayer
    {
        private string[] names;
        public string[] LayerNames => names;
        public BasicLayer(string[] names)
        {
            this.names = names;
        }

        public void Dispose()
        {
        }
    }
}
