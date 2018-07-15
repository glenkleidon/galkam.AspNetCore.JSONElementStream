using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Galkam.AspNetCore.JsonElementStreaming.Writers;
using Newtonsoft.Json;
using System.Linq;

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
        private byte[] chunk;
        private bool goodJson = true;

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

        public bool FlushComplete { get; set; } = false;

        public JsonElementStreamer(Stream sourceStream, Stream outStream, Dictionary<string, IElementStreamWriter> elements)
        {
            this.sourceStream = sourceStream;
            this.outStream = outStream;
            this.elements = elements;
            this.status = Enums.StreamerStatus.None;
        }

        public bool AlwaysStopOnNextData { get; set; } = false;
        public Enums.StreamerStatus Status { get { return status; } }
        public Enums.JsonStatus JsonStatus { get { return jsonStatus.Peek(); }  }
        public int ChunkSize { get; set; } = 5000;
        public string JsonPath { get { return elementStack.Peek(); } }
        public virtual bool IsComplete() {
            return !StreamIsValid || elements.All(e => e.Value.IsComplete);
        }
        /// <summary>
        /// Recursively searches for the next search element
        /// </summary>
        /// <returns></returns>
        private async Task SearchForElements()
        {
            var elementName = await NextElement();
            var endOfChunk = (chunkPosition >= bytesInChunk);
            var bytesRead = 0;
            while (
                !AlwaysStopOnNextData &&
                status != Enums.StreamerStatus.Complete &&
                !elements.ContainsKey(elementName)
            )
            {
                if (endOfChunk)
                {
                    bytesRead = await GetMoreBytesIfNeeded();
                    if (bytesRead == 0) break;
                }

                elementName = await NextElement();
                endOfChunk = (chunkPosition >= bytesInChunk);
                if ((Status == Enums.StreamerStatus.Searching || Status == Enums.StreamerStatus.StartOfData)
                    && IsComplete()) break; // force flushing
            }
        }
        private async Task<Enums.StreamerStatus> ProcessChunk()
        {

            switch (Status)
            {
                case Enums.StreamerStatus.Complete:
                    return Status;
                case Enums.StreamerStatus.None:
                    currentStreamPath = "$";
                    elementStack.Push(currentStreamPath);
                    await SearchForElements(); 
                    break;
                case Enums.StreamerStatus.StartOfData:
                    //Capture to stream if this is a key, otherwise go next.
                    if (elements.ContainsKey(JsonPath))
                    {
                        await StreamData();
                        // was the write complete-ok, recusrively call until complete.
                        while (Status == Enums.StreamerStatus.Streaming)
                        {
                            var bytesRead = await GetMoreBytesIfNeeded();
                            if (bytesRead == 0) throw new EndOfStreamException($"Incomplete data in Element {JsonPath}.");
                            await StreamData();
                        }
                    }
                    else await NextElement(); 
                    break;
                case Enums.StreamerStatus.Searching:
                case Enums.StreamerStatus.EndOfData:
                    await SearchForElements(); 
                    break;
                case Enums.StreamerStatus.Streaming:
                    throw new FormatException($"Unexpectedly encountered Streaming Status in Class {this.GetType().FullName}");
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
        public bool StreamIsValid { get => goodJson; }
        private void BadJson()
        {
            goodJson = false;
            chunkPosition = nextStartPoint;
            throw new FormatException("Invalid Json Sequence");
        }

        private Enums.JsonStatus PopStatus()
        {
            jsonStatus.Pop();
            return (jsonStatus.Count>0) ? jsonStatus.Peek(): Enums.JsonStatus.None;
        }

        private Enums.JsonStatus PushStatus(Enums.JsonStatus status)
        {
            jsonStatus.Push(status);
            return status;
        }
        private string PushArray(Enums.JsonStatus status)
        {
            var currentIndex = (arrayIndex.Count > 0) ? arrayIndex.Pop() : -1;
            arrayIndex.Push(++currentIndex);
            if (status == Enums.JsonStatus.NextArrayElement) status = jsonStatus.Pop();
            var currentpath = (status == Enums.JsonStatus.NextArrayElement) ? elementStack.Pop() : elementStack.Peek();
            var currentElement = arrayName(currentpath, currentIndex);
            elementStack.Push(currentElement);
            return currentElement;
        }

        private async Task NextChunkToStream(int startPoint, int endPoint, IElementStreamWriter writer=null,int endPointForNonIntercept=-1)
        {
            var bytesToWrite = 1+endPoint - startPoint;
            if (writer != null)
            {
                await writer.Write(chunk, startPoint, bytesToWrite);
                // Some writers are not allowed to intercept the data and must always be written.
                if (!writer.Intercept)
                {
                    if (endPointForNonIntercept == -1) endPointForNonIntercept = endPoint;
                    bytesToWrite = 1 + endPointForNonIntercept - startPoint;
                   await outStream.WriteAsync(chunk, startPoint, bytesToWrite);
                }
            }
            else
            {
                //TODO: Write use default writer for the stream.
                await outStream.WriteAsync(chunk, startPoint, bytesToWrite);
            }
        }

        private bool IsNullSequence(int startPoint, int endPoint)
        {
            if (endPoint - startPoint != 3) return false;
            var testNull = new byte[4];
            Array.Copy(chunk, startPoint, testNull,0, 4);
            return System.Text.Encoding.Default.GetString(testNull).Equals("null");
        }

        /// <summary>
        /// Used by NextElement to checks if its time to write out to the alternate stream 
        /// </summary>
        /// <param name="targetStream">alternate data stream to write to.</param>
        /// <param name="startPoint">position in the chunk to start writing from</param>
        /// <param name="endPoint">position in the chunk to stop writing</param>
        /// <returns>True when the alternate data has been written</returns>
        private async Task<bool> AtEndOfDataWrite(IElementStreamWriter writer, int startPoint, int endPoint, int endpointForNonIntercept=-1 )
        {
            if (writer!=null && status == Enums.StreamerStatus.Streaming)
            {
                // we are at the end of the data we want to redirect. So, end the write.
                if (IsNullSequence(startPoint, endPoint))
                {
                    //TODO: change NULL to default stream writer
                    if (endpointForNonIntercept == -1) endpointForNonIntercept = endPoint;
                   await NextChunkToStream(startPoint, endpointForNonIntercept, null);
                }
                else
                {
                   await NextChunkToStream(startPoint, endPoint, writer, endpointForNonIntercept);
                }    
                status = Enums.StreamerStatus.EndOfData;
                writer.IsComplete = true;
                return true;
            }
            else return false;
        }

        private string arrayName(string name, int index)
        {
            var i = name.LastIndexOf('[');
            if (i == -1) i = name.Length;
            return $"{name.Substring(0, i)}[{index}]";
        }



        protected async Task<string> NextElement(IElementStreamWriter writer=null)
        {
            byte b;
            var label = partialLabel;
            partialLabel = "";
            var hasData = false;
            var currentIndex = (arrayIndex.Count>0)?arrayIndex.Peek():-1;
            var startOfLastWhiteSpace = -1; // keeps track of white space after data.
            var elementPath = elementStack.Peek();
            var lastComma = -1;
            var searchCompleted = IsComplete(); // more efficient with local variable.


            var escaping = false;
            Enums.JsonStatus s = (jsonStatus.Count == 0) ? Enums.JsonStatus.None : jsonStatus.Peek();
            nextStartPoint = chunkPosition;

            // If there is no more work, set the chunk position to the end.
            if (status != Enums.StreamerStatus.Streaming && searchCompleted && !AlwaysStopOnNextData)
            {
                chunkPosition = bytesInChunk;
                FlushComplete = true;
                status = Enums.StreamerStatus.Searching;
                await NextChunkToStream(nextStartPoint, bytesInChunk-1, writer);
                return "";
            }
            // process the bytes in the stream;
            while (chunkPosition < bytesInChunk)
            {
                b = chunk[chunkPosition++];
                
                // Check "In Quotes" State
                if (s == Enums.JsonStatus.InLabel)
                {
                    if (b != DQuote || escaping)
                    {
                        label = label + Convert.ToChar(b);
                        escaping = (!escaping && b == BackSlash);
                        continue;
                    }
                }
                if (s == Enums.JsonStatus.InQuotedText)
                {
                    if (b != DQuote || escaping)
                    {
                        escaping = (!escaping && b == BackSlash);
                        continue;
                    }
                }
                // if we hit a terminator, where was the last byte that belonged to the data?
                var lastDataPosition = (startOfLastWhiteSpace > nextStartPoint) ? startOfLastWhiteSpace-1 : chunkPosition - 2;

                //act on the received byte
                switch (b)
                {
                    case DQuote:
                        switch (s)
                        {
                            case Enums.JsonStatus.NextObjectElement:
                                label = "";
                                jsonStatus.Pop();
                                s = PushStatus(Enums.JsonStatus.InLabel);
                                break;
                            case Enums.JsonStatus.InObject:
                                label = "";
                                s = PushStatus(Enums.JsonStatus.InLabel);
                                break;
                            case Enums.JsonStatus.InLabel:
                                if (label.Length == 0) BadJson();
                                elementPath = $"{elementStack.Peek()}.{label}";
                                elementStack.Push(elementPath);
                                jsonStatus.Pop();
                                s = PushStatus(Enums.JsonStatus.EndLabel);
                                break;
                            case Enums.JsonStatus.InQuotedText:
                                hasData = true;
                                if (await AtEndOfDataWrite(writer, nextStartPoint, chunkPosition - 2))
                                {
                                    //Needed to go back 3 positions. We are 1 past the DQuote now (-1), 
                                    //we dont want the Quote in the string (-1) and we need to indicate 
                                    //the last character in the text (-1), so -3 in total.
                                    chunkPosition--; // need to restart at the quote
                                    return "";
                                };
                                if (status == Enums.StreamerStatus.EndOfData)
                                {
                                    // special case:  End of quoted text for data being intercepted.
                                    // need to include the trialing double quote in the output.
                                    status = Enums.StreamerStatus.Searching;
                                }
                                s = PopStatus();
                                break;
                            case Enums.JsonStatus.NextArrayElement:
                            case Enums.JsonStatus.InArray:
                                elementPath = PushArray(s);
                                jsonStatus.Push(Enums.JsonStatus.InData);
                                jsonStatus.Push(Enums.JsonStatus.InQuotedText);
                                await NextChunkToStream(nextStartPoint, chunkPosition - 1, writer);
                                status = Enums.StreamerStatus.StartOfData;
                                return elementPath;
                            case Enums.JsonStatus.StartData:
                                // critcal point - break here
                                jsonStatus.Pop(); // get rid of StartData
                                jsonStatus.Push(Enums.JsonStatus.InData);
                                jsonStatus.Push(Enums.JsonStatus.InQuotedText);
                                await NextChunkToStream(nextStartPoint, chunkPosition - 1, writer,chunkPosition);
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
                        if (s == Enums.JsonStatus.StartData) s = jsonStatus.Pop();
                        switch (s)
                        {
                            case Enums.JsonStatus.StartData:
                            case Enums.JsonStatus.InData:
                            case Enums.JsonStatus.None:
                                break;
                            case Enums.JsonStatus.InArray:
                            case Enums.JsonStatus.NextArrayElement:
                                elementPath=PushArray(s);
                                break;
                            default:
                                BadJson();
                                break;
                        }
                        s = PushStatus(Enums.JsonStatus.InObject);
                        status = Enums.StreamerStatus.StartOfData;
                        await NextChunkToStream(nextStartPoint, chunkPosition - 1, writer);
                        return elementPath;
                    case RightBrace:
                        // unlikley to be at a data end point, but possible.
                        if (await AtEndOfDataWrite(writer, nextStartPoint, lastDataPosition,chunkPosition-2))
                        {
                            chunkPosition--; // in case this is the last character in the stream;
                            return "";
                        }
                        if (s == Enums.JsonStatus.InData || s==Enums.JsonStatus.EndArray)
                        {
                            s = PopStatus();
                            hasData = false;
                        }
                        switch (s)
                        {
                            case Enums.JsonStatus.InObject:
                                break;
                            case Enums.JsonStatus.EndObject:
                                s = jsonStatus.Pop();
                                break;
                            default:
                                BadJson();
                                break;
                        }
                        PopStatus();
                        if (s == Enums.JsonStatus.NextObjectElement) s = PopStatus();
                        s = PushStatus(Enums.JsonStatus.EndObject);
                        elementPath = elementStack.Pop();
                        break;
                    case LeftBracket:
                        currentIndex = -1;
                        arrayIndex.Push(currentIndex);
                        switch (s)
                        {
                            case Enums.JsonStatus.StartData:
                            case Enums.JsonStatus.None:
                            case Enums.JsonStatus.InArray:
                                if (s == Enums.JsonStatus.StartData) jsonStatus.Pop();
                                s = PushStatus(Enums.JsonStatus.InArray);
                                startOfLastWhiteSpace = -1;
                                status = Enums.StreamerStatus.StartOfData;
                                await NextChunkToStream(nextStartPoint, chunkPosition - 1, writer,chunkPosition);
                                return elementPath;
                            default:
                                BadJson();
                                break;
                        };
                        break;
                    case RightBracket:
                        if (jsonStatus.Peek() == Enums.JsonStatus.InData)
                        {
                            hasData = false;
                            jsonStatus.Pop();
                            s = jsonStatus.Peek();
                        }

                        switch (s)
                        {
                            case Enums.JsonStatus.InObject:
                            case Enums.JsonStatus.InArray:
                                break;
                            case Enums.JsonStatus.EndArray:
                            case Enums.JsonStatus.EndObject:
                                s = jsonStatus.Pop();
                                break;
                            default:
                                BadJson();
                                break;
                        }
                        if (await AtEndOfDataWrite(writer, nextStartPoint, lastDataPosition, chunkPosition-2))
                        {
                            chunkPosition--; // need to replay in case this is the end of the stream;
                            return "";
                        }
                        //pop the last indexed element name eg array[2]
                        currentIndex = arrayIndex.Pop();
                        if (currentIndex>-1) elementPath = elementStack.Pop();
                        PopStatus();
                        s = PushStatus(Enums.JsonStatus.EndArray);
                        break;
                    case Comma:
                        lastComma = chunkPosition-1;
                        switch (s)
                        {
                            case Enums.JsonStatus.EndArray:
                            case Enums.JsonStatus.EndObject:
                            case Enums.JsonStatus.InObject:
                                break;
                            case Enums.JsonStatus.InData:
                                if (!hasData) BadJson();
                                break;
                            default:
                                BadJson();
                                break;
                        }
                        s = PopStatus();
                        switch (s)
                        {
                            case Enums.JsonStatus.InArray:
                                s = PushStatus(Enums.JsonStatus.NextArrayElement);
                                break;
                            case Enums.JsonStatus.InObject:
                                elementPath = elementStack.Pop();
                                s = PushStatus(Enums.JsonStatus.NextObjectElement);
                                break;
                            default:
                                BadJson();
                                break;
                        };
                        // theoretically possible to be here in a data write, but unlikley.
                        if (await AtEndOfDataWrite(writer, nextStartPoint, lastDataPosition, chunkPosition-1)) return "";
                        break;
                    default:
                        if (Char.IsWhiteSpace(Convert.ToChar(b)))
                        {
                            if (hasData)
                            {
                                if (await AtEndOfDataWrite(writer, nextStartPoint, lastDataPosition, chunkPosition-2)) return "";
                            }
                        }
                        else
                        {
                            startOfLastWhiteSpace = chunkPosition;
                            switch (s)
                            {
                                case Enums.JsonStatus.StartData:
                                    // we are at the beginning of the next data.
                                    hasData = true;
                                    status = Enums.StreamerStatus.StartOfData;
                                    jsonStatus.Pop();
                                    s = PushStatus(Enums.JsonStatus.InData);
                                    await NextChunkToStream(nextStartPoint, chunkPosition - 2, writer);
                                    chunkPosition--;
                                    return elementPath;
                                case Enums.JsonStatus.InArray:
                                case Enums.JsonStatus.NextArrayElement:
                                    elementPath=PushArray(s);
                                    s = PushStatus(Enums.JsonStatus.InData);
                                    hasData = false;
                                    chunkPosition--;
                                    if (status == Enums.StreamerStatus.EndOfData)
                                    {
                                        status = Enums.StreamerStatus.Searching;
                                    }
                                    else
                                    {
                                        status = Enums.StreamerStatus.StartOfData;
                                        await NextChunkToStream(nextStartPoint, chunkPosition - 1, writer);
                                        return elementPath;
                                    };
                                    break;
                            }
                            hasData = true;
                        }
                        break;
                }
            }
            partialLabel = label;
            await NextChunkToStream(nextStartPoint, chunkPosition-1, writer);
            return "";
        }

        protected virtual async Task<int> GetMoreBytesIfNeeded()
        {
            var bytesRemaining = (chunkPosition==-1) ? 0 : (bytesInChunk - chunkPosition);
            if (bytesRemaining > 0) return 0;
            chunk = new byte[ChunkSize];
            bytesInChunk = await sourceStream.ReadAsync(chunk, 0, ChunkSize);
            chunkPosition = 0;
            return bytesInChunk;
        }

        protected virtual async Task<Enums.StreamerStatus> Flush()
        {
            FlushComplete = true;
            if (chunkPosition == -1)
            {
                chunkPosition = 0;
                var bytesRead = await GetMoreBytesIfNeeded();
                if (bytesRead == 0) chunkPosition = 0;
            }
            var bytesRemaining = (bytesInChunk - chunkPosition);
            while (bytesRemaining > 0 )
            {
                await NextChunkToStream(chunkPosition, bytesRemaining-1);
                chunkPosition = bytesInChunk;
                bytesRemaining = await GetMoreBytesIfNeeded();
            }
            status = Enums.StreamerStatus.Complete;
            return Status;
        }

        public async Task<Enums.StreamerStatus> FlushIfComplete()
        {
            if (!AlwaysStopOnNextData &&
                (Status == Enums.StreamerStatus.Searching || Status == Enums.StreamerStatus.None)
                && IsComplete())
            {
                status = Enums.StreamerStatus.Flushing;
                await Flush();
            }
            return Status;
        }


        public async Task<Enums.StreamerStatus> Next()
        {
            if (chunk == null)
            {
                chunkPosition = -1;
                bytesInChunk = 0;
            }
            await FlushIfComplete();
            if (Status!=Enums.StreamerStatus.Complete) 
            {
                FlushComplete = false;
                var bytesRead = await GetMoreBytesIfNeeded();
                //TODO : ADD CANCELLATION TOKEN for the async read.
                if (bytesInChunk > 0) await ProcessChunk();
                var bytesRemaining = (bytesInChunk - chunkPosition);
                if (bytesRemaining < 1)
                {
                    bytesRead = await GetMoreBytesIfNeeded();
                    if (bytesRead == 0)
                    {
                        status = Enums.StreamerStatus.Complete;
                    }
                    else
                    {
                        await FlushIfComplete();
                    }
                }
            }
            return Status;
        }


    }
}
