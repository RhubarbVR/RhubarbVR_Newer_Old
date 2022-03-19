using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using StereoKit;
using System.IO;

namespace RhuEngine
{
	public class StaticResources {
		public Stream GetStaticResourceStream(string name) {
			return Assembly.GetCallingAssembly().GetManifestResourceStream(name);
		}
		public byte[] GetStaticResource(string name) {
			var ms = new MemoryStream();
			GetStaticResourceStream(name).CopyTo(ms);
			return ms.ToArray();
		}

		public Tex LoadTexture(string name) {
			return Tex.FromMemory(GetStaticResource(name));
		}
		private Tex _rhubarbLogoV1;

		public Tex RhubarbLogoV1 => _rhubarbLogoV1 ??= LoadTexture("RhuEngine.Res.RhubarbVR.png");
		private Tex _rhubarbLogoV2;

		public Tex RhubarbLogoV2 => _rhubarbLogoV2 ??= LoadTexture("RhuEngine.Res.RhubarbVR2.png");

		private Tex _grip;

		public Tex Grid => _grip ??= LoadTexture("RhuEngine.Res.Grid.jpg");

		private Tex _null;

		public Tex Null => _null ??= LoadTexture("RhuEngine.Res.nulltexture.jpg");

		//public Font LoadFont(string name) {
		//	return Font.FromMemory(GetStaticResource(name));
		//}
	}
}
