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
            var handlerContext = streamContext.GetRequestContext(context);
            if (handlerContext == null)
            {
                await next.Invoke(context);
            }
            else
            {
                using (var incomingStream = new MemoryStream())
                {
                    var jsonStreamer = handlerContext.Streamer;
                    jsonStreamer.OutStream = incomingStream;
                    jsonStreamer.SourceStream = context.Request.Body;
                    try
                    {
                            do
                            {
                                await jsonStreamer.Next();
                                switch (jsonStreamer.Status)
                                {
                                    case Enums.StreamerStatus.StartOfData:
                                        handlerContext.ElementFoundHandler();
                                        break;
                                    case Enums.StreamerStatus.EndOfData:
                                        handlerContext.ElementCompleteHandler();
                                        break;
                                }
                           // Console.WriteLine(context.Request.Body.Length);
                            }
                            while (jsonStreamer.Status != Enums.StreamerStatus.Complete);
                    }
                    finally
                    {
                        context.Request.Body.Dispose();
                        context.Request.Body = incomingStream;
                        context.Request.Body.Position = 0;
                    }
                    await next.Invoke(context);
                }
            }
        }
    }
}
