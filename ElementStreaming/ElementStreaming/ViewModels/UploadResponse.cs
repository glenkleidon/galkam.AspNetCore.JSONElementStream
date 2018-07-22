using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElementStreaming.ViewModels
{
    public class UploadResponse
    {
        public long? BytesReceived { get; set; }
        public bool Success { get; set; }
        public string Location { get; set; }
    }
}
