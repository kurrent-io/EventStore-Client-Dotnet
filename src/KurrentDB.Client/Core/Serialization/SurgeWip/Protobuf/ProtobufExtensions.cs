using System.Collections.Concurrent;
using System.Reflection;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Kurrent.Surge.Schema.Serializers.Protobuf;

public static class ProtobufExtensions {
	public static MessageDescriptor GetProtoMessageDescriptor(this Type messageType) =>
        ProtobufMessages.System.GetDescriptor(messageType);
	
	public static bool IsProtoMessage(this Type messageType) =>
		typeof(IMessage).IsAssignableFrom(messageType);
}

class ProtobufMessages {
    public static ProtobufMessages System { get; } = new();

    ConcurrentDictionary<Type, (MessageParser Parser, MessageDescriptor Descriptor)> Types { get; } = new();

    public MessageDescriptor GetDescriptor(Type messageType) =>
        Types.GetOrAdd(messageType, GetContext).Descriptor;

    static (MessageParser Parser, MessageDescriptor Descriptor) GetContext(Type messageType) {
        return (GetMessageParser(messageType), GetMessageDescriptor(messageType));

        static MessageParser GetMessageParser(Type messageType) =>
            (MessageParser) messageType
                .GetProperty("Parser", BindingFlags.Public | BindingFlags.Static)!
                .GetValue(null)!;

        static MessageDescriptor GetMessageDescriptor(Type messageType) =>
            (MessageDescriptor) messageType
                .GetProperty("Descriptor", BindingFlags.Public | BindingFlags.Static)!
                .GetValue(null)!;
    }
}
