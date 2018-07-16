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
    public class TestJsonElementStreamerNext
    {
        private JsonElementStreamer testStreamer;
        private Stream outStream;
        private Dictionary<string, IElementStreamWriter> elements;

        private async Task SkipElements(int skipCount)
        {
            testStreamer.AlwaysStopOnNextData = true;
            for (var i = 0; i < skipCount; i++) await testStreamer.Next();
            testStreamer.AlwaysStopOnNextData = false;
        }



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
            testStreamer.AlwaysStopOnNextData = true;
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            Assert.AreEqual("$", testStreamer.JsonPath);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            Assert.AreEqual("$.SimpleNumber", testStreamer.JsonPath);
            Assert.AreEqual(Enums.JsonStatus.InData, testStreamer.JsonStatus);
            Assert.IsTrue(outStream.Length > 0, "Stream is empty");
        }

        [TestMethod]
        public async Task ElementStream_Skips_First_element()
        {
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(Constants.TestJSON));
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);

            await SkipElements(2); // we want to ignore the first element and the first object
            testStreamer.AlwaysStopOnNextData = true;
            await testStreamer.Next(); // stop at second element

            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            Assert.AreEqual("$.SimpleString", testStreamer.JsonPath);
            Assert.AreEqual(Enums.JsonStatus.InQuotedText, testStreamer.JsonStatus);
            Assert.IsTrue(outStream.Length > 0);

        }

        [TestMethod]
        public async Task ElementStream_Stops_at_First_Complex_Object()
        {
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(Constants.TestJSON));
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);

            await SkipElements(3);
            testStreamer.AlwaysStopOnNextData = true;
            await testStreamer.Next(); 

            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            Assert.AreEqual("$.Complex", testStreamer.JsonPath);
            Assert.AreEqual(Enums.JsonStatus.StartObject, testStreamer.JsonStatus);
            Assert.IsTrue(outStream.Length > 0);

        }
        [TestMethod]
        public async Task ElementStream_Stops_at_First_Array_element()
        {
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(Constants.TestJSON));
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);

            await SkipElements(11);
            testStreamer.AlwaysStopOnNextData = true;
            await testStreamer.Next(); 

            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            Assert.AreEqual("$.Complex.ArrayOfDigits", testStreamer.JsonPath);
            Assert.AreEqual(Enums.JsonStatus.InArray, testStreamer.JsonStatus);
            Assert.IsTrue(outStream.Length > 0);

        }

    }
}
