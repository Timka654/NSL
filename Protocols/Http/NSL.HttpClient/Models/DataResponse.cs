namespace DevExtensions.Blazor.Http.Models
{
    public class DataResponse<TData> : BaseResponse
    {
        public TData Data { get; set; }
    }
}
