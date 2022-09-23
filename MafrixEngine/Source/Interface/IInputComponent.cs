using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MafrixEngine.Source.Interface
{
    public interface IInputComponent : IComponent
    {
        void EnabledInput(bool enabled);
        void BindAxis(string name, Action<float> action);
        void BindAction(string name, Action action, bool isCtrl, bool isShift, bool isAlt);
    }
}
