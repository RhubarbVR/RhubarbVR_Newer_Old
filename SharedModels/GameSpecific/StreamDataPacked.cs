using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharedModels.GameSpecific
{
	public struct StreamDataPacked : IRelayNetPacked
	{
		public byte[] Data;
		public StreamDataPacked(byte[] data) : this() {
			Data = data;
		}
		public void DeSerlize(BinaryReader binaryReader) {
			var langth = binaryReader.ReadInt32();
			Data = binaryReader.ReadBytes(langth);
		}

		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(Data.Length);
			binaryWriter.Write(Data);
		}
	}
}
