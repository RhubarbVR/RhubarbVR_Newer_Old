using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using RhuEngine.WorldObjects;

using RNumerics;

namespace RhuEngine.Linker
{

	public interface IRAtlasTexture : IRTexture2D, IDisposable
	{
		public RTexture2D Atlas { get; set; }
		public Vector2f RegionPos { get; set; }
		public Vector2f RegionScale { get; set; }
		public Vector2f MarginPos { get; set; }
		public Vector2f MarginScale { get; set; }
		public bool FilterClip { get; set; }
		void Init(RAtlasTexture rAtlasTexture);
	}

	public class RAtlasTexture : RTexture2D, IDisposable
	{
		public static new Type Instance { get; set; }

		public IRAtlasTexture AtlasTexture => (IRAtlasTexture)Inst;
		public RTexture2D Atlas { get => AtlasTexture.Atlas; set => AtlasTexture.Atlas = value; }
		public Vector2f RegionPos { get => AtlasTexture.RegionPos; set => AtlasTexture.RegionPos = value; }
		public Vector2f RegionScale { get => AtlasTexture.RegionScale; set => AtlasTexture.RegionScale = value; }
		public Vector2f MarginPos { get => AtlasTexture.MarginPos; set => AtlasTexture.MarginPos = value; }
		public Vector2f MarginScale { get => AtlasTexture.MarginScale; set => AtlasTexture.MarginScale = value; }
		public bool FilterClip { get => AtlasTexture.FilterClip; set => AtlasTexture.FilterClip = value; }

		public RAtlasTexture(IRAtlasTexture tex) : base(tex) {
			if (typeof(RAtlasTexture) == GetType()) {
				Inst = tex ?? (IRAtlasTexture)Activator.CreateInstance(Instance);
				((IRAtlasTexture)Inst).Init(this);
			}
		}
	}
}
