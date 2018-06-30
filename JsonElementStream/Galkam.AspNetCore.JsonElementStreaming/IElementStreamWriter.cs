using System.IO;
using System.Threading.Tasks;

namespace Galkam.AspNetCore.JsonElementStreaming
{
    public interface IElementStreamWriter
    {
        Task<int> Write(byte[] buffer, int offset, int count);
        Task<int> Write(char[] buffer, int offset, int count);
        Task<int> WriteString(string text);
        Stream OutStream { get; set; }
    }
}