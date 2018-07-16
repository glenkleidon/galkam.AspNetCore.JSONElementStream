using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Galkam.AspNetCore.ElementStreaming.Writers
{
    /// <summary>
    /// A binary writer for decoding base64 data to a stream.
    /// </summary>
    public class Base64StreamWriter : IElementStreamWriter
    {
        private Stream outStream;
        private string unwritten = "";

        public Stream OutStream { get => outStream; set => value = outStream; }

        public IValueStreamWriter TypedValue => throw new NotImplementedException();

        public virtual bool CanIntercept => true;

        public virtual bool Intercept { get; set; } = true;
        public bool IsComplete { get; set; } = false;

        public Base64StreamWriter()
        {
            this.outStream = outStream = new MemoryStream();
        }
        public Base64StreamWriter(Stream outStream) 
        {
            this.outStream = outStream;
        }
        /// <summary>
        /// Accepts Base64 String data converts it to the binary stream
        /// </summary>
        /// <param name="text">A Snippet of Base64 text to be written to the stream</param>
        /// <returns>The number of characters consumed by the conversion (which may NOT be the whole string)</returns>
        public async Task<int> WriteString(string text)
        {
            var textToWrite = unwritten + text;
            var charsToWrite = 4*((int) (textToWrite.Length/4));
            var newChars = Convert.FromBase64String(textToWrite.Substring(0, charsToWrite));
            unwritten = (charsToWrite < textToWrite.Length) ? textToWrite.Substring(charsToWrite) : "";
            await outStream.WriteAsync(newChars,0, newChars.Length);
            return charsToWrite;
        }
        /// <summary>
        /// Accepts Base64 data encoded as a byte array and writes it to the binary stream.  Note: calls Write(Char[]) internally
        /// </summary>
        /// <param name="buffer">Array of bytes representing the base64 data</param>
        /// <param name="offset">Zero based element position to start encoding </param>
        /// <param name="count">The Number of bytes to decode</param>
        /// <returns>The number of bytes consumed by the conversion (which may not be the whole array)</returns>
        public async Task<int> Write(byte[] buffer, int offset, int count)
        {
            var b64chars = System.Text.Encoding.UTF8.GetString(buffer).ToCharArray(offset, count);
            var charsWritten = await Write(b64chars, 0, b64chars.Length);
            return charsWritten;
        }
        /// <summary>
        /// Accepts Base64 data encoded as a char array and writes it to the binary stream.
        /// </summary>
        /// <param name="buffer">Array of char representing the base64 data</param>
        /// <param name="offset">Zero based element position to start encoding</param>
        /// <param name="count">The Number of chars to decode</param>
        /// <returns>The number of bytes consumed by the conversion (which may not be the whole array)</returns>
        public async Task<int> Write(char[] buffer, int offset, int count)
        {
            // must write in multiples of 4.  We need to add any unwritten buffer to the front
            var charsToWrite = 4*((int)((count + unwritten.Length) / 4));
            var newChars = new char[charsToWrite];
            var newDataStartPoint = unwritten.Length;
            var newCount = charsToWrite - unwritten.Length;
            if (unwritten.Length > 0)
            {
                Array.Copy(unwritten.ToCharArray(), 0, newChars, 0, unwritten.Length);
            };
            Array.Copy(buffer, 0, newChars, newDataStartPoint, newCount);
            var newBytes = Convert.FromBase64CharArray(newChars, 0, charsToWrite);

            //keep the unwritten part for next time.
            var unwrittenSize = (count - newCount);
            if (unwrittenSize>0)
            {
                unwritten = new string(buffer, count-unwrittenSize, unwrittenSize);
            }
            await outStream.WriteAsync(newBytes, 0, newBytes.Length);
            return charsToWrite;
        }
    }
}
