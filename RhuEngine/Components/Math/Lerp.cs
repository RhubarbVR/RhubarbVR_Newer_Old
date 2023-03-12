using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RNumerics.Noise;

namespace RhuEngine.Components
{
	[GenericTypeConstraint()]
	[UpdateLevel(UpdateEnum.Movement)]
	[Category(new string[] { "Math" })]
	public sealed partial class Lerp<T> : Component
	{
		public readonly Linker<T> driver;
		public readonly Sync<T> from;
		public readonly Sync<T> to;
		public readonly SyncPlayback playback;
		public readonly Sync<double> time;
		public readonly Sync<bool> removeOnDone;

		[Exposed]
		public void StartLerp(ILinkerMember<T> target, T targetpos, double timef, bool removeondone = true) {
			driver.SetLinkerTarget(target);
			from.Value = driver.LinkedValue;
			to.Value = targetpos;
			time.Value = timef;
			removeOnDone.Value = removeondone;
			playback.Looping = false;
			playback.Play();
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			playback.StateChange += Playback_StateChange;
		}

		private double Playback_StateChange() {
			return time.Value;
		}

		protected override void Step() {
			if (driver.Linked) {
				try {
					var pos = playback.Position / playback.ClipLength;
					if (pos != 1f) {
						driver.LinkedValue = typeof(T) == typeof(Quaternionf)
							? (T)Quaternionf.Slerp((dynamic)from.Value, (dynamic)to.Value, (float)pos)
							: typeof(T) == typeof(Quaterniond)
								? (T)Quaterniond.Slerp((dynamic)from.Value, (dynamic)to.Value, pos)
								: MathUtil.DynamicLerp(from.Value, to.Value, pos);
					}
					else if (removeOnDone) {
						driver.LinkedValue = to.Value;
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
