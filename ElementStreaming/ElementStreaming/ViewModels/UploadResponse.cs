using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElementStreaming.ViewModels
{
    /// <summary>
    /// View model for the response to a document upload request.
    /// </summary>
    public class UploadResponse
    {
        public bool Success { get; set; }
        public long? BytesReceived { get; set; }
        public string Location { get; set; }
    }
}
