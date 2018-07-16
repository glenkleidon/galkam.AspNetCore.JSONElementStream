using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.ElementStreaming.Writers
{
    public class DynamicValueStreamWriter : BaseValueStreamWriter, IValueStreamWriter
    {
        public override Type ValueType => null;

        public dynamic Value { get => AsString(); } 

        public override bool IsNumber()
        {
            return IsInteger() || IsFloat();
        }
        public override bool IsFloat()
        {
            return AsFloat() != null;
        }
        public override bool IsDouble()
        {
            return AsDouble() != null;
        }
        public override bool IsDecimal()
        {
            return AsDecimal() != null;
        }
        public override bool IsDateTime()
        {
            return AsDateTime() != null;
        }
        public override bool IsInteger()
        {
            return AsInteger() != null;
        }

        public override float? AsFloat()
        {
            float v;
            var value = AsString();
            if (value == null) return null;
            if (float.TryParse(value, out v)) return v;
            return null;
        }
        public override Double? AsDouble()
        {
            Double v;
            var value = AsString();
            if (value == null) return null;
            if (Double.TryParse(value, out v)) return v;
            return null;

        }
        public override Decimal? AsDecimal()
        {
            Decimal v;
            var value = AsString();
            if (value == null) return null;
            if (Decimal.TryParse(value, out v)) return v;
            return null;

        }
        public override DateTime? AsDateTime()
        {
            DateTime v;
            var value = AsString();
            if (value == null) return null;
            if (DateTime.TryParse(value, out v)) return v;
            return null;
        }
        public override long? AsInteger()
        {
            long v;
            var value = AsString();
            if (value == null) return null;
            if (long.TryParse(value, out v)) return v;
            return null;
        }
        public override string AsString()
        {
            return this.ToString();
        }

        public override Single? AsSingle()
        {
            Single v;
            var value = AsString();
            if (value == null) return null;
            if (Single.TryParse(value, out v)) return v;
            return null;
        }


    }
}
