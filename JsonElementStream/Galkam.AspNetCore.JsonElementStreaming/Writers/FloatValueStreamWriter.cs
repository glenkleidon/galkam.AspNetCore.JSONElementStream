using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.ElementStreaming.Writers
{
    public class FloatValueStreamWriter: BaseValueStreamWriter
    {
        public float? Value { get => AsFloat(); }

        public override Type ValueType => typeof(float);

        public override float? AsFloat()
        {
            this.writer.Flush();
            var value = writer.ToString();
            if (string.IsNullOrEmpty(value)) return null;
            return float.Parse(value);
        }
        public override bool IsFloat()
        {
            return true;
        }
    }
}
