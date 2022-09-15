using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MafrixEngine.Source.Interface
{
    public interface IComponent
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsType(Type type);
    }

    public interface IComponents
    {
        public void Add(IComponent component);
        public void Remove(IComponent component);
        public bool Contains(IComponent component);
        public IComponent GetComponent(string name);
        public IComponent GetComponent(Type type);
    }
}
