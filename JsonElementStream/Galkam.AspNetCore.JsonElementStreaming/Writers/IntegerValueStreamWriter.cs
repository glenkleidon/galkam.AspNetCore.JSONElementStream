using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.ElementStreaming.Writers
{
    public class IntegerValueStreamWriter: BaseValueStreamWriter
    {
        public long? Value { get => AsInteger(); }

        public override Type ValueType => typeof(long);

        public override long? AsInteger()
        {
            this.writer.Flush();
            var value = writer.ToString();
            if (string.IsNullOrEmpty(value)) return null;
            return Int64.Parse(value);
        }

        public override bool IsInteger()
        {
            return true;
        }
    }
}
