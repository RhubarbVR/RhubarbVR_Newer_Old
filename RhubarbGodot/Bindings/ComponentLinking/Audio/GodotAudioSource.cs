using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhuEngine;
using RhuEngine.Components;
using RhuEngine.Linker;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class GodotAudioSource : WorldNodeLinked<AudioSource, AudioStreamPlayer>
	{
		public override bool GoToEngineRoot => false;
		public override string ObjectName => "AudioSource";

		public RhubarbAudioSource audioSource;

		public override void Remove() {
			base.Remove();
			audioSource?.Dispose();
		}

		private bool _shouldBePlaying = false;

		public override void Render() {
			if (_shouldBePlaying && !(node?.Playing ?? true)) {
				PlayAudio();
			}
		}
		public void PlayAudio() {
			if (node.Stream is null) {
				return;
			}
			_shouldBePlaying = true;
			node.Play();
			audioSource.Instansiate(node.GetStreamPlayback());
		}
		public override void StartContinueInit() {
			audioSource = new();
			audioSource.SetUpAudio(LinkedComp);
			node.Stream = audioSource.audio;
			PlayAudio();
			LinkedComp.Volume.Changed += Volume_Changed;
			LinkedComp.PitchScale.Changed += PitchScale_Changed;
			LinkedComp.MaxPolyphony.Changed += MaxPolyphony_Changed;
			LinkedComp.AudioBus.Changed += AudioBus_Changed;
			LinkedComp.MixTarget.Changed += MixTarget_Changed;
			Volume_Changed(null);
			PitchScale_Changed(null);
			MaxPolyphony_Changed(null);
			AudioBus_Changed(null);
			MixTarget_Changed(null);
		}

		private void MixTarget_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() =>
				node.MixTarget = LinkedComp.MixTarget.Value switch {
					AudioSource.TargetSource.Stereo => AudioStreamPlayer.MixTargetEnum.Stereo,
					AudioSource.TargetSource.Surround => AudioStreamPlayer.MixTargetEnum.Surround,
					AudioSource.TargetSource.Center => AudioStreamPlayer.MixTargetEnum.Center,
					_ => AudioStreamPlayer.MixTargetEnum.Stereo,
				}
			);
		}

		private void MaxPolyphony_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.MaxPolyphony = LinkedComp.MaxPolyphony);
		}

		private void PitchScale_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.PitchScale = LinkedComp.PitchScale);
		}

		private void Volume_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.VolumeDb = LinkedComp.Volume);
		}

		private void AudioBus_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Bus = LinkedComp.AudioBus.Value.ToString());
		}

		public override void Started() {
			RenderThread.ExecuteOnEndOfFrame(() => PlayAudio());
		}

		public override void Stopped() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				_shouldBePlaying = false;
				node.Stop();
			});
		}
	}
}
