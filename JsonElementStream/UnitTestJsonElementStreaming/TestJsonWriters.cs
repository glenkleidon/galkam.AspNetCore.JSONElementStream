using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Galkam.AspNetCore.JsonElementStreaming.Writers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestJsonElementStreaming
{
    [TestClass]
    public class TestJsonWriters
    {

    [TestInitialize]
        public void Setup()
        {

        }
    private byte[] TestNumbersBytes { get => new byte[] { 49, 50, 51, 52, 53, 54, 55, 56, 57, 48 }; }

    [TestMethod]
        public async Task TextWriter_stores_as_stream_as_string()
        {
            var writer = new StringValueStreamWriter();
            await writer.Write(TestNumbersBytes, 0, 10);
            Assert.AreEqual(Constants.TestNumbers, writer.Value);
            await writer.WriteString(Constants.TestMessage);
            Assert.AreEqual($"{Constants.TestNumbers}{Constants.TestMessage}", writer.Value);
        }

        [TestMethod]
        public async Task IntegerWriter_stores_as_stream_as_integer()
        {
            var writer = new IntegerValueStreamWriter();
            Assert.IsNull(writer.Value);
            await writer.Write(TestNumbersBytes, 0, 10);
            Assert.AreEqual(1234567890, writer.Value);
        }

        [TestMethod]
        public async Task DynamicWriter_stores_as_stream_as_dynamic()
        {
            var writer = new DynamicValueStreamWriter();
            Assert.IsNull(writer.Value);
            await writer.Write(TestNumbersBytes, 0, 7);
            await writer.WriteString(".");
            await writer.Write(TestNumbersBytes, 7, 3);
            Assert.IsTrue(writer.IsNumber());
            Assert.IsFalse(writer.IsInteger());
            Assert.AreEqual((float)1234567.890, writer.AsFloat());
            Assert.AreEqual((double)1234567.890, writer.AsDouble());
            Assert.AreEqual((decimal)1234567.890, writer.AsDecimal());
            Assert.IsNull(writer.AsInteger());
            Assert.AreEqual("1234567.890", writer.Value);
        }

        [TestMethod]
        public async Task DynamicWriter_stores_stream_as_dynamic_date()
        {
            var writer = new DynamicValueStreamWriter();
            var testDate = "2009-05-01T14:57:32-04:00";
            foreach (char c in testDate)
            {
                var ca = new char[1] { c };
                await writer.Write(ca, 0, 1);
            }
            var dt = DateTime.Parse(testDate);
            Assert.AreEqual(dt, writer.AsDate());
        }

        [TestMethod, ExpectedException(typeof(FormatException))]
        public async Task DynamicWriter_stores_stream_as_dynamic_Invalid_date()
        {
            var writer = new DynamicValueStreamWriter();
            var testDate = "2009-02-31T14:57:32-04:00";
            foreach (char c in testDate)
            {
                var ca = new char[1] { c };
                await writer.Write(ca, 0, 1);
            }
            var dt = DateTime.Parse(testDate);
            Assert.AreEqual(dt, writer.AsDate());
        }

        [TestMethod]
        public async Task FloatWriter_stores_as_stream_as_float()
        {
            var writer = new FloatValueStreamWriter();
            Assert.IsNull(writer.Value);
            await writer.Write(TestNumbersBytes, 0, 7);
            await writer.WriteString(".");
            await writer.Write(TestNumbersBytes, 7, 3);
            Assert.AreEqual((float)1234567.890, writer.Value);
        }
        [TestMethod]
        public async Task DoubleWriter_stores_as_stream_as_double()
        {
            var writer = new DoubleValueStreamWriter();
            Assert.IsNull(writer.Value);
            await writer.Write(TestNumbersBytes, 0, 5);
            await writer.WriteString(".");
            await writer.Write(TestNumbersBytes, 5, 5);
            Assert.AreEqual((Double)12345.67890, writer.Value);
        }
        [TestMethod]
        public async Task DecimalWriter_stores_as_stream_as_decimal()
        {
            var writer = new DecimalValueStreamWriter();
            Assert.IsNull(writer.Value);
            await writer.Write(TestNumbersBytes, 0, 8);
            await writer.WriteString(".");
            await writer.Write(TestNumbersBytes, 8, 2);
            Assert.AreEqual((Decimal)12345678.90, writer.Value);
        }
        [TestMethod]
        public async Task DateWriter_stores_as_stream_as_date()
        {
            var writer = new DateTimeValueStreamWriter();
            Assert.IsNull(writer.Value);
            var testDate = "2009-05-01T14:57:32-04:00";
            foreach (char c in testDate)
            {
                var ca = new char[1] { c };
                await writer.Write(ca, 0, 1);
            }
            var dt = DateTime.Parse(testDate);
            Assert.AreEqual(dt, writer.Value);
        }
    }
}
