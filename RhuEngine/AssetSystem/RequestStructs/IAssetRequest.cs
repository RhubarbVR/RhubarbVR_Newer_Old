using System;
using System.Collections.Generic;
using System.Text;

using MessagePack;

namespace RhuEngine.AssetSystem.RequestStructs
{
	public interface IAssetRequest
	{
		public string URL { get; }
	}
}
