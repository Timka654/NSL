namespace NSL.HttpClient.Models
{
    public class HttpDataResponse<TData> : BaseHttpResponse
    {
        public TData Data { get; set; }
    }
}
