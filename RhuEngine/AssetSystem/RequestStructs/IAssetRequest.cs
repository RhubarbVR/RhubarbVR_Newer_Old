using System;
using System.Collections.Generic;
using System.Text;

using MessagePack;

namespace RhuEngine.AssetSystem.RequestStructs
{
	[Union(0, typeof(RequestAsset))]
	[Union(1, typeof(AssetResponse))]
	[Union(2, typeof(PremoteAsset))]
	public interface IAssetRequest
	{
		public string URL { get; }
	}
}
