using Galkam.AspNetCore.ElementStreaming;
using Galkam.AspNetCore.ElementStreaming.ElementStreamingRequestContexts;
using Galkam.AspNetCore.ElementStreaming.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JsonElementStream
{
    public class ElementStreamingDemoContexts : ElementStreamingRequestContextCollection
    {
        private static readonly string documentJsonPath = "$.document";
        private static readonly string filenameJsonPath = "$.filename";

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
            elements.Add(documentJsonPath, new Base64ToTempFileWriter());
            elements.Add(filenameJsonPath, new StringValueStreamWriter());

            // Specify when to use this Context 
            var jsonEndpoints = new List<string>() { "/api/document/upload" };
            var jsonContentTypes = new List<string>() { "application/json", "application/json;charset=utf-8" };

            // Now plug in a Element Streamer.
            var elementStreamer = new JsonElementStreamer(elements);
            var jsonRequestContext = new ElementStreamingRequestContext(jsonEndpoints, jsonContentTypes, elements, elementStreamer);


            // Finally add what we new want to do when we receive data (use a lambda or specific funciton).
            jsonRequestContext.OnElementStarting = s => true; // do nothing for starting
            jsonRequestContext.OnElementCompleted = s =>
            {
                var handled = false;
                var docElement = s.Elements[documentJsonPath];
                var fnameElement = s.Elements[filenameJsonPath];

                // We can rename file with the correct extension or delete unwanted files. 
                if (docElement.IsComplete && fnameElement.IsComplete)
                {
                    var fname = fnameElement.TypedValue.AsString();
                    var extn = Path.GetExtension(fname);
                    var tmpFileName = docElement.TypedValue.AsString();

                    // Get rid of the Base64Streamer and change it to a String
                    s.Elements.DiscardElement(documentJsonPath);
                    var newFileName = Path.ChangeExtension(tmpFileName, extn);
                    var newElement = new StringValueStreamWriter(newFileName);
                    s.Elements.Add(documentJsonPath, newElement);
                    handled = true;
                }

                return handled;

            };
            this.ElementStreamingRequestContexts.Add(jsonRequestContext);             

        }

    }
}
