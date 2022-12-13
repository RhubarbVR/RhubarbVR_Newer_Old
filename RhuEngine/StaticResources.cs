using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using RhuEngine.Linker;
using System.Threading.Tasks;
using RNumerics;

namespace RhuEngine
{
	public sealed class StaticResources
	{

		public async Task LoadAllData() {
#if DEBUG
			var v1 = LoadTextureAsync("MilkSnake.png");
#else
			var v1 = LoadTextureAsync("RhubarbVR.png");
#endif

#if DEBUG
			var v2 = LoadTextureAsync("MilkSnake.png");
#else
			var v2 = LoadTextureAsync("RhubarbVR2.png");
#endif
			var egrip = LoadTextureAsync("Grid.jpg");

			var enull = LoadTextureAsync("nulltexture.jpg");

			var eicons = LoadTextureAsync("Icons.png");

			_rhubarbLogoV1 ??= await v1;
			_rhubarbLogoV2 ??= await v2;

			_grip ??= await egrip;
			_null ??= await enull;
			_icons ??= await eicons;

		}

		public static Stream GetStaticResourceStream(string name) {
			return Assembly.GetCallingAssembly().GetManifestResourceStream("RhuEngine.Res." + name);
		}
		public static Stream GetStaticResource(string name) {
			return File.Exists(EngineHelpers.BaseDir + "/" + name)
				? File.OpenRead(EngineHelpers.BaseDir + "/" + name)
				: File.Exists(EngineHelpers.BaseDir + "/res/" + name)
				? File.OpenRead(EngineHelpers.BaseDir + "/" + name)
				: File.Exists(EngineHelpers.BaseDir + "/Res/" + name)
				? File.OpenRead(EngineHelpers.BaseDir + "/Res/" + name)
				: File.Exists(EngineHelpers.BaseDir + "/OverRide/" + name)
				? File.OpenRead(EngineHelpers.BaseDir + "/OverRide/" + name)
				: File.Exists(EngineHelpers.BaseDir + "/override/" + name)
				? File.OpenRead(EngineHelpers.BaseDir + "/override/" + name)
				: GetStaticResourceStream(name);
		}
		public static async Task<RTexture2D> LoadTextureAsync(string name) {
			var img = new RImage(null);
			using (var mem = new MemoryStream()) {
				await GetStaticResource(name).CopyToAsync(mem);
				var bytes = mem.ToArray();
				if (!img.LoadPng(bytes)) {
					img.LoadJpg(bytes);
				}
			}
			img.Compress(RCompressMode.Bptc);
			var texture = new RImageTexture2D(img);
			return texture;
		}
		public static RTexture2D LoadTexture(string name, bool compress, string fileEx) {
			var img = new RImage(null);
			var resourse = GetStaticResourceStream(name);
			img.Load(resourse, fileEx);
			if (compress) {
				img.Compress(RCompressMode.Bptc);
			}
			var texture = new RImageTexture2D(img);
			return texture;
		}
		private RTexture2D _rhubarbLogoV1;
#if DEBUG
		public RTexture2D RhubarbLogoV1 => _rhubarbLogoV1 ??= LoadTexture("MilkSnake.png", true, "png");
#else
		public RTexture2D RhubarbLogoV1 => _rhubarbLogoV1 ??= LoadTexture("RhubarbVR.png",true, "png");
#endif

		private RTexture2D _rhubarbLogoV2;
#if DEBUG
		public RTexture2D RhubarbLogoV2 => _rhubarbLogoV2 ??= LoadTexture("MilkSnake.png", true, "png");
#else
		public RTexture2D RhubarbLogoV2 => _rhubarbLogoV2 ??= LoadTexture("RhubarbVR2.png",true, "png");
#endif

		private RTexture2D _grip;

		public RTexture2D Grid => _grip ??= LoadTexture("Grid.jpg", true, "jpg");

		private RTexture2D _null;

		public RTexture2D Null => _null ??= LoadTexture("nulltexture.jpg", true, "jpg");

		private RTexture2D _icons;

		public RTexture2D Icons => _icons ??= LoadTexture("Icons.png", false, "png");

		private RFont _mainFont;
		public RFont MainFont => _mainFont ??= LoadMainFont();

		private static RFont LoadFontFromStream(Stream stream) {
			var font = new RFont(null);
			font.LoadDynamicFont(stream);
			return font;
		}

		private RhubarbAtlasSheet _rhubarbAtlasSheet;

		private RhubarbAtlasSheet BuildStack() {
			var data = new RhubarbAtlasSheet {
				Atlas = Icons,
				GridSize = new Vector2i(26, 7)
			};
			return data;
		}

		public RhubarbAtlasSheet IconSheet => _rhubarbAtlasSheet ??= BuildStack();

		private static RFont LoadMainFont() {
			var MainFont = new RFont(null);
			MainFont.LoadDynamicFont(GetStaticResource("Fonts.NotoSans-Regular.ttf"));
			MainFont.AddFallBack(LoadFontFromStream(GetStaticResource("Fonts.NotoEmoji-Regular.ttf")));
			MainFont.AddFallBack(LoadFontFromStream(GetStaticResource("Fonts.NotoSansSymbols-Regular.ttf")));
			MainFont.AddFallBack(LoadFontFromStream(GetStaticResource("Fonts.NotoSansSymbols2-Regular.ttf")));
			MainFont.AddFallBack(LoadFontFromStream(GetStaticResource("Fonts.NotoSansEgyptianHieroglyphs-Regular.ttf")));
			MainFont.MultichannelSignedDistanceField = true;
			return MainFont;
		}
	}
}
