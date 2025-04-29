namespace Kurrent.Surge.Schema.Serializers;

/// <summary>
/// Defines the interface for a schema serializer.
/// </summary>
public interface ISchemaSerializer {
    /// <summary>
    /// The type of the schema.
    /// </summary>
    public SchemaDataFormat SchemaDataFormat { get; }

    /// <summary>
    /// Serializes the given value into bytes according to the provided context.
    /// </summary>
    /// <param name="value">The object to be serialized.</param>
    /// <param name="context">The context providing additional information for the serialization process.</param>
    public ValueTask<ReadOnlyMemory<byte>> Serialize(object? value, SerializationContext context);

    /// <summary>
    /// Deserializes the given bytes into an object according to the provided context.
    /// </summary>
    /// <param name="data">The bytes to be deserialized.</param>
    /// <param name="context">The context providing additional information for the deserialization process.</param>
    public ValueTask<object?> Deserialize(ReadOnlyMemory<byte> data, SerializationContext context);

    // /// <summary>
    // /// Deserializes the given stream of data into an object of the specified type using the provided context.
    // /// </summary>
    // /// <param name="data">The stream containing the serialized data to be deserialized.</param>
    // /// <param name="type">The type of the object to deserialize into.</param>
    // /// <param name="context">The context providing additional information for the deserialization process.</param>
    // public ValueTask<object?> Deserialize(Stream data, Type type, SerializationContext context);
}

public record DeserializeResult {
    public record Deserialized : DeserializeResult {
	    public object Value     { get; init; } = null!;
        public Type   ValueType { get; init; } = null!;
    }

    public record Empty : DeserializeResult;

    public record Error : DeserializeResult {
        public Exception Exception { get; init; } = null!;
    }
}

/// <summary>
/// Delegate for the Serialize method.
/// </summary>
/// <param name="value">The object to be serialized.</param>
/// <param name="headers">The headers providing additional information for the serialization process.</param>
public delegate ValueTask<ReadOnlyMemory<byte>> Serialize(object? value, Headers headers);

/// <summary>
/// Delegate for the Deserialize method.
/// </summary>
/// <param name="data">The bytes to be deserialized.</param>
/// <param name="headers">The headers providing additional information for the deserialization process.</param>
public delegate ValueTask<object?> Deserialize(ReadOnlyMemory<byte> data, Headers headers);
