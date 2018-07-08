using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.JsonElementStreaming.Writers
{
    public class BoolValueStreamWriter: BaseValueStreamWriter
    {
        public bool? Value { get => AsBool(); }

        public override Type ValueType => typeof(bool);

        public override bool? AsBool()
        {
            this.writer.Flush();
            var value = writer.ToString();
            if (string.IsNullOrEmpty(value)) return null;
            if (value.ToLower().Equals("t") || value.ToLower().Equals("true")) return true;
            if (value.ToLower().Equals("f") || value.ToLower().Equals("false")) return false;
            return Int32.Parse(value)!=0;
        }

        public override bool IsBool()
        {
            return true;
        }
    }
}
