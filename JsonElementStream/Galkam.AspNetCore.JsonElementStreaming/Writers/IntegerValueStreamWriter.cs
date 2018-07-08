using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.JsonElementStreaming.Writers
{
    public class IntegerValueStreamWriter: BaseValueStreamWriter
    {
        public long? Value {
            get {
                this.writer.Flush();
                var value = writer.ToString();
                if (string.IsNullOrEmpty(value)) return null;
                return Int64.Parse(value);

            }
        }
    }
}
