using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deps.Cljr;


public class HelpCommand : ShortCircuitCommand
{
    public HelpCommand() { }

    public override void Execute()
    {
        Program.PrintHelp();
        Program.EndExecution(0);
    }
}
