using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTestJsonElementStreaming
{
    public class Constants
    {
        public const string TestMessage = "Test Message";
        public const string TestMessageB64 = "VGVzdCBNZXNzYWdlCg==";
        public const string TestJSON =
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

    }
}
