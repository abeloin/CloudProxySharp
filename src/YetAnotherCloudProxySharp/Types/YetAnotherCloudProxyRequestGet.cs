
using Newtonsoft.Json;

namespace YetAnotherCloudProxySharp.Types
{
    public class YetAnotherCloudProxyRequestGet : YetAnotherCloudProxyRequest
    {
        [JsonProperty("headers")]
        public string Headers;

        [JsonProperty("maxTimeout")]
        public int MaxTimeout;
    }
}