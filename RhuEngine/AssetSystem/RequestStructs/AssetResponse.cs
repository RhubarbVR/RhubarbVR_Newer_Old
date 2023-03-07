using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using RNumerics;

namespace RhuEngine.AssetSystem.RequestStructs
{
	public class AssetResponse : IAssetRequest, ISerlize<AssetResponse>
	{
		public string URL { get; set; }
		public string MimeType { get; set; }
		public byte[] PartBytes { get; set; }
		public int CurrentPart { get; set; }
		public int SizeOfPart { get; set; }
		public long SizeOfData { get; set; }

		public void DeSerlize(BinaryReader binaryReader) {
			URL = binaryReader.ReadString();
			MimeType = binaryReader.ReadString();
			var length = binaryReader.ReadInt32();
			binaryReader.ReadBytes(length);
			CurrentPart = binaryReader.ReadInt32();
			SizeOfPart = binaryReader.ReadInt32();
			SizeOfData = binaryReader.ReadInt64();
		}

		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(URL);
			binaryWriter.Write(MimeType);
			binaryWriter.Write(PartBytes.Length);
			binaryWriter.Write(PartBytes);
			binaryWriter.Write(SizeOfPart);
			binaryWriter.Write(SizeOfData);
		}
	}
}
