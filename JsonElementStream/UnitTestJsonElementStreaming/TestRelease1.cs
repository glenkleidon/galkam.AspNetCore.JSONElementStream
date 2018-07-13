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
            throw new NotImplementedException();
        }
        [TestMethod]
        public async Task Empty_Objects_and_Arrays_allowed()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public async Task Empty_Array_elements_are_not_allowed()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public async Task Base64_string_is_extracted_as_expected()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public async Task Two_separate_Base64_string_is_extracted_as_expected()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public async Task Simple_Text_Element_is_extracted_as_expected()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public async Task Simple_Null_Element_is_extracted_as_expected()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public async Task Simple_Date_Element_is_extracted_as_expected()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public async Task Simple_Number_Element_is_extracted_as_expected()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public async Task Simple_Text_Element_and_Base64_extracted_as_expected()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public async Task Basic_Optimisation_Stopes_Searching_When_Elements_filled()
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
            throw new NotImplementedException();
        }
        public async Task Content_exceeding_2_buffer_sizes_extracted_as_expected()
        {
            throw new NotImplementedException();
        }


    }
}
