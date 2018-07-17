using Microsoft.VisualStudio.TestTools.UnitTesting;
using Galkam.AspNetCore.ElementStreaming;
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Galkam.AspNetCore.ElementStreaming.Writers;

namespace UnitTestJsonElementStreaming
{
    [TestClass]
    public class TestJsonElementStreamer
    {
        private JsonElementStreamer testStreamer;
        private Stream outStream;
        private StreamedElements elements;

        [TestInitialize]
        public void Setup()
        {
            outStream = new MemoryStream();
            elements = new StreamedElements();
            testStreamer = null;
        }

        [TestMethod]
        public async Task ElementStreamer_locates_a_base64_target()
        {
            var documentHeader = "{\"document\" : \"";
            var documentTail = "\"}";
            var json = $"{documentHeader}{Constants.TestMessageB64}{documentTail}";
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(json));
            elements.Add("$.document", new Base64StreamWriter(new MemoryStream()));
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.EndOfData, testStreamer.Status);
            Assert.IsTrue(elements["$.document"].OutStream.Length > 0);
            elements["$.document"].OutStream.Position = 0;
            var elementStreamContent = new StreamReader(elements["$.document"].OutStream).ReadToEnd();
            await testStreamer.Next();
            // check that the content contains the document
            Assert.AreEqual(Enums.StreamerStatus.Complete, testStreamer.Status);
            Assert.IsTrue(outStream.Length > 0);
            outStream.Position = 0;
            var outstreamContent = new StreamReader(outStream).ReadToEnd();

            // Finally check the content
            Assert.AreEqual($"{documentHeader}{documentTail}", outstreamContent);
            Assert.AreEqual(Constants.TestMessage, elementStreamContent);

        }

        [TestMethod]
        public async Task ElementStreamer_locates_a_null_target()
        {
            var json = "{\"document\" : null}";
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(json));
            elements.Add("$.document", new Base64StreamWriter(new MemoryStream()));
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            outStream.Position = 0;
            var outstreamContent = new StreamReader(outStream).ReadToEnd();
            Assert.AreEqual("{\"document\" : ", outstreamContent);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.EndOfData, testStreamer.Status);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.Complete, testStreamer.Status);
            outStream.Position = 0;
            outstreamContent = new StreamReader(outStream).ReadToEnd();
            Assert.AreEqual(json, outstreamContent);
        }

        [TestMethod]
        public async Task ElementStreamer_locates_integer_0_array_element()
        {
            var json = "{\"Array1\" : [1,2,3,4]}";
            var intWriter = new IntegerValueStreamWriter();
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(json));
            elements.Add("$.Array1[0]", intWriter);
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            Assert.AreEqual("$.Array1[0]", testStreamer.JsonPath);
            await testStreamer.Next();
            Assert.AreEqual(1, intWriter.Value);
            Assert.AreEqual(Enums.StreamerStatus.EndOfData, testStreamer.Status);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.Complete, testStreamer.Status);
            outStream.Position = 0;
            var outstreamContent = new StreamReader(outStream).ReadToEnd();
            Assert.AreEqual(json, outstreamContent);
        }
        [TestMethod]
        public async Task ElementStreamer_locates_integer_1_array_element()
        {
            var json = "{\"Array1\" : [1,2,3,4]}";
            var intWriter = new IntegerValueStreamWriter();
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(json));
            elements.Add("$.Array1[1]", intWriter);
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            await testStreamer.Next();
            Assert.AreEqual(2, intWriter.Value);
            Assert.AreEqual(Enums.StreamerStatus.EndOfData, testStreamer.Status);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.Complete, testStreamer.Status);
            outStream.Position = 0;
            var outstreamContent = new StreamReader(outStream).ReadToEnd();
            Assert.AreEqual(json, outstreamContent);
        }
        [TestMethod]
        public async Task ElementStreamer_locates_integer_last_array_element_Value()
        {
            var json = "{\"Array1\" : [1,2,3,4]}";
            var intWriter = new IntegerValueStreamWriter();
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(json));
            elements.Add("$.Array1[3]", intWriter);
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            await testStreamer.Next();
            Assert.AreEqual(4, intWriter.Value);
            Assert.AreEqual(Enums.StreamerStatus.EndOfData, testStreamer.Status);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.Complete, testStreamer.Status);
            outStream.Position = 0;
            var outstreamContent = new StreamReader(outStream).ReadToEnd();
            Assert.AreEqual(json, outstreamContent);
        }

        [TestMethod]
        public async Task ElementStreamer_locates_String_value_without_intercepting()
        {
            var json = "{\"stringValue\" : \"text123\"}";
            var stringWriter = new StringValueStreamWriter();
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(json));
            elements.Add("$.stringValue", stringWriter);
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            await testStreamer.Next();
            Assert.AreEqual("text123", stringWriter.Value);
            Assert.AreEqual(Enums.StreamerStatus.EndOfData, testStreamer.Status);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.Complete, testStreamer.Status);
            outStream.Position = 0;
            var outstreamContent = new StreamReader(outStream).ReadToEnd();
            Assert.AreEqual(json, outstreamContent);
        }
        [TestMethod]
        public async Task ElementStreamer_locates_String_and_Intercepts()
        {
            var json = "{\"stringValue\" : \"text123\"}";
            var stringWriter = new StringValueStreamWriter(true);
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(json));
            elements.Add("$.stringValue", stringWriter);
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            await testStreamer.Next();
            Assert.AreEqual("text123", stringWriter.Value);
            Assert.AreEqual(Enums.StreamerStatus.EndOfData, testStreamer.Status);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.Complete, testStreamer.Status);
            outStream.Position = 0;
            var outstreamContent = new StreamReader(outStream).ReadToEnd();
            Assert.AreEqual("{\"stringValue\" : \"\"}", outstreamContent);
        }

        //[TestMethod]
        public async Task ElementStreamer_returns_whole_object_when_requested()
        {
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(Constants.TestJSON));
            elements.Add("$.Complex.Object1", new Base64StreamWriter(new MemoryStream()));
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.Complete, testStreamer.Status);
            outStream.Position = 0;
            var outstreamContent = new StreamReader(outStream).ReadToEnd();
            Assert.AreEqual(Constants.ComplexObject1, outstreamContent);
        }

     //   [TestMethod]
        public async Task ElementStreamer_Optimizer_skips_whole_Object()
        {
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(Constants.TestJSON));
            elements.Add("$.Complex.Object1", new Base64StreamWriter(new MemoryStream()));
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            throw new NotImplementedException("Optimizer is not implemented as yet.");
        }
      //  [TestMethod]
        public async Task ElementStreamer_Optimizer_skips_whole_Array()
        {
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(Constants.TestJSON));
            elements.Add("$.Complex.Object1", new Base64StreamWriter(new MemoryStream()));
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            throw new NotImplementedException("Optimizer is not implemented as yet.");
        }
    }
}