﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Galkam.AspNetCore.ElementStreaming
{
    public static class Enums
    {
        public enum StreamerStatus { None=0, Error, Searching, StartOfData, EndOfData, Streaming, Flushing, Complete };
        public enum JsonStatus {
            None, InObject, InLabel, StartObject, StartData, InData,
            InQuotedText, InArray, NextObjectElement, NextArrayElement,
            EndLabel, EndQuotedText, EndArray, EndObject
        }
    }
}
