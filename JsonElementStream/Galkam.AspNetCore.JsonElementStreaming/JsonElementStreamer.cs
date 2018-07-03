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
        private const byte Colon = 0x3A;
        private const byte LeftBrace = 0x7B;
        private const byte RightBrace = 0x7D;
        private const byte LeftBracket = 0x5B;
        private const byte RightBracket = 0x5D;
        private const byte Comma = 0x2C;
        private const byte BackSlash = 0x5C;


        private Enums.StreamerStatus status;

        public JsonElementStreamer(Stream sourceStream, Stream outStream, Dictionary<string, IElementStreamWriter> elements)
        {
            this.sourceStream = sourceStream;
            this.outStream = outStream;
            this.elements = elements;
            this.status = Enums.StreamerStatus.None;
        }

        public Enums.StreamerStatus Status { get { return status; } }
        public Enums.JsonStatus JsonStatus { get { return jsonStatus.Peek(); }  }
        public int ChunkSize { get; set; } = 5000;
        public string JsonPath { get { return elementStack.Peek(); } }
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
                case Enums.StreamerStatus.StartOfData:
                    //Capture to stream if this is a key, otherwise go next.
                    if (elements.ContainsKey(JsonPath))
                    {
                        throw new NotImplementedException();
                    }
                    else NextElement();
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

        private Enums.JsonStatus PopStatus()
        {
            jsonStatus.Pop();
            return jsonStatus.Peek();
        }

        private Enums.JsonStatus PushStatus(Enums.JsonStatus status)
        {
            jsonStatus.Push(status);
            return status;
        }


        private string NextElement()
        {
            var elementPath = elementStack.Peek();
            byte b;
            var label = partialLabel;
            partialLabel = "";
            var hasData = false;
            var currentIndex = -1;
            var escaping = false;
            Enums.JsonStatus s = (jsonStatus.Count == 0) ? Enums.JsonStatus.None : jsonStatus.Peek();
            while (chunkPosition < ChunkSize - 1)
            {
                b = chunk[chunkPosition++];
                
                // Check "In Quotes" State
                if (s == Enums.JsonStatus.InLabel)
                {
                    if (b != DQuote)
                    {
                        label = label + Convert.ToChar(b);
                        escaping = (b == BackSlash);
                        continue;
                    }
                }
                if (s == Enums.JsonStatus.InQuotedText && b != DQuote) 
                {
                    escaping = (b == BackSlash);
                    continue;
                }

                //act on the received byte
                switch (b)
                {
                    case DQuote:
                        switch (s)
                        {
                            case Enums.JsonStatus.NextObjectElement:
                            case Enums.JsonStatus.InObject:
                                label = "";
                                s = PushStatus(Enums.JsonStatus.InLabel);
                                break;
                            case Enums.JsonStatus.InLabel:
                                if (escaping)
                                {
                                    label = label + Convert.ToChar(b);
                                    escaping = false;
                                    continue;
                                }
                                if (label.Length == 0) BadJson();
                                elementPath = elementPath + '.' + label;
                                elementStack.Push(elementPath);
                                jsonStatus.Pop();
                                s = PushStatus(Enums.JsonStatus.EndLabel);
                                break;
                            case Enums.JsonStatus.InQuotedText:
                                if (!escaping) s = PopStatus();
                                escaping = false;
                                break;
                            case Enums.JsonStatus.NextArrayElement:
                            case Enums.JsonStatus.StartData:
                                // critcal point - break here
                                jsonStatus.Pop(); // get rid of StartData
                                jsonStatus.Push(Enums.JsonStatus.InData);
                                jsonStatus.Push(Enums.JsonStatus.InQuotedText);
                                status = Enums.StreamerStatus.StartOfData;
                                return elementPath;
                            default:
                                BadJson();
                                break;
                        }
                        break;
                    case Colon:
                        if (s != Enums.JsonStatus.EndLabel) BadJson();
                        jsonStatus.Pop();
                        s = PushStatus(Enums.JsonStatus.StartData);
                        break;
                    case LeftBrace:
                        switch (s)
                        {
                            case Enums.JsonStatus.StartData:
                            case Enums.JsonStatus.None:
                            case Enums.JsonStatus.InArray:
                            case Enums.JsonStatus.NextArrayElement:
                                s = PushStatus(Enums.JsonStatus.InObject);
                                break;
                            default:
                                BadJson();
                                break;
                        }
                        break;
                    case RightBrace:
                        if (s != Enums.JsonStatus.InObject) BadJson();
                        s = PopStatus();
                        if (s == Enums.JsonStatus.NextObjectElement) s = PopStatus();
                        break;
                    case LeftBracket:
                        switch (s)
                        {
                            case Enums.JsonStatus.StartData:
                            case Enums.JsonStatus.None:
                            case Enums.JsonStatus.InArray:
                                s = PushStatus(Enums.JsonStatus.InArray);
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
                        s = PopStatus();
                        if (s == Enums.JsonStatus.NextArrayElement) s = PopStatus();
                        currentIndex = arrayIndex.Pop();
                        break;
                    case Comma:
                        if (s != Enums.JsonStatus.InData || !hasData) BadJson();
                        s = PopStatus();
                        switch (s)
                        {
                            case Enums.JsonStatus.InArray:
                                s = PushStatus(Enums.JsonStatus.NextArrayElement);
                                currentIndex++;
                                break;
                            case Enums.JsonStatus.InObject:
                                s = PushStatus(Enums.JsonStatus.NextObjectElement);
                                elementStack.Pop();
                                elementPath = elementStack.Peek();
                                break;
                            default:
                                BadJson();
                                break;
                        };
                        break;
                    default:
                        if (Char.IsWhiteSpace(Convert.ToChar(b))) continue;
                        if (s == Enums.JsonStatus.StartData)
                        {
                            // we are at the beginning of the next data.
                            hasData = true;
                            status = Enums.StreamerStatus.StartOfData;
                            jsonStatus.Pop();
                            s = PushStatus(Enums.JsonStatus.InData);
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
            var newChunk = new byte[ChunkSize];
            if (chunk == null) chunk = new byte[ChunkSize];

            //Copy the remaining bytes from the exisitng chunk into the new chunk
            var bytesRemaining = (bytesInChunk - chunkPosition - 1);
            if (chunkPosition >= 0)
            {
                if (bytesRemaining > 0)
                {
                    Array.Copy(chunk, chunkPosition, newChunk, 0, bytesRemaining);
                    chunk = newChunk;
                    bytesInChunk = bytesRemaining;
                }
            }
            else
            {
                bytesRemaining = 0;
            }
            chunkPosition = 0;
            var maxBytesToRead = ChunkSize - bytesRemaining;
            var chunkSize = await sourceStream.ReadAsync(chunk,ChunkSize-maxBytesToRead, maxBytesToRead);
            bytesInChunk = bytesInChunk + chunkSize;
            ProcessChunk();
            bytesRemaining = (bytesInChunk - chunkPosition - 1);
            if (chunkSize == 0 && bytesRemaining<1 ) status = Enums.StreamerStatus.Complete;

            return Status;
        }


    }
}
