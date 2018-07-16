using System.Collections.Generic;
using Galkam.AspNetCore.JsonElementStreaming.Writers;

namespace Galkam.AspNetCore.JsonElementStreaming
{
    public interface IJsonStreamingRequestContext
    {
        Dictionary<string, IElementStreamWriter> Elements { get; set; }
        List<string> EndPoints { get; set; }
    }
}