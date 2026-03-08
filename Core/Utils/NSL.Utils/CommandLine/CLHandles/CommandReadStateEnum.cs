using System;

namespace NSL.Utils.CommandLine.CLHandles
{
    public enum CommandReadStateEnum
    {
        /// <summary>
        /// Success status for return from a command that executed successfully
        /// </summary>
        Success,

        /// <summary>
        /// 
        /// </summary>
        HelpInvoked,

        /// <summary>
        /// 
        /// </summary>
        InvalidPathHelpInvoked,

        /// <summary>
        /// Invalid path for execute command
        /// </summary>
        InvalidPath,

        /// <summary>
        /// 
        /// </summary>
        InvalidArgumentHelpInvoked,

        /// <summary>
        /// Invalid argument for execute command, or argument required not found
        /// </summary>
        InvalidArgument,

        FinishPath,

        /// <summary>
        /// Status for return from a command that failed to execute
        /// </summary>
        Failed,

        /// <summary>
        /// Status for return from a command that was cancelled
        /// </summary>
        Cancelled
    }

    public delegate void CommandReadResultDelegate(CommandReadResult result);

    public struct CommandReadResult
    {
        public CommandReadStateEnum State { get; set; }
        public string Description { get; set; }
        public Action Callback { get; set; }
        public CommandReadResultDelegate RootExecutorDelegate { get; set; }

        public CommandReadResult(CommandReadStateEnum state, string description = null, Action callback = null, CommandReadResultDelegate rootExecutorDelegate = null)
        {
            State = state;
            Description = description;
            Callback = callback;
            RootExecutorDelegate = rootExecutorDelegate;
        }

        public void FinalizeLogic()
        {
            Callback?.Invoke();
            RootExecutorDelegate?.Invoke(this);
        }

        public static implicit operator CommandReadResult(CommandReadStateEnum state) => new CommandReadResult(state);
    }
}
