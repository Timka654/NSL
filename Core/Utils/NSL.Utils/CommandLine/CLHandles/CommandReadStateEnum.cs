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
        /// Invalid path for execute command
        /// </summary>
        InvalidPath,

        /// <summary>
        /// Invalid argument for execute command, or argument required not found
        /// </summary>
        InvalidArgument,

        FinishPath,

        /// <summary>
        /// Status for return from a command that failed to execute
        /// </summary>
        Failed
    }
}
