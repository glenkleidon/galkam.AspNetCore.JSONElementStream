using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.JsonElementStreaming.Writers
{
    public class DecimalValueStreamWriter: BaseValueStreamWriter
    {
        public Decimal? Value { get => AsDecimal(); }

        public override Type ValueType => typeof(Decimal);

        public override Decimal? AsDecimal()
        {
            this.writer.Flush();
            var value = writer.ToString();
            if (string.IsNullOrEmpty(value)) return null;
            return Decimal.Parse(value);
        }

        public override bool IsDecimal()
        {
            return true;
        }

    }
}
