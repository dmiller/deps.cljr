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
        public CljOpts CljOpts { get; init; }
        public string? CommandAliases { get; init; }
        public List<string> Args { get; init; }


        public CommandBase(CljOpts cljOpts, string? commandAliases, List<string> args)
        {
            CljOpts = cljOpts;
            CommandAliases = commandAliases;
            Args = args;
        }
    }
}
