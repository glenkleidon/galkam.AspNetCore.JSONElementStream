using System.IO;
using System.Threading.Tasks;

namespace Galkam.AspNetCore.ElementStreaming
{
    public interface IElementStreamer
    {
        bool AlwaysStopOnNextData { get; set; }
        int ChunkSize { get; set; }
        bool FlushComplete { get; set; }
        string ElementPath { get; }
        Enums.StreamerStatus Status { get; }
        bool StreamIsValid { get; }
        Task<Enums.StreamerStatus> FlushIfComplete();
        bool IsComplete();
        Task<Enums.StreamerStatus> Next();
        void Continue();
        Task StartOfElementData();
        Task EndOfData();
        Task WriteAlternateContent(string content);
        Stream OutStream { get; set; }
        Stream SourceStream { get; set; }
        
        
    }
}