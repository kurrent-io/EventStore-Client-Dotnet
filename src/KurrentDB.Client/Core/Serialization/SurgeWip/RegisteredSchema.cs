namespace Kurrent.Surge.Schema;

public record RegisteredSchema {
	public static readonly RegisteredSchema None = new();

	public string           Subject    { get; init; } = null!;
	public SchemaDataFormat SchemaType { get; init; }
	public string           RevisionId { get; init; } = null!;
	public string           Definition { get; init; } = null!;
	public int              Version    { get; init; }
	public DateTimeOffset   CreatedAt  { get; init; }

    public SchemaInfo ToSchemaInfo() => new(Subject, SchemaType);
}
