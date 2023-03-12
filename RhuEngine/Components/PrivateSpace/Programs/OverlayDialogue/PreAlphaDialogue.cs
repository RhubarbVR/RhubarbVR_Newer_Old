using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components.PrivateSpace.Programs.OverlayDialogues
{
	[PrivateSpaceOnly]
	public sealed partial class PreAlphaDialogue : OverlayDialogue
	{
		public void BuildUI() {
			var scrollArray = programWindow.Entity.AddChild().AttachComponent<ScrollContainer>();
			var items = scrollArray.Entity.AddChild("List").AttachComponent<BoxContainer>();
			items.Vertical.Value = true;
			items.VerticalFilling.Value = RFilling.Fill | RFilling.Expand;
			items.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;

			var elemntheader = items.Entity.AddChild().AttachComponent<TextLabel>();
			elemntheader.MinSize.Value = new Vector2i(0, 40);
			elemntheader.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			elemntheader.TextSize.Value = 20;
			elemntheader.Text.Value = "Pre-Alpha Warning";

			var elemntText = items.Entity.AddChild().AttachComponent<TextLabel>();
			elemntText.MinSize.Value = new Vector2i(0, 40);
			elemntText.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			elemntText.VerticalFilling.Value = RFilling.Fill | RFilling.Expand;
			elemntText.TextSize.Value = 17;
			elemntText.AutowrapMode.Value = RAutowrapMode.WordSmart;
			elemntText.Text.Value = "RhubarbVR is in Pre-Alpha, so things are subject to change drastically between updates. This might break saved files which may not load after an update. If you have any bugs or suggestions you can report them on GitHub.";
			var button = items.Entity.AddChild().AttachComponent<Button>();
			button.Text.Value = "I Understand";
			var caller = button.Entity.AttachComponent<DelegateCall>();
			button.Pressed.Target = caller.CallDelegate;
			caller.action = () => CloseWindow();
		}


		public override void Close() {

		}

		public override void Opened() {

		}
	}
}
