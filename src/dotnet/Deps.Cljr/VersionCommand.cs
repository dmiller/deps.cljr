using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deps.Cljr
{
    internal class VersionCommand : ShortCircuitCommand
    {
        public VersionCommand() { }

        public override void Execute()
        {
            Console.WriteLine("How am I supposed to know what version this is?");
            Program.EndExecution(0);
        }

    }
}

