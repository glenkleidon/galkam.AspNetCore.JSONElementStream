using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Galkam.AspNetCore.ElementStreaming.Writers
{
    public delegate bool ElementStreamingHandler(IElementStreamingRequestContextCollection requestContext);

    /// <summary>
    /// Accepts a Base64 Stream and writes it to a temporary file stream.
    /// </summary>
    public class Base64ToTempFileWriter : Base64StreamWriter
    {
        #region Class properties needed for File 
        private FileStream fileStream;
        private string filePrefix;
        public string Basefilepath { get; set; } = Path.GetTempPath();
        public string TemporarySubFolder { get; set; } = "StreamedFiles";
        public string Filename { get; set; }
        public string FileDirectory()
        {
            return Path.Combine(Basefilepath, TemporarySubFolder);
        }
        public string FullFilePath()
        {
            var fname = (string.IsNullOrWhiteSpace(Filename)) ? Path.GetRandomFileName() : Filename;
            return Path.Combine(Basefilepath, TemporarySubFolder,fname);
        }
        #endregion

        #region Overrides for IElementStreamWriter interface
        private StringValueStreamWriter writer;
        public override IValueStreamWriter TypedValue
        {
            get
            {
                if (Filename == null) return null;
                if (writer == null || !writer.Value.Equals(FullFilePath()))
                {
                    writer = new StringValueStreamWriter(FullFilePath());
                }
                return writer;
            }
        }
        public override Stream OutStream
        {
            get
            {
                if (fileStream == null)
                {
                    if (!Directory.Exists(FileDirectory())) Directory.CreateDirectory(FileDirectory());
                    fileStream = new FileStream(FullFilePath(), FileMode.CreateNew, FileAccess.Write, FileShare.Read);
                    ownsOutStream = true;
                }
                return fileStream;
            }
            set
            {
                ReleaseStream();
                if (fileStream.GetType().IsSubclassOf(typeof(FileStream))) fileStream = (FileStream)value;
            }
        }
        public override void ReleaseStream()
        {
            if (ownsOutStream)
            {
                ownsOutStream = false;
                fileStream.Dispose();
            }
            writer.Dispose();
            disposed = true;
        }
        #endregion

    }

}
