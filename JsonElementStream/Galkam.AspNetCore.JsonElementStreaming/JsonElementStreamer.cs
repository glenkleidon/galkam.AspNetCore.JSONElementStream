﻿using System;
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
        private int nextStartPoint = 0;
        private int dataStartPoint = 0;
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
        private async Task<Enums.StreamerStatus> ProcessChunk()
        {
            switch (Status)
            {
                case Enums.StreamerStatus.Complete:
                    return Status;
                case Enums.StreamerStatus.None:
                    currentStreamPath = "$";
                    elementStack.Push(currentStreamPath);
                    await NextElement();
                    break;
                case Enums.StreamerStatus.StartOfData:
                    //Capture to stream if this is a key, otherwise go next.
                    if (elements.ContainsKey(JsonPath))
                    {
                        await StreamData();
                        // was the write complete-ok, recusrively call until complete.
                        if (status == Enums.StreamerStatus.Streaming) await Next();
                    }
                    else await NextElement();
                    break;
                case Enums.StreamerStatus.Searching:
                    break;
                case Enums.StreamerStatus.Streaming:
                    break;

            }
            return Status;
        }
        private async Task StreamData()
        {
            status = Enums.StreamerStatus.Streaming;
            var streamTo = elements[JsonPath];
            if (streamTo == null) throw new ArgumentNullException($"Element {JsonPath} does not have an assigned Stream Writer");
            await NextElement(streamTo);
            //now locate the end of the data.
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

        private async Task NextChunkToStream(int startPoint, int endPoint, IElementStreamWriter writer=null)
        {
            if (writer != null)
            {
                await writer.Write(chunk, startPoint, endPoint - nextStartPoint);
            }
            else
            {
                await outStream.WriteAsync(chunk, startPoint, endPoint - nextStartPoint);
            }
        }

        /// <summary>
        /// Used by NextElement to checks if its time to write out to the alternate stream 
        /// </summary>
        /// <param name="targetStream">alternate data stream to write to.</param>
        /// <param name="startPoint">position in the chunk to start writing from</param>
        /// <param name="endPoint">position in the chunk to stop writing</param>
        /// <returns>True when the alternate data has been written</returns>
        private async Task<bool> AtEndOfDataWrite(IElementStreamWriter writer, int startPoint, int endPoint)
        {
            if (writer!=null && status == Enums.StreamerStatus.Streaming)
            {
                // we are at the end of the data we want to redirect. So, end the write.
                await NextChunkToStream(startPoint, endPoint, writer);
                status = Enums.StreamerStatus.EndOfData;
                return true;
            }
            else return false;
        }


        private async Task<string> NextElement(IElementStreamWriter writer=null)
        {
            byte b;
            var label = partialLabel;
            partialLabel = "";
            var hasData = false;
            var currentIndex = -1;
            var startOfLastWhiteSpace = -1; // keeps track of white space after data.
            var elementPath = elementStack.Peek();

            var escaping = false;
            Enums.JsonStatus s = (jsonStatus.Count == 0) ? Enums.JsonStatus.None : jsonStatus.Peek();
            nextStartPoint = chunkPosition;

            // process the bytes in the stream;
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
                var endpoint = (startOfLastWhiteSpace > nextStartPoint) ? startOfLastWhiteSpace : chunkPosition - 1;
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
                                if (await AtEndOfDataWrite(writer, nextStartPoint, chunkPosition - 2)) return "";
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
                        startOfLastWhiteSpace = -1;
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
                        if (await AtEndOfDataWrite(writer, nextStartPoint, chunkPosition - 2)) return "";
                        // unlikley to be at a data end point, but possible.
                        if (await AtEndOfDataWrite(writer, nextStartPoint, endpoint)) return "";
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
                                startOfLastWhiteSpace = -1;
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
                        if (await AtEndOfDataWrite(writer, nextStartPoint, endpoint)) return "";
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
                        // theoretically possible to be here in a data write, but unlikley.
                        if (await AtEndOfDataWrite(writer, nextStartPoint, endpoint)) return "";
                        break;
                    default:
                        if (Char.IsWhiteSpace(Convert.ToChar(b))) continue;
                        startOfLastWhiteSpace = chunkPosition + 1;
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
            await NextChunkToStream(nextStartPoint, chunkPosition, writer);
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
            //TODO : ADD CANCELLATION TOKEN for the async read.
            var chunkSize = await sourceStream.ReadAsync(chunk,ChunkSize-maxBytesToRead, maxBytesToRead);
            bytesInChunk = bytesInChunk + chunkSize;
            if (bytesInChunk>0) await ProcessChunk();
            bytesRemaining = (bytesInChunk - chunkPosition - 1);
            if (chunkSize == 0 && bytesRemaining<1 ) status = Enums.StreamerStatus.Complete;

            return Status;
        }


    }
}
