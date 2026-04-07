namespace NSL.UDP.Interface
{
    public interface IBindingUDPOptions : ISTUNOptions
    {
        int ReceiveChannelCount { get; set; }
    }
}
