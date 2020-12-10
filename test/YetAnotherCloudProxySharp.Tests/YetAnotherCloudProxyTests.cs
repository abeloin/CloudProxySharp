using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using YetAnotherCloudProxySharp.Constants;
using YetAnotherCloudProxySharp.Exceptions;
using YetAnotherCloudProxySharp.Solvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace YetAnotherCloudProxySharp.Tests
{
    [TestClass]
    public class YetAnotherCloudProxyTests
    {
        [TestMethod]
        public async Task SolveOk()
        {
            var uri = new Uri("https://www.google.com/");
            var YetAnotherCloudProxy = new YetAnotherCloudProxy(Settings.YetAnotherCloudProxyApiUrl);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            var YetAnotherCloudProxyResponse = await YetAnotherCloudProxy.Solve(request);
            Assert.AreEqual("ok", YetAnotherCloudProxyResponse.Status);
            Assert.AreEqual("", YetAnotherCloudProxyResponse.Message);
            Assert.IsTrue(YetAnotherCloudProxyResponse.StartTimestamp > 0);
            Assert.IsTrue(YetAnotherCloudProxyResponse.EndTimestamp > YetAnotherCloudProxyResponse.StartTimestamp);
            Assert.AreEqual("1.0.3", YetAnotherCloudProxyResponse.Version);

            Assert.AreEqual("https://www.google.com/", YetAnotherCloudProxyResponse.Solution.Url);
            Assert.IsTrue(YetAnotherCloudProxyResponse.Solution.Response.Contains("<title>Google</title>"));
            Assert.IsTrue(YetAnotherCloudProxyResponse.Solution.Cookies.Any());
            Assert.IsTrue(YetAnotherCloudProxyResponse.Solution.UserAgent.Contains(" Chrome/"));

            var firstCookie = YetAnotherCloudProxyResponse.Solution.Cookies.First();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(firstCookie.Name));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(firstCookie.Value));
        }

        [TestMethod]
        public async Task SolveOkUserAgent()
        {
            const string userAgent = "Mozilla/5.0 (X11; Linux i686; rv:77.0) Gecko/20100101 Firefox/77.0";
            var uri = new Uri("https://www.google.com/");
            var YetAnotherCloudProxy = new YetAnotherCloudProxy(Settings.YetAnotherCloudProxyApiUrl);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add(HttpHeaders.UserAgent, userAgent);

            var YetAnotherCloudProxyResponse = await YetAnotherCloudProxy.Solve(request);
            Assert.AreEqual("ok", YetAnotherCloudProxyResponse.Status);
            Assert.AreEqual(userAgent, YetAnotherCloudProxyResponse.Solution.UserAgent);
        }

        [TestMethod]
        public async Task SolveError()
        {
            var uri = new Uri("https://www.google.bad1/");
            var YetAnotherCloudProxy = new YetAnotherCloudProxy(Settings.YetAnotherCloudProxyApiUrl);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            try
            {
                await YetAnotherCloudProxy.Solve(request);
                Assert.Fail("Exception not thrown");
            }
            catch (YetAnotherCloudProxyException e)
            {
                Assert.AreEqual("NS_ERROR_UNKNOWN_HOST at https://www.google.bad1/", e.Message);
            }
            catch (Exception e)
            {
                Assert.Fail("Unexpected exception: " + e);
            }
        }

        [TestMethod]
        public async Task SolveErrorConfig()
        {
            var uri = new Uri("https://www.google.com/");
            var YetAnotherCloudProxy = new YetAnotherCloudProxy("http://localhost:44445");
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            try
            {
                await YetAnotherCloudProxy.Solve(request);
                Assert.Fail("Exception not thrown");
            }
            catch (YetAnotherCloudProxyException e)
            {
                Assert.IsTrue(e.Message.Contains("Error connecting to YetAnotherCloudProxy server"));
            }
            catch (Exception e)
            {
                Assert.Fail("Unexpected exception: " + e);
            }
        }
    }
}