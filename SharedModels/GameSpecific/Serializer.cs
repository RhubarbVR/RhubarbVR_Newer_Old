using System;
using System.Linq;
using System.Reflection;

using MessagePack;
using MessagePack.Formatters;

namespace SharedModels.GameSpecific
{

	public class FormatterAttribute : Attribute
	{
	}
	public static class Serializer
	{
		public static bool TryToRead<T>(byte[] data,out T value) {
			try {
				value = MessagePackSerializer.Deserialize<T>(data, Options);
				return true;
			}
			catch {
				value = default;
				return false;
			}
		}

		public static T Read<T>(byte[] data) {
			return MessagePackSerializer.Deserialize<T>(data, Options);
		}
		public static MessagePackSerializerOptions Options = SerializerOptions();


		public static MessagePackSerializerOptions SerializerOptions() {
			var data = from e in AppDomain.CurrentDomain.GetAssemblies().AsParallel().SelectMany(x => x.GetTypes())
					   where typeof(IMessagePackFormatter).IsAssignableFrom(e)
					   where e.GetCustomAttribute<FormatterAttribute>() is not null
					   select (IMessagePackFormatter)Activator.CreateInstance(e);

			var custom = MessagePack.Resolvers.CompositeResolver.Create(data.ToArray());

			var resolver = MessagePack.Resolvers.CompositeResolver.Create(
				MessagePack.Resolvers.DynamicUnionResolver.Instance,
							MessagePack.Resolvers.AttributeFormatterResolver.Instance,
							MessagePack.Resolvers.DynamicEnumAsStringResolver.Instance,
							custom,
							MessagePack.Resolvers.StandardResolver.Instance
			);
			var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
			var lz4Options = options.WithCompression(MessagePackCompression.Lz4BlockArray);
			return lz4Options;
		}

		public static bool TrySave<T>(T data,out byte[]  outData) {
			try {
				outData = MessagePackSerializer.Serialize(data, Options);
				return true;
			}
			catch {
				outData = null;
				return false;
			}
		}
		public static byte[] Save<T>(T data) {
			return MessagePackSerializer.Serialize(data, Options);
		}
	}
}
