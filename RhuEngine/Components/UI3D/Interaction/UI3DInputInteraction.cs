using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using System;
using System.Runtime.ConstrainedExecution;

namespace RhuEngine.Components
{


	[Category(new string[] { "UI3D/Interaction" })]
	public sealed class UI3DInputInteraction : UI3DInteractionComponent
	{
		[Default(true)]
		public readonly Sync<bool> Laserable;
		[Default(0.5f)]
		public readonly Sync<float> PrimaryNeededForce;
		[Default(0.5f)]
		public readonly Sync<float> GripNeededForce;
		[Default(0.5f)]
		public readonly Sync<float> SecodaryNeededForce;
		[Default(true)]
		public readonly Sync<bool> Touchable;

		[Default(true)]
		public readonly Sync<bool> CustomTochable;
		[Default(1024)]
		public readonly Sync<int> PixelPerMeter;

		public readonly Sync<Vector2f> Tilt;

		public readonly Linker<Vector2i> SizeSeter;

		public readonly SyncRef<IInputInterface> InputInterface;

		protected override void Step() {
			base.Step();
			if (UIRect is null) {
				return;
			}
			if (SizeSeter.Linked) {
				var newSizef = (UIRect.CachedCanvas?.scale.Value.Xy ?? Vector2f.Zero) / 10f * (UIRect.TrueMax - UIRect.TrueMin);
				var newSize = new Vector2i((int)(PixelPerMeter.Value * newSizef.x), (int)(PixelPerMeter.Value * newSizef.y));
				if (newSize != SizeSeter.LinkedValue) {
					SizeSeter.LinkedValue = newSize;
				}
			}
			foreach (var item in UIRect.GetRectHitData()) {
				var pos = item.HitPointOnCanvas;
				pos -= UIRect.TrueMin;
				pos /= UIRect.TrueMax - UIRect.TrueMin;
				var isPrime = item.PressForce >= PrimaryNeededForce.Value;
				var isSec = item.GripForces >= GripNeededForce.Value;
				var isMed = InputManager.GetInputAction(InputTypes.Secondary).HandedValue(item.Side) >= SecodaryNeededForce.Value;
				if (item.Lazer) {
					if (Laserable) {
						InputInterface.Target?.SendInput(pos, Tilt.Value, item.PressForce, item.Side, (int)item.TouchUndex, true, isPrime, isSec, isMed);
					}
				}
				else if (item.CustomTouch) {
					if (CustomTochable) {
						InputInterface.Target?.SendInput(pos, Tilt.Value, item.PressForce, item.Side, (int)item.TouchUndex, false, isPrime, isSec, isMed);
					}
				}
				else {
					if (Touchable) {
						InputInterface.Target?.SendInput(pos, Tilt.Value, item.PressForce, item.Side, (int)item.TouchUndex, false, isPrime, isSec, isMed);
					}
				}
			}
		}
	}
}
