using System;

namespace Galkam.AspNetCore.JsonElementStreaming.Writers
{
    public interface IValueStreamWriter
    {
        DateTime? AsDateTime();
        decimal? AsDecimal();
        double? AsDouble();
        float? AsFloat();
        long? AsInteger();
        string AsString();
        dynamic AsDynamic();
        bool? AsBool();
        Single? AsSingle();
        bool IsDateTime();
        bool IsDecimal();
        bool IsDouble();
        bool IsFloat();
        bool IsInteger();
        bool IsNumber();
        bool IsString();
        bool IsDynamic();
        bool IsBool();
        bool IsSingle();
        Type ValueType { get; }
        dynamic StreamedValue { get; }

    }
}