namespace NSL.SMTP.ASPNET
{
    public class SMTPClientConfigurationModel
    { 
        public string Host { get; set; }

        public bool EnableSsl { get; set; } = true;

        public int Port { get; set; } = 25;

        public string UserName { get; set; }

        public string Password { get; set; }

        public string DisplayName { get; set; }
    }
}
