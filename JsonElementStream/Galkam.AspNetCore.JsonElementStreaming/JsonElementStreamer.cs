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
        private int bytesInChunk = 0;
        private byte[] chunk;

        private Enums.StreamerStatus status;

        public JsonElementStreamer(Stream sourceStream, Stream outStream, Dictionary<string, IElementStreamWriter> elements)
        {
            this.sourceStream = sourceStream;
            this.outStream = outStream;
            this.elements = elements;
            this.status = Enums.StreamerStatus.None;
        }

        public Enums.StreamerStatus Status { get { return status; } }; 
        public int ChunkSize { get; set; } = 5000;
        private Enums.StreamerStatus ProcessChunk()
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
            var newChunk = new Byte[ChunkSize];
            var bytesRemaining = (ChunkSize - chunkPosition - 1);
            if (bytesRemaining>0) Array.Copy(chunk, chunkPosition, newChunk, 0, bytesRemaining);
            var maxBytesToRead = ChunkSize - bytesRemaining;
            var chunkSize = await sourceStream.ReadAsync(chunk,ChunkSize-maxBytesToRead, maxBytesToRead);
            ProcessChunk();
            if (chunkSize == 0) status = Enums.StreamerStatus.Complete;

            return Status;
        }


    }
}
