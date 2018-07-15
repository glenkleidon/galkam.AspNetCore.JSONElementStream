using Microsoft.VisualStudio.TestTools.UnitTesting;
using Galkam.AspNetCore.JsonElementStreaming;
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Galkam.AspNetCore.JsonElementStreaming.Writers;

namespace UnitTestJsonElementStreaming
{
    [TestClass]
    public class TestRelease1
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
        public async Task All_Paths_are_detected_correctly()
        {
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(Constants.TestJSON));
            elements.Add("$.dummy", new Base64StreamWriter(new MemoryStream()));
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            testStreamer.AlwaysStopOnNextData = true;
            foreach (var p in Constants.JsonPaths)
            {
                await testStreamer.Next();
                Assert.AreEqual(p, testStreamer.JsonPath , $"Faulted at {p} with {testStreamer.JsonPath}");
                Console.WriteLine(p);
            }
        }

        [TestMethod]
        public async Task When_Nothing_Detected_Whole_stream_is_in_outstream()
        {
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(Constants.TestJSON));
            elements.Add("$.ArrayOfObjects[99].string", new StringValueStreamWriter(true));
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);

            // Read to End
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.Searching, testStreamer.Status);
            Assert.IsTrue(outStream.Length > 0);

            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.Complete, testStreamer.Status);
            Assert.IsTrue(outStream.Length > 0);

            outStream.Position = 0;
            var outstreamContent = new StreamReader(outStream).ReadToEnd();
            var text = elements["$.ArrayOfObjects[99].string"].TypedValue;

            //Test contents
            Assert.IsNull(text.AsString());
            Assert.AreEqual(Constants.TestJSON, outstreamContent);
        }
        [TestMethod]
        public async Task Empty_Objects_and_Arrays_allowed()
        {
            throw new NotImplementedException();
        }
        [TestMethod,ExpectedException(typeof(FormatException))]
        public async Task Empty_Array_elements_are_not_allowed()
        {
            var json = "{\"Array1\" : [1,,3,4]}";
            var intWriter = new IntegerValueStreamWriter();
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(json));
            elements.Add("$.Array1[3]", intWriter);
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            await testStreamer.Next();
        }

        [TestMethod]
        public async Task Base64_string_is_extracted_as_expected()
        {
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(Constants.TestJSON));
            elements.Add("$.Complex.Object1.ElementBase64", new Base64StreamWriter(new MemoryStream()));
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            // Locate the Element
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            Assert.AreEqual("$.Complex.Object1.ElementBase64", testStreamer.JsonPath);

            // Read the element contents
            await testStreamer.Next();
            var b64stream = elements["$.Complex.Object1.ElementBase64"].OutStream;
            Assert.IsNotNull(b64stream);
            b64stream.Position = 0;
            var elementStreamContent = new StreamReader(b64stream).ReadToEnd();
            Assert.IsTrue(elementStreamContent.Length > 0);

            // Read to End
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.Complete, testStreamer.Status);
            Assert.IsTrue(outStream.Length > 0);
            var OutContents = Constants.TestJSON.Substring(0, Constants.TestJSON.IndexOf(Constants.TestMessageB64));
            OutContents = OutContents + Constants.TestJSON.Substring(
                  Constants.TestJSON.IndexOf(Constants.TestMessageB64) + Constants.TestMessageB64.Length);
            outStream.Position = 0;
            var outstreamContent = new StreamReader(outStream).ReadToEnd();

            //Test contents
            Assert.AreEqual(Constants.TestMessage, elementStreamContent);
            Assert.AreEqual(OutContents, outstreamContent);
        }
        private async Task DoTwoBase64Strings(JsonElementStreamer testStreamer)
        {
            elements.Add("$.Complex.Object1.ElementBase64", new Base64StreamWriter(new MemoryStream()));
            elements.Add("$.ArrayOfComplexObjects[1].CO2.string", new Base64StreamWriter(new MemoryStream()));

            // Locate the Element
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            Assert.AreEqual("$.Complex.Object1.ElementBase64", testStreamer.JsonPath);

            // Read the b64 element contents
            await testStreamer.Next();
            var b64stream = elements["$.Complex.Object1.ElementBase64"].OutStream;
            Assert.IsNotNull(b64stream);
            b64stream.Position = 0;
            var elementStreamContent = new StreamReader(b64stream).ReadToEnd();
            Assert.IsTrue(elementStreamContent.Length > 0);


            // Locate the 2nd Base64 Element Contents
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            Assert.AreEqual("$.ArrayOfComplexObjects[1].CO2.string", testStreamer.JsonPath);

            // Read the b64 element contents
            await testStreamer.Next();
            var b64Stream2 = elements["$.ArrayOfComplexObjects[1].CO2.string"].OutStream;
            Assert.IsNotNull(b64Stream2);
            b64Stream2.Position = 0;
            var element2StreamContent = new StreamReader(b64Stream2).ReadToEnd();
            Assert.IsTrue(element2StreamContent.Length > 0);

            // Read to End
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.Complete, testStreamer.Status);
            Assert.IsTrue(outStream.Length > 0);

            var bpos = Constants.TestJSON.IndexOf(Constants.TestMessageB64);
            var tpos = Constants.TestJSON.IndexOf(Constants.TestMessageB64_2);
            var segment1Length = tpos - bpos - Constants.TestMessageB64.Length;

            var OutContents = Constants.TestJSON.Substring(0, bpos) +
                              Constants.TestJSON.Substring(bpos + Constants.TestMessageB64.Length, segment1Length) +
                              Constants.TestJSON.Substring(tpos + Constants.TestMessageB64_2.Length);
            outStream.Position = 0;
            var outstreamContent = new StreamReader(outStream).ReadToEnd();

            //Test contents
            Assert.AreEqual(Constants.TestMessage, elementStreamContent);
            Assert.AreEqual(Constants.TestMessage2, element2StreamContent);
            Assert.AreEqual(OutContents, outstreamContent);
        }
        [TestMethod]
        public async Task Two_separate_Base64_string_is_extracted_as_expected()
        {
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(Constants.TestJSON));
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            await DoTwoBase64Strings(testStreamer);
        }

        [TestMethod]
        public async Task Simple_Text_Element_is_extracted_as_expected()
        {
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(Constants.TestJSON));
            elements.Add("$.ArrayOfObjects[1].string", new StringValueStreamWriter(true));
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);

            // Locate the Text Element COntents
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            Assert.AreEqual("$.ArrayOfObjects[1].string", testStreamer.JsonPath);

            // Read the text contents
            await testStreamer.Next();
            var text = elements["$.ArrayOfObjects[1].string"].TypedValue;
            Assert.IsNotNull(text);

            // Read to End
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.Complete, testStreamer.Status);
            Assert.IsTrue(outStream.Length > 0);

            var bpos = Constants.TestJSON.IndexOf("\"text4\"");
            var OutContents = Constants.TestJSON.Substring(0, bpos+1) +
                              Constants.TestJSON.Substring(bpos+6);
            outStream.Position = 0;
            var outstreamContent = new StreamReader(outStream).ReadToEnd();

            //Test contents
            Assert.AreEqual("text4", text.AsString());
            Assert.AreEqual(OutContents, outstreamContent);
        }

        [TestMethod]
        public async Task Simple_Null_Element_is_located_as_expected()
        {
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(Constants.TestJSON));
            elements.Add("$.Complex.Object1.ElementNull", new IntegerValueStreamWriter());
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            // Locate the Element
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            Assert.AreEqual("$.Complex.Object1.ElementNull", testStreamer.JsonPath);

            // Read the element value
            await testStreamer.Next();
            var str = elements["$.Complex.Object1.ElementNull"].TypedValue.AsInteger();
            Assert.IsTrue(str == null);

            // check remaining text.
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.Complete, testStreamer.Status);
            Assert.IsTrue(outStream.Length > 0);
            outStream.Position = 0;
            var outstreamContent = new StreamReader(outStream).ReadToEnd();
            Assert.AreEqual(Constants.TestJSON, outstreamContent);
        }
        [TestMethod]
        public async Task Simple_Null_Element_is_ignored_when_intercepting_as_expected()
        {
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(Constants.TestJSON));
            elements.Add("$.Complex.Object1.ElementNull", new Base64StreamWriter());
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            // Locate the Element
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            Assert.AreEqual("$.Complex.Object1.ElementNull", testStreamer.JsonPath);

            // Read the element contents
            await testStreamer.Next();
            var b64stream = elements["$.Complex.Object1.ElementNull"].OutStream;
            Assert.IsNotNull(b64stream);
            b64stream.Position = 0;
            var elementStreamContent = new StreamReader(b64stream).ReadToEnd();
            Assert.IsTrue(elementStreamContent.Length == 0);

            // check remaining text.
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.Complete, testStreamer.Status);
            Assert.IsTrue(outStream.Length > 0);
            outStream.Position = 0;
            var outstreamContent = new StreamReader(outStream).ReadToEnd();
            Assert.AreEqual(Constants.TestJSON, outstreamContent);
        }

        [TestMethod]
        public async Task Simple_Date_Element_is_extracted_as_expected()
        {
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(Constants.TestJSON));
            elements.Add("$.Complex.Object1.ElementDate", new DateTimeValueStreamWriter());
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            // Locate the Element
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            Assert.AreEqual("$.Complex.Object1.ElementDate", testStreamer.JsonPath);

            // Read the element contents
            await testStreamer.Next();
            //2001-01-01T00:00:01Z
            var dt = elements["$.Complex.Object1.ElementDate"].TypedValue.AsDateTime();
            Assert.IsTrue(dt != null);

            // Read to End
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.Complete, testStreamer.Status);
            Assert.IsTrue(outStream.Length > 0);
            outStream.Position = 0;
            var outstreamContent = new StreamReader(outStream).ReadToEnd();

            //Test contents
            Assert.AreEqual(DateTime.Parse("2001-01-01T00:00:01Z").ToUniversalTime(), dt.Value.ToUniversalTime());
            Assert.AreEqual(Constants.TestJSON, outstreamContent);
        }
        [TestMethod]
        public async Task Complex_Number_Element_is_extracted_as_expected()
        {
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(Constants.TestJSON));
            elements.Add("$.ArrayOfComplexObjects[0].CO1.number", new DecimalValueStreamWriter());
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            // Locate the Element
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            Assert.AreEqual("$.ArrayOfComplexObjects[0].CO1.number", testStreamer.JsonPath);

            // Read the element value
            await testStreamer.Next();
            var val = elements["$.ArrayOfComplexObjects[0].CO1.number"];
            Assert.IsTrue(val != null);

            // check remaining text.
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.Complete, testStreamer.Status);
            Assert.IsTrue(outStream.Length > 0);
            outStream.Position = 0;
            var outstreamContent = new StreamReader(outStream).ReadToEnd();

            Assert.AreEqual((Decimal)66.2, val.TypedValue.AsDecimal());
            Assert.AreEqual(Constants.TestJSON, outstreamContent);
        }
        [TestMethod]
        public async Task Simple_Text_Element_and_Base64_extracted_as_expected()
        {
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(Constants.TestJSON));
            elements.Add("$.Complex.Object1.ElementBase64", new Base64StreamWriter(new MemoryStream()));
            elements.Add("$.ArrayOfObjects[1].string", new StringValueStreamWriter(true));
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            // Locate the Element
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            Assert.AreEqual("$.Complex.Object1.ElementBase64", testStreamer.JsonPath);

            // Read the b64 element contents
            await testStreamer.Next();
            var b64stream = elements["$.Complex.Object1.ElementBase64"].OutStream;
            Assert.IsNotNull(b64stream);
            b64stream.Position = 0;
            var elementStreamContent = new StreamReader(b64stream).ReadToEnd();
            Assert.IsTrue(elementStreamContent.Length > 0);


            // Locate the Text Element COntents
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.StartOfData, testStreamer.Status);
            Assert.AreEqual("$.ArrayOfObjects[1].string", testStreamer.JsonPath);
            
            // Read the text element contents
            await testStreamer.Next();
            var text = elements["$.ArrayOfObjects[1].string"].TypedValue;
            Assert.IsNotNull(text);

            // Read to End
            await testStreamer.Next();
            Assert.AreEqual(Enums.StreamerStatus.Complete, testStreamer.Status);
            Assert.IsTrue(outStream.Length > 0);

            var bpos = Constants.TestJSON.IndexOf(Constants.TestMessageB64);
            var tpos = Constants.TestJSON.IndexOf("\"text4\"");
            var segment1Length = tpos - bpos - Constants.TestMessageB64.Length+1;

            var OutContents = Constants.TestJSON.Substring(0, bpos) +
                              Constants.TestJSON.Substring(bpos + Constants.TestMessageB64.Length, segment1Length) +
                              Constants.TestJSON.Substring(tpos + 6);
            outStream.Position = 0;
            var outstreamContent = new StreamReader(outStream).ReadToEnd();

            //Test contents
            Assert.AreEqual(Constants.TestMessage, elementStreamContent);
            Assert.AreEqual("text4", text.AsString());
            Assert.AreEqual(OutContents, outstreamContent);
        }
        [TestMethod]
        public async Task Basic_Optimisation_Stops_Searching_when_lements_filled()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public async Task Faulty_JSON_is_allowed_to_pass_through()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public async Task Content_exceeding_buffer_size_extracted_as_expected()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public async Task Content_exactly_matching_buffer_size_extracted_as_expected()
        {
            var TestStream = new MemoryStream(Encoding.ASCII.GetBytes(Constants.TestJSON));
            testStreamer = new JsonElementStreamer(TestStream, outStream, elements);
            testStreamer.ChunkSize = (Int32)TestStream.Length;
            await DoTwoBase64Strings(testStreamer);
        }
        public async Task Content_exceeding_2_buffer_sizes_extracted_as_expected()
        {
            throw new NotImplementedException();
        }


    }
}
