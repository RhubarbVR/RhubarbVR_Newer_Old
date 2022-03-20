using System;
using System.Collections.Generic;
using System.Text;

using MessagePack;
using MessagePack.Formatters;

namespace SharedModels.GameSpecific
{

	[Formatter]
	public class OtherUserLeftFormatter : IMessagePackFormatter<OtherUserLeft>
	{
		public void Serialize(ref MessagePackWriter writer, OtherUserLeft value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<ushort>().Serialize(ref writer,value.id, options);
		}

		public OtherUserLeft Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var id = options.Resolver.GetFormatterWithVerify<ushort>().Deserialize(ref reader, options);
			return new OtherUserLeft(id);
		}
	}
	public struct OtherUserLeft
	{
		public ushort id;
		public OtherUserLeft(ushort id) : this() {
			this.id = id;
		}
	}
}
