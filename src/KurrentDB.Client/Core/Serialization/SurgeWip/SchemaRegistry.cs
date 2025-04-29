using System.Data;
using Kurrent.Surge.Protocol.Schema;
using Kurrent.Surge.Schema.Serializers.Bytes;
using Kurrent.Surge.Schema.Serializers.Json;
using Kurrent.Surge.Schema.Serializers.Protobuf;
using Google.Protobuf;
using JetBrains.Annotations;
using Kurrent.Surge.Schema.Serializers;
using KurrentDB.Client.Core.Serialization;

namespace Kurrent.Surge.Schema;

public interface ISchemaRegistry {
    SchemaRegistryOptions Options { get; }

    Type ResolveMessageType(string subject, SchemaDataFormat schemaType);

    ValueTask<RegisteredSchema> RegisterSchema(SchemaInfo schemaInfo, string definition, Type messageType, CancellationToken cancellationToken = default);

    ValueTask<RegisteredSchema> GetSchema(SchemaInfo schemaInfo, CancellationToken cancellationToken = default);

    ValueTask<RegisteredSchema> GetOrRegisterSchema(SchemaInfo schemaInfo, Type messageType);

    Task<List<RegisteredSchema>> ListMessageSchemas(
        Type messageType, SchemaDataFormat schemaType = SchemaDataFormat.Unspecified, CancellationToken cancellationToken = default
    );

    SchemaInfo CreateSchemaInfo(Type messageType, SchemaDataFormat dataFormat);

    ISchemaRegistry RegisterSerializer(ISchemaSerializer serializer);

    ISchemaSerializer GetSerializer(SchemaDataFormat dataFormat);

    bool SupportsSchema(SchemaDataFormat dataFormat);
}

public record SchemaRegistryOptions {
    public ISchemaSubjectNameStrategy SubjectNameStrategy { get; init; } = SchemaSubjectNameStrategies.FromMessage;
    public bool                       AutoRegister        { get; init; } = true;
}

[PublicAPI]
public class SchemaRegistry(ISchemaRegistryClient client, SchemaRegistryOptions options) : ISchemaSerializer, ISchemaRegistry {
    public static readonly SchemaRegistry Global = new SchemaRegistry(new InMemorySchemaRegistryClient(), new SchemaRegistryOptions())
        .UseBytes()
        .UseProtobuf()
        .UseJson();

    public static readonly Type MissingType = Type.Missing.GetType();

    public SchemaRegistry(ISchemaRegistryClient client) : this(client, new SchemaRegistryOptions()) { }

    public SchemaRegistry() : this(new InMemorySchemaRegistryClient(), new SchemaRegistryOptions()) { }

    ISchemaRegistryClient                           Client       { get; } = client;
    MessageTypeRegistry                             TypeRegistry { get; } = new();
    Dictionary<SchemaDataFormat, ISchemaSerializer> Serializers  { get; } = new();

    public SchemaRegistryOptions Options { get; } = options;

    public Type ResolveMessageType(string subject, SchemaDataFormat schemaType) =>
        TypeRegistry.TryGetMessageType(subject, schemaType, out var messageType) ? messageType : MissingType;

    public async ValueTask<RegisteredSchema> RegisterSchema(
        SchemaInfo schemaInfo, string definition, Type messageType, CancellationToken cancellationToken = default
    ) {
        if (schemaInfo.SchemaNameMissing)
            schemaInfo = schemaInfo with { SchemaName = Options.SubjectNameStrategy.GetSubjectName(messageType, StreamId.None) };

        var request = new CreateOrUpdateSchema {
            Subject    = schemaInfo.SchemaName,
            SchemaType = (SchemaType)schemaInfo.SchemaDataFormat,
            Definition = !string.IsNullOrWhiteSpace(definition) ? ByteString.CopyFromUtf8(definition) : ByteString.Empty
        };

        var response = await Client.CreateOrUpdateSchema(request, cancellationToken);

        var schema = new RegisteredSchema {
            Subject    = schemaInfo.SchemaName,
            SchemaType = schemaInfo.SchemaDataFormat,
            Definition = definition,
            RevisionId = response.RevisionId,
            Version    = response.Version,
            CreatedAt  = response.CreatedAt.ToDateTimeOffset()
        };

        TypeRegistry.Register(messageType, schemaInfo.SchemaName, schemaInfo.SchemaDataFormat);

        return schema;
    }

    public async ValueTask<RegisteredSchema> GetSchema(SchemaInfo schemaInfo, CancellationToken cancellationToken = default) {
        if (schemaInfo.SchemaNameMissing)
            throw new ArgumentException("The subject is missing from the schema info.", nameof(schemaInfo.SchemaName));

        var request = new GetLatestSchema {
            Subject    = schemaInfo.SchemaName,
            SchemaType = (SchemaType)schemaInfo.SchemaDataFormat
        };

        var response = await Client.GetLatestSchema(request, cancellationToken);

        if (response.Schema is null)
            return RegisteredSchema.None;

        return new RegisteredSchema {
            Subject    = schemaInfo.SchemaName,
            SchemaType = schemaInfo.SchemaDataFormat,
            Definition = response.Schema.Definition.ToStringUtf8(),
            RevisionId = response.Schema.RevisionId,
            Version    = response.Schema.Version,
            CreatedAt  = response.Schema.CreatedAt.ToDateTimeOffset()
        };
    }

    public async ValueTask<RegisteredSchema> GetOrRegisterSchema(SchemaInfo schemaInfo, Type messageType) {
        var registeredSchema = schemaInfo.SchemaNameMissing
            ? TypeRegistry.TryGetSubject(messageType, schemaInfo.SchemaDataFormat, out var foundSubject)
                ? await GetSchema(schemaInfo with { SchemaName = foundSubject })
                : RegisteredSchema.None
            : await GetSchema(schemaInfo);

        if (registeredSchema != RegisteredSchema.None)
            return registeredSchema;

        if (!Options.AutoRegister)
            throw new Exception($"The message schema for {messageType.FullName} is not registered and auto registration is disabled.");

        return await RegisterSchema(schemaInfo, "", messageType);
    }

    public async Task<List<RegisteredSchema>> ListMessageSchemas(
        Type messageType, SchemaDataFormat schemaType = SchemaDataFormat.Unspecified, CancellationToken cancellationToken = default
    ) {
        var result = TypeRegistry.TypesBySubject
            .Where(x => x.Value == messageType && (schemaType == SchemaDataFormat.Unspecified || x.Key.SchemaType == schemaType))
            .Select(x => new SchemaInfo(x.Key.Subject, x.Key.SchemaType))
            .ToList();

        var schemas = new List<RegisteredSchema>();

        foreach (var schemaInfo in result) {
            var schema = await GetSchema(schemaInfo, cancellationToken);
            schemas.Add(schema);
        }

        return schemas;
    }

    public SchemaInfo CreateSchemaInfo(Type messageType, SchemaDataFormat dataFormat) =>
        new(Options.SubjectNameStrategy.GetSubjectName(messageType, StreamId.None), dataFormat);

    #region . Serialization .

    public ISchemaRegistry RegisterSerializer(ISchemaSerializer serializer) {
        Serializers[serializer.SchemaDataFormat] = serializer;
        return this;
    }

    public ISchemaSerializer GetSerializer(SchemaDataFormat dataFormat) =>
        Serializers[dataFormat];

    public bool SupportsSchema(SchemaDataFormat dataFormat) =>
        Serializers.ContainsKey(dataFormat);

    #endregion . Serialization .

    #region . ISchemaSerializer .

    SchemaDataFormat ISchemaSerializer.SchemaDataFormat => SchemaDataFormat.Unspecified;

    public async ValueTask<ReadOnlyMemory<byte>> Serialize(object? value, SerializationContext context) {
        if (Serializers.TryGetValue(context.SchemaInfo.SchemaDataFormat, out var serializer))
            return await serializer.Serialize(value, context);

        throw new SerializerNotFoundException(context.SchemaInfo.SchemaDataFormat, Serializers.Keys.ToArray());
    }

    public async ValueTask<object?> Deserialize(ReadOnlyMemory<byte> data, SerializationContext context) {
        if (Serializers.TryGetValue(context.SchemaInfo.SchemaDataFormat, out var serializer))
            return await serializer.Deserialize(data, context);

        throw new SerializerNotFoundException(context.SchemaInfo.SchemaDataFormat, Serializers.Keys.ToArray());
    }

    #endregion . ISchemaSerializer .
}

public static class SchemaRegistryExtensions {
    public static ValueTask<RegisteredSchema> RegisterSchema<T>(
        this ISchemaRegistry registry, SchemaInfo schemaInfo, string definition = "", CancellationToken cancellationToken = default
    ) => registry.RegisterSchema(schemaInfo, definition, typeof(T), cancellationToken);

    public static ValueTask<RegisteredSchema> RegisterSchema<T>(
        this ISchemaRegistry registry,
        SchemaDataFormat schemaType,
        string               definition        = "",
        CancellationToken    cancellationToken = default
    ) => registry.RegisterSchema(new SchemaInfo("", schemaType), definition, typeof(T), cancellationToken);

    public static ValueTask<RegisteredSchema> GetSchema<T>(
        this ISchemaRegistry registry, SchemaDataFormat schemaType, CancellationToken cancellationToken = default
    ) =>
        registry.GetSchema(registry.CreateSchemaInfo<T>(schemaType), cancellationToken);

    public static Task<List<RegisteredSchema>> ListMessageSchemas<T>(
        this ISchemaRegistry registry, SchemaDataFormat schemaType = SchemaDataFormat.Unspecified, CancellationToken cancellationToken = default
    ) => registry.ListMessageSchemas(typeof(T), schemaType, cancellationToken);

    public static SchemaInfo CreateSchemaInfo<T>(this ISchemaRegistry registry, SchemaDataFormat schemaType) =>
        registry.CreateSchemaInfo(typeof(T), schemaType);
}
