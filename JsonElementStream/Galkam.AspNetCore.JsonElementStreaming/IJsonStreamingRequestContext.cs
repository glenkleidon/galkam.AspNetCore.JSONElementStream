using System.Collections.Generic;
using Galkam.AspNetCore.ElementStreaming.Writers;

namespace Galkam.AspNetCore.ElementStreaming
{
    public interface IJsonStreamingRequestContext
    {
        Dictionary<string, IElementStreamWriter> Elements { get; set; }
        List<string> EndPoints { get; set; }
    }
}