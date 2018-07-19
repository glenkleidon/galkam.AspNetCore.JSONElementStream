using Galkam.AspNetCore.ElementStreaming.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Galkam.AspNetCore.ElementStreaming.ElementStreamingRequestContexts
{
    class Base64ToFileRequestContexts : ElementStreamingRequestContextCollection
    {
        public override void Configure()
        {
            // 1. Identify the elements, and their types.
            var elements = new StreamedElements();
            // I want to be able to intercept a Base64 encoded element and string property
            elements.Add("$.document", new Base64StreamWriter());
            elements.Add("$.Category", new StringValueStreamWriter());
            
            /*
            
            var jsonContentTypes = new List<string>() { "application/json", "application/json;charset=utf-8" };
            var jsonEndpoints = ""

            var jsonRequestContext= new ElementStreamingRequestContext(jsonEndpoints, jsonContentTypes, jsonElements, jsonStreamer)
            */
        }
       
    }
}
