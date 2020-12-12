
using Newtonsoft.Json;

namespace YetAnotherCloudProxySharp.Types
{
    public class YetAnotherCloudProxyRequestPost : YetAnotherCloudProxyRequest
    {
        [JsonProperty("postData")]
        public string PostData;

        [JsonProperty("maxTimeout")]
        public int MaxTimeout;
    }
}