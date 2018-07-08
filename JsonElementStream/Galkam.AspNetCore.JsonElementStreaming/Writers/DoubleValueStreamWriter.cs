using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.JsonElementStreaming.Writers
{
    public class DoubleValueStreamWriter: BaseValueStreamWriter
    {
        public Double? Value
        {
            get
            {
                this.writer.Flush();
                var value = writer.ToString();
                if (string.IsNullOrEmpty(value)) return null;
                return Double.Parse(value);

            }
        }
    }
}
