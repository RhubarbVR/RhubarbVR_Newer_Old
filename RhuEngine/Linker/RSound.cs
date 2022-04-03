using System;
using System.Collections.Generic;
using System.Text;

using RNumerics;

namespace RhuEngine.Linker
{
	public interface IRSound
	{
		public int GetTotalSamples(object sou);
		public int GetCursorSamples(object sou);
		public int GetUnreadSamples(object sou);

		public RSound CreateStream(float v);

		public void WriteSamples(object sou,float[] audioSamps, int count);

		public void WriteSamples(object sou, float[] audioSamps);

		public RSoundInst Play(object sou, Vector3f translation, float value);

		public void ReadSamples(object sou, ref float[] audioPacked);
	}

	public class RSound
	{
		public static IRSound Instance { get; set; }
		public object Sound { get; set; }

		public RSound(object sow) {
			Sound = sow;
		}

		public int TotalSamples => Instance.GetTotalSamples(Sound);
		public int CursorSamples => Instance.GetCursorSamples(Sound);
		public int UnreadSamples => Instance.GetUnreadSamples(Sound);

		public static RSound CreateStream(float v) {
			return Instance.CreateStream(v);
		}

		public void WriteSamples(float[] audioSamps, int count) {
			Instance.WriteSamples(Sound, audioSamps, count);
		}

		public void WriteSamples(float[] audioSamps) {
			Instance.WriteSamples(Sound, audioSamps);
		}

		public RSoundInst Play(Vector3f translation, float value) {
			return Instance.Play(Sound, translation, value);
		}

		public void ReadSamples(ref float[] audioPacked) {
			Instance.ReadSamples(Sound, ref audioPacked);
		}
	}
}
