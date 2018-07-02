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
        private string partialLabel = "";
        private Stack<Enums.JsonStatus> jsonStatus = new Stack<Enums.JsonStatus>();
        private Stack<int> arrayIndex = new Stack<int>();

        private int chunkPosition = -1;
        private int bytesInChunk = 0;
        private byte[] chunk;

        //json Elements
        private const byte DQuote = 0x22;
        private const byte Colon = 0xA3;
        private const byte LeftBrace = 0xB7;
        private const byte RightBrace = 0xD7;
        private const byte LeftBracket = 0xB5;
        private const byte RightBracket = 0xD5;
        private const byte Comma = 0xc2;


        private Enums.StreamerStatus status;

        public JsonElementStreamer(Stream sourceStream, Stream outStream, Dictionary<string, IElementStreamWriter> elements)
        {
            this.sourceStream = sourceStream;
            this.outStream = outStream;
            this.elements = elements;
            this.status = Enums.StreamerStatus.None;
        }

        public Enums.StreamerStatus Status { get { return status; } }
        public int ChunkSize { get; set; } = 5000;
        public string JsonPath { get { return currentStreamPath; } }
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
        private void BadJson()
        {
            throw new ArgumentOutOfRangeException("Invalid Json Sequence");
        }

        private string NextElement()
        {
            var elementPath = elementStack.Peek();
            byte b;
            var label = partialLabel;
            partialLabel = "";
            var hasData = false;
            var currentIndex = -1;
            Enums.JsonStatus s = (jsonStatus.Count == 0) ? Enums.JsonStatus.None : jsonStatus.Peek();
            while (chunkPosition < ChunkSize - 1)
            {
                b = chunk[chunkPosition++];
                if (b != DQuote)
                {
                    if (s == Enums.JsonStatus.InLabel)
                    {
                        label = label + Convert.ToChar(b);
                        continue;
                    }
                    else if (s == Enums.JsonStatus.InQuotedText) continue;
                }
                switch (b)
                {
                    case DQuote:
                        switch (s)
                        {
                            case Enums.JsonStatus.InObject:
                                label = "";
                                s = Enums.JsonStatus.InLabel;
                                jsonStatus.Push(s);
                                break;
                            case Enums.JsonStatus.InLabel:
                                if (label.Length == 0) BadJson();
                                elementPath = elementPath + '.' + label;
                                elementStack.Push(elementPath);
                                s = jsonStatus.Pop();
                                break;
                            case Enums.JsonStatus.InQuotedText:
                                s = jsonStatus.Pop();
                                break;
                            case Enums.JsonStatus.NextArrayElement:
                            case Enums.JsonStatus.StartData:
                                s = Enums.JsonStatus.InQuotedText;
                                jsonStatus.Push(s);
                                // critcal point - break here
                                return elementPath;
                            default:
                                BadJson();
                                break;
                        }
                        break;
                    case Colon:
                        if (s != Enums.JsonStatus.EndLabel) BadJson();
                        s = Enums.JsonStatus.StartData;
                        jsonStatus.Push(s);
                        break;
                    case LeftBrace:
                        switch (s)
                        {
                            case Enums.JsonStatus.StartData:
                            case Enums.JsonStatus.None:
                            case Enums.JsonStatus.InArray:
                            case Enums.JsonStatus.NextArrayElement:
                                s = Enums.JsonStatus.InObject;
                                jsonStatus.Push(s);
                                break;
                            default:
                                BadJson();
                                break;
                        }
                        break;
                    case RightBrace:
                        if (s != Enums.JsonStatus.InObject) BadJson();
                        s = jsonStatus.Pop();
                        if (s == Enums.JsonStatus.NextObjectElement) s = jsonStatus.Pop();
                        break;
                    case LeftBracket:
                        switch (s)
                        {
                            case Enums.JsonStatus.StartData:
                            case Enums.JsonStatus.None:
                            case Enums.JsonStatus.InArray:
                                s = Enums.JsonStatus.InArray;
                                jsonStatus.Push(s);
                                arrayIndex.Push(currentIndex);
                                currentIndex = -1;
                                break;
                            default:
                                BadJson();
                                break;
                        };
                        break;
                    case RightBracket:
                        if (s != Enums.JsonStatus.InArray) BadJson();
                        s = jsonStatus.Pop();
                        if (s == Enums.JsonStatus.NextArrayElement) s = jsonStatus.Pop();
                        currentIndex = arrayIndex.Pop();
                        break;
                    case Comma:
                        if (s != Enums.JsonStatus.InData || !hasData) BadJson();
                        s = jsonStatus.Pop();
                        switch (s)
                        {
                            case Enums.JsonStatus.InArray:
                                s = Enums.JsonStatus.NextArrayElement;
                                jsonStatus.Push(s);
                                currentIndex++;
                                break;
                            case Enums.JsonStatus.InObject:
                                jsonStatus.Push(Enums.JsonStatus.NextObjectElement);
                                break;
                            default:
                                BadJson();
                                break;
                        };
                        break;
                    default:
                        if (s == Enums.JsonStatus.StartData)
                        {
                            if (Char.IsWhiteSpace(Convert.ToChar(b))) continue;
                            // we are at the beginning of the next data.
                            status = Enums.StreamerStatus.StartOfData;
                            return elementPath;
                        }
                        hasData = true;
                        break;
                }
            }
        partialLabel = label; 
            return "";
        }

        public async Task<Enums.StreamerStatus> Next()
        {
            var newChunk = new Byte[ChunkSize];
            var bytesRemaining = (ChunkSize - chunkPosition - 1);
            if (chunkPosition >= 0)
            {
                if (bytesRemaining > 0) Array.Copy(chunk, chunkPosition, newChunk, 0, bytesRemaining);
            }
            else
            {
                bytesRemaining = 0;
            }
            chunkPosition = 0;
            var maxBytesToRead = ChunkSize - bytesRemaining;
            var chunkSize = await sourceStream.ReadAsync(chunk,ChunkSize-maxBytesToRead, maxBytesToRead);
            ProcessChunk();
            if (chunkSize == 0) status = Enums.StreamerStatus.Complete;

            return Status;
        }


    }
}
