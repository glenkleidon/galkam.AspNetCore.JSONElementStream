using Galkam.AspNetCore.ElementStreaming.Writers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.ElementStreaming
{
    public class JsonStreamingRequestContext : IJsonStreamingRequestContext
    {
        public Dictionary<string, IElementStreamWriter> Elements { get; set; } = new Dictionary<string, IElementStreamWriter>();
        public List<string> EndPoints { get; set; } = new List<string>();



    }
}
