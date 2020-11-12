
using Newtonsoft.Json;

namespace CloudProxySharp.Types
{
    public class CloudProxyRequest
    {
        [JsonProperty("method")]
        public string Method;

        [JsonProperty("cmd")]
        public string Cmd;

        [JsonProperty("url")]
        public string Url;

        [JsonProperty("userAgent")]
        public string UserAgent;

        [JsonProperty("maxTimeout")]
        public int MaxTimeout;
    }
}