using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.JsonElementStreaming.Writers
{
    public class StringValueStreamWriter: BaseValueStreamWriter
    {
        public string Value {
            get {
                this.writer.Flush();
                return writer.ToString();
            }
        }
    }
}
