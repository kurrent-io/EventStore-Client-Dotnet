using JetBrains.Annotations;

namespace Kurrent.Surge.Schema.Serializers;

/// <summary>
/// Allows the serialization operations to be autonomous.
/// </summary>
[PublicAPI]
public readonly record struct SerializationContext(Headers Headers, CancellationToken CancellationToken = default) {
    /// <summary>
    /// The headers present in the record.
    /// </summary>
    public Headers Headers { get; } = Headers;

    /// <summary>
    /// The token used to propagate cancellation notifications in the serialization context.
    /// </summary>
    public CancellationToken CancellationToken { get; } = CancellationToken;

    /// <summary>
    /// The schema information extracted from the headers.
    /// If the headers do not contain schema information, it will return an undefined schema information.
    /// </summary>
    public SchemaInfo SchemaInfo => SchemaInfo.FromHeaders(Headers);

    /// <summary>
    /// Creates a new instance of the <see cref="SerializationContext"/> record struct with the provided headers.
    /// </summary>
    /// <param name="headers">The headers to be included in the serialization context.</param>
    public static SerializationContext From(Headers headers) => new(headers);

    /// <summary>
    /// Creates a new instance of the <see cref="SerializationContext"/> record struct with the provided schema information.
    /// The schema information is added to a new instance of the <see cref="Headers"/> class which is then used to create the serialization context.
    /// </summary>
    /// <param name="schemaInfo">The schema information to be included in the serialization context.</param>
    public static SerializationContext From(SchemaInfo schemaInfo) =>
        From(new Headers().WithSchemaInfo(schemaInfo));
}
