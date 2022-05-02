using System.Threading.Tasks;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Linq;
using RhuEngine.Linker;

namespace RhuEngine
{
	public class RawAudioClip : SyncObject, IAssetProvider<RSound>
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

		public override void OnLoaded() {
			if (!Engine.EngineLink.CanAudio) {
				return;
			}
			audio = RSound.CreateStream(5f);
			Load(audio);
		}

		public void WriteSamples(float[] audioSamps) {
			if (audio is null) {
				return;
			}
			audio.WriteSamples(audioSamps);
		}

		public void WriteSamples(float[] audioSamps, int count) {
			if (audio is null) {
				return;
			}
			audio.WriteSamples(audioSamps, count);
		}

		public void PlayAudio(IntPtr data, IntPtr samples, uint count, long pts) {
			//			Witness
			//				267102006613639180 (humbletim)
			//			you:
			//				F32L!
			//			vlc:
			//				S16N.
			//			you:
			//				F32L!!!!!!
			//			vlc:
			//				S16N.
			//			you:
			//				S16N(FL32).
			//			vlc:
			//				correct.
			if (audio is null) {
				return;
			};
			var newbuff = new float[count];
			for (var i = 0; i < count; i++) {
				newbuff[i] = Marshal.ReadInt16(samples + (sizeof(short) * i)) * (1 / 32768.0f);
			}
			audio.WriteSamples(newbuff);
		}


		public float[] GetAudio(short[] data) {
			return (from samp in data
					select (float)samp * (1 / 32768.0f)).ToArray();
		}

	}

}
