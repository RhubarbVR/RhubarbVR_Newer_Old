using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.Rendering)]
	public class SoundSource : Component
	{
		[OnAssetLoaded(nameof(LoadAudio))]
		public readonly AssetRef<RSound> sound;

		[Default(1f)]
		[OnChanged(nameof(ChangeVolume))]
		public readonly Sync<float> volume;

		private RSoundInst _soundInst;

		public void LoadAudio() {
			if (!Engine.EngineLink.CanAudio) {
				return;
			}
			if (_soundInst?.IsPlaying??false) {
				_soundInst.Stop();
			}
			if (volume.Value > 0) {
				if (sound.Asset is not null) {
					_soundInst = sound.Asset.Play(Entity.GlobalTrans.Translation, volume.Value);
				}
			}
		}

		public void ChangeVolume() {
			if (!Engine.EngineLink.CanAudio) {
				return;
			}
			if (_soundInst?.IsPlaying??false) {
				_soundInst.Volume = volume.Value;
				if (volume.Value <= 0) {
					_soundInst.Stop();
				}
			}
			else {
				if (volume.Value > 0) {
					if (sound.Asset is not null) {
						_soundInst = sound.Asset.Play(Entity.GlobalTrans.Translation, volume.Value);
					}
				}
			}
		}

		public override void Dispose() {
			base.Dispose();
			_soundInst?.Stop();
		}

		public override void OnLoaded() {
			LoadAudio();
		}

		public override void Step() {
			if (!Engine.EngineLink.CanAudio) {
				return;
			}
			if (_soundInst?.IsPlaying??false) {
				_soundInst.Position = Entity.GlobalTrans.Translation;
			}
		}
	}
}
