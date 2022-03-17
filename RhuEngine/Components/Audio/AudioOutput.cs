using System;
using System.Runtime.InteropServices;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	public class AudioOutput : SyncObject, IAssetProvider<Sound>
	{

		public event Action<Sound> OnAssetLoaded;

		public Sound Value { get; private set; }

		public void Load(Sound data) {
			Value = data;
			Loaded = data != null;
			OnAssetLoaded?.Invoke(data);
		}

		public bool Loaded { get; private set; } = false;

		public override void Dispose() {
			Load(null);
			base.Dispose();
		}

		public Sound audio;

		public override void OnLoaded() {
			audio = Sound.CreateStream(1f);
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
