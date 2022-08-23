using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MafrixEngine.GraphicsWrapper
{
    public interface IVkExtension : IDisposable
    {
        public string[] ExtensionNames { get; }
    }

    public class BasicExtension : IVkExtension
    {
        private string[] names;
        public string[] ExtensionNames => names;
        public BasicExtension(string[] names)
        {
            this.names = names;
        }

        public void Dispose()
        {
        }
    }
}
