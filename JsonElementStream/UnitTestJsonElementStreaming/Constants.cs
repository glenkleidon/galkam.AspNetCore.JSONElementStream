using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTestJsonElementStreaming
{
    public class Constants
    {
        public const string TestNumbers = "1234567890";
        public const string TestMessage = "Test Message";
        public const string TestMessageB64 = "VGVzdCBNZXNzYWdlCg==";
        public const string ComplexObject1 =
                "{" +
                "		\"Object1\": {" +
                "			\"ElementNull\": null," +
                "			\"ElementNumber\": 35.2," +
                "			\"ElementBoolean\": true," +
                "			\"ElementDate\": \"2001-01-01T00:00:01Z\"," +
                "			\"ElementString\": \"Text2\"" +
                "		}";

        public const string TestJSON =
                "{" +
                "	\"SimpleNumber\": 23," +
                "	\"SimpleString\": \"\\\"Text1\\\"\"," +
                "	\"Complex\": "+ComplexObject1+"," +
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
                "           \"COString\": \"Text4\"," +
                "			\"CO2\": {" +
                "				\"number\": 77," +
                "				\"string\": \"text6\"" +
                "			}" +
                "		}" +
                "	]" +
                "}";

        public static readonly string[] JsonPaths = {
                "$" ,
                "$.SimpleNumber" ,
                "$.SimpleString" ,
                "$.Complex" ,
                "$.Complex.Object1" ,
                "$.Complex.Object1.ElementNull" ,
                "$.Complex.Object1.ElementNumber" ,
                "$.Complex.Object1.ElementBoolean" ,
                "$.Complex.Object1.ElementDate" ,
                "$.Complex.Object1.ElementString" ,
                "$.Complex.ArrayOfDigits" ,
                "$.Complex.ArrayOfDigits[0]" ,
                "$.Complex.ArrayOfDigits[1]" ,
                "$.Complex.ArrayOfDigits[2]" ,
                "$.Complex.ArrayOfDigits[3]" ,
                "$.Complex.ArrayOfString" ,
                "$.Complex.ArrayOfString[0]" ,
                "$.Complex.ArrayOfString[1]" ,
                "$.Complex.ArrayOfString[2]" ,
                "$.Complex.ArrayOfString[3]" ,
                "$.ArrayOfObjects" ,
                "$.ArrayOfObjects[0]" ,
                "$.ArrayOfObjects[0].number" ,
                "$.ArrayOfObjects[0].string" ,
                "$.ArrayOfObjects[1]" ,
                "$.ArrayOfObjects[1].number" ,
                "$.ArrayOfObjects[1].string" ,
                "$.ArrayOfComplexObjects" ,
                "$.ArrayOfComplexObjects[0]" ,
                "$.ArrayOfComplexObjects[0].CO1" ,
                "$.ArrayOfComplexObjects[0].CO1.number" ,
                "$.ArrayOfComplexObjects[0].CO1.string" ,
                "$.ArrayOfComplexObjects[1]" ,
                "$.ArrayOfComplexObjects[1].COString" ,
                "$.ArrayOfComplexObjects[1].CO2" ,
                "$.ArrayOfComplexObjects[1].CO2.number" ,
                "$.ArrayOfComplexObjects[1].CO2.string"
        };


    }
}
