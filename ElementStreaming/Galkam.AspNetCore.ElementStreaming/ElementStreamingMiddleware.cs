using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Linq;
using Galkam.AspNetCore.ElementStreaming.ElementStreamingRequestContexts;

namespace Galkam.AspNetCore.ElementStreaming
{
    public class ElementStreamingMiddleware
    {
        private readonly RequestDelegate next;

        public ElementStreamingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, IElementStreamingRequestContextCollection streamContext)
        {
            var handler = streamContext.GetRequestHandler(context);
            if (handler!=null)
            {
                await next.Invoke(context);
            }
            else
            {
                using (var incomingStream = new MemoryStream())
                {
                    var JsonStreamer = handler.Streamer;
                    try
                    {
                        do
                        {
                            await JsonStreamer.Next();
                            switch (JsonStreamer.Status)
                            {
                                case Enums.StreamerStatus.StartOfData:
                                    handler.ElementFoundHandler();
                                    break;
                                case Enums.StreamerStatus.EndOfData:
                                    handler.ElementCompleteHandler();
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
