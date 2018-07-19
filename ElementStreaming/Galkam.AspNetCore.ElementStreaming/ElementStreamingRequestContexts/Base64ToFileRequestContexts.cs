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
            // This application needs to handle the folowing situations:
            // Writing incoming base64 data to a temporary file.
            ConfigureJsonFileWriter();

            // You COULD also support XML if you use an implementation of XMLElementStreamer (not available in Release 1).
            // Configure XMLFileWriter();
           
        }

        private void ConfigureJsonFileWriter()
        {
            // 1. Identify the elements, and their types.
            var elements = new StreamedElements();
            // I want to be able to intercept a Base64 encoded element and string property
            elements.Add("$.document", new Base64StreamWriter());
            elements.Add("$.Category", new StringValueStreamWriter());
            var jsonEndpoints = new List<string>() { "/api/document/upload" };
            var jsonContentTypes = new List<string>() { "application/json", "application/json;charset=utf-8" };

            // Now 

            var jsonRequestContext = new ElementStreamingRequestContext(jsonEndpoints, jsonContentTypes, elements, null);
            
        }

    }
}
