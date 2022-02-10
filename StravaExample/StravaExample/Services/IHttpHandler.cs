using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StravaExample.Services
{
    public interface IHttpHandler
    {
        Uri BaseAddress { get; set; }
        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content);
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
    }
}
