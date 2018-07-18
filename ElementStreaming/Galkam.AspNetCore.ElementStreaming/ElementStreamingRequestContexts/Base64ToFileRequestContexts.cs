using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.ElementStreaming.ElementStreamingRequestContexts
{
    class Base64ToFileRequestContexts : ElementStreamingRequestContextCollection
    {
        public override void Configure()
        {
            // 1. Json Format handler..
            var jsonContentTypes = new List<string>() { "application/json", "application/json;charset=utf-8" };
            var jsonEndpoints = ""

            var jsonRequestContext= new ElementStreamingRequestContext(jsonEndpoints, jsonContentTypes, jsonElements, jsonStreamer)
        }
       
    }
}
