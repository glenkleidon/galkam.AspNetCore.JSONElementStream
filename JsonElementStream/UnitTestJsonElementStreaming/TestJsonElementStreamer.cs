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
        public async Task ElementStreamer_locates_a_target()
        {
            var json = "{\"document\" : \"" + Constants.TestMessageB64 + "\"}";
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(json));
            elements.Add("$.document", new Base64StreamWriter(new MemoryStream()));
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.Complete, testStreamer.Status);
            Assert.IsTrue(elements["$.document"].OutStream.Length> 0);

        }
    }
}
