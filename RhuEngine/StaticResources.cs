using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using StereoKit;
using System.IO;

namespace RhuEngine
{
	public class StaticResources
	{
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

		public Tex RhubarbLogoV1 => LoadTexture("RhuEngine.Res.RhubarbVR.png");
		public Tex RhubarbLogoV2 => LoadTexture("RhuEngine.Res.RhubarbVR2.png");

		public Tex Grid => LoadTexture("RhuEngine.Res.Grid.jpg");
		public Tex Null => LoadTexture("RhuEngine.Res.nulltexture.jpg");

		//public Font LoadFont(string name) {
		//	return Font.FromMemory(GetStaticResource(name));
		//}
	}
}
