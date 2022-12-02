using System;
using System.Collections.Generic;
using System.Text;

using MessagePack;

using static RhuEngine.WorldObjects.World;

namespace RhuEngine.AssetSystem.RequestStructs
{
	[Union(0, typeof(RequestAsset))]
	[Union(1, typeof(AssetResponse))]
	[Union(2, typeof(PremoteAsset))]
	public interface IAssetRequest : INetPacked
	{
		public string URL { get; }
	}
}
