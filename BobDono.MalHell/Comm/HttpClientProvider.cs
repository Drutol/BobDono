using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using BobDono.Interfaces;

namespace BobDono.MalHell.Comm
{
    public class HttpClientProvider : IHttpClientProvider
    {
        public HttpClient HttpClient { get; }

        public HttpClientProvider()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                
            };
            HttpClient = new HttpClient(handler);
        }
    }
}
