using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using RhuEngine.Linker;

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

		public RTexture2D LoadTexture(string name) {
			return RTexture2D.FromMemory(GetStaticResource(name));
		}
		private RTexture2D _rhubarbLogoV1;

		public RTexture2D RhubarbLogoV1 => _rhubarbLogoV1 ??= LoadTexture("RhuEngine.Res.RhubarbVR.png");
		private RTexture2D _rhubarbLogoV2;

		public RTexture2D RhubarbLogoV2 => _rhubarbLogoV2 ??= LoadTexture("RhuEngine.Res.RhubarbVR2.png");

		private RTexture2D _grip;

		public RTexture2D Grid => _grip ??= LoadTexture("RhuEngine.Res.Grid.jpg");

		private RTexture2D _null;

		public RTexture2D Null => _null ??= LoadTexture("RhuEngine.Res.nulltexture.jpg");

		//public Font LoadFont(string name) {
		//	return Font.FromMemory(GetStaticResource(name));
		//}
	}
}
