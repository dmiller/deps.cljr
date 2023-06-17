using System.Runtime.InteropServices;

namespace Deps.Cljr;



public class Program
{
    static int Main(string[] args)
    {
        foreach (string arg in args)
        {
            Console.WriteLine(arg);
        }

        return 0;
    }


    static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    static string HomeDir => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    static void EndExecution(int exitCode, string message)
    {
        Warn(message);
        EndExecution(exitCode);
    }

    public static void EndExecution(int exitCode) => Environment.Exit(exitCode);

    public static void Warn(string message) => Console.Error.WriteLine(message);

    static readonly List<string> DeprecatedPrefixes = new()
    {
        "-R", "-C", "-O"
    };

    static bool StartsWithDeprecatedPrefix(string arg) => DeprecatedPrefixes.Any(p => arg.StartsWith(p));

    static string? NonBlank(string s) => string.IsNullOrEmpty(s) ? null : s;

    static string ExtractAlias(string s) => s[2..];

    static List<string> GetArgs(int i, string[] args) => args.Skip(i).ToList();

    public static CommandBase ParseArgs(string[] args)
    {
        CljOpts opts = new();

        int i = 0;
        while (i < args.Length)
        {
            var arg = args[i++];

            // PowerShell workaround
            if (IsWindows)
            {
                switch (arg)
                {
                    case "-M:":
                    case "-X:":
                    case "-T:":
                    case "-A:":
                        if (i >= args.Length)
                            return new ErrorCommand(1, $"Invalid arguments, no value following {arg}.");
                        else
                            arg += args[i++];
                        break;
                }
            }

            if (StartsWithDeprecatedPrefix(arg))
                return new ErrorCommand(1, $"{arg[..2]} is no longer supported, use -A with repl, -M for main, -X for exec, -T for tool");

            if (arg == "-Sresolve-tags")
                return new ErrorCommand(1, "Option changed, use: clj -X:deps git-resolve-tags");


            if (arg == "-version" || arg == "--version")
            {
                return new VersionCommand();
            }

            if (arg == "-h" || arg == "--help")
            {
                return new HelpCommand();
            }

            if (arg.StartsWith("-J"))
            {
                Warn("We are CLR!  -J options do not apply! (option ignored)");
                continue;
            }

            if (arg == "-P")
            {
                opts = opts with { Flags = opts.Flags | CommandLineFlags.Prep };
                continue;
            }

            if (arg.StartsWith("-S"))
            {
                switch (ExtractAlias(arg))
                {
                    case "deps":
                        if (i >= args.Length)
                            return new ErrorCommand(1, $"Invalid arguments, no value following {arg}.");
                        var edn = args[i++];
                        opts = opts with { Deps = edn };
                        break;
                    case "pom":
                        Warn("We are CLR! We don't do POM,");
                        opts = opts.WithFlag(CommandLineFlags.Pom); ;
                        break;
                    case "path":
                        opts = opts.WithFlag(CommandLineFlags.Path); ;
                        break;
                    case "tree":
                        opts = opts.WithFlag(CommandLineFlags.Tree);
                        break;
                    case "cp":
                        if (i >= args.Length)
                            return new ErrorCommand(1, $"Invalid arguments, no value following {arg}.");
                        var cp = args[i++];
                        opts = opts with { Classpaths = cp };
                        break;
                    case "repro":
                        opts = opts.WithFlag(CommandLineFlags.Repro);
                        break;
                    case "force":
                        opts = opts.WithFlag(CommandLineFlags.Force);
                        break;
                    case "verbose":
                        opts = opts.WithFlag(CommandLineFlags.Verbose);
                        break;
                    case "describe":
                        opts = opts.WithFlag(CommandLineFlags.Describe);
                        break;
                    case "threads":
                        if (i >= args.Length)
                            return new ErrorCommand(1, $"Invalid arguments, no value following {arg}.");
                        if (Int32.TryParse(args[i++], out var numThreads))
                            opts = opts with { Threads = numThreads };
                        else
                            return new ErrorCommand(1, $"Invalid argument, non-integer following {arg}");
                        break;
                    case "trace":
                        opts = opts.WithFlag(CommandLineFlags.Trace);
                        break;
                    default:
                        return new ErrorCommand(1, $"Unknown command line argument: {arg}");
                }
                continue;
            }

            if (arg == "-A")
            {
                return new ErrorCommand(1, "-A requires an alias");
            }

            if (arg.StartsWith("-A"))
            {
                opts.ReplAliases.Add(ExtractAlias(arg));
                continue;
            }

            if (arg.StartsWith("-M"))
            {
                return new MainCommand(opts, NonBlank(ExtractAlias(arg)), GetArgs(i, args));
            }

            if (arg.StartsWith("-X"))
            {
                return new ExecCommand(opts, NonBlank(ExtractAlias(arg)), GetArgs(i, args));
            }

            if (arg.StartsWith("-T:"))
            {
                return new ToolCommand(opts, null, NonBlank(ExtractAlias(arg)), GetArgs(i, args));
            }

            if (arg.StartsWith("-T"))
            {
                return new ToolCommand(opts, NonBlank(ExtractAlias(arg)), null, GetArgs(i, args));
            }

            if (arg == "--")
            {
                return new ReplCommand(opts, null, GetArgs(i, args));
            }
        }

        return new ReplCommand(opts, null, new());
    }


    public static void PrintHelp()
    {
        Console.WriteLine($"Version: ???");
        Console.WriteLine();
        Console.WriteLine(@"You use the Clojure tools('clj' or 'clojure') to run Clojure programs
on the JVM, e.g.to start a REPL or invoke a specific function with data.
The Clojure tools will configure the JVM process by defining a classpath
(of desired libraries), an execution environment(JVM options) and
specifying a main class and args.


Using a deps.edn file(or files), you tell Clojure where your source code
resides and what libraries you need.Clojure will then calculate the full
set of required libraries and a classpath, caching expensive parts of this
process for better performance.

The internal steps of the Clojure tools, as well as the Clojure functions
you intend to run, are parameterized by data structures, often maps.Shell
command lines are not optimized for passing nested data, so instead you
will put the data structures in your deps.edn file and refer to them on the
command line via 'aliases' - keywords that name data structures.

'clj' and 'clojure' differ in that 'clj' has extra support for use as a REPL
in a terminal, and should be preferred unless you don't want that support,
then use 'clojure'.

Usage:
  Start a REPL clj     [clj-opt*] [-Aaliases]
  Exec fn(s) clojure [clj-opt*] -X[aliases][a / fn *][kpath v]*
  Run main      clojure[clj - opt *] -M[aliases][init - opt *][main - opt][arg *]
  Run tool clojure [clj-opt*] -T[name | aliases] a/fn[kpath v] kv-map?
  Prepare       clojure[clj - opt *] -P[other exec opts]

exec-opts:
 -Aaliases Use concatenated aliases to modify classpath
 -X[aliases] Use concatenated aliases to modify classpath or supply exec fn/args
 -M[aliases] Use concatenated aliases to modify classpath or supply main opts
 -P Prepare deps - download libs, cache classpath, but don't exec

clj-opts:
 -Jopt Pass opt through in java_opts, ex: -J-Xmx512m
 -Sdeps EDN     Deps data to use as the last deps file to be merged
 -Spath Compute classpath and echo to stdout only
 -Stree Print dependency tree
 -Scp CP        Do NOT compute or cache classpath, use this one instead
 -Srepro Ignore the ~/.clojure/deps.edn config file
 -Sforce Force recomputation of the classpath(don't use the cache)
 -Sverbose Print important path info to console
 -Sdescribe Print environment and command parsing info as data
 -Sthreads Set specific number of download threads
 -Strace Write a trace.edn file that traces deps expansion
 --             Stop parsing dep options and pass remaining arguments to clojure.main
 --version Print the version to stdout and exit
 -version Print the version to stdout and exit

The following non-standard options are available only in deps.clj:

 -Sdeps-file Use this file instead of deps.edn
 -Scommand A custom command that will be invoked. Substitutions: { { classpath} }, {{main-opts
}}.

init - opt:
 -i, --init path Load a file or resource
 -e, --eval string   Eval exprs in string; print non-nil values
 --report target     Report uncaught exception to ""file"" (default), ""stderr"", or ""none""

main-opt:
 -m, --main ns - name  Call the -main function from namespace w/args
 -r, --repl Run a repl
 path Run a script from a file or resource
 -                   Run a script from standard input
 -h, -?, --help Print this help message and exit

Programs provided by :deps alias:
 -X:deps mvn-pom Generate (or update) pom.xml with deps and paths
 -X:deps list              List full transitive deps set and licenses
 -X:deps tree              Print deps tree
 -X:deps find-versions Find available versions of a library
 -X:deps prep              Prepare all unprepped libs in the dep tree
 -X:deps mvn-install Install a maven jar to the local repository cache
 -X:deps git-resolve-tags Resolve git coord tags to shas and update deps.edn

For more info, see:
 https://clojure.org/guides/deps_and_cli
 https://clojure.org/reference/repl_and_main");
    }
}



