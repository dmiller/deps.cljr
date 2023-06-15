using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deps.Cljr
{
    internal record CljOpts(
        CommandLineFlags Flags , 
        List<string> ReplAliases, 
        string Deps, 
        string Classpaths,
        int Threads)
    {
        public CljOpts() : this(default, new(),  string.Empty, string.Empty, 1) { }
        public CljOpts WithFlag(CommandLineFlags f) => this with { Flags = this.Flags | f };

    }
}
