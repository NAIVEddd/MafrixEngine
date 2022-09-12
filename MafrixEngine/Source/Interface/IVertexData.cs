using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace MafrixEngine.Source.Interface
{
    public interface IVertexData
    {
        VertexInputBindingDescription BindingDescription { get; }
        VertexInputAttributeDescription[] AttributeDescriptions { get; }
    }
}
