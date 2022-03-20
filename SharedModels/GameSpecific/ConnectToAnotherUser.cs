using System;
using System.Collections.Generic;
using System.Text;

using MessagePack;
using MessagePack.Formatters;

namespace SharedModels.GameSpecific
{

	[Formatter]
	public class ConnectToAnotherUserFormatter : IMessagePackFormatter<ConnectToAnotherUser>
	{
		public void Serialize(ref MessagePackWriter writer, ConnectToAnotherUser value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<string>().Serialize(ref writer,value.Key, options);
		}

		public ConnectToAnotherUser Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var id = options.Resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
			return new ConnectToAnotherUser(id);
		}
	}
	public struct ConnectToAnotherUser
	{
		public string Key;
		public ConnectToAnotherUser(string key) : this() {
			Key = key;
		}
	}
}
