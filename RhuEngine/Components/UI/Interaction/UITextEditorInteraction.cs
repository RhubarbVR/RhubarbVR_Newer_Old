using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using System;

namespace RhuEngine.Components
{
	public interface IKeyboardInteraction {
		public void KeyboardBind();
		public void KeyboardUnBind();

	}

	[Category(new string[] { "UI\\Interaction" })]
	public class UITextEditorInteraction : UIInteractionComponent, IKeyboardInteraction
	{
		public readonly SyncRef<ITextComp> TextComp;

		public readonly Linker<string> Value;

		[OnChanged(nameof(UserReload))]
		public readonly SyncRef<User> CurrentUser;

		public bool CurrentEditor { get; private set; }

		public int CurrsorPos;

		public int CursorLength;
		private void UserReload() {
			CurrentEditor = CurrentUser.Target == LocalUser;
		}

		public override void Step() {
			base.Step();
		}

		[Exsposed]
		public void EditingClick() {
			if (CurrentUser.Target is null) {
				CurrentUser.Target = LocalUser;
			}
			else if (CurrentUser.Target == LocalUser) {
				CurrentUser.Target = null;
			}
		}
		[Exsposed]
		public void ForceEndEditing() {
			CurrentUser.Target = null;
		}

		[Exsposed]
		public void EndEditing() {
			if (CurrentUser.Target == LocalUser) {
				CurrentUser.Target = null;
			}
		}
		[Exsposed]
		public void ForceStartEditing() {
			CurrentUser.Target = LocalUser;
		}
		[Exsposed]
		public void StartEditing() {
			if (CurrentUser.Target is null) {
				CurrentUser.Target = null;
			}
		}

		public void KeyboardBind() {
			StartEditing();
		}

		public void KeyboardUnBind() {
			EndEditing();
		}
	}
}
