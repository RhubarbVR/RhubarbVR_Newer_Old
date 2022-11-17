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
	public sealed class Open_WithDialogue : OverlayDialogue
	{
		public void LoadImport(string file, Stream stream, string mimeType, string ex, object[] args = null) {
			var foundProgam = false;
			var scrollArray = programWindow.Entity.AddChild().AttachComponent<ScrollContainer>();
			var items = scrollArray.Entity.AddChild("List").AttachComponent<BoxContainer>();
			items.Vertical.Value = true;
			items.VerticalFilling.Value = RFilling.Fill | RFilling.Expand;
			items.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;

			var elemntheader = items.Entity.AddChild().AttachComponent<TextLabel>();
			elemntheader.MinSize.Value = new Vector2i(0, 40);
			elemntheader.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			var elemntheaderlocliztion = items.Entity.AttachComponent<StandardLocale>();
			elemntheaderlocliztion.TargetValue.Target = elemntheader.Text;
			elemntheaderlocliztion.Key.Value = "OpenWith.Header";
			elemntheader.TextSize.Value = 30;
			foreach (var data in ProgramOpenWithAttribute.GetAllPrograms(mimeType, Assembly.GetExecutingAssembly())) {
				foundProgam = true;
				
				var programInfo = data.GetProgramInfo();
				var elemnt = items.Entity.AddChild(data.Name).AttachComponent<Button>();
				elemnt.MinSize.Value = new Vector2i(0, 40);
				elemnt.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
				elemnt.Text.Value = programInfo.ProgramName ?? "NULL";
				elemnt.ExpandIcon.Value = true;
				elemnt.IconAlignment.Value = RButtonAlignment.Left;
				var texture = elemnt.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
				var caller = elemnt.Entity.AttachComponent<DelegateCall>();
				elemnt.Pressed.Target = caller.CallDelegate;
				caller.action = () => {
					ProgramManager.OpenProgram(data, args, stream, mimeType, ex);
					CloseWindow();
				};
				elemnt.Icon.Target = texture;
				texture.LoadAsset(programInfo.icon);
			}
			if (!foundProgam) {
				var elemnt = items.Entity.AddChild().AttachComponent<TextLabel>();
				elemnt.MinSize.Value = new Vector2i(0, 40);
				elemnt.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
				elemnt.AutowrapMode.Value = RAutowrapMode.WordSmart;
				var locliztion = items.Entity.AttachComponent<StandardLocale>();
				locliztion.TargetValue.Target = elemnt.Text;
				locliztion.Key.Value = "OpenWith.NoneFound";
				elemnt.TextSize.Value = 30;

			}
		}


		public override void Close() {

		}

		public override void Opened() {

		}
	}
}
