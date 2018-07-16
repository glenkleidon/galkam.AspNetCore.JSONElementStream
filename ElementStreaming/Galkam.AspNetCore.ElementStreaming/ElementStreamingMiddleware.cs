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
            if (!streamContext.IsTargetRequest(context))
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
                            if (json)
                        }
                        while (JsonStreamer.Status != Enums.StreamerStatus.Complete);
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
