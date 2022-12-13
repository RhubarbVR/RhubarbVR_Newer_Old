using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components.PrivateSpace
{
	[OverlayOnly]
	public sealed class RawAssetProvider<A>:AssetProvider<A> where A : class
	{
		public override bool AutoDisposes => false;

		public void LoadAsset(A asset) {
			Load(asset);
		}
	}
}
