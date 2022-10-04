﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using RhuEngine.Linker;
using SixLabors.Fonts;

namespace RhuEngine
{
	public sealed class StaticResources {
		public static Stream GetStaticResourceStream(string name) {
			return Assembly.GetCallingAssembly().GetManifestResourceStream("RhuEngine.Res." + name);
		}
		public static Stream GetStaticResource(string name) {
			return File.Exists(Engine.BaseDir + "/" + name)
				? File.OpenRead(Engine.BaseDir + "/" + name)
				: File.Exists(Engine.BaseDir + "/res/" + name)
				? File.OpenRead(Engine.BaseDir + "/" + name)
				: File.Exists(Engine.BaseDir + "/Res/" + name)
				? File.OpenRead(Engine.BaseDir + "/Res/" + name)
				: File.Exists(Engine.BaseDir + "/OverRide/" + name)
				? File.OpenRead(Engine.BaseDir + "/OverRide/" + name)
				: File.Exists(Engine.BaseDir + "/override/" + name)
				? File.OpenRead(Engine.BaseDir + "/override/" + name)
				: GetStaticResourceStream(name);
		}

		public RTexture2D LoadTexture(string name) {
			return new ImageSharpTexture(GetStaticResource(name)).CreateTextureAndDisposes();
		}
		private RTexture2D _rhubarbLogoV1;
#if DEBUG
		public RTexture2D RhubarbLogoV1 => _rhubarbLogoV1 ??= LoadTexture("MilkSnake.png");
#else
		public RTexture2D RhubarbLogoV1 => _rhubarbLogoV1 ??= LoadTexture("RhubarbVR.png");
#endif

		private RTexture2D _rhubarbLogoV2;
#if DEBUG
		public RTexture2D RhubarbLogoV2 => _rhubarbLogoV2 ??= LoadTexture("MilkSnake.png");
#else
		public RTexture2D RhubarbLogoV2 => _rhubarbLogoV2 ??= LoadTexture("RhubarbVR2.png");
#endif

		private RTexture2D _grip;

		public RTexture2D Grid => _grip ??= LoadTexture("Grid.jpg");

		private RTexture2D _null;

		public RTexture2D Null => _null ??= LoadTexture("nulltexture.jpg");

		private RTexture2D _icons;

		public RTexture2D Icons => _icons ??= LoadTexture("Icons.png");
		
		private RFont _mainFont;
		public RFont MainFont => _mainFont ??= LoadMainFont();

		private RFont LoadMainFont() {
			var fonts = new FontCollection();
			var main = fonts.Add(GetStaticResource("Fonts.NotoSans-Regular.ttf"));
			fonts.Add(GetStaticResource("Fonts.NotoEmoji-Regular.ttf"));
			fonts.Add(GetStaticResource("Fonts.NotoSansSymbols-Regular.ttf"));
			fonts.Add(GetStaticResource("Fonts.NotoSansSymbols2-Regular.ttf"));
			fonts.Add(GetStaticResource("Fonts.NotoSansEgyptianHieroglyphs-Regular.ttf"));
			return new RFont(main.CreateFont(RFont.FONTSIZE), fonts);
		}
	}
}
