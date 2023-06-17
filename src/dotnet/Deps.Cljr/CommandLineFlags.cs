using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deps.Cljr;

[Flags]
public enum CommandLineFlags
{
    Describe=1,
    Force=2,
    Path=4,
    Pom=8,
    Prep=16,
    Repro=32,
    Trace=64,
    Tree=128,
    Verbose=256
}
