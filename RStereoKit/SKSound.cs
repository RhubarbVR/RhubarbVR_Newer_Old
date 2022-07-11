using System.Text;
using StereoKit;
using RhuEngine;
using RhuEngine.Linker;
using RNumerics;
using RhuEngine.WorldObjects;
using System.Numerics;

namespace RStereoKit
{
	public class SKMic : IRMicrophone
	{
		public string[] Devices => Microphone.GetDevices();

		public class MicInst : IRMicReader
		{
			public MicInst(Sound sound) {
				Sound = sound;
				SoundClip = new RSound(sound);
			}
			public RSound SoundClip { get; set; }

			public int SamplesAmount => Sound.TotalSamples;

			public int GetPosition => Sound.CursorSamples;

			public bool IsRecording => true;

			public Sound Sound { get; }

			public void Read(ref float[] ReadSamples) {
				Sound.ReadSamples(ref ReadSamples);
			}
		}
		public bool Start(string deviceName,out IRMicReader reader) {
			var start = Microphone.Start(deviceName);
			reader = new MicInst(Microphone.Sound);
			return start;
		}
	}

	public class MSoundInst
	{
		public SoundInst SoundInst;

		public MSoundInst(SoundInst soundInst) {
			SoundInst = soundInst;
		}
	}

	public class SKSoundInst : IRSoundInst
	{

		public bool GetIsPlaying(object sound) {
			return ((MSoundInst)sound).SoundInst.IsPlaying;
		}

		public Vector3f GetPosition(object sound) {
			return (Vector3)((MSoundInst)sound).SoundInst.Position;
		}

		public float GetVolume(object sound) {
			return ((MSoundInst)sound).SoundInst.Volume;
		}


		public void SetPosition(object sound, Vector3f value) {
			((MSoundInst)sound).SoundInst.Position = (Vector3)value;
		}

		public void SetVolume(object sound, float value) {
			((MSoundInst)sound).SoundInst.Volume = value;
		}

		public void Stop(object sound) {
			((MSoundInst)sound).SoundInst.Stop();
		}
	}

	public class SKSound : IRSound
	{

		public RSound CreateStream(float v) {
			return new RSound(Sound.CreateStream(v));
		}

		public int GetCursorSamples(object sou) {
			return ((Sound)sou).CursorSamples;
		}

		public int GetTotalSamples(object sou) {
			return ((Sound)sou).TotalSamples;
		}

		public RSoundInst Play(object sou, Vector3f translation, float value) {
			return new RSoundInst(new MSoundInst(((Sound)sou).Play((Vector3)translation, value)));
		}

		public void WriteSamples(object sou, float[] audioSamps, int count) {
			((Sound)sou).WriteSamples(audioSamps, count);
		}
	}
}
