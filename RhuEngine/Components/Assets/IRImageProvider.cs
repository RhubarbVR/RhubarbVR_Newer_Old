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


	public abstract partial class ImageTexture : AssetProvider<RTexture2D>, IRImageProvider
	{
		protected RImage _image;

		public RImage Image => _image;
	}
}
