﻿using System.Collections.Generic;
using Galkam.AspNetCore.ElementStreaming.Writers;
using Microsoft.AspNetCore.Http;

namespace Galkam.AspNetCore.ElementStreaming
{
    public interface IElementStreamingRequestContext
    {
        Dictionary<string, IElementStreamWriter> Elements { get; set; }
        List<string> EndPoints { get; set; }
        List<string> ContentTypes { get; set; }
        bool IsTargetRequest(HttpContext context);
        IElementStreamer Streamer { get; set; }
    }
}