namespace NSL.UDP.Interface
{
    public interface IUDPOptions
    {
        int SendFragmentSize { get; set; }

        int ClientLimitSendRate { get; set; }

        int ReliableSendRepeatDelay { get; set; }
    }
}
