using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.ElementStreaming.ElementStreamingRequestContexts
{
    interface IElementStreamingRequestContextCollection
    {
        ICollection<IElementStreamingRequestContext> ElementStreamingRequestContexts { get; set; }
        IElementStreamingRequestContext GetRequestHandler(HttpContext context);
        void Configure();
    }
}
