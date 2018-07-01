using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.JsonElementStreaming
{
    public static class Enums
    {
        public enum StreamerStatus { None=0, Error, Searching, Streaming, Complete };
        public enum JsonStatus {
            None, StartObject, StartLabel, StartData,
            StartQuotedText, StartArray, NextObjectElement, NextArrayElement,
            EndLabel, EndQuotedText, EndArray, EndObject
        }
    }
}
