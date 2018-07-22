using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.ElementStreaming.Writers
{
    public class StringValueStreamWriter : BaseValueStreamWriter
    {
        private string defaultValue;
        public StringValueStreamWriter() : base()
        {
        }
        public StringValueStreamWriter(bool intercept)
        {
            this.Intercept = intercept;     
        }
        public StringValueStreamWriter(string valueIfEmpty, bool intercept=false): base()
        {
            this.defaultValue = valueIfEmpty;
            this.Intercept = intercept;
        }
        public string Value => AsString();

        public override Type ValueType => typeof(string);

        public override string AsString()
        {
            if (!hasWrites)
            {
                return (defaultValue != null) ? defaultValue: null;
            }
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
