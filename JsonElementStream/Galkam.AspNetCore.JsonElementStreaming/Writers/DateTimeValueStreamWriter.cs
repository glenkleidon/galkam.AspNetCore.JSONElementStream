using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.JsonElementStreaming.Writers
{
    public class DateTimeValueStreamWriter: BaseValueStreamWriter
    {
        public DateTime? Value {
            get {
                this.writer.Flush();
                var value = writer.ToString();
                if (string.IsNullOrEmpty(value)) return null;
                DateTime dt;
                if (DateTime.TryParse(value, out dt)) return dt;
                throw new InvalidCastException($"Could not convert {value} to DateTime");
            }
        }
    }
}
