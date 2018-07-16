using Galkam.AspNetCore.ElementStreaming.Writers;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Galkam.AspNetCore.ElementStreaming
{
    public class ElementStreamingRequestContext : IElementStreamingRequestContext
    {
        private IElementStreamer streamer;
        public ElementStreamingRequestContext(IElementStreamer streamer)
        {
            Streamer = streamer;
        }
        public Dictionary<string, IElementStreamWriter> Elements { get; set; } = new Dictionary<string, IElementStreamWriter>();
        public List<string> EndPoints { get; set; } = new List<string>();
        public List<string> ContentTypes { get; set; } = new List<string>();
        public IElementStreamer Streamer { get => streamer; set { streamer = value; }  }
        public virtual bool IsTargetRequest(HttpContext context)
        {
            return
                // Is NOT a Get Request
                !context.Request.Method.ToLower().Equals(HttpMethods.Get.ToLower()) &&
                // The request is of the correct specified content Type 
                ContentTypes.Any(
                     c => c.ToLower().Equals(context.Request.ContentType.ToLower())) &&
                // The path indicates one of the specified endpoints
                EndPoints.Any(
                   p => p.StartsWith(context.Request.Path) ||
                        p.StartsWith(context.Request.PathBase));
        }
        protected virtual bool 
    }
}
