using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deps.Cljr;


public abstract class ShortCircuitCommand : CommandBase
{
    public ShortCircuitCommand() : base(new(), null, new()) { }

    public abstract void Execute();
}
