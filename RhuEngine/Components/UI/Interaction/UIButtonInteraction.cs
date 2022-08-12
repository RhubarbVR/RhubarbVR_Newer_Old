﻿using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using System;
using static RhuEngine.Components.UICanvas;

namespace RhuEngine.Components
{
	public struct ButtonEvent
	{
		public bool IsPressing;

		public bool IsClicked;

		public bool IsReleased;

		public Vector3f WorldPos;

		public Vector2f WindowPos;

		public uint FingerIndex;

		public float Force;

		public bool Lazer;

		public bool Touch;

		public bool CustomTouch;
	}

	[Category(new string[] { "UI/Interaction" })]
	public class UIButtonInteraction : UIInteractionComponent
	{
		[Default(true)]
		public readonly Sync<bool> Laserable;

		[Default(true)]
		public readonly Sync<bool> Touchable;
		
		[Default(true)]
		public readonly Sync<bool> CustomTouchable;

		[Default(0.5f)]
		public readonly Sync<float> PressForce;

		public readonly SyncDelegate<Action<ButtonEvent>> ButtonEvent;

		[Exposed]
		[NoWriteExsposed]
		public bool IsClicking { get; private set; }

		private HitData _lastHitData;

		private void SendEvent(ButtonEvent buttonEvent) {
			RWorld.ExecuteOnEndOfFrame(this, () => ButtonEvent.Target?.Invoke(buttonEvent));
		}

		private void RunButtonClickEvent(HitData hitData) {
			_lastHitData = hitData;
			UIRect?.Canvas?.LockPysics();
			SendEvent(new ButtonEvent {
				IsPressing = false,
				IsClicked = true,
				IsReleased = false,
				WorldPos = hitData.Hitpointworld,
				WindowPos = hitData.HitPointOnCanvas,
				FingerIndex = hitData.TouchUndex,
				Force = hitData.PressForce,
				Lazer = hitData.Lazer,
				Touch = !hitData.Lazer,
				CustomTouch = hitData.CustomTouch,
			});
		}

		private void RunButtonPressingEvent(HitData hitData) {
			_lastHitData = hitData;
			SendEvent(new ButtonEvent {
				IsPressing = true,
				IsClicked = false,
				IsReleased = false,
				WorldPos = hitData.Hitpointworld,
				WindowPos = hitData.HitPointOnCanvas,
				FingerIndex = hitData.TouchUndex,
				Force = hitData.PressForce,
				Lazer = hitData.Lazer,
				Touch = !hitData.Lazer,
				CustomTouch = hitData.CustomTouch,
			});

		}
		private void RunButtonReleaseEvent(HitData hitData) {
			_lastHitData = hitData;
			IsClicking = false;
			UIRect.AddAddedDepth(0f);
			UIRect?.Canvas?.UnLockPysics();
			SendEvent(new ButtonEvent {
				IsPressing = false,
				IsClicked = false,
				IsReleased = true,
				WorldPos = hitData.Hitpointworld,
				WindowPos = hitData.HitPointOnCanvas,
				FingerIndex = hitData.TouchUndex,
				Force = hitData.PressForce,
				Lazer = hitData.Lazer,
				Touch = !hitData.Lazer,
				CustomTouch = hitData.CustomTouch,
			});
		}

		public override void Step() {
			base.Step();
			if (UIRect is null) {
				return;
			}
			var StillClicking = false;
			var minPress = 1f;
			foreach (var item in UIRect.GetRectHitData()) {
				void Touch() {
					if (item.PressForce >= PressForce) {
						minPress = Math.Min(minPress, item.PressForce);
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
				if (item.Lazer) {
					if (Laserable) {
						Touch();
					}
				}
				else if (item.CustomTouch) {
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
			if (IsClicking) {
				UIRect.AddAddedDepth(0.005f + (-(UIRect.Depth.Value * 0.999f * minPress)));
			}
			if (IsClicking && !StillClicking) {
				RunButtonReleaseEvent(_lastHitData);
			}
		}
	}
}
