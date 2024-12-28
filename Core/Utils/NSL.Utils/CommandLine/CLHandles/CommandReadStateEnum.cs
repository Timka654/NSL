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
}
