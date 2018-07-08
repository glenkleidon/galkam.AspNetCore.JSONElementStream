using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Galkam.AspNetCore.JsonElementStreaming.Writers

{
    public abstract class BaseValueStreamWriter: IElementStreamWriter, IValueStreamWriter
    {
        protected StringWriter writer = new StringWriter();
        public Stream OutStream {
            get => null;
            set => throw new ArgumentException($"Output streams cannot be assigned to {this.GetType().Name}");
        }
        public IValueStreamWriter TypedValue { get => this; }
        public override string ToString()
        {
            this.writer.Flush();
            if (string.IsNullOrWhiteSpace(writer.ToString())) return null;
            return writer.ToString();
        }
        public dynamic StreamedValue
        {
            get
            {
                if (IsInteger()) return AsInteger();
                if (IsFloat()) return AsFloat();
                if (IsDecimal()) return AsDecimal();
                if (IsDouble()) return AsDecimal();
                if (IsBool()) return AsBool();
                return ToString();
            }
        }

        public abstract Type ValueType { get; }

        public virtual DateTime? AsDateTime()
        {
            return null;
        }
        public virtual bool? AsBool()
        {
            return null;
        }

        public virtual decimal? AsDecimal()
        {
            return null;
        }

        public virtual double? AsDouble()
        {
            return null;
        }

        public virtual dynamic AsDynamic()
        {
            return null;
        }

        public virtual float? AsFloat()
        {
            return null;
        }

        public virtual long? AsInteger()
        {
            return null;
        }

        public virtual string AsString()
        {
            return null;
        }

        public virtual bool IsDateTime()
        {
            return false;
        }

        public virtual bool IsDecimal()
        {
            return false;
        }

        public virtual bool IsDouble()
        {
            return false;
        }

        public virtual bool IsDynamic()
        {
            return false;
        }

        public virtual bool IsFloat()
        {
            return false;
        }

        public virtual bool IsInteger()
        {
            return false;
        }

        public virtual bool IsNumber()
        {
            return false;
        }

        public virtual bool IsString()
        {
            return false;
        }

        public virtual bool IsBool()
        {
            return false;
        }

        public virtual async Task<int> Write(byte[] buffer, int offset, int count)
        {
            var bytes = new byte[count];
            Array.Copy(buffer, offset, bytes, 0, count);
            var charBuffer = System.Text.Encoding.Default.GetString(bytes).ToCharArray();
            return await Write(charBuffer, 0, count);
        }

        public virtual async Task<int> Write(char[] buffer, int offset, int count)
        {
            await writer.WriteAsync(buffer, offset, count);
            return count;
        }

        public virtual async Task<int> WriteString(string text)
        {
            await writer.WriteAsync(text);
            return  text.Length;
        }
    }
}
