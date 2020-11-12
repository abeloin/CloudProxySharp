using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CloudProxySharp.Sample
{
    public static class ClearanceHandlerSample
    {

        public static async Task Sample()
        {
            var handler = new ClearanceHandler("http://127.0.0.1:8192/")
            {
                UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.75 Safari/537.36",
                MaxTimeout = 240000
            };

            var client = new HttpClient(handler);
            var content = await client.GetStringAsync("https://www2.yggtorrent.si/engine/search?category=all&name=\"test\"&description=&file=&uploader=&sub_category=&do=search&order=desc&sort=publish_date&attempt=1");
            Console.WriteLine(content);
        }

    }
}