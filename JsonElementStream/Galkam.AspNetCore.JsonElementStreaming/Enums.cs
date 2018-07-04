using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.JsonElementStreaming
{
    public static class Enums
    {
        public enum StreamerStatus { None=0, Error, Searching, StartOfData, EndOfData, Streaming, Complete };
        public enum JsonStatus {
            None, InObject, InLabel, StartData, InData,
            InQuotedText, InArray, NextObjectElement, NextArrayElement,
            EndLabel, EndQuotedText, EndArray, EndObject
        }
    }
}
