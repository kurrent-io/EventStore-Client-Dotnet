#pragma warning disable CS0618 // Type or member is obsolete

using System.Net.Http.Headers;
using JetBrains.Annotations;

namespace Kurrent.Surge.Schema;

[PublicAPI]
public record SchemaInfo(string SchemaName, SchemaDataFormat SchemaDataFormat) {
	internal static readonly MediaTypeHeaderValue JsonContentTypeHeader     = new("application/json");
    internal static readonly MediaTypeHeaderValue ProtobufContentTypeHeader = new("application/vnd.google.protobuf");
    internal static readonly MediaTypeHeaderValue AvroContentTypeHeader     = new("application/vnd.apache.avro+json");
    internal static readonly MediaTypeHeaderValue BytesContentTypeHeader    = new("application/octet-stream");

	public static readonly SchemaInfo None = new("", SchemaDataFormat.Unspecified);

	public MediaTypeHeaderValue ContentTypeHeader { get; } = SchemaDataFormat switch {
		SchemaDataFormat.Json     => JsonContentTypeHeader,
		SchemaDataFormat.Protobuf => ProtobufContentTypeHeader,
		_                         => BytesContentTypeHeader,
	};

	public string ContentType => ContentTypeHeader.MediaType!;

    public bool SchemaNameMissing => string.IsNullOrWhiteSpace(SchemaName);

	public SchemaInfo InjectIntoHeaders(Headers headers) {
		headers.Set(HeaderKeys.SchemaName, SchemaName);
		headers.Set(HeaderKeys.SchemaDataFormat, SchemaDataFormat.ToString().ToLower());
		return this;
	}

    public SchemaInfo InjectSchemaNameIntoHeaders(Headers headers) {
        headers.Set(HeaderKeys.SchemaName, SchemaName);
        return this;
    }

	public static SchemaInfo FromHeaders(Headers headers) {
		return new(ExtractSchemaName(headers), ExtractSchemaDataFormat(headers));

		static string ExtractSchemaName(Headers headers) {
			return headers.GetString(
				HeaderKeys.SchemaName,
				headers.GetString(HeaderKeys.SchemaSubject, string.Empty)
			);
		}

		static SchemaDataFormat ExtractSchemaDataFormat(Headers headers) {
			return headers.GetEnum(
				HeaderKeys.SchemaDataFormat,
				headers.GetEnum(HeaderKeys.SchemaType, SchemaDataFormat.Unspecified)
			);
		}
	}

	/// <summary>
	/// For legacy purposes, we need to be able to create the schema info from the content type.
	/// </summary>
	public static SchemaInfo FromContentType(string schemaName, string contentType) {
		if (string.IsNullOrEmpty(schemaName))
			throw new ArgumentNullException(nameof(schemaName));
		
		if (string.IsNullOrEmpty(contentType))
			throw new ArgumentNullException(nameof(contentType));

		var schemaDataFormat = contentType == JsonContentTypeHeader.MediaType
			? SchemaDataFormat.Json
			: contentType == ProtobufContentTypeHeader.MediaType
				? SchemaDataFormat.Protobuf
				: contentType == BytesContentTypeHeader.MediaType
					? SchemaDataFormat.Bytes
					: SchemaDataFormat.Unspecified;

		return new(schemaName, schemaDataFormat);
	}
}
