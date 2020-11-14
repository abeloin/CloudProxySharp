
using Newtonsoft.Json;

namespace CloudProxySharp.Types
{
    public class CloudProxyRequestGet : CloudProxyRequest
    {
        [JsonProperty("headers")]
        public string Headers;

        [JsonProperty("maxTimeout")]
        public int MaxTimeout;
    }
}