using StravaExample.Services.Impl;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(HttpHandler))]
namespace StravaExample.Services.Impl
{
    public class HttpHandler : IHttpHandler
    {

        private HttpClient httpClient = new HttpClient();

        public Uri BaseAddress
        {
            get => httpClient.BaseAddress;
            set => httpClient.BaseAddress = value;
        }

        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            return httpClient.PostAsync(requestUri, content);
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return httpClient.SendAsync(request);
        }
    }
}
