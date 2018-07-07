using Microsoft.VisualStudio.TestTools.UnitTesting;
using Galkam.AspNetCore.JsonElementStreaming;
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnitTestJsonElementStreaming
{
    [TestClass]
    public class TestJsonElementStreamer
    {
        private JsonElementStreamer testStreamer;
        private Stream outStream;
        private Dictionary<string, IElementStreamWriter> elements;

        [TestInitialize]
        public void Setup()
        {
            outStream = new MemoryStream();
            elements = new Dictionary<string, IElementStreamWriter>();
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
            var documentHeader = "{\"document\" : ";
            var documentTail = "}";
            var json = $"{documentHeader}null{documentTail}";
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(json));
            elements.Add("$.document", new Base64StreamWriter(new MemoryStream()));
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.EndOfData, testStreamer.Status);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.Complete, testStreamer.Status);
            outStream.Position = 0;
            var outstreamContent = new StreamReader(outStream).ReadToEnd();
            Assert.AreEqual(json, outstreamContent);

        }
        [TestMethod]
        public async Task ElementStreamer_returns_full_output_when_no_elements_detected()
        {
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(Constants.TestJSON));
            elements.Add("$.ssssss", new Base64StreamWriter(new MemoryStream()));
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.Complete, testStreamer.Status);
            outStream.Position = 0;
            var outstreamContent = new StreamReader(outStream).ReadToEnd();
            Assert.AreEqual(Constants.TestJSON, outstreamContent);
        }
    }
}