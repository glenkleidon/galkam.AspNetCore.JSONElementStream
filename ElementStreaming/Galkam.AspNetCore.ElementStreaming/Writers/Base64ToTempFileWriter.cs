﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Galkam.AspNetCore.ElementStreaming.Writers
{
    public delegate bool ElementStreamingHandler(IElementStreamingRequestContext requestContext);

    /// <summary>
    /// Accepts a Base64 Stream and writes it to a temporary file stream.
    /// </summary>
    public class Base64ToTempFileWriter : Base64StreamWriter
    {
        #region Class properties needed for File 
        private FileStream fileStream;
        public string Basefilepath { get; set; } = Path.GetTempPath();
        public string TemporarySubFolder { get; set; } = "StreamedFiles";
        public string Filename { get; set; }
        public string FileDirectory()
        {
            return Path.Combine(Basefilepath, TemporarySubFolder);
        }
        public string FullFilePath()
        {
            if (string.IsNullOrWhiteSpace(Filename))
            {
                Filename = Path.Combine(Basefilepath, TemporarySubFolder, Path.GetRandomFileName());
            }
            return Filename;
        }
        #endregion

        #region Overrides for IElementStreamWriter interface
        private StringValueStreamWriter writer;
        public override IValueStreamWriter TypedValue
        {
            get
            {
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
