using System.Collections.Generic;
using Galkam.AspNetCore.ElementStreaming.Writers;
using Microsoft.AspNetCore.Http;


namespace Galkam.AspNetCore.ElementStreaming
{
    public interface IElementStreamingRequestContext
    {
        StreamedElements Elements { get; set; }
        List<string> EndPoints { get; set; }
        List<string> ContentTypes { get; set; }
        IElementStreamer Streamer { get; set; }
        bool CanHandleRequest(HttpContext context);
        bool ElementFoundHandler();
        bool ElementCompleteHandler();
        bool Active { get; set; }
    }
}