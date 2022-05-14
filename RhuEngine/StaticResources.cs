using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using RhuEngine.Linker;

namespace RhuEngine
{
	public class StaticResources {
		public static Stream GetStaticResourceStream(string name) {
			return Assembly.GetCallingAssembly().GetManifestResourceStream("RhuEngine.Res." + name);
		}
		public static byte[] GetStaticResource(string name) {
			if(File.Exists(Engine.BaseDir + "\\" + name)) {
				return File.ReadAllBytes(Engine.BaseDir + "\\" + name);
			}
			if (File.Exists(Engine.BaseDir + "\\res\\" + name)) {
				return File.ReadAllBytes(Engine.BaseDir + "\\" + name);
			}
			if (File.Exists(Engine.BaseDir + "\\Res\\" + name)) {
				return File.ReadAllBytes(Engine.BaseDir + "\\Res\\" + name);
			}
			if (File.Exists(Engine.BaseDir + "\\OverRide\\" + name)) {
				return File.ReadAllBytes(Engine.BaseDir + "\\OverRide\\" + name);
			}
			if (File.Exists(Engine.BaseDir + "\\override\\" + name)) {
				return File.ReadAllBytes(Engine.BaseDir + "\\override\\" + name);
			}
			var ms = new MemoryStream();
			GetStaticResourceStream(name).CopyTo(ms);
			return ms.ToArray();
		}

		public RTexture2D LoadTexture(string name) {
			return RTexture2D.FromMemory(GetStaticResource(name));
		}
		private RTexture2D _rhubarbLogoV1;

		public RTexture2D RhubarbLogoV1 => _rhubarbLogoV1 ??= LoadTexture("RhubarbVR.png");
		private RTexture2D _rhubarbLogoV2;

		public RTexture2D RhubarbLogoV2 => _rhubarbLogoV2 ??= LoadTexture("RhubarbVR2.png");

		private RTexture2D _grip;

		public RTexture2D Grid => _grip ??= LoadTexture("Grid.jpg");

		private RTexture2D _null;

		public RTexture2D Null => _null ??= LoadTexture("nulltexture.jpg");

		private RTexture2D _icons;

		public RTexture2D Icons => _icons ??= LoadTexture("Icons.png");


		//public Font LoadFont(string name) {
		//	return Font.FromMemory(GetStaticResource(name));
		//}
	}
}
