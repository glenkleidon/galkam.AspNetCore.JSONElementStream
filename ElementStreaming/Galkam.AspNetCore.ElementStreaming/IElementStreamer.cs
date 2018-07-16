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
        Task StartOfElementData();
        Task EndOfData();
        
    }
}