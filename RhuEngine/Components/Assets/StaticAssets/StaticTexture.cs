﻿using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/StaticAssets" })]
	public sealed class StaticTexture : StaticAsset<RTexture2D>
	{

		[Default(TexSample.Anisotropic)]
		[OnChanged(nameof(TextValueChanged))]
		public readonly Sync<TexSample> sampleMode;

		[Default(TexAddress.Wrap)]
		[OnChanged(nameof(TextValueChanged))]
		public readonly Sync<TexAddress> addressMode;

		[Default(3)]
		[OnChanged(nameof(TextValueChanged))]
		public readonly Sync<int> anisoptropy;

		private void TextValueChanged() {
			if(Value is null) {
				return;
			}
			Value.Anisoptropy = anisoptropy;
			Value.AddressMode = addressMode;
			Value.SampleMode = sampleMode;
		}

		public override void LoadAsset(byte[] data) {
			try {
				if (!Engine.EngineLink.CanRender) {
					return;
				}
				Load(null);
				Load(RTexture2D.FromMemory(data));
				TextValueChanged();
			}
			catch(Exception err) {
				RLog.Err($"Failed to load Static Texture Error {err}");
			}
		}
	}
}
