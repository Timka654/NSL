namespace DevExtensions.Blazor.Http.Models
{
    public class HttpDataResponse<TData> : BaseHttpResponse
    {
        public TData Data { get; set; }
    }
}
