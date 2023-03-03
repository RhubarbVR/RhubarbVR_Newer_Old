using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GDExtension;

using RhuEngine.Linker;

using RNumerics;

namespace RhubarbVR.Bindings.TextureBindings
{
	public class GodotAtlasTexture : GodotTexture2D, IRAtlasTexture
	{

		public GodotAtlasTexture() {

		}

		private RTexture2D _targetRhubarbTexture;

		public RTexture2D Atlas
		{
			get => _targetRhubarbTexture; set {
				_targetRhubarbTexture = value;
				if (_targetRhubarbTexture.Inst is GodotTexture2D godotTexture) {
					AtlasTexture.Atlas = godotTexture.Texture2D;
				}
			}
		}
		public Vector2f RegionPos
		{
			get {
				var pos = AtlasTexture.Region.position;
				return new Vector2f(pos.x, pos.y);
			}
			set {
				var data = AtlasTexture.Region;
				data.position = new Vector2(value.x,value.y);
				AtlasTexture.Region = data;
			}
		}
		public Vector2f RegionScale
		{
			get {
				var scale = AtlasTexture.Region.size;
				return new Vector2f(scale.x, scale.y);
			}
			set {
				var data = AtlasTexture.Region;
				data.size = new Vector2(value.x, value.y);
				AtlasTexture.Region = data;
			}
		}
		public Vector2f MarginPos
		{
			get {
				var pos = AtlasTexture.Margin.position;
				return new Vector2f(pos.x, pos.y);
			}
			set {
				var data = AtlasTexture.Margin;
				data.position = new Vector2(value.x, value.y);
				AtlasTexture.Margin = data;
			}
		}
		public Vector2f MarginScale
		{
			get {
				var scale = AtlasTexture.Margin.size;
				return new Vector2f(scale.x, scale.y);
			}
			set {
				var data = AtlasTexture.Margin;
				data.size = new Vector2(value.x, value.y);
				AtlasTexture.Margin = data;
			}
		}
		public bool FilterClip { get => AtlasTexture.FilterClip; set => AtlasTexture.FilterClip = value; }

		public AtlasTexture AtlasTexture => (AtlasTexture)Texture;

		public RAtlasTexture RAtlasTexture { get; private set; }

		public void Init(RAtlasTexture rAtlasTexture) {
			RAtlasTexture = rAtlasTexture;
			if (typeof(GodotAtlasTexture) == GetType()) {
				Texture ??= new AtlasTexture();
			}
		}
	}
}
