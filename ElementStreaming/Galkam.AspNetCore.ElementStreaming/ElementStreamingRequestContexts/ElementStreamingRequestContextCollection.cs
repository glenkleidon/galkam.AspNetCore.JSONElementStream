using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Galkam.AspNetCore.ElementStreaming.ElementStreamingRequestContexts
{
    public abstract class ElementStreamingRequestContextCollection : IElementStreamingRequestContextCollection
    {
        public ElementStreamingRequestContextCollection()
        {
            Configure();
        }
        public ICollection<IElementStreamingRequestContext> ElementStreamingRequestContexts { get ; set; }

        public abstract void Configure();

        public IElementStreamingRequestContext GetRequestHandler(HttpContext context)
        {
            return ElementStreamingRequestContexts.Where(r => r.CanHandleRequest(context)).First();
        }

    }
}
