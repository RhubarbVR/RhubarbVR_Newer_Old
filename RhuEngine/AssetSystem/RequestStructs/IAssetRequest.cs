using System;
using System.Collections.Generic;
using System.Text;

using MessagePack;

using static RhuEngine.WorldObjects.World;

namespace RhuEngine.AssetSystem.RequestStructs
{
	public interface IAssetRequest : INetPacked
	{
		public string URL { get; }
	}
}
