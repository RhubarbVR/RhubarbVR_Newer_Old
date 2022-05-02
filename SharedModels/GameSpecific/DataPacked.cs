using System;
using System.Collections.Generic;
using System.Text;

using MessagePack;
using MessagePack.Formatters;

namespace SharedModels.GameSpecific
{
	[Formatter]
	public class DataPackedFormatter : IMessagePackFormatter<DataPacked>
	{
		public void Serialize(ref MessagePackWriter writer, DataPacked value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<(byte[],ushort)>().Serialize(ref writer, (value.Data,value.Id), options);
		}

		public DataPacked Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (data,id) = options.Resolver.GetFormatterWithVerify<(byte[], ushort)>().Deserialize(ref reader, options);
			return new DataPacked(data,id);
		}
	}
	public struct DataPacked
	{
		public byte[] Data;
		public ushort Id;
		public DataPacked(byte[] data, ushort id) : this() {
			Data = data;
			Id = id;
		}
	}
}
