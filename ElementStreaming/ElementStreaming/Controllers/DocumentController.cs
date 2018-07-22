using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElementStreaming.ViewModels;
using Galkam.AspNetCore.ElementStreaming;
using Galkam.AspNetCore.ElementStreaming.ElementStreamingRequestContexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace ElementStreaming.Utilities
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly ElementStreamingRequestContext requestContext;

        public DocumentController(IElementStreamingRequestContextCollection requestContexts)
        {
            requestContext = requestContexts.ActiveContext();
        }
        [Route("upload")]
        [HttpPost]
        public ActionResult Upload([FromBody] UploadRequest request)
        {
            // Check that eh request context was received as expected.
            if (requestContext == null) ModelState.AddModelError("RequestContext", "Expected Element Streaming Context was not found in request");
            if (ModelState.IsValid)
            {
                // this example extracts the values without using a helper to make it easier to see what is going on.
                
                // Check if the file type was accepted.
                var invalidFileType = requestContext.Elements[Constants.DocumentJsonPath]?.Ignore;
                if (invalidFileType == true)
                {
                    return Forbid($"File {request.fileName} is not supported.");
                }

                // Ok it looks like the file was received.  The temporary filename should be in the request now
                // as the document field instead of the base64 data.
                if (request.document!=null && System.IO.File.Exists(request.document))
                {
                    var fileSize = requestContext.Elements[Constants.ByteSizeJsonPath]?.TypedValue.AsInteger();

                    var response = new UploadResponse
                    {
                        BytesReceived = fileSize,
                        Success = true,
                        Location = request.fileName
                    };
                    return Ok(response);
                }
                else
                {
                    // ok something went wrong writing the file.
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new { ErrorMsg = $"The uploaded file {request.fileName} could not be written" }
                    );
                }
            } else return BadRequest(ModelState);
        }
     
    }
}