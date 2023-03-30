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
			CurrentPart = binaryReader.ReadInt32();
			SizeOfPart = binaryReader.ReadInt32();
			SizeOfData = binaryReader.ReadInt64();
			var length = binaryReader.ReadInt32();
			PartBytes = binaryReader.ReadBytes(length);
		}

		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(URL);
			binaryWriter.Write(MimeType);
			binaryWriter.Write(CurrentPart);
			binaryWriter.Write(SizeOfPart);
			binaryWriter.Write(SizeOfData);
			binaryWriter.Write(PartBytes.Length);
			binaryWriter.Write(PartBytes);
		}
	}
}
