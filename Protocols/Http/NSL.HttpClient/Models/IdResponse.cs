namespace NSL.HttpClient.Models
{
    public class IdResponse<TId> : BaseResponse
    {
        public TId Id { get; set; }
    }
}
