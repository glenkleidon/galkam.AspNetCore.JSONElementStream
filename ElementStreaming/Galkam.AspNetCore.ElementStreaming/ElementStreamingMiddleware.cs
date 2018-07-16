using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Linq;

namespace Galkam.AspNetCore.ElementStreaming
{
    public class ElementStreamingMiddleware
    {
        private readonly IElementStreamingRequestContext streamContext;
        private readonly RequestDelegate next;

        public ElementStreamingMiddleware(RequestDelegate next, IElementStreamingRequestContext streamContext)
        {
            this.streamContext = streamContext;
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (
                context.Request.Method.ToLower().Equals(HttpMethods.Get.ToLower()) ||
                !context.Request.ContentType.ToLower().Contains("json") ||
                !streamContext.EndPoints.Any(
                    p => p.StartsWith(context.Request.Path) ||
                         p.StartsWith(context.Request.PathBase)
                    ) 
               )
            {
                await next.Invoke(context);
            }
            else
            {
                var incomingStream = new MemoryStream();
                try
                {
                    var JsonStreamer = new JsonElementStreamer(context.Request.Body, incomingStream, streamContext.Elements);
                    try
                    {

                        do
                        {
                            await JsonStreamer.Next();
                        } while (JsonStreamer.Status != Enums.StreamerStatus.Complete);

                    }
                    finally
                    {
                        context.Request.Body.Dispose();
                        context.Request.Body = incomingStream;
                    }
                    
                    await next.Invoke(context);
                }
                finally
                {
                    incomingStream.Dispose();
                }
            }
        }
    }
}
