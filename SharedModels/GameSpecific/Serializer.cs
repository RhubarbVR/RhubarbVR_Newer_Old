using System;
using System.Linq;
using System.Numerics;
using System.Reflection;

using MessagePack;
using MessagePack.Formatters;

namespace SharedModels.GameSpecific
{
	[Formatter]
	public sealed class MatrixSaver : IMessagePackFormatter<Matrix4x4>
	{
		public Matrix4x4 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			return new Matrix4x4(
				reader.ReadSingle(),
				reader.ReadSingle(),
				reader.ReadSingle(),
				reader.ReadSingle(),
				reader.ReadSingle(),
				reader.ReadSingle(),
				reader.ReadSingle(),
				reader.ReadSingle(),
				reader.ReadSingle(),
				reader.ReadSingle(),
				reader.ReadSingle(),
				reader.ReadSingle(),
				reader.ReadSingle(),
				reader.ReadSingle(),
				reader.ReadSingle(),
				reader.ReadSingle()
				);


		}

		public void Serialize(ref MessagePackWriter writer, Matrix4x4 value, MessagePackSerializerOptions options) {
			writer.Write(value.M11);
			writer.Write(value.M12);
			writer.Write(value.M13);
			writer.Write(value.M14);
			writer.Write(value.M21);
			writer.Write(value.M22);
			writer.Write(value.M23);
			writer.Write(value.M24);
			writer.Write(value.M31);
			writer.Write(value.M32);
			writer.Write(value.M33);
			writer.Write(value.M34);
			writer.Write(value.M41);
			writer.Write(value.M42);
			writer.Write(value.M43);
			writer.Write(value.M44);
		}
	}
	public class FormatterAttribute : Attribute
	{
	}
	public static class Serializer
	{
		public static bool TryToRead<T>(byte[] data, out T value) {
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

		private static MessagePackSerializerOptions _options;

		public static MessagePackSerializerOptions Options => _options ??= SerializerOptions();


		public static MessagePackSerializerOptions SerializerOptions() {
			var data = from e in AppDomain.CurrentDomain.GetAssemblies().AsParallel()
					   .SelectMany(x => {
				//Mono is broke and this is to get around it
				try { return x.GetTypes(); }
				catch { return Array.Empty<Type>(); }
			})
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

		public static bool TrySave<T>(T data, out byte[] outData) {
			try {
				outData = MessagePackSerializer.Serialize<T>(data, Options);
				return true;
			}
			catch {
				outData = null;
				return false;
			}
		}
		public static byte[] Save<T>(T data) {
			return MessagePackSerializer.Serialize<T>(data, Options);
		}
	}
}
