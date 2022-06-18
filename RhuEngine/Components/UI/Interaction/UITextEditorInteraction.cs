using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using System;
using TextCopy;

namespace RhuEngine.Components
{
	public interface ICurrsorTextProvider : ISyncObject
	{
		[Exposed]
		public int CurrsorPos { get; }
		[Exposed]
		public int CurrsorLength { get; }
		[Exposed]
		public bool RenderCurrsor { get; }
	}

	public interface IKeyboardInteraction
	{
		public void KeyboardBind();
		public void KeyboardUnBind();

	}

	[Category(new string[] { "UI\\Interaction" })]
	public class UIGroupTextEditorInteraction : AbstractTextEditorInteraction
	{
		public override bool CurrentEditor { get; set; }

		public readonly Sync<int> SyncedCurrsorPos;

		public override int RenderCurrsorPos { get => SyncedCurrsorPos.Value; set => SyncedCurrsorPos.Value = value; }
		public readonly Sync<int> SyncedCurrsorLength;

		public override int RenderCurrsorLength { get => SyncedCurrsorLength.Value; set => SyncedCurrsorLength.Value = value; }

		public override void KeyboardBind() {
			CurrentEditor = true;
		}

		public override void KeyboardUnBind() {
			CurrentEditor = false;
			OnDoneEditing.Target?.Invoke();
		}
		[Exposed]
		public override void EditingClick() {
			CurrentEditor = !CurrentEditor;
			if (!CurrentEditor) {
				OnDoneEditing.Target?.Invoke();
			}
		}
	}

	[Category(new string[] { "UI\\Interaction" })]
	public class UITextEditorInteraction : AbstractTextEditorInteraction
	{
		[OnChanged(nameof(UserReload))]
		public readonly SyncRef<User> CurrentUser;
		private void UserReload() {
			CurrentEditor = CurrentUser.Target == LocalUser;
			if (!CurrentEditor && (Engine.KeyboardInteraction == this)) {
				Engine.KeyboardInteractionUnBind(this);
			}
		}
		public override bool CurrentEditor { get; set; }
		public override int RenderCurrsorPos { get; set; }
		public override int RenderCurrsorLength { get; set; }

		[Exposed]
		public override void EditingClick() {
			if (CurrentUser.Target is null) {
				ForceStartEditing();
			}
			else if (CurrentUser.Target == LocalUser) {
				ForceEndEditing();
			}
		}
		[Exposed]
		public void ForceEndEditing() {
			Engine.KeyboardInteractionUnBind(this);
			CurrentUser.Target = null;
			OnDoneEditing.Target?.Invoke();
		}
		[Exposed]
		public void ForceStartEditing() {
			Engine.KeyboardInteractionBind(this);
			CurrentUser.Target = LocalUser;
		}

		[Exposed]
		public void EndEditing() {
			if (CurrentUser.Target == LocalUser) {
				ForceEndEditing();
			}
		}

		[Exposed]
		public void StartEditing() {
			if (CurrentUser.Target is null) {
				ForceStartEditing();
			}
		}

		public override void KeyboardBind() {
			StartEditing();
		}

		public override void KeyboardUnBind() {
			EndEditing();
		}

		public override void Dispose() {
			Engine.KeyboardInteractionUnBind(this);
			base.Dispose();
		}
	}

	public abstract class AbstractTextEditorInteraction : UIInteractionComponent, ICurrsorTextProvider, IKeyboardInteraction
	{
		public readonly SyncDelegate OnDoneEditing;

		public readonly Linker<string> Value;

		public abstract bool CurrentEditor { get; set; }

		public int CurrsorPos => RenderCurrsorPos;

		public int CurrsorLength => RenderCurrsorLength;

		public bool RenderCurrsor => CurrentEditor;

		public abstract int RenderCurrsorPos { get; set; }

		public abstract int RenderCurrsorLength { get; set; }

		public abstract void KeyboardBind();

		public abstract void KeyboardUnBind();
		[Exposed]
		public abstract void EditingClick();

		public override void Step() {
			if (!Value.Linked) {
				return;
			}
			if (!CurrentEditor) {
				return;
			}
			var deltaType = RInput.TypeDelta;
			if (RInput.Key(Key.Alt).IsActive()) {
				return;
			}
			if (RInput.Key(Key.Return).IsActive() && !RInput.Key(Key.Shift).IsActive()) {
				EditingClick();
				return;
			}
			if (RInput.Key(Key.Ctrl).IsActive() && RInput.Key(Key.Z).IsJustActive()) {
				var nstartpoint = Value.LinkedValue.Length - CurrsorPos;
				var nEndpoint = nstartpoint + CurrsorLength;
				if (nEndpoint < nstartpoint) {
					(nstartpoint, nEndpoint) = (nEndpoint, nstartpoint);
				}
				ClipboardService.SetText(Value.LinkedValue.Substring(nstartpoint, nEndpoint - nstartpoint));
				var endbit = Value.LinkedValue.Substring(nEndpoint);
				Value.LinkedValue = Value.LinkedValue.Substring(0, nstartpoint) + endbit;
				RenderCurrsorLength = 0;
				RenderCurrsorPos = endbit.Length;
				return;
			}
			if (RInput.Key(Key.Ctrl).IsActive() && RInput.Key(Key.C).IsJustActive()) {
				var nstartpoint = Value.LinkedValue.Length - CurrsorPos;
				var nEndpoint = nstartpoint + CurrsorLength;
				if (nEndpoint < nstartpoint) {
					(nstartpoint, nEndpoint) = (nEndpoint, nstartpoint);
				}
				ClipboardService.SetText(Value.LinkedValue.Substring(nstartpoint, nEndpoint - nstartpoint));
				return;
			}
			if (RInput.Key(Key.Ctrl).IsActive() && RInput.Key(Key.A).IsJustActive()) {
				RenderCurrsorPos = 0;
				RenderCurrsorLength = -Value.LinkedValue.Length;
				return;
			}
			if (RInput.Key(Key.Left).IsJustActive()) {
				if (RInput.Key(Key.Shift).IsActive()) {
					RenderCurrsorLength--;
					RenderCurrsorLength = Math.Min(Math.Max(RenderCurrsorLength, -(Value.LinkedValue.Length - RenderCurrsorPos)), RenderCurrsorPos);
				}
				else {
					var nstartpoint = Value.LinkedValue.Length - CurrsorPos;
					var nEndpoint = nstartpoint + CurrsorLength;
					if (nEndpoint < nstartpoint) {
						(nstartpoint, nEndpoint) = (nEndpoint, nstartpoint);
					}
					if (nEndpoint != nstartpoint) {
						RenderCurrsorPos = Value.LinkedValue.Length - nstartpoint - 1;
						RenderCurrsorLength = 0;
					}
					RenderCurrsorPos++;
					RenderCurrsorPos = Math.Min(RenderCurrsorPos, Value.LinkedValue.Length - RenderCurrsorLength);
				}
				return;
			}
			if (RInput.Key(Key.Right).IsJustActive()) {
				if (RInput.Key(Key.Shift).IsActive()) {
					RenderCurrsorLength++;
					RenderCurrsorLength = Math.Min(Math.Max(RenderCurrsorLength, -(Value.LinkedValue.Length - RenderCurrsorPos)), RenderCurrsorPos);
				}
				else {
					var nstartpoint = Value.LinkedValue.Length - CurrsorPos;
					var nEndpoint = nstartpoint + CurrsorLength;
					if (nEndpoint < nstartpoint) {
						(nstartpoint, nEndpoint) = (nEndpoint, nstartpoint);
					}
					if (nEndpoint != nstartpoint) {
						RenderCurrsorPos = Value.LinkedValue.Length - nEndpoint + 1;
						RenderCurrsorLength = 0;
					}
					RenderCurrsorPos--;
					RenderCurrsorPos = Math.Max(RenderCurrsorPos, 0);
				}
				return;
			}
			if (RInput.Key(Key.Ctrl).IsActive() && RInput.Key(Key.V).IsJustActive()) {
				deltaType = ClipboardService.GetText();
			}
			else if (RInput.Key(Key.Ctrl).IsActive()) {
				return;
			}
			try { 
			if (RenderCurrsorPos == 0 && RenderCurrsorLength == 0) {
				if (deltaType == "") {
					return;
				}
				var newstring = (Value.LinkedValue + deltaType).ApplyStringFunctions();
				Value.LinkedValue = newstring.ApplyStringFunctions();
				return;
			}
			if (RenderCurrsorLength == 0) {
				if (deltaType == "") {
					return;
				}
				var pos = Value.LinkedValue.Length - CurrsorPos;
				var newstring = (Value.LinkedValue.Substring(0, pos) + deltaType).ApplyStringFunctions();
				Value.LinkedValue = newstring + Value.LinkedValue.Substring(pos);
				return;
			}
			if (RenderCurrsorPos == 0) {
				if (deltaType == "") {
					return;
				}
				if (deltaType[0] == '\b') {
					deltaType = "";
				}
				var pos = Value.LinkedValue.Length + CurrsorLength;
				var newstring = (Value.LinkedValue.Substring(0, pos) + deltaType).ApplyStringFunctions();
				Value.LinkedValue = newstring;
				RenderCurrsorLength = 0;
				return;
			}
			if (RenderCurrsorPos == Value.LinkedValue.Length) {
				if (deltaType == "") {
					return;
				}
				if (deltaType[0] == '\b') {
					deltaType = "";
				}
				var pos = -CurrsorLength;
				if (pos >= 0) {
					var newstring = (deltaType + Value.LinkedValue.Substring(pos)).ApplyStringFunctions();
					Value.LinkedValue = newstring;
				}
				else {
					Value.LinkedValue = deltaType.ApplyStringFunctions();
					RenderCurrsorPos = 0;
				}
				RenderCurrsorLength = 0;
				return;
			}
			if (deltaType == "") {
				return;
			}
			if (deltaType[0] == '\b') {
				deltaType = "";
			}
			var startpoint = Value.LinkedValue.Length - CurrsorPos;
			var Endpoint = startpoint + CurrsorLength;
			if (Endpoint < startpoint) {
				(startpoint, Endpoint) = (Endpoint, startpoint);
			}
			var nstring = (Value.LinkedValue.Substring(0, startpoint) + deltaType).ApplyStringFunctions();
			var addend = Value.LinkedValue.Substring(Endpoint);
			nstring += addend;
			Value.LinkedValue = nstring;
			RenderCurrsorPos = addend.Length;
			RenderCurrsorLength = 0;
			}
			catch {
				RenderCurrsorPos = 0;
				RenderCurrsorLength = 0;
			}
		}
	}
}
