using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Deps.Cljr
{
    internal abstract class CommandBase
    {
        public CljOpts CljOpts { get; set; }

        public CommandBase(CljOpts cljOpts)
        {
            CljOpts = cljOpts;
        }
    }
}
