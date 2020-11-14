
using Newtonsoft.Json;

namespace CloudProxySharp.Types
{
    public class CloudProxyRequestPost : CloudProxyRequest
    {
        [JsonProperty("headers")]
        public string Headers;

        [JsonProperty("postData")]
        public string PostData;

        [JsonProperty("maxTimeout")]
        public int MaxTimeout;
    }
}