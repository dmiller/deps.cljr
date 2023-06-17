using Deps.Cljr;
using System.Runtime.CompilerServices;
using Xunit;

namespace Deps.Cljr.Tests;

public class ParsingTests
{
    [Theory]
    [InlineData("-h")]
    [InlineData("--help")]
    [InlineData("-A:X:Y --help")]
    [InlineData("-Srepro --help")]
    public void HelpTests(string cli)
    {
        string[] args = cli.Split(new char[] { ' ' });
        Assert.IsType<HelpCommand>(Program.ParseArgs(args));
    }

    [Theory]
    [InlineData("-version")]
    [InlineData("--version")]
    [InlineData("-A:X:Y -version")]
    [InlineData("-Srepro --version")]
    public void VersionTests(string cli)
    {
        string[] args = cli.Split(new char[] { ' ' });
        Assert.IsType<VersionCommand>(Program.ParseArgs(args));
    }

    [Theory]
    [InlineData("-Sdescribe -X:A:B 12 13", CommandLineFlags.Describe)]
    [InlineData("-Sforce -X:A:B 12 13", CommandLineFlags.Force)]
    [InlineData("-Spath -X:A:B 12 13", CommandLineFlags.Path)]
    [InlineData("-Spom -X:A:B 12 13", CommandLineFlags.Pom)]
    [InlineData("-P -X:A:B 12 13", CommandLineFlags.Prep)]
    [InlineData("-Srepro -X:A:B 12 13", CommandLineFlags.Repro)]
    [InlineData("-Strace -X:A:B 12 13", CommandLineFlags.Trace)]
    [InlineData("-Stree -X:A:B 12 13", CommandLineFlags.Tree)]
    [InlineData("-Sverbose -X:A:B 12 13", CommandLineFlags.Verbose)]
    public void FlagTests(string cli, CommandLineFlags trueFlag)
    {
        string[] args = cli.Split(new char[] { ' ' });
        var cmd = Program.ParseArgs(args);
        Assert.IsType<ExecCommand>(cmd);
        foreach (var flag in Enum.GetValues<CommandLineFlags>())
        {
            var flagSet = (cmd.CljOpts.Flags & flag);
            if (flag == trueFlag)
                Assert.True(flagSet == flag);
            else
                Assert.False(flagSet == flag);
        }
    }

    [Theory]
    [InlineData("-Spom -X:A:B 12 13", "We are CLR")]
    [InlineData("-Jsomething -X:A:B 12 13", "We are CLR")]
    public void OptionsThatWarnTests(string cli, string msgPrefix) 
    {
        try
        {
            using StringWriter sw = new StringWriter();
            Console.SetError(sw);
            string[] args = cli.Split(new char[] { ' ' });
            var cmd = Program.ParseArgs(args);
            Assert.IsType<ExecCommand>(cmd);
            Assert.StartsWith(msgPrefix, sw.ToString());

        }
        finally
        {
            Console.SetError(new StreamWriter(Console.OpenStandardError())); 
        }
    }

    [Theory]
    [InlineData("-Srepro -Sresolve-tags -X:A: B 12 13")]
    [InlineData("-Srepro -Othing -X:A: B 12 13")]
    [InlineData("-Srepro -Cthing -X:A: B 12 13")]
    [InlineData("-Srepro -Rthing -X:A: B 12 13")]
    public void DeprecatedOptionsTests(string cli)
    {
        string[] args = cli.Split(new char[] { ' ' });
        var cmd = Program.ParseArgs(args);
        Assert.IsType<ErrorCommand>(cmd);
    }

    [Theory]
    [InlineData("-Srepro -Sdeps", "Invalid arguments")]
    [InlineData("-Srepro -Scp", "Invalid arguments")]
    [InlineData("-Srepro -Sthreads", "Invalid arguments")]
    [InlineData("-Srepro -Sthreads a", "Invalid argument")]
    [InlineData("-Srepro -Swhat! things","Unknown command")]
    public void MissingOrBadArgumentsToOptions(string cli, string msgPrefix)
    {
        string[] args = cli.Split(new char[] { ' ' });
        var cmd = Program.ParseArgs(args);
        Assert.IsType<ErrorCommand>(cmd);
        var ec = (ErrorCommand) cmd;
        Assert.StartsWith(msgPrefix, ec.Message);
    }

    [Fact]
    public void StringArgumentsForOptionsTests()
    {
        string cli = "-Srepro -Sdeps ABC -Scp DEF -Sthreads 12 -X:A: B 12 13";
        string[] args = cli.Split(new char[] { ' ' });
        var cmd = Program.ParseArgs(args);
        Assert.IsType<ExecCommand>(cmd);
        Assert.True(cmd.CljOpts.Deps.Equals("ABC"));
        Assert.True(cmd.CljOpts.Classpaths.Equals("DEF"));
        Assert.True(cmd.CljOpts.Threads == 12);
    }

    [Fact]
    public void AOptionRequiresAnAliasTest()
    {
        string cli = "-Srepro -A  -X:A: B 12 13";
        string[] args = cli.Split(new char[] { ' ' });
        var cmd = Program.ParseArgs(args);
        Assert.IsType<ErrorCommand>(cmd);
        var ec = (ErrorCommand)cmd;
        Assert.StartsWith("-A requires an alias", ec.Message);
    }

    [Theory]
    [InlineData("-Srepro -X:A: B 12 13","B","12","13")]
    [InlineData("-Srepro -- B 12 13", "B", "12", "13")]
    public void ArgsGetPassedTests(string cli, params string[] expectedAargs)
    {
        string[] args = cli.Split(new char[] { ' ' });
        var cmd = Program.ParseArgs(args);
        Assert.True(cmd.Args.Zip(expectedAargs.ToList()).All(x => x.First.Equals(x.Second)));
    }

    [Theory]
    [InlineData("-Srepro -X:A:B 12 13", typeof(ExecCommand), ":A:B", "12", "13")]
    [InlineData("-Srepro -M:A:B 12 13", typeof(MainCommand), ":A:B", "12", "13")]
    [InlineData("-Srepro -T:A:B 12 13", typeof(ToolCommand), ":A:B", "12", "13")]
    [InlineData("-Srepro -- 12 13", typeof(ReplCommand), null, "12", "13")]
    public void CorrectCommandTypeCreated(string cli, Type cmdType, string cmdAliases, params string[] expectedArgs)
    {
        string[] args = cli.Split(new char[] { ' ' });
        var cmd = Program.ParseArgs(args);
        Assert.IsType(cmdType, cmd);
        Assert.True(cmd.Args.Zip(expectedArgs.ToList()).All(x => x.First.Equals(x.Second)));
        Assert.Equal(cmdAliases, cmd.CommandAliases);
    }

    [Fact]
    public void ToolWithAliasTest()
    {
        string cli = "-Srepro -T:A:B 12 13";
        string[] args = cli.Split(new char[] { ' ' });
        var cmd = Program.ParseArgs(args);
        Assert.IsType<ToolCommand>(cmd);
        var tcmd = (ToolCommand)cmd;
        Assert.Null(tcmd.ToolName);
        Assert.Equal(":A:B", tcmd.CommandAliases);
    }


    [Fact]
    public void ToolWithToolNameTest()
    {
        string cli = "-Srepro -Tname 12 13";
        string[] args = cli.Split(new char[] { ' ' });
        var cmd = Program.ParseArgs(args);
        Assert.IsType<ToolCommand>(cmd);
        var tcmd = (ToolCommand)cmd;
        Assert.Equal("name",tcmd.ToolName);
        Assert.Null(tcmd.CommandAliases);
    }


    [Theory]
    [InlineData("-Srepro -X 12 13", typeof(ExecCommand), "12", "13")]
    [InlineData("-Srepro -M 12 13", typeof(MainCommand), "12", "13")]
    [InlineData("-Srepro -T 12 13", typeof(ToolCommand), "12", "13")]
    public void CommandWithNoAliasTests(string cli, Type cmdType, params string[] expectedArgs)
    {
        string[] args = cli.Split(new char[] { ' ' });
        var cmd = Program.ParseArgs(args);
        Assert.IsType(cmdType, cmd);
        Assert.Null(cmd.CommandAliases);
    }

    [Theory]
    [InlineData("-Srepro -A:A:B -A:C:D -X:A:B 12 13", typeof(ExecCommand), ":A:B", ":C:D")]
    [InlineData("-Srepro -A:A:B -A:C:D -M:A:B 12 13", typeof(MainCommand), ":A:B", ":C:D")]
    [InlineData("-Srepro -A:A:B -A:C:D -T:A:B 12 13", typeof(ToolCommand), ":A:B", ":C:D")]
    [InlineData("-Srepro -A:A:B -A:C:D -- 12 13", typeof(ReplCommand), ":A:B", ":C:D")]
    public void AArgPassesReplAliases(string cli, Type cmdType, params string[] replAliases)
    {
        string[] args = cli.Split(new char[] { ' ' });
        var cmd = Program.ParseArgs(args);
        Assert.IsType(cmdType, cmd);
        Assert.True(cmd.CljOpts.ReplAliases.Zip(replAliases.ToList()).All(x => x.First.Equals(x.Second)));
    }

    [Theory]
    [InlineData("-Srepro -A:A:B -A:C:D -X:")]
    [InlineData("-Srepro -A:A:B -A:C:D  -M:")]
    [InlineData("-Srepro -A:A:B -A:C:D  -T:")]
    [InlineData("-Srepro -A:A:B -A:C:D  -A:")]
    public void PowerShellWorkaroundFailTests(string cli)
    {
        string[] args = cli.Split(new char[] { ' ' });
        var cmd = Program.ParseArgs(args);
        Assert.IsType<ErrorCommand>(cmd);
    }

    [Theory]
    [InlineData("-Srepro -X: A:B 12 13", typeof(ExecCommand), ":A:B", "12", "13")]
    [InlineData("-Srepro -M: A:B 12 13", typeof(MainCommand), ":A:B", "12", "13")]
    [InlineData("-Srepro -T: A:B 12 13", typeof(ToolCommand), ":A:B", "12", "13")]
    [InlineData("-Srepro -A: A:B -- 12 13", typeof(ReplCommand), null, "12", "13")]
    public void PowerShellWorkaroundSuccessTests(string cli, Type cmdType, string cmdAliases, params string[] expectedArgs)
    {
        string[] args = cli.Split(new char[] { ' ' });
        var cmd = Program.ParseArgs(args);
        Assert.IsType(cmdType, cmd);
        Assert.True(cmd.Args.Zip(expectedArgs.ToList()).All(x => x.First.Equals(x.Second)));
        Assert.Equal(cmdAliases, cmd.CommandAliases);
    }

    [Theory]
    [InlineData("-Srepro -A: A:B -A: C:D -X:A:B 12 13", typeof(ExecCommand), ":A:B", ":C:D")]
    [InlineData("-Srepro -A: A:B -A: C:D -M:A:B 12 13", typeof(MainCommand), ":A:B", ":C:D")]
    [InlineData("-Srepro -A: A:B -A: C:D -T:A:B 12 13", typeof(ToolCommand), ":A:B", ":C:D")]
    [InlineData("-Srepro -A: A:B -A: C:D -- 12 13", typeof(ReplCommand), ":A:B", ":C:D")]
    public void PowerShellWorkaroundForASuccessTests(string cli, Type cmdType, params string[] replAliases)
    {
        string[] args = cli.Split(new char[] { ' ' });
        var cmd = Program.ParseArgs(args);
        Assert.IsType(cmdType, cmd);
        Assert.True(cmd.CljOpts.ReplAliases.Zip(replAliases.ToList()).All(x => x.First.Equals(x.Second)));
    }
}