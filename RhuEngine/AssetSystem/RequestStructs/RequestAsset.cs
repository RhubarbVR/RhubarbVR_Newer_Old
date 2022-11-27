using System;
using System.Collections.Generic;
using System.Text;

using MessagePack;

namespace RhuEngine.AssetSystem.RequestStructs
{
	[MessagePackObject]
	public class RequestAsset : IAssetRequest
	{

		[Key(0)]
		public string URL { get; set; }
	}
}
