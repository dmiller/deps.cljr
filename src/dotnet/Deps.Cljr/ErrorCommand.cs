using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deps.Cljr
{
    internal class ErrorCommand : ShortCircuitCommand
    {
        public int ExitCode { get; init; }
        public string Message { get; init; }

        public ErrorCommand(int exitCode, string message)
        {
            ExitCode = exitCode;
            Message = message;
        }

        public override void Execute()
        {
            Program.Warn(Message);
            Program.EndExecution(ExitCode);
        }
    }
}
