using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RhuEngine.Wasm;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/StaticAssets" })]
	public sealed partial class StaticBinaryAsset : StaticAsset<IBinaryAsset>, IBinaryAsset
	{
		public Stream CreateStream() {
			return new MemoryStream(Data);
		}

		public byte[] Data;

		public override bool AutoDisposes => false;

		public override void LoadAsset(byte[] data) {
			Data = data;
			Load(this);
		}
	}
}
