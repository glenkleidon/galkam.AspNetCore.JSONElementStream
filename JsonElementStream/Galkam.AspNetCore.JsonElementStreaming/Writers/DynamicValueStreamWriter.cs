using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.JsonElementStreaming.Writers
{
    public class DynamicValueStreamWriter: BaseValueStreamWriter
    {
        public dynamic Value
        {
            get
            {
                this.writer.Flush();
                if (string.IsNullOrWhiteSpace(writer.ToString())) return null;
                return writer.ToString();
            }
        }

        public bool IsNumber()
        {
            return IsInteger() || IsFloat();

        }
        public bool IsFloat()
        {
            return AsFloat() != null;
        }
        public bool IsDouble()
        {
            return AsDouble() != null;
        }
        public bool IsDecimal()
        {
            return AsDecimal() != null;
        }
        public bool IsDate()
        {
            return AsDate() != null;
        }
        public bool IsInteger()
        {
            return AsInteger() != null;
        }

        public float? AsFloat()
        {
            float v;
            var value = Value.ToString();
            if (value == null) return null;
            if (float.TryParse(value, out v)) return v;
            return null;
        }
        public Double? AsDouble()
        {
            Double v;
            var value = Value.ToString();
            if (value == null) return null;
            if (Double.TryParse(value, out v)) return v;
            return null;

        }
        public Decimal? AsDecimal()
        {
            Decimal v;
            var value = Value.ToString();
            if (value == null) return null;
            if (Decimal.TryParse(value, out v)) return v;
            return null;

        }
        public DateTime? AsDate()
        {
            DateTime v;
            var value = Value.ToString();
            if (value == null) return null;
            if (DateTime.TryParse(value, out v)) return v;
            return null;
        }
        public long? AsInteger()
        {
            long v;
            var value = Value.ToString();
            if (value == null) return null;
            if (long.TryParse(value, out v)) return v;
            return null;
        }


    }
}
