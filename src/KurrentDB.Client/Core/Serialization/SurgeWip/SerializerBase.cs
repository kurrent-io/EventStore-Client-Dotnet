namespace Kurrent.Surge.Schema.Serializers;

public abstract class SerializerBase(SchemaRegistry? registry = null) : ISchemaSerializer {
	SchemaRegistry Registry { get; } = registry ?? SchemaRegistry.Global;

	public abstract SchemaDataFormat SchemaDataFormat { get; }

	async ValueTask<ReadOnlyMemory<byte>> ISchemaSerializer.Serialize(object? value, SerializationContext context) {
        if (context.SchemaInfo.SchemaDataFormat != SchemaDataFormat)
            throw new UnsupportedSchemaException(SchemaDataFormat, context.SchemaInfo.SchemaDataFormat);

        if (value is null)
			return ReadOnlyMemory<byte>.Empty;

        var messageType = value.GetType();

        // we don't really care about verifying or registering the schema for bytes because we gave control to the developer/user
        // the schema name is always set when passing bytes
        if (messageType == typeof(byte[]) || messageType == typeof(ReadOnlyMemory<byte>) || messageType == typeof(Memory<byte>))
            return (ReadOnlyMemory<byte>)value;

        var registeredSchema = await Registry.GetOrRegisterSchema(context.SchemaInfo, messageType).ConfigureAwait(false);

        if (context.SchemaInfo.SchemaNameMissing)
            context.Headers.Set(HeaderKeys.SchemaName, registeredSchema.Subject);

		try {
			return await Serialize(value, context).ConfigureAwait(false);
		}
		catch (Exception ex) {
			throw new SerializationFailedException(SchemaDataFormat, registeredSchema.ToSchemaInfo(),  ex);
		}
	}

	async ValueTask<object?> ISchemaSerializer.Deserialize(ReadOnlyMemory<byte> data, SerializationContext context) {
        if (context.SchemaInfo.SchemaDataFormat != SchemaDataFormat)
            throw new UnsupportedSchemaException(SchemaDataFormat, context.SchemaInfo.SchemaDataFormat);

        if (data.IsEmpty)
			return null;

		if (context.SchemaInfo.SchemaDataFormat == SchemaDataFormat.Bytes)
			return data;

		// TODO SS: the schema registry should be used to validate the message schema before deserializing
		var messageType = Registry.ResolveMessageType(context.SchemaInfo.SchemaName, context.SchemaInfo.SchemaDataFormat);

		try {
			return await Deserialize(data, messageType, context).ConfigureAwait(false);
		}
		catch (Exception ex) {
			throw new DeserializationFailedException(SchemaDataFormat, new SchemaInfo(context.SchemaInfo.SchemaName, context.SchemaInfo.SchemaDataFormat),  ex);
		}
	}

	protected abstract ValueTask<ReadOnlyMemory<byte>> Serialize(object? value, SerializationContext context);

	protected abstract ValueTask<object?> Deserialize(ReadOnlyMemory<byte> data, Type resolvedType, SerializationContext context);
}