using System.CommandLine;
using System.CommandLine.Invocation;

public class ExampleCommand : Command
{
    public ExampleCommand() : base(name: "example", "Example description")
    {
        AddOption(new Option<string>(new string[] { "--title", "-t" }, "Title of the Example description"));
        AddOption(new Option<string>(new string[] { "--engines", "-e" }, "设置模板类型"));
    }


    public new class Handler : ICommandHandler
    {
        public string Title { get; set; }
        public string Engines { get; set; }
        public Task<int> InvokeAsync(InvocationContext context)
        {
            Console.WriteLine(Title);
            return Task.FromResult(0);
        }
    }
}
