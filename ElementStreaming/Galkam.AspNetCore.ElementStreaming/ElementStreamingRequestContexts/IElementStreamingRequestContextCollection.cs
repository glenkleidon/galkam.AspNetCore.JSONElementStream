using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.ElementStreaming.ElementStreamingRequestContexts
{
    public interface IElementStreamingRequestContextCollection
    {
        ICollection<ElementStreamingRequestContext> ElementStreamingRequestContexts { get; set; }
        ElementStreamingRequestContext GetRequestContext(HttpContext context);
        void Configure();
        ElementStreamingRequestContext ActiveContext();
    }
}
