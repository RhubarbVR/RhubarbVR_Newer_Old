using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using System;

namespace RhuEngine.Components
{
	public struct ButtonEvent
	{
		public bool IsPressing;

		public bool IsClicked;

		public bool IsReleased;

		public Vector3f WorldPos;

		public Vector3f WindowPos;

		public uint FingerIndex;

		public float Force;

		public bool Lazer;

		public bool Touch;

		public bool CustomTouch;
	}

	[Category(new string[] { "UI\\Rects" })]
	public class UIButtonInteraction : UIInteractionComponent
	{
		[Default(true)]
		public Sync<bool> Laserable;

		[Default(true)]
		public Sync<bool> Touchable;
		
		[Default(true)]
		public Sync<bool> CustomTouchable;

		[Default(true)]
		public Sync<bool> AllowOtherZones;

		[Default(0.3f)]
		public Sync<float> PressForce;

		public SyncDelegate<Action<ButtonEvent>> ButtonEvent;

		[Exsposed]
		[NoWriteExsposed]
		public bool IsClicking { get; private set; }

		private HitData _lastHitData;

		private void SendEvent(ButtonEvent buttonEvent) {
			ButtonEvent.Target?.Invoke(buttonEvent);
		}

		private void RunButtonClickEvent(HitData hitData) {
			_lastHitData = hitData;
			RLog.Info("Click");
			SendEvent(new ButtonEvent {
					IsPressing = false,
					IsClicked = true,
					IsReleased = false,
					WorldPos = hitData.HitPosWorld,
					WindowPos = hitData.HitPos,
					FingerIndex = hitData.Touchindex,
					Force = hitData.PressForce,
					Lazer = hitData.Laser,
					Touch = !hitData.Laser,
					CustomTouch = hitData.CustomTouch,
			});
		}

		private void RunButtonPressingEvent(HitData hitData) {
			_lastHitData = hitData;
			SendEvent(new ButtonEvent {
				IsPressing = true,
				IsClicked = false,
				IsReleased = false,
				WorldPos = hitData.HitPosWorld,
				WindowPos = hitData.HitPos,
				FingerIndex = hitData.Touchindex,
				Force = hitData.PressForce,
				Lazer = hitData.Laser,
				Touch = !hitData.Laser,
				CustomTouch = hitData.CustomTouch,
			});

		}
		private void RunButtonReleaseEvent(HitData hitData) {
			_lastHitData = hitData;
			IsClicking = false;
			RLog.Info("Release");
			SendEvent(new ButtonEvent {
				IsPressing = false,
				IsClicked = false,
				IsReleased = true,
				WorldPos = hitData.HitPosWorld,
				WindowPos = hitData.HitPos,
				FingerIndex = hitData.Touchindex,
				Force = hitData.PressForce,
				Lazer = hitData.Laser,
				Touch = !hitData.Laser,
				CustomTouch = hitData.CustomTouch,
			});
		}

		public override void Step() {
			base.Step();
			if(Rect is null) {
				return;
			}
			var StillClicking = false;
			foreach (var item in Rect.HitPoses(!AllowOtherZones.Value)) {
				void Touch() {
					if (item.PressForce >= PressForce) {
						if (IsClicking) {
							RunButtonPressingEvent(item);
						}
						else {
							IsClicking = true;
							RunButtonClickEvent(item);
						}
						StillClicking = true;
					}
				}
				if (item.Laser) {
					if (Laserable) {
						Touch();
					}
				}
				else if(item.CustomTouch) {
					if (CustomTouchable) {
						Touch();
					}
				}
				else {
					if (Touchable) {
						Touch();
					}
				}
			}
			if (IsClicking && !StillClicking) {
				RunButtonReleaseEvent(_lastHitData);
			}
		}
	}
}
