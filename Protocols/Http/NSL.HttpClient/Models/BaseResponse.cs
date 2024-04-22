using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;

namespace DevExtensions.Blazor.Http.Models
{
    public class BaseResponse
    {
        [JsonIgnore]
        public Dictionary<string, List<string>>? Errors { get; set; } = new Dictionary<string, List<string>>();

        [JsonIgnore]
        public HttpStatusCode StatusCode { get; set; }

        [JsonIgnore]
        public bool IsSuccess => StatusCode == HttpStatusCode.OK;

        [JsonIgnore]
        public bool IsBadRequest => StatusCode == HttpStatusCode.BadRequest;
    }
}
