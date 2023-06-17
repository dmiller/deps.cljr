using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deps.Cljr;

public class ToolCommand : CommandBase
{
    public string? ToolName { get; init; }

    public ToolCommand(CljOpts cljOpts, string? toolName, string? commandAliases, List<string> commandArgs) : base(cljOpts, commandAliases, commandArgs) 
    {
        ToolName = toolName;
    }
}
