# _ASPNetCore JSON Elements stream_
------------------------------------
Galkam.ASPNetCore.JSONElementStream is an ASPNetCore middleware component that handles streaming large JSON objects into manageable objects.

For JSON objects that contain large content object such as Base64Encoded data, the middleware allows you to target the element, and convert it to a binary object in a request context before it hits the controller.

Raw JSON Message containing Base64 Image in the element _$.document.imagedata_ element
```
       IStreamDictionary
JSON-->JSONElementStream --> Controller --> [FromBody] --> ViewModel
           |                       |                       Element Stream
           |___ <Base 64 Removed>_/                             |             
           |____ Base64 text--> IStream<IStreamWriter>--> Binary Output
```
_StreamDictionary_ defines a Dictionary<Path,IStreamWriter> where the IStreamWriter implements a StreamWriter to handle the content contained within the path.

