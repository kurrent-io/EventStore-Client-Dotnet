using KurrentDB.Client.Core.Serialization.Schema;

namespace KurrentDB.Client.Tests.Core.Serialization.Schema;

public class SchemaNameStrategyTests {
	[Theory]
	[MemberData(nameof(MessageSchemaNameTestCases))]
	public void MessageSchemaName(Type messageType, SchemaNameOutputFormat format, string streamName, string expectedSchemaName) {
		var strategy = new MessageSchemaNameStrategy(format);

		var schemaName = strategy.GenerateSchemaName(messageType, streamName);
		Assert.Equal(expectedSchemaName, schemaName);
	}
	
	[Theory]
	[MemberData(nameof(CategorySchemaNameTestCases))]
	public void CategorySchemaName(Type messageType, SchemaNameOutputFormat format, string streamName, string expectedSchemaName) {
		var strategy = new CategorySchemaNameStrategy(format);

		var schemaName = strategy.GenerateSchemaName(messageType, streamName);
		Assert.Equal(expectedSchemaName, schemaName);
	}
	
	[Theory]
	[InlineData(null), InlineData(""), InlineData(" "), InlineData("\t")]
	public void CategorySchemaName_Throws_WhenStreamNameIsEmpty(string? streamName) {
		var strategy = new CategorySchemaNameStrategy();
		Assert.Throws<ArgumentException>(() => strategy.GenerateSchemaName(typeof(UserRegistered), streamName));
	}
	
	[Theory]
	[MemberData(nameof(NamespaceSchemaNameTestCases))]
	public void NamespaceSchemaName(Type messageType, SchemaNameOutputFormat format, string messageNamespace, string streamName, string expectedSchemaName) {
		var strategy = new NamespaceSchemaNameStrategy(messageNamespace, format);

		var schemaName = strategy.GenerateSchemaName(messageType, streamName);
		Assert.Equal(expectedSchemaName, schemaName);
	}
	
	[Theory]
	[InlineData(null), InlineData(""), InlineData(" "), InlineData("\t")]
	public void NamespaceSchemaName_Throws_WhenNamespaceIsEmpty(string? messageNamespace) {
		Assert.Throws<ArgumentException>(() => new NamespaceSchemaNameStrategy(messageNamespace!));
	}
	
	[Theory]
	[MemberData(nameof(NamespaceCategorySchemaNameTestCases))]
	public void NamespaceCategorySchemaName(Type messageType, SchemaNameOutputFormat format, string messageNamespace, string streamName, string expectedSchemaName) {
		var strategy = new NamespaceCategorySchemaNameStrategy(messageNamespace, format);

		var schemaName = strategy.GenerateSchemaName(messageType, streamName);
		Assert.Equal(expectedSchemaName, schemaName);
	}
	
	[Theory]
	[InlineData(null), InlineData(""), InlineData(" "), InlineData("\t")]
	public void NamespaceCategorySchemaName_Throws_WhenNamespaceIsEmpty(string? messageNamespace) {
		Assert.Throws<ArgumentException>(() => new NamespaceCategorySchemaNameStrategy(messageNamespace!));
	}
	
	[Theory]
	[InlineData(null), InlineData(""), InlineData(" "), InlineData("\t")]
	public void NamespaceCategorySchemaNamee_Throws_WhenStreamNameIsEmpty(string? streamName) {
		var strategy = new NamespaceCategorySchemaNameStrategy("test");
		Assert.Throws<ArgumentException>(() => strategy.GenerateSchemaName(typeof(UserRegistered), streamName));
	}

	public static TheoryData<Type, SchemaNameOutputFormat, string, string> MessageSchemaNameTestCases() {
		var messageType = typeof(UserRegistered);
		var streamName  = "user-123abc";
		
		return new TheoryData<Type, SchemaNameOutputFormat, string, string>
		{
			{ messageType, SchemaNameOutputFormat.None, null!, "KurrentDB.Client.Tests.Core.Serialization.Schema.UserRegistered" },
			{ messageType, SchemaNameOutputFormat.None, streamName, "KurrentDB.Client.Tests.Core.Serialization.Schema.UserRegistered" },
			{ messageType, SchemaNameOutputFormat.KebabCase, streamName, "kurrent-db.client.tests.core.serialization.schema.user-registered" },
			{ messageType, SchemaNameOutputFormat.SnakeCase, streamName, "kurrent_db.client.tests.core.serialization.schema.user_registered" },
			{ messageType, SchemaNameOutputFormat.Urn, streamName, "urn:kurrent_db.client.tests.core.serialization.schema:user_registered" }
		};
	}
	
	public static TheoryData<Type, SchemaNameOutputFormat, string, string> CategorySchemaNameTestCases() {
		var messageType = typeof(UserRegistered);
		var streamName  = "user-123abc";
		
		return new TheoryData<Type, SchemaNameOutputFormat, string, string>
		{
			{ messageType, SchemaNameOutputFormat.None, streamName, "user.UserRegistered" },
			{ messageType, SchemaNameOutputFormat.KebabCase, streamName, "user.user-registered" },
			{ messageType, SchemaNameOutputFormat.SnakeCase, streamName, "user.user_registered" },
			{ messageType, SchemaNameOutputFormat.Urn, streamName, "urn:user:user_registered" }
		};
	}
	
	public static TheoryData<Type, SchemaNameOutputFormat, string, string, string> NamespaceSchemaNameTestCases() {
		var messageType = typeof(UserRegistered);
		var messageNamespace = "identity";
		var streamName  = "user-123abc";
		
		return new TheoryData<Type, SchemaNameOutputFormat, string, string, string>
		{
			{ messageType, SchemaNameOutputFormat.None, messageNamespace, null!, "identity.UserRegistered" },
			{ messageType, SchemaNameOutputFormat.None, messageNamespace, streamName, "identity.UserRegistered" },
			{ messageType, SchemaNameOutputFormat.KebabCase, messageNamespace, streamName, "identity.user-registered" },
			{ messageType, SchemaNameOutputFormat.SnakeCase, messageNamespace, streamName, "identity.user_registered" },
			{ messageType, SchemaNameOutputFormat.Urn, messageNamespace, streamName, "urn:identity:user_registered" }
		};
	}
	
	public static TheoryData<Type, SchemaNameOutputFormat, string, string, string> NamespaceCategorySchemaNameTestCases() {
		var messageType      = typeof(UserRegistered);
		var messageNamespace = "identity";
		var streamName       = "user-123abc";
		
		return new TheoryData<Type, SchemaNameOutputFormat, string, string, string>
		{
			{ messageType, SchemaNameOutputFormat.None, messageNamespace, streamName, "identity.user.UserRegistered" },
			{ messageType, SchemaNameOutputFormat.KebabCase, messageNamespace, streamName, "identity.user.user-registered" },
			{ messageType, SchemaNameOutputFormat.SnakeCase, messageNamespace, streamName, "identity.user.user_registered" },
			{ messageType, SchemaNameOutputFormat.Urn, messageNamespace, streamName, "urn:identity.user:user_registered" }
		};
	}
	
	class UserRegistered;
}
