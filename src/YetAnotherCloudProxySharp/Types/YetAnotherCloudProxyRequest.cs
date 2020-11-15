
using Newtonsoft.Json;

namespace YetAnotherCloudProxySharp.Types
{
    public class YetAnotherCloudProxyRequest
    {
        [JsonProperty("cmd")]
        public string Cmd;

        [JsonProperty("url")]
        public string Url;

        [JsonProperty("userAgent")]
        public string UserAgent;
    }
}