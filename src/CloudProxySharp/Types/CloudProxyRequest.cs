
using Newtonsoft.Json;

namespace CloudProxySharp.Types
{
    public class CloudProxyRequest
    {
        [JsonProperty("cmd")]
        public string Cmd;

        [JsonProperty("url")]
        public string Url;

        [JsonProperty("userAgent")]
        public string UserAgent;
    }
}