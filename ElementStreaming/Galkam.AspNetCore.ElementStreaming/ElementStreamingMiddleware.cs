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
        private readonly RequestDelegate next;

        public ElementStreamingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, IElementStreamingRequestContext streamContext)
        {
            if (!streamContext.CanHandleRequest(context))
            {
                await next.Invoke(context);
            }
            else
            {
                using (var incomingStream = new MemoryStream())
                {
                    var JsonStreamer = new JsonElementStreamer(context.Request.Body, incomingStream, streamContext.Elements);
                    try
                    {
                        do
                        {
                            await JsonStreamer.Next();
                            switch (JsonStreamer.Status)
                            {
                                case Enums.StreamerStatus.StartOfData:
                                    streamContext.DataLocatedHandler();
                                    break;
                                case Enums.StreamerStatus.EndOfData:
                                    streamContext.DataEndedHandler();
                                    break;
                            }
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
            }
        }
    }
}
