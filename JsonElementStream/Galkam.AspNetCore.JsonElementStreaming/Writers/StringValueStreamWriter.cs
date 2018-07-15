using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.JsonElementStreaming.Writers
{
    public class StringValueStreamWriter : BaseValueStreamWriter
    {
        public StringValueStreamWriter() : base()
        {
        }
        public StringValueStreamWriter(bool intercept)
        {
            this.Intercept = intercept;     
        }
        public string Value { get => AsString(); }

        public override Type ValueType => typeof(string);

        public override string AsString()
        {
            if (!hasWrites) return null;
            this.writer.Flush();
            return writer.ToString();
        }
        public override bool IsString()
        {
            return true;
        }
        public override bool CanIntercept => true;
    }
}
