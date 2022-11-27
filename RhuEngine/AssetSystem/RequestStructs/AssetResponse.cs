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
		public byte[] Bytes { get; set; }
	}
}
