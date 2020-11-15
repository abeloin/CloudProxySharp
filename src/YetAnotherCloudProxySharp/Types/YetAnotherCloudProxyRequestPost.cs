
using Newtonsoft.Json;

namespace YetAnotherCloudProxySharp.Types
{
    public class YetAnotherCloudProxyRequestPost : YetAnotherCloudProxyRequest
    {
        [JsonProperty("headers")]
        public string Headers;

        [JsonProperty("postData")]
        public string PostData;

        [JsonProperty("maxTimeout")]
        public int MaxTimeout;
    }
}