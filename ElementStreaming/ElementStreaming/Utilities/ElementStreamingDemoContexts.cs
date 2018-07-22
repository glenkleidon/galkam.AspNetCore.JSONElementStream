using ElementStreaming.Utilities;
using Galkam.AspNetCore.ElementStreaming;
using Galkam.AspNetCore.ElementStreaming.ElementStreamingRequestContexts;
using Galkam.AspNetCore.ElementStreaming.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace JsonElementStream
{
    public class ElementStreamingDemoContexts : ElementStreamingRequestContextCollection
    {

        public override void Configure()
        {
            // This application needs to handle the folowing situations:
            // Writing incoming base64 data to a temporary file.
            this.ElementStreamingRequestContexts = new List<ElementStreamingRequestContext>();
            ConfigureJsonFileWriter();

            // You COULD also support XML if you use an implementation of XMLElementStreamer (not available in Release 1).
            // Configure XMLFileWriter();

        }

        private void ConfigureJsonFileWriter()
        {
            // 1. Identify the elements, and their types.
            var elements = new StreamedElements();
            // I want to be able to intercept a Base64 encoded element and string property
            elements.Add(Constants.DocumentJsonPath, new Base64ToTempFileWriter());
            elements.Add(Constants.FilenameJsonPath, new StringValueStreamWriter());

            // Specify when to use this Context 
            var jsonEndpoints = new List<string>() { "/api/document/upload" };
            var jsonContentTypes = new List<string>() { "application/json", "application/json;charset=utf-8" };

            // Now plug in a Element Streamer.
            var elementStreamer = new JsonElementStreamer(elements);
            var jsonRequestContext = new ElementStreamingRequestContext(jsonEndpoints, jsonContentTypes, elements, elementStreamer);


            // Finally add what we new want to do when we receive data (use a lambda or specific funciton).
            jsonRequestContext.OnElementStarting = s => true; // do nothing for starting

            jsonRequestContext.OnElementCompleted =  s =>
            {
                var handled = false;
                var docElement = s.Elements[Constants.DocumentJsonPath];
                if (docElement != null && s.Streamer.ElementPath == Constants.DocumentJsonPath && docElement.IsComplete)
                {
                    //Add a new context element indicating the size of the stream.
                    var sizeElement = new DynamicValueStreamWriter(docElement.OutStream.Length.ToString());
                    s.Elements.Add(Constants.ByteSizeJsonPath, sizeElement);

                    // substitute the original base64 code with the filename:
                    var tmpFileName = docElement.TypedValue.AsString();
                    var newFilenameElement = new DynamicValueStreamWriter(tmpFileName);
                    s.Elements.Add(Constants.TempFileJsonPath, newFilenameElement);
                    
                    //warning: this will write synchronously
                    s.Streamer.WriteAlternateContent(tmpFileName);

                    // and now get rid of that streamer - as we now have a file reference.
                    s.Elements.DiscardElement(Constants.DocumentJsonPath);
                    // was this a file we previously decided to discard?
                    if (docElement.Ignore)
                    {
                        if (File.Exists(tmpFileName)) File.Delete(tmpFileName);
                    }
                    handled = true;
                }
                else
                {
                    // Check for unwanted file types we need to delete or block. 
                    var fnameElement = s.Elements[Constants.FilenameJsonPath];
                    var newFilenameElement = s.Elements[Constants.TempFileJsonPath];

                    if (s.Streamer.ElementPath == Constants.FilenameJsonPath && fnameElement.IsComplete)
                    {
                        var fname = fnameElement.TypedValue.AsString();
                        var extn = Path.GetExtension(fname);
                        var blockedTypes = new List<string>() { ".exe", ".svg", ".dll", ".bat", ".com", ".sh", ".ps1" };
                        var blockFile = blockedTypes.Any(t => t.ToLower() == extn);
                        if (docElement != null)
                        {
                            // encounterd FileName first - so we can block the file from being written
                            docElement.Ignore = true;
                        }
                        if (newFilenameElement != null)
                        {
                            var tmpFileName = newFilenameElement.TypedValue.AsString();
                            // we have already written it, delete it now.
                            if (File.Exists(tmpFileName)) File.Delete(tmpFileName);
                        }
                        handled = true;
                    }
                }

                return handled;

            };
            this.ElementStreamingRequestContexts.Add(jsonRequestContext);

        }

    }
}
