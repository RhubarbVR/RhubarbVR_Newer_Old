using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.Rendering)]
	public class SoundSource : Component
	{
		[OnAssetLoaded(nameof(LoadAudio))]
		public AssetRef<Sound> sound;

		[Default(1f)]
		[OnChanged(nameof(ChangeVolume))]
		public Sync<float> volume;

		private SoundInst _soundInst;

		public void LoadAudio() {
			if (_soundInst.IsPlaying) {
				_soundInst.Stop();
			}
			if (volume.Value > 0) {
				if (sound.Asset is not null) {
					_soundInst = sound.Asset.Play(Entity.GlobalTrans.Translation, volume.Value);
				}
			}
		}

		public void ChangeVolume() {
			if (_soundInst.IsPlaying) {
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

		public override void OnLoaded() {
			LoadAudio();
		}

		public override void Step() {
			if (_soundInst.IsPlaying) {
				_soundInst.Position = Entity.GlobalTrans.Translation;
			}
		}
	}
}
