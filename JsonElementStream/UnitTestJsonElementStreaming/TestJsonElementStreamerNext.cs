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
        public async Task ElementStreamer_Stops_At_First_Element()
        {
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(Constants.TestJSON));
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);

            await testStreamer.Next();

            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            Assert.AreEqual("$.SimpleNumber", testStreamer.JsonPath);
            Assert.AreEqual(Enums.JsonStatus.InData, testStreamer.JsonStatus);
            Assert.IsTrue(outStream.Length > 0);
        }

        [TestMethod]
        public async Task ElementStream_Skips_First_element()
        {
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(Constants.TestJSON));
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);

            await testStreamer.Next(); // we want to ignore the first element
            await testStreamer.Next(); // stop at second element

            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            Assert.AreEqual("$.SimpleString", testStreamer.JsonPath);
            Assert.AreEqual(Enums.JsonStatus.InQuotedText, testStreamer.JsonStatus);
            Assert.IsTrue(outStream.Length > 0);

        }

    }
}
