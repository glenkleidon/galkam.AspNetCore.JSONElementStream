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
                await next(context);
            }
            else
            {
                var bodyStream = context.Response.Body;
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
                                        handlerContext?.ElementFoundHandler();
                                        break;
                                    case Enums.StreamerStatus.EndOfData:
                                        handlerContext.ElementCompleteHandler();
                                        break;
                                }
                            }
                            while (jsonStreamer.Status != Enums.StreamerStatus.Complete);
                    }
                    finally
                    {
                        context.Request.Body.Dispose();
                        incomingStream.Position = 0;
                        // logging:
                        var logbuffer = new byte[incomingStream.Length];
                        incomingStream.Read(logbuffer, 0, (int)incomingStream.Length);
                        Console.WriteLine($"Request to {context.Request.Path}");
                        Console.WriteLine(Encoding.Default.GetString(logbuffer));
                        incomingStream.Position = 0;
                        context.Request.Body = incomingStream;
                    }
                    await next(context);
                }
            }
        }
    }
}
