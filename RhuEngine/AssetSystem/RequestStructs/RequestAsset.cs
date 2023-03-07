using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RhuEngine.AssetSystem.RequestStructs
{
	public class RequestAsset : IAssetRequest
	{

		public string URL { get; set; }

		public void DeSerlize(BinaryReader binaryReader) {
			URL = binaryReader.ReadString();
		}

		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(URL);
		}
	}
}
