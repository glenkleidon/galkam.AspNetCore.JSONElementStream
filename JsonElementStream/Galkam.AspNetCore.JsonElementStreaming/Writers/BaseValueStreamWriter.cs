using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Galkam.AspNetCore.JsonElementStreaming.Writers

{
    public class BaseValueStreamWriter: IElementStreamWriter
    {
        protected StringWriter writer = new StringWriter();
        public Stream OutStream {
            get => null;
            set => throw new ArgumentException($"Output streams cannot be assigned to {this.GetType().Name}");
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
