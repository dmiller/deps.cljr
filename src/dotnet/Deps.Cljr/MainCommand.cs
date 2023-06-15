using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deps.Cljr
{
    internal class MainCommand : CommandBase
    {
        public MainCommand(CljOpts cljOpts, string? commandAliases, List<string> commandArgs) : base(cljOpts, commandAliases, commandArgs) { }
    }
}
