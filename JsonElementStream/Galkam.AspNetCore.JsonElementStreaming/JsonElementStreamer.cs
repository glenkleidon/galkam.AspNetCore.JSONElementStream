using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Galkam.AspNetCore.JsonElementStreaming
{
    /// <summary>
    /// Accepts a Stream which will extract JSON elements into a stream 
    /// </summary>
    public class JsonElementStreamer 
    {
        private readonly Stream sourceStream;
        private readonly Stream outStream;
        private readonly Dictionary<string, IElementStreamWriter> elements;

        private string currentStreamPath = String.Empty;
        private Stack<string> elementStack = new Stack<string>();
        private int chunkPosition = 0;

        public JsonElementStreamer(Stream sourceStream, Stream outStream, Dictionary<string, IElementStreamWriter> elements)
        {
            this.sourceStream = sourceStream;
            this.outStream = outStream;
            this.elements = elements;
        }

        public Enums.StreamerStatus Status { get; } = Enums.StreamerStatus.None;
        public int ChunkSize { get; set; } = 5000;
        private Enums.StreamerStatus ProcessChunk(byte[] buffer)
        {
            switch (Status)
            {
                case Enums.StreamerStatus.Complete:
                    return Status;
                case Enums.StreamerStatus.None:
                    currentStreamPath = "$";
                    elementStack.Push(currentStreamPath);
                    NextElement();
                    break;
                case Enums.StreamerStatus.Searching:
                    break;
                case Enums.StreamerStatus.Streaming:
                    break;
            }
            return Status;
        }

        private string NextElement()
        {
            var elementPath = elementStack.Peek();
            return "";
        }

        public async Task<Enums.StreamerStatus> Next()
        {
            var buffer = new Byte[ChunkSize];
            var bytesRead = await sourceStream.ReadAsync(buffer,0, ChunkSize);
            return Status;
        }


    }
}
