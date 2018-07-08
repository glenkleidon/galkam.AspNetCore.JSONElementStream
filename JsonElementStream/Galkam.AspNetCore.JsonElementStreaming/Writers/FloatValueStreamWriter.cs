using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.JsonElementStreaming.Writers
{
    public class FloatValueStreamWriter: BaseValueStreamWriter
    {
        public float? Value {
            get {
                this.writer.Flush();
                var value = writer.ToString();
                if (string.IsNullOrEmpty(value)) return null;
                return float.Parse(value);

            }
        }
    }
}
