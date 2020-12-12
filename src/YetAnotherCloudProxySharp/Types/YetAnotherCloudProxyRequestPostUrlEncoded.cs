
using Newtonsoft.Json;

namespace YetAnotherCloudProxySharp.Types
{
    public class YetAnotherCloudProxyRequestPostUrlEncoded : YetAnotherCloudProxyRequestPost
    {
        [JsonProperty("headers")]
        public HeadersPost Headers;
    }

    public class HeadersPost
    {
        [JsonProperty(PropertyName = "Content-Type")]
        public string ContentType;

        [JsonProperty(PropertyName = "Content-Length")]
        public string ContentLength;
    }
}