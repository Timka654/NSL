namespace NSL.Extensions.DBEngine
{
    public class ConnectionOptions
    {
        public DbOptions CSOptions { get; set; }

        public string ConnectionString { get; set; }

        public bool RecoveryWhenFailedTry { get; set; }

        public short RecoveryTryCount { get; set; } = 1;

        public short RecoveryTryDelay { get; set; }

        public bool DropApplicationWhenFailed { get; set; } = true;
    }
}
