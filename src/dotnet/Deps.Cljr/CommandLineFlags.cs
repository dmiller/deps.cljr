using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deps.Cljr
{
    [Flags]
    internal enum CommandLineFlags
    {
        Describe,
        Force,
        Path,
        Pom,
        Prep,
        Repro,
        Trace,
        Tree,
        Verbose
    }
}
