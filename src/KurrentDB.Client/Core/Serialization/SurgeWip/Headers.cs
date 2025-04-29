#pragma warning disable CS0618 // Type or member is obsolete

using System.Globalization;
using System.Numerics;
using System.Text.Json;
using JetBrains.Annotations;
using Kurrent.Surge.Schema;
using static System.Globalization.CultureInfo;

namespace Kurrent.Surge;

[PublicAPI]
public class Headers : Dictionary<string, string?> {
    public Headers()
        : base(StringComparer.Ordinal) { }

    public Headers(Headers headers)
        : base(headers, StringComparer.Ordinal) { }

    public Headers(Dictionary<string, string?> headers)
        : base(headers, StringComparer.Ordinal) { }

    public Headers(IEnumerable<KeyValuePair<string, string?>> headers, Predicate<KeyValuePair<string, string?>> predicate)
        : base(headers.Where(x => predicate(x)).ToDictionary(x => x.Key, x => x.Value), StringComparer.Ordinal) { }

    public Headers(IEnumerable<KeyValuePair<string, string?>> headers)
        : base(headers.ToDictionary(x => x.Key, x => x.Value), StringComparer.Ordinal) { }

    public new Headers Add(string key, string? value) {
        try {
            base.Add(key, value);
            return this;
        }
        catch (Exception ex) {
            throw new($"Failed to add header [{key}]", ex);
        }
    }

    public Headers Set(string key, string? value) {
        try {
            base[key] = value;
            return this;
        }
        catch (Exception ex) {
            throw new($"Failed to set header [{key}]", ex);
        }
    }

    public Headers Add(string key, Guid value) => Add(key, value.ToString());
    public Headers Set(string key, Guid value) => Set(key, value.ToString());

#if NET8_0_OR_GREATER
    public Headers Add<T>(string key, T value) where T : INumber<T> => Add(key, value.ToString());
    public Headers Set<T>(string key, T value) where T : INumber<T> => Set(key, value.ToString());
#endif

    public T GetEnum<T>(string key, T defaultValue) where T : struct, Enum =>
        TryGetValue(key, out var value) && Enum.TryParse<T>(value, true, out var enumValue)
            ? enumValue : defaultValue;

#if NET8_0_OR_GREATER
    public T GetNumber<T>(string key, T defaultValue) where T : INumber<T> =>
        TryGetValue(key, out var value) && T.TryParse(value, NumberStyles.Any, InvariantCulture, out var numberValue)
            ? numberValue : defaultValue;
#endif

    public string GetString(string key, string defaultValue) =>
        TryGetValue(key, out var value) && value is not null
            ? value : defaultValue;

    public static ReadOnlyMemory<byte> Encode(Headers headers) =>
        JsonSerializer.SerializeToUtf8Bytes(headers);

    public static Headers Decode(ReadOnlyMemory<byte> bytes) {
        if (bytes.IsEmpty)
            return new();

        try {
            return JsonSerializer.Deserialize<Headers>(bytes.Span) ?? new Headers();
        }
        catch (Exception) {
            return new();
        }
    }

    public Headers WithSchemaInfo(SchemaInfo schemaInfo) {
        schemaInfo.InjectIntoHeaders(this);
        return this;
    }

    public Dictionary<string, string?> WithoutSystemInfo() =>
        this.Where(x => !x.Key.StartsWith(HeaderKeys.SystemPrefix)).ToDictionary(x => x.Key, x => x.Value);

    public Dictionary<string, string?> WithoutSchemaInfo() {
        string[] keys = {
            HeaderKeys.SchemaName,
            HeaderKeys.SchemaDataFormat,
            HeaderKeys.SchemaType,
            HeaderKeys.SchemaSubject
        };

        return this.Where(x => !keys.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
    }

    public Dictionary<string, string?> SystemInfo() =>
        this.Where(x => x.Key.StartsWith(HeaderKeys.SystemPrefix)).ToDictionary(x => x.Key, x => x.Value);
}
