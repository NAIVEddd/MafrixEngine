using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MafrixEngine.Source.Interface
{
    public interface IStaticScene : ISubsystem
    {
        public IRender render { get; set; }

    }
}
