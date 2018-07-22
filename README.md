# _ASPNetCore JSON Elements stream_
------------------------------------

## Purpose:
_*Galkam.ASPNetCore.ElementStreaming*_ is an ASPNetCore Web Host middleware that allows large json objects to be intercepted in the request body and streamed directly to an alternate stream and retreived in the controller in a context object.

The primary purose is to minimise large objects building up in memory.  

## Reducing the load on your service:
Imagine you have to accept documents from a client sending base64 encoded files as a json element.
```
{
    "Category":"Image",
    "filename":"testpic.png",
    "fileType":"PNG",
    "document":"iVBORw0KGgoAAAANSUhEUgAAAE0A...<20MB>=="
}
```
The View model for this Json is: 
```
 public class UploadRequest
    {
        public string category { get; set; }
        public string fileName { get; set; }
        public string fileType { get; set; }
        public string document { get; set; }
    }
```

Without any middleware to manage this, a 20MB document would be parsed by the body parser and passed into your controller as a string on the view model.

Because it is base64 encoded, the 20MB document becomes (4/3 * 20MB) = 26.7MB.

Now in order to convert the base64 data back into a binary stream so that it can be written out to a file or stored in the database, you would need to convert the result in your controller or service layer to an Array of bytes and then to a FileStream or local memory stream.  
```
   var bytes = Convert.FromBase64String(request.document);
   var memstream = new MemoryStream(bytes)
   // now we can post
```

This seems pretty simple right?  But take a moment to think of what you are holding in memory. You have 26MB String, 20MB byte array, and a 20MB memory stream.  Depending on how clever the compiler has been, you are likely to have more than 3 times the data in memory that is required for the object.  This does not take into account any work that the body parser may have done to create the string object.

You can see that very quickly as the number of users build, memory will become a bottleneck in your service.

## How does the middleware help?

The ElementStreaming middleware is told to watch out for a json element called _*"document"*_ coming in to the particular endpoint say _/api/document/upload_.  The middleware can be asked to intercept the base64 data directly into binary bytes as they arrive. 

As bytes arrive in the request body, the middleware will decode the base64 data in small chunks so that the overall memory required is just the original 20MB when it eventually arrives on the controller.  

Taking this one step further, by descending the middleware _Base64StremWriter_ class, it is possible to redirect the binary bytes directly to a file.  In fact, release 1 includes a generic implementation called _Base64ToTempFileWriter_ which redirects base64 encoded data to a file in the service user's _**TEMP**_ folder. This reducing the overall in-memory load down to a few hundred bytes.  Instead of the bytes arriving on the controller, the _**document**_ element content is transparently replaced with the file path and the resultant json object becomes:

```
{
    "Category":"Image",
    "filename":"testpic.png",
    "fileType":"PNG",
    "document":"c:\\users\\svcuser\\local\\temp\\streamedfiles\\abc3232.rX3"
}
```

## What data can be read?
In release 1, there is one type of _**Streamer**_ available for handling json POST requests. This is the _**JsonElementStreamer**_ class which implements the _**IElementStreamer**_ Interface.

The _IElementStreamer_ interface is suitable for implementing any other type of request including XML and HTML Forms data. 

This interface allows any number of elements to be specified to intercept or sample.  Any Json datatype can be detected.  In release 1, _**quoted text fields**_ can be _**intercepted**_ (removed from the stream) and optionally replaced with alternate text.  This includes _**string, base64, and date**_ format json elements.  Other data types including _**number, boolean and null**_ can be _**sampled**_ (read but not removed). 

Sampling data is typically used to help decide how to deal with larger objects.  For example, the filename and or file type might be needed to determine the correct name to give the intercepted file.

For the JsonElementStreamer, elements can be specified by specifying a JsonPath for the elemnent the type of _ElementStreamWriter_ to handle the element.
For example, to intercept a base64 element called "document" and write it to a temporary file, and the filename to ensure the type of file is acceptable to the service.

```
var elements = new StreamedElements();
// Intercept a Base64 encoded element
// Read a string property
elements.Add("$.document", new Base64ToTempFileWriter());
elements.Add("$.fileName", new StringValueStreamWriter());
```

## Example

### Startup - Add Using
To use the ElementStreaming Middleware, First add the Using statment to _Startup.cs_  
```
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseHsts();
    }
    app.UseMiddleware<ElementStreamingMiddleware>();
    app.UseMvc();
}

```
### Create an Instance of the Configuration Class
Next create a descendent of the _**ElementStreamingRequestContextCollection**_ Class in a new file.  This new class will contain the specific behaviour for different end points and document types. The class can eventually contain many types of context for many different endpoints and document types.

In future releases, some of this configuration can be done using the configuration tools and json, but for Release 1, you must include configuration in your context collection class.

```
public class ElementStreamingDemoContexts : ElementStreamingRequestContextCollection
{
    public override void Configure()
    {
        this.ElementStreamingRequestContexts = new List<ElementStreamingRequestContext>();
        ConfigureJsonFileWriter();
    }

    private void ConfigureJsonFileWriter()
    {
        // 1. Identify the elements, and their types.
        var elements = new StreamedElements();
        
        // I want to be able to intercept a Base64 encoded element and string property
        elements.Add("$.document", new Base64ToTempFileWriter());
        elements.Add("$.fileName", new StringValueStreamWriter());

        // Now plug in a Element Streamer.
        var elementStreamer = new JsonElementStreamer(elements);
        var jsonRequestContext = new ElementStreamingRequestContext(jsonEndpoints, jsonContentTypes, elements, elementStreamer);
          
        // Add event handlers to manage the events - do nothing.   
        jsonRequestContext.OnElementStarting = s => true; // do nothing when the element name is detected

        jsonRequestContext.OnElementCompleted = s =>
        true; // do nothing when the element data is complete. 

        this.ElementStreamingRequestContexts.Add(jsonRequestContext);

    }

}

```
### Startup.cs - Add the configuration class as a scoped injection service 
We will come back to the event handlers later.  But next we can now add the class to the _AspNetCore_ injection system.  

In the _Startup.cs_ file add your new class to the injection services.

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
    // Add the ElementStreamerCollection and (optionally) HTTP context accessor
    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    services.AddScoped<IElementStreamingRequestContextCollection, ElementStreamingDemoContexts>();
}
```

### Accessing the Context in the Contoller:

```
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
                if (request.document != null )
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
```

Finally, the code for handling the events when the specific elements are detected need to be added.

There are two delegates which are called when the middleware detects each element name and the data is ready to be received, and also when the data has been handled by the specific writer and the data is complete.

Recall from the _ConfigureJsonFileWriter_ method above, we did nothing with the events.

```
// Add event handlers to manage the events    
jsonRequestContext.OnElementStarting = s => true; // do nothing when the element name is detected

jsonRequestContext.OnElementCompleted = s =>
true; // do nothing when the element data is complete. 

```   
The snippet of code above does nothing with the events which means that in the contoller, the ElementStreamWriters for each element will be holding a reference to the data in memory. 


The following configuration is an example of how the Event handlers can be used to achieve powerful results.



You can create Anonymous methods directly in your configure method or attach 