using Galkam.AspNetCore.ElementStreaming.Writers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.ElementStreaming
{
    public class StreamedElements : Dictionary<string,IElementStreamWriter>
    {
        public StreamedElements() : base() {}
        public StreamedElements(int capacity) : base(capacity) {}
        public void DiscardElement(string key)
        {
            var element = this[key];
            if (element != null)
            {
                Remove(key);
                element.Dispose();
            }

        }
        
    }
}
