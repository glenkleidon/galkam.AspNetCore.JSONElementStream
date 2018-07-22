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
        private readonly HttpContext httpContext;

        public DocumentController(IElementStreamingRequestContextCollection requestContexts, IHttpContextAccessor httpContext)
        {
            requestContext = requestContexts.ActiveContext();
            this.httpContext = httpContext.HttpContext;
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
                var invalidFileType = requestContext.GetElement(Constants.DocumentJsonPath)?.Ignore;
                if (invalidFileType == true)
                {
                    var errorMsg = $"File {request.fileName} is not supported.";
                    if (request.document == null) errorMsg= "Document cannot be null";
                    return BadRequest(errorMsg);
                }

                // Ok it looks like the file was received.  The temporary filename should be in the request now
                // as the document field instead of the base64 data.
                if (request.document != null && System.IO.File.Exists(request.document))
                {

                    var fileSize = requestContext.GetElement(Constants.ByteSizeJsonPath)?.TypedValue.AsInteger();

                    // rename the file.
                    var extn = Path.GetExtension(request.fileName);
                    var storeFilename = Path.ChangeExtension(request.document, extn);
                    try
                    {
                        System.IO.File.Move(request.document, storeFilename);

                        var uploadPath = httpContext.Request.Path.ToString().Replace("upload", "download",
                            StringComparison.CurrentCultureIgnoreCase);

                        var returnPath = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{uploadPath}/{Path.GetFileName(storeFilename)}";

                        var response = new UploadResponse
                        {
                            BytesReceived = fileSize,
                            Success = true,
                            Location = returnPath
                        };
                        return Ok(response);
                    }
                    catch (Exception e)
                    {
                        return StatusCode(500, e);
                    }
                }
                else
                {
                    // ok something went wrong writing the file.
                    var ErrorMsg = $"The uploaded file {request.fileName} could not be written";
                    return StatusCode(StatusCodes.Status500InternalServerError, ErrorMsg);
                }
            }
            else return BadRequest(ModelState);
        }
    }
}