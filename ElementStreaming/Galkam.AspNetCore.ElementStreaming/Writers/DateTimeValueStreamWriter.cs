using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.ElementStreaming.Writers
{
    public class DateTimeValueStreamWriter: BaseValueStreamWriter
    {
        public override Type ValueType => typeof(DateTime);

        public DateTime? Value { get => AsDateTime(); }

        public override DateTime? AsDateTime() {
            this.writer.Flush();
            var value = writer.ToString();
            if (string.IsNullOrEmpty(value)) return null;
            DateTime dt;
            if (DateTime.TryParse(value, out dt)) return dt;
            throw new InvalidCastException($"Could not convert {value} to DateTime");
        }
        public override bool IsDateTime() => true;
    }
}
