using System;
using System.IO;
using System.Threading.Tasks;

namespace Galkam.AspNetCore.ElementStreaming.Writers
{
    public interface IElementStreamWriter : IDisposable
    {
        Task<int> Write(byte[] buffer, int offset, int count);
        Task<int> Write(char[] buffer, int offset, int count);
        Task<int> WriteString(string text);
        Stream OutStream { get; set; }
        IValueStreamWriter TypedValue { get; }
        bool CanIntercept { get; }
        bool Intercept { get; set; }
        bool IsComplete { get; set; }
        bool Ignore { get; set; }
     }
}