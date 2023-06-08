namespace Deps.Cljr;



internal class Program
{
    static int Main(string[] args)
    {
        foreach (string arg in args)
        {
            Console.WriteLine(arg);
        }

        return 0;
    }


    enum EMode { Repl, Main, Exec, Tool }

    static void ParseArgs(string[] args)
    {

    }

}