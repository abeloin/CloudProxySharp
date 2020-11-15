using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YetAnotherCloudProxySharp.Constants;
using YetAnotherCloudProxySharp.Exceptions;
using YetAnotherCloudProxySharp.Extensions;
using YetAnotherCloudProxySharp.Solvers;
using YetAnotherCloudProxySharp.Types;
using Cookie = System.Net.Cookie;

namespace YetAnotherCloudProxySharp
{
    /// <summary>
    /// A HTTP handler that transparently manages Cloudflare's protection bypass.
    /// </summary>
    public class ClearanceHandler : DelegatingHandler
    {
        private readonly HttpClient _client;
        private readonly YetAnotherCloudProxy _YetAnotherCloudProxy;

        /// <summary>
        /// The User-Agent which will be used accross this session (null means default YetAnotherCloudProxy User-Agent).
        /// </summary>
        public string UserAgent = null;

        /// <summary>
        /// Max timeout to solve the challenge.
        /// </summary>
        public int MaxTimeout = 60000;

        private HttpClientHandler HttpClientHandler => InnerHandler.GetMostInnerHandler() as HttpClientHandler;

        /// <summary>
        /// Creates a new instance of the <see cref="ClearanceHandler"/>.
        /// </summary>
        /// <param name="YetAnotherCloudProxyApiUrl">YetAnotherCloudProxy API URL. If null or empty it will detect the challenges, but
        /// they will not be solved. Example: "http://localhost:8191/"</param>
        public ClearanceHandler(string YetAnotherCloudProxyApiUrl)
            : base(new HttpClientHandler())
        {
            _client = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                CookieContainer = new CookieContainer()
            });

            if (!string.IsNullOrWhiteSpace(YetAnotherCloudProxyApiUrl))
            {
                _YetAnotherCloudProxy = new YetAnotherCloudProxy(YetAnotherCloudProxyApiUrl)
                {
                    MaxTimeout = MaxTimeout
                };
            }
        }

        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Change the User-Agent if required
            OverrideUserAgentHeader(request);

            // Perform the original user request
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // Detect if there is a challenge in the response
            if (ChallengeDetector.IsClearanceRequired(response))
            {
                if (_YetAnotherCloudProxy == null)
                    throw new YetAnotherCloudProxyException("Challenge detected but YetAnotherCloudProxy is not configured");

                // Resolve the challenge using YetAnotherCloudProxy API
                var YetAnotherCloudProxyResponse = await _YetAnotherCloudProxy.Solve(request);

                // Change the cookies in the original request with the cookies provided by YetAnotherCloudProxy
                InjectCookies(request, YetAnotherCloudProxyResponse);
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                // Detect if there is a challenge in the response
                if (ChallengeDetector.IsClearanceRequired(response))
                    throw new YetAnotherCloudProxyException("The cookies provided by YetAnotherCloudProxy are not valid");

                // Add the "Set-Cookie" header in the response with the cookies provided by YetAnotherCloudProxy
                InjectSetCookieHeader(response, YetAnotherCloudProxyResponse);
            }

            return response;
        }

        private void OverrideUserAgentHeader(HttpRequestMessage request)
        {
            if (UserAgent == null)
                return;
            if (request.Headers.UserAgent.ToString().Equals(UserAgent))
                return;
            request.Headers.UserAgent.Clear();
            request.Headers.Add(HttpHeaders.UserAgent, UserAgent);
        }

        private void InjectCookies(HttpRequestMessage request, YetAnotherCloudProxyResponse YetAnotherCloudProxyResponse)
        {
            var rCookies = YetAnotherCloudProxyResponse.Solution.Cookies;
            if (!rCookies.Any())
                return;
            var rCookiesList = rCookies.Select(x => x.Name).ToList();

            if (HttpClientHandler.UseCookies)
            {
                var oldCookies = HttpClientHandler.CookieContainer.GetCookies(request.RequestUri);
                foreach (Cookie oldCookie in oldCookies)
                    if (rCookiesList.Contains(oldCookie.Name))
                        oldCookie.Expired = true;
                foreach (var rCookie in rCookies)
                    HttpClientHandler.CookieContainer.Add(request.RequestUri, rCookie.ToCookieObj());
            }
            else
            {
                foreach (var rCookie in rCookies)
                    request.Headers.Add(HttpHeaders.Cookie, rCookie.ToHeaderValue());
            }
        }

        private void InjectSetCookieHeader(HttpResponseMessage response, YetAnotherCloudProxyResponse YetAnotherCloudProxyResponse)
        {
            var rCookies = YetAnotherCloudProxyResponse.Solution.Cookies;
            if (!rCookies.Any())
                return;

            // inject set-cookie headers in the response
            foreach (var rCookie in rCookies)
                response.Headers.Add(HttpHeaders.SetCookie, rCookie.ToHeaderValue());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _client.Dispose();

            base.Dispose(disposing);
        }

    }
}
