using BrainfuckInterpreter;

#if !DEBUG
try
{
#endif
Interpreter interpreter = new();
// interpreter.OutputType = OutputType.Number;
if (args.Length == 1)
{
    string fileName = args[0];
    if (fileName == "execute_file")
    {
        Console.Write("Enter file name: ");
        fileName = Console.ReadLine()!;
    }
    interpreter.Run(File.ReadAllText(fileName));
}
else
{
    bool running = true;
    while (running)
    {
        string? code = Console.ReadLine();
        if (code != null && code != ".exit")
        {
            interpreter.Run(code);
        }
        else
            running = false;
    }
}
#if !DEBUG
}
catch (Exception ex)
{
    Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
}
#endif

Console.WriteLine();
Console.WriteLine("Press ENTER to exit");
Console.ReadLine();