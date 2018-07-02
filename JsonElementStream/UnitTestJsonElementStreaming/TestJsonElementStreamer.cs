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
        const string TestMessage = "Test Message";
        const string TestMessageB64 = "VGVzdCBNZXNzYWdlCg==";
        const string TestJSON =
                "{" +
                "	\"SimpleNumber\": 23," +
                "	\"SimpleString\": \"Text1\"," +
                "	\"Complex\": {" +
                "		\"Object1\": {" +
                "			\"ElementNull\": null," +
                "			\"ElementNumber\": 35.2," +
                "			\"ElementBoolean\": true," +
                "			\"ElementDate\": \"2001-01-01T00:00:01Z\"," +
                "			\"ElementString\": \"Text2\"" +
                "		}," +
                "		\"ArrayOfDigits\": [" +
                "			0," +
                "			1," +
                "			2," +
                "			3" +
                "		]," +
                "		\"ArrayOfString\": [" +
                "			\"Zero\"," +
                "			\"One\"," +
                "			\"Two\"," +
                "			\"Three\"" +
                "		]" +
                "	}," +
                "	\"ArrayOfObjects\": [{" +
                "			\"number\": 44," +
                "			\"string\": \"text3\"" +
                "		}," +
                "		{" +
                "			\"number\": 55," +
                "			\"string\": \"text4\"" +
                "		}" +
                "	]," +
                "	\"ArrayOfComplexObjects\": [{" +
                "			\"CO1\": {" +
                "				\"number\": 66," +
                "				\"string\": \"text5\"" +
                "			}" +
                "		}," +
                "		{" +
                "			\"CO2\": {" +
                "				\"number\": 77," +
                "				\"string\": \"text6\"" +
                "			}" +
                "		}" +
                "	]" +
                "}";

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
            var json = "{\"document\" : \"" + TestMessageB64 + "\"}";
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(json));
            elements.Add("$.document", new Base64StreamWriter(new MemoryStream()));
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.Complete, testStreamer.Status);

        }
    }
}
