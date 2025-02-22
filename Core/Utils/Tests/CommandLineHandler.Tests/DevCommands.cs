using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using NSL.Utils.CommandLine.CLHandles.Arguments;
using System.Xml.Linq;

namespace TCPExample.Client
{

    [CLArgument("echo", typeof(CLContainsType))]
    [CLArgument("echo2", typeof(CLContainsType))]
    [CLArgument("echo3", typeof(CLContainsType))]
    [CLArgument("string", typeof(string))]
    [CLArgument("optional", typeof(string), true, Description = $"Is \"optional\" parameter description")]
    [CLHandleSelect("query")]
    public class TestCommandProcessor : CLHandler
    {
        public override string Command => "test";

        public TestCommandProcessor()
        {
            AddArguments(SelectArguments());
        }

        [CLArgumentValue("echo")] public bool echo { get; set; }

        [CLArgumentValue("echo2")] private bool echo_private { get; set; }

        [CLArgumentValue("optional")] private string optional { get; set; }

        [CLArgumentExists("optional")] private bool optional_ex { get; set; }


        public override Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            ProcessingAutoArgs(values);
            Console.WriteLine($"Command '{Command}' Echo {echo} Echo2 {echo_private} Optional {optional} Optional-exists {optional_ex}");
            return Task.FromResult(CommandReadStateEnum.Success);
        }
    }
    [CLHandleSelect("query")]
    public class Test2CommandProcessor : CLHandler
    {
        public override string Command => "test2";

        public Test2CommandProcessor()
        {
            AddArguments(SelectArguments());
        }

        public override Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            Console.WriteLine($"Command '{Command}'");
            return Task.FromResult(CommandReadStateEnum.Success);
        }
    }

    public class RunCommandProcessor : CLHandler
    {
        public override string Command => "run";

        public RunCommandProcessor()
        {
            AddCommands(SelectSubCommands<CLHandleSelectAttribute>("query", true));
            //AddCommands(CLHandler<TestCommandProcessor>.Instance);
        }

        public override Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader)
        {
            return base.ProcessCommand(reader);
        }
    }

    public class CommandsProcessor : CLHandler
    {
        public CommandsProcessor()
        {
            base.AddCommands(CLHandler<RunCommandProcessor>.Instance);
        }
    }

    public class DevCommands
    {
        public static async Task Run()
        {
            var result = await CLHandler<CommandsProcessor>.Instance.ProcessCommand(new CommandLineArgs().CreateReader());
        }
    }
}
