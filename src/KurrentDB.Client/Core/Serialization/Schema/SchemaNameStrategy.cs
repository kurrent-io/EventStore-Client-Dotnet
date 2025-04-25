using System.Text.RegularExpressions;

namespace KurrentDB.Client.Core.Serialization.Schema;

/// <summary>
/// Defines the format options for schema name generation, which determine
/// how schema names are composed and represented.
/// </summary>
public enum SchemaNameOutputFormat {
	None,
	KebabCase,
	SnakeCase,
	Urn
}

public interface ISchemaNameStrategy {
	/// <summary>
	/// Generates a schema name based on the specified message type and optional stream name,
	/// formatted according to the schema naming strategy.
	/// </summary>
	/// <param name="messageType">The type of the message for which the schema name is being generated. Must not be null.</param>
	/// <param name="streamName">An optional stream name that influences the schema name generation. Defaults to null.</param>
	/// <returns>The generated schema name as a string, formatted according to the specified schema name format.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the provided message type is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when the schema name format is unsupported.</exception>
	string GenerateSchemaName(Type messageType, string? streamName = null);
}

/// <summary>
/// Base class for schema naming strategies
/// </summary>
public abstract class SchemaNameStrategy(SchemaNameOutputFormat format) : ISchemaNameStrategy {
	/// <summary>
	/// Generates a schema name based on the specified message type and optional stream name,
	/// formatted according to the schema naming strategy.
	/// </summary>
	/// <param name="messageType">The type of the message for which the schema name is being generated. Must not be null.</param>
	/// <param name="streamName">An optional stream name that influences the schema name generation. Defaults to null.</param>
	/// <returns>The generated schema name as a string, formatted according to the specified schema name format.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the provided message type is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when the schema name format is unsupported.</exception>
	public string GenerateSchemaName(Type messageType, string? streamName = null) {
		if (messageType is null || messageType == Type.Missing.GetType())
			throw new ArgumentNullException(nameof(messageType));

		var (nid, nss) = Generate(messageType, streamName ?? string.Empty);

		return format switch {
			SchemaNameOutputFormat.None      => $"{nid}.{nss}",
			SchemaNameOutputFormat.KebabCase => $"{nid.ToKebabCase()}.{nss.ToKebabCase()}",
			SchemaNameOutputFormat.SnakeCase => $"{nid.ToSnakeCase()}.{nss.ToSnakeCase()}",
			SchemaNameOutputFormat.Urn       => SchemaNameUrn.Create(nid.AsSpan(), nss.AsSpan()),
			_                                => throw new ArgumentOutOfRangeException()
		};
	}

	protected abstract (string Nid, string Nss) Generate(Type messageType, string streamName);
}

/// <summary>
/// A schema naming strategy that generates schema names based on the type's full name,
/// optionally formatted according to the specified schema name format.
/// </summary>
/// <param name="format">The format specification for the schema name, determining how the schema name is structured.</param>
public class MessageSchemaNameStrategy(SchemaNameOutputFormat format = SchemaNameOutputFormat.None) : SchemaNameStrategy(format) {
	protected override (string Nid, string Nss) Generate(Type messageType, string streamName) {
		return (messageType.Namespace!, messageType.Name);
	}
}

/// <summary>
/// Schema naming strategy that derives the schema name based on a specified
/// category from the stream name and the name of the message type.
/// </summary>
/// <remarks>
/// This strategy uses the first segment of the stream name, separated by a hyphen ('-'),
/// as the namespace identifier and combines it with the name of the message type.
/// A non-empty stream name is required for this strategy to function correctly.
/// </remarks>
/// <param name="format">The format specification for the schema name, determining how the schema name is structured.</param>
/// <exception cref="ArgumentException">Thrown if the stream name is empty or white space.</exception>
public class CategorySchemaNameStrategy(SchemaNameOutputFormat format = SchemaNameOutputFormat.None) : SchemaNameStrategy(format) {
	protected override (string Nid, string Nss) Generate(Type messageType, string streamName) {
		if (string.IsNullOrWhiteSpace(streamName))
			throw new ArgumentException("Stream name cannot be empty or whitespace", nameof(streamName));

		return (streamName.Split('-').First(), messageType.Name);
	}
}

/// <summary>
/// A schema naming strategy that uses the provided namespace identifier combined with the message type name
/// to generate a unique schema name.
/// </summary>
public class NamespaceSchemaNameStrategy : SchemaNameStrategy {
	readonly string _namespaceIdentifier;

	/// <summary>
	/// A schema naming strategy that uses the provided namespace identifier combined with the message type name
	/// to generate a unique schema name.
	/// </summary>
	/// <param name="namespaceIdentifier">The fixed namespace identifier to be used as part of the schema name.</param>
	/// <param name="format">The format specification for the schema name, determining how the schema name is structured.</param>
	/// <exception cref="ArgumentException">Thrown if the namespace identifier is empty or white space.</exception>
	public NamespaceSchemaNameStrategy(string namespaceIdentifier, SchemaNameOutputFormat format = SchemaNameOutputFormat.None) : base(format) {
		if (string.IsNullOrWhiteSpace(namespaceIdentifier))
			throw new ArgumentException("Namespace Identifier cannot be null or whitespace", nameof(namespaceIdentifier));

		_namespaceIdentifier = namespaceIdentifier;
	}

	protected override (string Nid, string Nss) Generate(Type messageType, string streamName) =>
		(_namespaceIdentifier, messageType.Name);
}

/// <summary>
/// Schema naming strategy that generates schema names by combining a fixed namespace identifier with a category derived
/// from the stream name and the message type name.
/// </summary>
public class NamespaceCategorySchemaNameStrategy : SchemaNameStrategy {
	readonly string _namespaceIdentifier;

	/// <summary>
	/// Schema naming strategy that generates schema names by combining a fixed namespace identifier with a category derived
	/// from the stream name and the message type name.
	/// </summary>
	public NamespaceCategorySchemaNameStrategy(string namespaceIdentifier, SchemaNameOutputFormat format = SchemaNameOutputFormat.None) : base(format) {
		if (string.IsNullOrWhiteSpace(namespaceIdentifier))
			throw new ArgumentException("Namespace Identifier cannot be null or whitespace", nameof(namespaceIdentifier));

		_namespaceIdentifier = namespaceIdentifier;
	}

	protected override (string Nid, string Nss) Generate(Type messageType, string streamName) {
		if (string.IsNullOrWhiteSpace(streamName))
			throw new ArgumentException("Stream name cannot be empty or whitespace", nameof(streamName));

		return ($"{_namespaceIdentifier}.{streamName.Split('-').First()}", messageType.Name);
	}
}

/// <summary>
/// Provides factory methods for creating instances of different schema name strategies.
/// These strategies define how schema names are generated and represented based on the schema format and type-specific rules.
/// </summary>
public static class SchemaNameStrategies {
	public static SchemaNameStrategy MessageSchemaNameStrategy(SchemaNameOutputFormat format = SchemaNameOutputFormat.None) =>
		new MessageSchemaNameStrategy(format);

	public static SchemaNameStrategy CategorySchemaNameStrategy(SchemaNameOutputFormat format = SchemaNameOutputFormat.None) =>
		new CategorySchemaNameStrategy(format);

	public static SchemaNameStrategy NamespaceSchemaNameStrategy(string namespaceIdentifier, SchemaNameOutputFormat format = SchemaNameOutputFormat.None) =>
		new NamespaceSchemaNameStrategy(namespaceIdentifier, format);

	public static SchemaNameStrategy NamespaceCategorySchemaNameStrategy(string namespaceIdentifier, SchemaNameOutputFormat format = SchemaNameOutputFormat.None) =>
		new NamespaceCategorySchemaNameStrategy(namespaceIdentifier, format);
}
