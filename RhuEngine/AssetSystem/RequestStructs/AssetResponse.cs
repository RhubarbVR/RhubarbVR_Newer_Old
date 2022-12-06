using System;
using System.Collections.Generic;
using System.Text;

using MessagePack;
namespace RhuEngine.AssetSystem.RequestStructs
{
	[MessagePackObject]
	public class AssetResponse : IAssetRequest
	{
		[Key(0)]
		public string URL { get; set; }
		[Key(1)]
		public string MimeType { get; set; }
		[Key(2)]
		public byte[] PartBytes { get; set; }
		[Key(3)]
		public int CurrentPart { get; set; }
		[Key(4)]
		public int SizeOfPart { get; set; }
		[Key(5)]
		public long SizeOfData { get; set; }

	}
}
