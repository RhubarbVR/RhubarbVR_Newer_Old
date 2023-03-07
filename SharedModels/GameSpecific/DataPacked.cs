using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace SharedModels.GameSpecific
{
	public struct DataPacked : IRelayNetPacked
	{
		public byte[] Data;
		public ushort Id;
		public DataPacked(byte[] data, ushort id) : this() {
			Data = data;
			Id = id;
		}

		public void DeSerlize(BinaryReader binaryReader) {
			var length = binaryReader.ReadInt32();
			Data = binaryReader.ReadBytes(length);
			Id = binaryReader.ReadUInt16();
		}

		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(Data.Length);
			binaryWriter.Write(Data);
			binaryWriter.Write(Id);
		}
	}
}
