# _ASPNetCore JSON Elements stream_
------------------------------------
Galkam.ASPNetCore.JSONElementStream is an ASPNetCore middleware component that handles streaming large JSON objects into manageable objects.

For JSON objects that contain large content object such as Base64Encoded data, the middleware allows you to target the element, and convert it to a binary object in a request context before it hits the controller.

Raw JSON Message containing Base64 Image in the element _$.document.imagedata_ element
JSON-->JSONElementStream --> Controller --> [FromBody] --> ViewModel
           |_____ Base64-->IStream<ImageData>   


