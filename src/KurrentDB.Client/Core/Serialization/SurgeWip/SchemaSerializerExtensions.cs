namespace Kurrent.Surge.Schema.Serializers;

public static class SchemaSerializerExtensions {
    /// <summary>
    /// Serializes the given value into bytes according to the provided headers.
    /// </summary>
    /// <param name="value">The object to be serialized.</param>
    /// <param name="headers">The headers providing additional information for the serialization process.</param>
    /// <returns>A byte memory representing the serialized object.</returns>
    public static ValueTask<ReadOnlyMemory<byte>> Serialize(this ISchemaSerializer serializer, object? value, Headers headers) =>
        serializer.Serialize(value, SerializationContext.From(headers));
    
    /// <summary>
    /// Deserializes the given bytes into an object according to the provided headers.
    /// </summary>
    /// <param name="data">The bytes to be deserialized.</param>
    /// <param name="headers">The headers providing additional information for the deserialization process.</param>
    /// <returns>The deserialized object.</returns>
    public static ValueTask<object?> Deserialize(this ISchemaSerializer serializer, ReadOnlyMemory<byte> data, Headers headers) =>
        serializer.Deserialize(data, SerializationContext.From(headers));
    
    /// <summary>
    /// Serializes the given value into a bytes according to the provided schema information.
    /// </summary>
    /// <param name="value">The object to be serialized.</param>
    /// <param name="schemaInfo">The schema information providing additional information for the serialization process.</param>
    public static ValueTask<ReadOnlyMemory<byte>> Serialize(this ISchemaSerializer serializer, object? value, SchemaInfo schemaInfo) =>
        serializer.Serialize(value, SerializationContext.From(schemaInfo));
    
    /// <summary>
    /// Deserializes the given bytes into an object according to the provided schema information.
    /// </summary>
    /// <param name="data">The bytes to be deserialized.</param>
    /// <param name="schemaInfo">The schema information providing additional information for the deserialization process.</param>
    public static ValueTask<object?> Deserialize(this ISchemaSerializer serializer, ReadOnlyMemory<byte> data, SchemaInfo schemaInfo) =>
        serializer.Deserialize(data,SerializationContext.From(schemaInfo) );
    
    /// <summary>
    /// Attempts to deserialize the provided byte data into an object using the specified schema information.
    /// </summary>
    /// <param name="serializer">The schema serializer instance used for deserialization.</param>
    /// <param name="data">The byte data to be deserialized.</param>
    /// <param name="schemaInfo">Information about the schema, such as name and format, used for deserialization.</param>
    /// <returns>
    /// A <see cref="DeserializeResult"/> indicating the outcome of the deserialization process,
    /// which could be a successful deserialization, an empty result, or an error containing additional exception details.
    /// </returns>
    public static async ValueTask<DeserializeResult> TryDeserialize(this ISchemaSerializer serializer, ReadOnlyMemory<byte> data, SchemaInfo schemaInfo) {
        try {
            var result = await serializer.Deserialize(data, schemaInfo).ConfigureAwait(false);
            return result switch {
                null => new DeserializeResult.Empty(),
                _    => new DeserializeResult.Deserialized { Value = result, ValueType = result.GetType() }
            };
        }
        catch (Exception ex) {
            return new DeserializeResult.Error { Exception = ex };
        }
    }
}
