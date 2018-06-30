using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace Galkam.AspNetCore.JsonElementStreaming
{
    public class JsonElementStreamingMiddleware
    {
        private readonly RequestDelegate _next;

        public JsonElementStreamingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var IncomingRequestStream = new MemoryStream();

            try
            {
                var requestLog =
                $"REQUEST HttpMethod: {context.Request.Method}, Path: {context.Request.Path}";

                using (var bodyReader = new StreamReader(context.Request.Body))
                {
                    var bodyAsText = bodyReader.ReadToEnd();
                    if (string.IsNullOrWhiteSpace(bodyAsText) == false)
                    {
                        requestLog += $", Body : {bodyAsText}";
                    }

                    var bytesToWrite = Encoding.UTF8.GetBytes(bodyAsText);
                    IncomingRequestStream.Write(bytesToWrite, 0, bytesToWrite.Length);
                    IncomingRequestStream.Seek(0, SeekOrigin.Begin);
                    context.Request.Body = IncomingRequestStream;
                }

                await _next.Invoke(context);
            }
            finally
            {
                IncomingRequestStream.Dispose();
            }
        }
    }
}
