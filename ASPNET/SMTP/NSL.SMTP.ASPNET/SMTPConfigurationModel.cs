namespace NSL.SMTP.ASPNET
{
    public class SMTPConfigurationModel : SMTPClientConfigurationModel
    {
        public bool Enabled { get; set; }
        public bool DisableTrap { get; set; }

        public int RequestDelaySeconds { get; set; } = 120;

        public int IterDelayMSeconds { get; set; } = 250;

        public int ServerThrowDelayMSeconds { get; set; } = 30_000;
    }
}
