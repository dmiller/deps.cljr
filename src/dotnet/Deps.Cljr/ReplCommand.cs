using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deps.Cljr
{
    public class ReplCommand : CommandBase
    {
        public ReplCommand(CljOpts cljOpts, string? commandAliases, List<string> commandArgs) : base(cljOpts, commandAliases, commandArgs) { }
    }
}
