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
	public sealed class GodotAudioSource3D : WorldPositionLinked<AudioSource3D, AudioStreamPlayer3D>
	{
		public override bool GoToEngineRoot => true;
		public override string ObjectName => "AudioSource3D";

		public RhubarbAudioSource audioSource;

		public int _playedWithoutFrames = 0;

		public override void Render() {
			base.Render();
			if (_shouldBePlaying && !(node?.Playing??true)) {
				_playedWithoutFrames++;
				if(_playedWithoutFrames > 3) {
					RLog.Info("Audio Skiping");
					PlayAudio();
				}
			}
			else {
				_playedWithoutFrames = 0;
			}
		}

		public override void Remove() {
			base.Remove();
			audioSource?.Dispose();
		}

		private bool _shouldBePlaying = false;

		public void PlayAudio() {
			if(node.Stream is null) {
				return;
			}
			_shouldBePlaying = true;
			node.Play();
			audioSource.Instansiate(node.GetStreamPlayback());
		}

		public override void StartContinueInit() {
			audioSource = new();
			audioSource.SetUpAudio(LinkedComp);
			node.Stream = audioSource.Audio;
			PlayAudio();
			LinkedComp.Volume.Changed += Volume_Changed;
			LinkedComp.PitchScale.Changed += PitchScale_Changed;
			LinkedComp.MaxPolyphony.Changed += MaxPolyphony_Changed;
			LinkedComp.AudioBus.Changed += AudioBus_Changed;
			LinkedComp.AttenutionModel.Changed += AttenutionModel_Changed;
			LinkedComp.UnitSize.Changed += UnitSize_Changed;
			LinkedComp.MaxdB.Changed += MaxdB_Changed;
			LinkedComp.MaxDistance.Changed += MaxDistance_Changed;
			LinkedComp.PanningStrength.Changed += PanningStrength_Changed;
			LinkedComp.Mask.Changed += Mask_Changed;
			LinkedComp.EmissionAngleEnabled.Changed += EmissionAngleEnabled_Changed;
			LinkedComp.EmissionAngleDegrees.Changed += EmissionAngleDegrees_Changed;
			LinkedComp.EmissionAngleFilterAttenuation.Changed += EmissionAngleFilterAttenuation_Changed;
			LinkedComp.AttenuationCutOffHz.Changed += AttenuationCutOffHz_Changed;
			LinkedComp.AttenuationdB.Changed += AttenuationdB_Changed;
			LinkedComp.Doppler.Changed += Doppler_Changed;
			Volume_Changed(null);
			PitchScale_Changed(null);
			MaxPolyphony_Changed(null);
			AudioBus_Changed(null);
			AttenutionModel_Changed(null);
			UnitSize_Changed(null);
			MaxdB_Changed(null);
			MaxDistance_Changed(null);
			PanningStrength_Changed(null);
			Mask_Changed(null);
			EmissionAngleEnabled_Changed(null);
			EmissionAngleDegrees_Changed(null);
			EmissionAngleFilterAttenuation_Changed(null);
			AttenuationCutOffHz_Changed(null);
			AttenuationdB_Changed(null);
			Doppler_Changed(null);
		}

		private void Doppler_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.DopplerTracking = LinkedComp.Doppler.Value ? AudioStreamPlayer3D.DopplerTrackingEnum.IdleStep : AudioStreamPlayer3D.DopplerTrackingEnum.Disabled);
		}

		private void AttenuationdB_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.AttenuationFilterDb = LinkedComp.AttenuationdB);
		}

		private void AttenuationCutOffHz_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.AttenuationFilterCutoffHz = LinkedComp.AttenuationCutOffHz);
		}

		private void EmissionAngleFilterAttenuation_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.EmissionAngleFilterAttenuationDb = LinkedComp.EmissionAngleFilterAttenuation);
		}

		private void EmissionAngleDegrees_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.EmissionAngleDegrees = LinkedComp.EmissionAngleDegrees);
		}

		private void EmissionAngleEnabled_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.EmissionAngleEnabled = LinkedComp.EmissionAngleEnabled);
		}

		private void Mask_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.AreaMask = (uint)LinkedComp.Mask.Value);
		}

		private void PanningStrength_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.PanningStrength = LinkedComp.PanningStrength);
		}

		private void MaxDistance_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.MaxDistance = LinkedComp.MaxDistance);
		}

		private void MaxdB_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.MaxDb = LinkedComp.MaxdB);
		}

		private void UnitSize_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.UnitSize = LinkedComp.UnitSize);
		}

		private void AttenutionModel_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() =>
				node.AttenuationModel = LinkedComp.AttenutionModel.Value switch {
					AudioSource3D.AttenutionModelEnum.Inverse => AudioStreamPlayer3D.AttenuationModelEnum.InverseDistance,
					AudioSource3D.AttenutionModelEnum.InverseSquare => AudioStreamPlayer3D.AttenuationModelEnum.InverseSquareDistance,
					AudioSource3D.AttenutionModelEnum.Logarithmic => AudioStreamPlayer3D.AttenuationModelEnum.Logarithmic,
					AudioSource3D.AttenutionModelEnum.Disabled => AudioStreamPlayer3D.AttenuationModelEnum.Disabled,
					_ => AudioStreamPlayer3D.AttenuationModelEnum.Disabled,
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
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (node is null) {
					return;
				}
				node.Visible = true;
				PlayAudio();
			});
		}

		public override void Stopped() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (node is null) {
					return;
				}
				node.Visible = false;
				_shouldBePlaying = false;
				node.Stop();
			});
		}
	}
}
