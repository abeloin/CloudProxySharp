﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CloudProxySharp.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudProxySharp.Tests
{
    [TestClass]
    public class ClearanceHandlerTests
    {
        private readonly Uri _protectedUri = new Uri("http://www.bteye.org/");

        [TestMethod]
        public async Task SolveOk()
        {
            var uri = new Uri("https://www.google.com/");
            var handler = new ClearanceHandler(Settings.CloudProxyApiUrl)
            {
                UserAgent = null,
                MaxTimeout = 60000
            };

            var client = new HttpClient(handler);
            var response = await client.GetAsync(uri);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task SolveOkCloudflare()
        {
            var handler = new ClearanceHandler(Settings.CloudProxyApiUrl)
            {
                UserAgent = "Mozilla/5.0 (X11; Linux i686; rv:77.0) Gecko/20100101 Firefox/77.0",
                MaxTimeout = 60000
            };

            var client = new HttpClient(handler);
            var response = await client.GetAsync(_protectedUri);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
        
        [TestMethod]
        public async Task SolveError()
        {
            var uri = new Uri("https://www.google.bad1/");
            var handler = new ClearanceHandler(Settings.CloudProxyApiUrl)
            {
                UserAgent = null,
                MaxTimeout = 60000
            };

            var client = new HttpClient(handler);
            try
            {
                await client.GetAsync(uri);
                Assert.Fail("Exception not thrown");
            }
            catch (HttpRequestException e)
            {
                Assert.IsTrue(e.Message.Contains("Name or service not know"));
            }
            catch (Exception e)
            {
                Assert.Fail("Unexpected exception: " + e);
            }
        }

        [TestMethod]
        public async Task SolveErrorBadConfig()
        {
            var handler = new ClearanceHandler("http://localhost:44445")
            {
                UserAgent = null,
                MaxTimeout = 60000
            };

            var client = new HttpClient(handler);
            try
            {
                await client.GetAsync(_protectedUri);
                Assert.Fail("Exception not thrown");
            }
            catch (CloudProxyException e)
            {
                Assert.IsTrue(e.Message.Contains("Error connecting to CloudProxy server"));
            }
            catch (Exception e)
            {
                Assert.Fail("Unexpected exception: " + e);
            }
        }
 
        [TestMethod]
        public async Task SolveErrorNoConfig()
        {
            var handler = new ClearanceHandler("")
            {
                UserAgent = null,
                MaxTimeout = 60000
            };

            var client = new HttpClient(handler);
            try
            {
                await client.GetAsync(_protectedUri);
                Assert.Fail("Exception not thrown");
            }
            catch (CloudProxyException e)
            {
                Assert.IsTrue(e.Message.Contains("Challenge detected but CloudProxy is not configured"));
            }
            catch (Exception e)
            {
                Assert.Fail("Unexpected exception: " + e);
            }
        }

    }
}