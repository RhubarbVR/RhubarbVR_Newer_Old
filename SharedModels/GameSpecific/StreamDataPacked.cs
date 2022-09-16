using System;
using System.Collections.Generic;
using System.Text;

using MessagePack;
using MessagePack.Formatters;

namespace SharedModels.GameSpecific
{

	[Formatter]
	public sealed class StreamDataPackedformat : IMessagePackFormatter<StreamDataPacked>
	{
		public void Serialize(ref MessagePackWriter writer, StreamDataPacked value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<byte[]>().Serialize(ref writer, value.Data, options);
		}

		public StreamDataPacked Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var data = options.Resolver.GetFormatterWithVerify<byte[]>().Deserialize(ref reader, options);
			return new StreamDataPacked(data);
		}
	}
	public struct StreamDataPacked
	{
		public byte[] Data;
		public StreamDataPacked(byte[] data) : this() {
			Data = data;
		}
	}
}
