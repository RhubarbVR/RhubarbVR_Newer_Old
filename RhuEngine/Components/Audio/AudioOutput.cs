using System;
using System.Runtime.InteropServices;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;

namespace RhuEngine.Components
{
	public class AudioOutput : SyncObject, IAssetProvider<RSound>
	{

		public event Action<RSound> OnAssetLoaded;

		public RSound Value { get; private set; }

		public void Load(RSound data) {
			Value = data;
			Loaded = data != null;
			OnAssetLoaded?.Invoke(data);
		}

		public bool Loaded { get; private set; } = false;

		public override void Dispose() {
			Load(null);
			base.Dispose();
		}

		public RSound audio;

		protected override void OnLoaded() {
			if (!Engine.EngineLink.CanAudio) {
				return;
			}
			audio = RSound.CreateStream(1f);
			Load(audio);
		}

		public void WriteAudio(float[] data) {
			if(audio is null) {
				return;
			}
			audio.WriteSamples(data);
		}

	}
}
