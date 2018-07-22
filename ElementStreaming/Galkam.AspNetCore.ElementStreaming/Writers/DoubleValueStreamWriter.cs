using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.ElementStreaming.Writers
{
    public class DoubleValueStreamWriter: BaseValueStreamWriter
    {
        public Double? Value { get => AsDouble(); }

        public override Type ValueType => typeof(Double);

        public override Double? AsDouble()
        {
            this.writer.Flush();
            var value = writer.ToString();
            if (string.IsNullOrEmpty(value)) return null;
            return Double.Parse(value);
        }

        public override bool IsDouble()
        {
            return true;
        }
    }
}
