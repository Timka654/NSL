﻿using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;

namespace NSL.HttpClient.Models
{
    public class BaseResponse
    {
        [JsonIgnore]
        public Dictionary<string, List<string>> Errors { get; set; } = new Dictionary<string, List<string>>();

        [JsonIgnore]
        public HttpStatusCode StatusCode { get; set; }

        [JsonIgnore]
        public bool IsSuccess => ((int)StatusCode >= 200) && ((int)StatusCode <= 299);

        [JsonIgnore]
        public bool IsBadRequest => StatusCode == HttpStatusCode.BadRequest;
    }
}
