using Galkam.AspNetCore.ElementStreaming.Writers;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Galkam.AspNetCore.ElementStreaming
{
    /// <summary>
    /// Generic Element Streaming Request Context which can be used or descended to produce specific
    /// behaviour: eg to stream a file to disk as it arrives create a FileStreamingElementRequestContext.
    /// </summary>
    public class ElementStreamingRequestContext : IElementStreamingRequestContext
    {
        private IElementStreamer streamer;
        public ElementStreamingRequestContext()
        {
            InitWithEmptyValues();
        }
        public ElementStreamingRequestContext(List<string> endPoints, 
                   List<string> contentTypes, StreamedElements elements,
                   IElementStreamer elementStreamer)
        {
            streamer = elementStreamer;
            EndPoints = EndPoints;
            Elements = elements;
            ContentTypes = contentTypes;
        }

        private void InitWithEmptyValues()
        {
            Elements  = new StreamedElements();
            EndPoints = new List<string>();
            ContentTypes= new List<string>();
        }

        public ElementStreamingRequestContext(IElementStreamer streamer)
        {
            Streamer = streamer;
        }
        public StreamedElements Elements { get; set; }
        public List<string> EndPoints { get; set; } 
        public List<string> ContentTypes { get; set; } 
        public IElementStreamer Streamer { get => streamer; set { streamer = value; }  }
        public virtual bool CanHandleRequest(HttpContext context)
        {
            return
                // Is NOT a Get Request
                !context.Request.Method.ToLower().Equals(HttpMethods.Get.ToLower()) &&
                // The request is of the correct specified content Type 
                ContentTypes.Any(
                     c => c.ToLower().Equals(context.Request.ContentType.ToLower())) &&
                // The path indicates one of the specified endpoints
                (EndPoints.Count==0 || EndPoints.Any(
                   p => p.StartsWith(context.Request.Path) ||
                        p.StartsWith(context.Request.PathBase))
                        );
        }
        public virtual bool DataLocatedHandler()
        {
            return true;
        }
        public virtual bool DataEndedHandler()
        {
            return true;
        }
    }
}
