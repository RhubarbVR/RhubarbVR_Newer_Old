using System;
using System.Collections.Generic;
using System.Text;

using MessagePack;

namespace RhuEngine.AssetSystem.RequestStructs
{
	[Union(0, typeof(AssetChunk))]
	[Union(1, typeof(AssetResponse))]
	[Union(2, typeof(RequestAsset))]
	public interface IAssetRequest
	{
		public string URL { get; }
	}
}
