using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RNumerics.Noise;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.Movement)]
	[Category(new string[] { "Math" })]
	public class Lerp<T> : Component
	{
		public readonly Linker<T> driver;
		public readonly Sync<T> from;
		public readonly Sync<T> to;
		public readonly SyncPlayback playback;
		public readonly Sync<double> time;
		public readonly Sync<bool> removeOnDone;
		
		[Exsposed]
		public void StartLerp(ILinkerMember<T> target,T targetpos,double timef,bool removeondone = true) {
			driver.SetLinkerTarget(target);
			from.Value = driver.LinkedValue;
			to.Value = targetpos;
			time.Value = timef;
			removeOnDone.Value = removeondone;
			playback.Looping = false;
			playback.Play();
		}

		public override void OnLoaded() {
			base.OnLoaded();
			playback.StateChange += Playback_StateChange;
		}

		private double Playback_StateChange() {
			return time.Value;
		}

		public override void Step() {
			if (driver.Linked) {
				try {
					var pos = playback.Position / playback.ClipLength;
					if (pos != 1f) {
						driver.LinkedValue = MathUtil.DynamicLerp(from.Value, to.Value, pos);
					}
					else if (removeOnDone) {
						Destroy();
					}
				}
				catch {
					driver.LinkedValue = default;
				}
			}
		}
	}
}
