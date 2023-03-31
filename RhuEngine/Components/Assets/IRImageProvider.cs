using System;
using RNumerics;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	public interface IRImageProvider
	{
		public RImage Image { get; }
	}


	public abstract partial class ImageTexture : AssetProvider<RTexture2D>, IAssetProvider<RTexture>, IRImageProvider
	{
		protected RImage _image;

		public RImage Image => _image;

		RTexture IAssetProvider<RTexture>.Value => Value;

		event Action<RTexture> IAssetProvider<RTexture>.OnAssetLoaded
		{
			add => OnAssetLoaded += value;
			remove => OnAssetLoaded -= value;
		}
	}
}
