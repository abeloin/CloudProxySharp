using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using YetAnotherCloudProxySharp.Exceptions;
using YetAnotherCloudProxySharp.Types;
using YetAnotherCloudProxySharp.Utilities;
using Newtonsoft.Json;

namespace YetAnotherCloudProxySharp.Solvers
{
    public class YetAnotherCloudProxy
    {
        private static readonly SemaphoreLocker Locker = new SemaphoreLocker();
        private HttpClient _httpClient;
        private readonly Uri _YetAnotherCloudProxyUri;

        public int MaxTimeout = 60000;

        public YetAnotherCloudProxy(string YetAnotherCloudProxyApiUrl)
        {
            var apiUrl = YetAnotherCloudProxyApiUrl;
            if (!apiUrl.EndsWith("/"))
                apiUrl += "/";
            _YetAnotherCloudProxyUri = new Uri(apiUrl + "v1");
        }

        public async Task<YetAnotherCloudProxyResponse> Solve(HttpRequestMessage request)
        {
            YetAnotherCloudProxyResponse result = null;

            await Locker.LockAsync(async () =>
            {
                HttpResponseMessage response;
                try
                {
                    _httpClient = new HttpClient();
                    response = await _httpClient.PostAsync(_YetAnotherCloudProxyUri, GenerateYetAnotherCloudProxyRequest(request));
                }
                catch (HttpRequestException e)
                {
                    throw new YetAnotherCloudProxyException("Error connecting to YetAnotherCloudProxy server: " + e);
                }
                catch (Exception e)
                {
                    throw new YetAnotherCloudProxyException(e.ToString());
                }
                finally
                {
                    _httpClient.Dispose();
                }

                var resContent = await response.Content.ReadAsStringAsync();
                try
                {
                    result = JsonConvert.DeserializeObject<YetAnotherCloudProxyResponse>(resContent);
                }
                catch (Exception)
                {
                    throw new YetAnotherCloudProxyException("Error parsing response, check YetAnotherCloudProxy version. Response: " + resContent);
                }

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new YetAnotherCloudProxyException(result.Message);
            });

            return result;
        }

        private HttpContent GenerateYetAnotherCloudProxyRequest(HttpRequestMessage request)
        {
            YetAnotherCloudProxyRequest req;

            if (request.Method == HttpMethod.Get)
            {
                req = new YetAnotherCloudProxyRequestGet
                {
                    Cmd = "request.get",
                    Url = request.RequestUri.ToString(),
                    MaxTimeout = MaxTimeout
                };
            }
            else if (request.Method == HttpMethod.Post)
            {
                throw new YetAnotherCloudProxyException("Not currently implemented HttpMethod: POST");
            }
            else
            {
                throw new YetAnotherCloudProxyException("Unsupported HttpMethod: " + request.Method.ToString());
            }

            var userAgent = request.Headers.UserAgent.ToString();
            if (!string.IsNullOrWhiteSpace(userAgent))
                req.UserAgent = userAgent;

            var payload = JsonConvert.SerializeObject(req);
            HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
            return content;
        }
 
    }
}