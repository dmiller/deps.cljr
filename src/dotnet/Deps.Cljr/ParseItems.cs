using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deps.Cljr;

public enum EMode { Version, Help, Repl, Tool, Exec, Main }

public class ParseItems
{
    public HashSet<string> Flags = new();
    public string? Deps { get; set; } =null;
    public string? ForceClasspath { get; set; } = null;
    public int Threads { get; set; } = 0;
    public string? ToolName { get; set; } = null;
    public List<string> JvmOpts { get; } = new();
    public List<string> CommandArgs { get; set; } = new();
    public EMode Mode { get; set; } = EMode.Repl;
    public bool IsError { get; set; } = false;
    public string? ErrorMessage { get; set; } = null;
    public Dictionary<EMode,string> CommandAliases { get; } = new();

    public ParseItems SetError(string message)
    {
        IsError = true;
        ErrorMessage= message;
        return this;
    }

    public void AddReplAliases(string aliases)
    {
        var currValue = CommandAliases.TryGetValue(EMode.Repl, out var currVal) ? currVal : "";
        CommandAliases[EMode.Repl] = currValue + aliases;
    }

    public void SetCommandAliases(EMode mode, string? alias)
    {
        if ( alias is  not null)
            CommandAliases[mode] = alias;
    }

    public void AddFlag(string flag)
    {
        Flags.Add(flag);
    }
}