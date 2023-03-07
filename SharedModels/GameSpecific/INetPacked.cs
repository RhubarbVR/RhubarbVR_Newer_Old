using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SharedModels.GameSpecific
{

	public interface IRelayNetPacked
	{
		public void Serlize(BinaryWriter binaryWriter);

		public void DeSerlize(BinaryReader binaryReader);
	}

	public static class RelayNetPacked
	{
		public static void Serlize(BinaryWriter binaryWriter, IRelayNetPacked relayNetPacked) {
			var data = relayNetPacked.GetType();
			if (data == typeof(DataPacked)) {
				binaryWriter.Write((byte)0);
			}
			else if (data == typeof(StreamDataPacked)) {
				binaryWriter.Write((byte)1);
			}
			else if (data == typeof(ConnectToAnotherUser)) {
				binaryWriter.Write((byte)2);
			}
			relayNetPacked.Serlize(binaryWriter);
		}

		public static IRelayNetPacked DeSerlize(BinaryReader binaryReader, byte? @byte = null) {
			var data = @byte ?? binaryReader.ReadByte();
			IRelayNetPacked relayNet = data switch {
				0 => new DataPacked(),
				1 => new StreamDataPacked(),
				2 => new ConnectToAnotherUser(),
				_ => null,
			};
			relayNet.DeSerlize(binaryReader);
			return relayNet;
		}
	}
}
