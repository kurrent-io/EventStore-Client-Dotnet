using JetBrains.Annotations;

namespace Kurrent.Surge;

[PublicAPI]
public static class HeaderKeys {
    public const string SystemPrefix = "$";

    public const string ProducerId        = $"{SystemPrefix}producer.id";
    public const string ProducerRequestId = $"{SystemPrefix}producer.request-id";

    public const string SchemaName       = $"{SystemPrefix}schema.name";         // replaces legacy event type and legacy surge schema subject
    public const string SchemaDataFormat = $"{SystemPrefix}schema.data-format";  // replaces legacy content type and legacy schema type
    public const string SchemaVersionId  = $"{SystemPrefix}schema.version-id";
    public const string SchemaId         = $"{SystemPrefix}schema.id";           // combination of the schema name and version id in a single key in urn format

    public const string PartitionKey    = $"{SystemPrefix}record.partition-key"; // partition key placeholder
    public const string RecordTimestamp = $"{SystemPrefix}record.timestamp";     // replaces legacy created time

    // legacy keys
    public const string LegacyEventMetadata = $"{SystemPrefix}legacy-metadata";

    [Obsolete("Use 'SchemaName' instead.")]
    public const string SchemaSubject = "kurrentdb.schema.subject";

    [Obsolete("Use 'SchemaDataFormat' instead.")]
    public const string SchemaType = "kurrentdb.schema.type";
}
