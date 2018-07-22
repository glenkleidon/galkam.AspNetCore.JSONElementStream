using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ElementStreaming.ViewModels
{
    public class UploadRequest
    {
        public string category { get; set; }
        [Required]
        public string fileName { get; set; }
        [Required]
        public string fileType { get; set; }
        public string document { get; set; }
    }
}
