using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.Components;
using NAudio.Wave;
using StereoKit;
using NAudio.Wave.SampleProviders;
namespace RStereoKit
{
	

	public class AudioSources : EngineWorldLinkBase<SoundSource>
	{
		public Sound sound;
		public SoundInst soundInst;
		public override void Init() {
			sound = Sound.CreateStream(1);
			soundInst = sound.Play(Vec3.Zero, LinkedComp.volume);
			LinkedComp.sound.LoadChange += Sound_LoadChange;
			LinkedComp.volume.Changed += Volume_Changed;
		}

		private void Volume_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			soundInst.Volume = LinkedComp.volume.Value;
		}

		private void Sound_LoadChange(IWaveProvider obj) {
			_waveProvider?.Dispose();
			_waveProvider = null;
			_sampler = null;
			if (obj is null) {
				return;
			}
			_waveProvider = new MediaFoundationResampler(obj, WaveFormat.CreateIeeeFloatWaveFormat(48000, 1));
			_sampler = _waveProvider.ToSampleProvider();
		}

		public override void Remove() {
			sound = null;
		}

		public override void Render() {
			if(sound is null) {
				return;
			}
			if (!IsPlaying) {
				return;
			}
			soundInst.Position = (System.Numerics.Vector3)LinkedComp.Entity.GlobalTrans.Translation;
			if (_sampler is null) {
				return;
			}
			if (sound.UnreadSamples < 9600) {
				var buffer = new float[9600];
				var amountRead = _sampler.Read(buffer, 0, 9600);
				sound.WriteSamples(buffer, amountRead);
			}
		}

		public bool IsPlaying = true;

		public override void Started() {
			IsPlaying = true;
			soundInst = sound.Play(Vec3.Zero, LinkedComp.volume);
		}

		public override void Stopped() {
			IsPlaying = false;
			soundInst.Stop();
		}


		private MediaFoundationResampler _waveProvider;
		private ISampleProvider _sampler;

	}
}
