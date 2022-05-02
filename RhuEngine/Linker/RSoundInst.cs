using System;
using System.Collections.Generic;
using System.Text;

using RNumerics;

namespace RhuEngine.Linker
{
	public interface IRSoundInst
	{
		public bool GetIsPlaying(object sound);
		public float GetVolume(object sound);

		public void SetVolume(object sound, float value);
		public Vector3f GetPosition(object sound);
		public void SetPosition(object sound,Vector3f value);

		public void Stop(object sound);
	}

	public class RSoundInst {
		public static IRSoundInst Instance { get; set; }

		public object SoundObj { get; set; }
		public RSoundInst(object sou) {
			SoundObj = sou;
		}
		public bool IsPlaying => Instance.GetIsPlaying(SoundObj);
		public float Volume { get => Instance.GetVolume(SoundObj); set => Instance.SetVolume(SoundObj, value); }
		public Vector3f Position { get => Instance.GetPosition(SoundObj); set => Instance.SetPosition(SoundObj, value); }

		public void Stop() {
			Instance.Stop(SoundObj);
		}
	}
}
