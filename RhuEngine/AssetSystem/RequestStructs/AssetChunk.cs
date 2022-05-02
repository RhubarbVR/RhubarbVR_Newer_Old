using System;
using System.Collections.Generic;
using System.Text;
using MessagePack;
namespace RhuEngine.AssetSystem.RequestStructs
{
	[MessagePackObject]
	public class AssetChunk : IAssetRequest
	{
		[Key(0)]
		public string URL { get; set; }
		[Key(1)]
		public long ChunkID;
		[Key(2)]
		public byte[] data;
	}
}
