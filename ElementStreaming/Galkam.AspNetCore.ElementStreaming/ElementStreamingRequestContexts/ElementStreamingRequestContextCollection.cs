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
        public ICollection<ElementStreamingRequestContext> ElementStreamingRequestContexts { get ; set; }

        public ElementStreamingRequestContext ActiveContext()
        {
            return ElementStreamingRequestContexts.Where(c => c.Active).FirstOrDefault();
        }

        public abstract void Configure();

        public ElementStreamingRequestContext GetRequestContext(HttpContext context)
        {
            var activeContext = ElementStreamingRequestContexts.Where(r => r.CanHandleRequest(context)).FirstOrDefault();
            if (activeContext!=null) activeContext.Active = true;
            return activeContext;
        }

    }
}
