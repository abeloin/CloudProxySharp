using System.Net.Http;

namespace YetAnotherCloudProxySharp.Exceptions
{
    /// <summary>
    /// The exception that is thrown if YetAnotherCloudProxy fails
    /// </summary>
    public class YetAnotherCloudProxyException : HttpRequestException
    {
        public YetAnotherCloudProxyException(string message) : base(message)
        {
        }
    }
}
