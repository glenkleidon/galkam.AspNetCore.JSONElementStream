using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.JsonElementStreaming.Writers
{
    public class SingleValueStreamWriter: BaseValueStreamWriter
    {
        public Single? Value { get => AsSingle(); }

        public override Type ValueType => typeof(Single);

        public override Single? AsSingle()
        {
            this.writer.Flush();
            var value = writer.ToString();
            if (string.IsNullOrEmpty(value)) return null;
            return Single.Parse(value);
        }

        public override bool IsSingle()
        {
            return true;
        }
    }
}
