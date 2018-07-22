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

Sampling data is typically used to help decide how to deal with larger objects.  



 


