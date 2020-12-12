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
                    throw new YetAnotherCloudProxyException("Error connecting to CloudProxy server: " + e);
                }
                catch (Exception e)
                {
                    throw new YetAnotherCloudProxyException("Exception: " + e.ToString());
                }
                finally
                {
                    _httpClient.Dispose();
                }

                // Don't try parsing if CloudProxy hasn't resturned 200 or 500
                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.InternalServerError)
                {
                    throw new YetAnotherCloudProxyException("HTTP StatusCode not 200 or 500. Status is :" + response.StatusCode);
                }

                var resContent = await response.Content.ReadAsStringAsync();
                try
                {
                    result = JsonConvert.DeserializeObject<YetAnotherCloudProxyResponse>(resContent);
                }
                catch (Exception)
                {
                    throw new YetAnotherCloudProxyException("Error parsing response, check CloudProxy. Response: " + resContent);
                }

                    try
                {
                    Enum.TryParse(result.Status, true, out CloudProxyStatusCode returnStatusCode);

                    if (returnStatusCode.Equals(CloudProxyStatusCode.ok))
                    {
                        return result;
                    }
                    else if (returnStatusCode.Equals(CloudProxyStatusCode.warning))
                    {
                        throw new YetAnotherCloudProxyException(
                            "CloudProxy was able to process the request, but a captcha was detected. Message: "
                            + result.Message);
                    }
                    else if (returnStatusCode.Equals(CloudProxyStatusCode.error))
                    {
                        throw new YetAnotherCloudProxyException(
                            "CloudProxy was unable to process the request, please check CloudProxy's logs. Message: "
                            + result.Message);
                    }
                    else
                    {
                        throw new YetAnotherCloudProxyException("Unable to map CloudProxy returned status code, received code: "
                            + result.Status + ". Message: " + result.Message);
                    }
                }
                catch (ArgumentException)
                {
                    throw new YetAnotherCloudProxyException("Error parsing status code, check CloudProxy log. Status: "
                            + result.Status + ". Message: " + result.Message);
                }
            });

            return result;
        }

        private HttpContent GenerateYetAnotherCloudProxyRequest(HttpRequestMessage request)
        {
            YetAnotherCloudProxyRequest req;

            var url = request.RequestUri.ToString();
            var userAgent = request.Headers.UserAgent.ToString();

            if (!string.IsNullOrWhiteSpace(userAgent))
                userAgent = null;

            if (request.Method == HttpMethod.Get)
            {
                req = new YetAnotherCloudProxyRequestGet
                {
                    Cmd = "request.get",
                    Url = url,
                    MaxTimeout = MaxTimeout,
                    UserAgent = userAgent
                };
            }
            else if (request.Method == HttpMethod.Post)
            {
                var contentTypeType = request.Content.GetType();

                if (contentTypeType == typeof(FormUrlEncodedContent))
                {
                    var contentTypeValue = request.Content.Headers.ContentType.ToString();
                    var postData = request.Content.ReadAsStringAsync().Result;

                    req = new YetAnotherCloudProxyRequestPostUrlEncoded
                    {
                        Cmd = "request.post",
                        Url = url,
                        PostData = postData,
                        Headers = new HeadersPost
                        {
                            ContentType = contentTypeValue,
                            // ContentLength will be filled automatically in Chrome
                            ContentLength = null
                        },
                        MaxTimeout = MaxTimeout,
                        UserAgent = userAgent
                    };
                }
                else if (contentTypeType == typeof(MultipartFormDataContent))
                {
                    //TODO Implement - check if we just need to pass the content-type with the relevent headers
                    throw new YetAnotherCloudProxyException("Unimplemented POST Content-Type: " + request.Content.Headers.ContentType.ToString());
                }
                else if (contentTypeType == typeof(StringContent))
                {
                    //TODO Implement - check if we just need to pass the content-type with the relevent headers
                    throw new YetAnotherCloudProxyException("Unimplemented POST Content-Type: " + request.Content.Headers.ContentType.ToString());
                }
                else
                {
                    throw new YetAnotherCloudProxyException("Unsupported POST Content-Type: " + request.Content.Headers.ContentType.ToString());
                }
            }
            else
            {
                throw new YetAnotherCloudProxyException("Unsupported HttpMethod: " + request.Method.ToString());
            }

            var payload = JsonConvert.SerializeObject(req, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
            return content;
        }
 
    }
}