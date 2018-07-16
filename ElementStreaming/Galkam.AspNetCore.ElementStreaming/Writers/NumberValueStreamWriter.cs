using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.ElementStreaming.Writers
{
    public class NumberValueStreamWriter: BaseValueStreamWriter
    {
        public Decimal? Value { get => AsDecimal(); }

        public override Type ValueType { get
            {
                var v = ToString();
                if (string.IsNullOrWhiteSpace(v)) return null;
                if (!v.Contains('.') && AsInteger() != null) return typeof(long);
                return typeof(Decimal);
            }
        }

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
        public override bool IsInteger()
        {
            return AsInteger() != null;
        }

        public override float? AsFloat()
        {
            float v;
            var value = ToString();
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (float.TryParse(value, out v)) return v;
            return null;
        }
        public override Double? AsDouble()
        {
            Double v;
            var value = ToString();
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (Double.TryParse(value, out v)) return v;
            return null;

        }
        public override Decimal? AsDecimal()
        {
            Decimal v;
            var value = ToString();
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (Decimal.TryParse(value, out v)) return v;
            return null;

        }
        public override long? AsInteger()
        {
            long v;
            var value = ToString();
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (long.TryParse(value, out v)) return v;
            return null;
        }

        public override Single? AsSingle()
        {
            Single v;
            var value = AsString();
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (Single.TryParse(value, out v)) return v;
            return null;
        }
    }
}
