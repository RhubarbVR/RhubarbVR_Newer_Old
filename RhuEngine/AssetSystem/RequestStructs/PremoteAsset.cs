using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using RNumerics;

namespace RhuEngine.AssetSystem.RequestStructs
{
	public class PremoteAsset : IAssetRequest, ISerlize<PremoteAsset>
	{
		public string URL { get; set; }
		public string NewURL { get; set; }

		public void DeSerlize(BinaryReader binaryReader) {
			URL = binaryReader.ReadString();
			NewURL = binaryReader.ReadString();
		}

		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(URL);
			binaryWriter.Write(NewURL);
		}
	}
}
