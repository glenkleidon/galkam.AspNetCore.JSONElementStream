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
    public class TestJsonElementStreamerNext
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
        public async Task ElementStreamer_Parses()
        {
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(Constants.TestJSON));
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            await testStreamer.Next();

            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            Assert.AreEqual("$.SimpleNumber", testStreamer.JsonPath);

            

        }
    }
}
