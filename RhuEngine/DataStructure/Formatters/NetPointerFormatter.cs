
using MessagePack;
using MessagePack.Formatters;

using RhuEngine.Datatypes;

using SharedModels;
using SharedModels.GameSpecific;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class NetPointerFormatter : IMessagePackFormatter<NetPointer>
	{
		public void Serialize(ref MessagePackWriter writer, NetPointer value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<ulong>().Serialize(ref writer, value.id, options);
		}

		public NetPointer Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var id = options.Resolver.GetFormatterWithVerify<ulong>().Deserialize(ref reader, options);
			return new NetPointer { id = id };
		}
	}
}
