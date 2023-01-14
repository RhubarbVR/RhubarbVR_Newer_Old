using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Assimp;

using DiscordRPC;

using NYoutubeDL.Options;

using RhubarbCloudClient;

using RhuEngine.Commads;
using RhuEngine.Components.PrivateSpace;
using RhuEngine.Components.PrivateSpace.Programs.OverlayDialogues;
using RhuEngine.Components.UI;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[PrivateSpaceOnly]
	public sealed class SessionsFinderProgram : PrivateSpaceProgram
	{
		private UIElement _element;

		public override RhubarbAtlasSheet.RhubarbIcons IconFind => RhubarbAtlasSheet.RhubarbIcons.Worlds;

		public override string ProgramNameLocName => "Programs.SessionsFinder.Name";

		public override void StartProgram(Stream file = null, string mimetype = null, string ex = null, params object[] args) {
			var window = AddWindow();
			var _scrollContationer = window.Entity.AddChild("Scroll").AttachComponent<ScrollContainer>();
			var box = _scrollContationer.Entity.AddChild("Box").AttachComponent<BoxContainer>();
			box.Vertical.Value = true;
			box.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			box.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
			var headerbox = box.Entity.AddChild("Box").AttachComponent<BoxContainer>();
			headerbox.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			headerbox.Alignment.Value = RBoxContainerAlignment.Center;
			var refreshButton = headerbox.Entity.AddChild("RefreshButton").AttachComponent<Button>();
			refreshButton.Pressed.Target = Refresh;
			refreshButton.Text.Value = "refresh";
			_element = box.Entity.AddChild("CenterINer").AttachComponent<BoxContainer>();
			_element.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			_element.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
			Refresh();
		}

		[Exposed]
		public void Refresh() {
			Task.Run(RefreshAsync);
		}


		public async Task RefreshAsync() {
			if (IsRemoved | IsDestroying) {
				return;
			}
			if(_element is null) {
				return;
			}
			_element.Entity.DestroyChildren();
			if (!Engine.netApiManager.Client.IsLogin) {
				// Show error
				var textLabel = _element.Entity.AddChild("Text").AttachComponent<TextLabel>();
				textLabel.Text.Value = Engine.localisationManager.GetLocalString("Programs.SessionsFinder.Error");
				textLabel.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
				textLabel.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
				textLabel.VerticalAlignment.Value = RVerticalAlignment.Center;
				textLabel.HorizontalAlignment.Value = RHorizontalAlignment.Center;
				return;
			}
			var data = await Engine.netApiManager.Client.GetTopPublicSessions();
			if (IsRemoved | IsDestroying) {
				return;
			}
			if (data is null) {
				var textLabel = _element.Entity.AddChild("Text").AttachComponent<TextLabel>();
				textLabel.Text.Value = Engine.localisationManager.GetLocalString("Programs.SessionsFinder.Error");
				textLabel.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
				textLabel.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
				textLabel.VerticalAlignment.Value = RVerticalAlignment.Center;
				textLabel.HorizontalAlignment.Value = RHorizontalAlignment.Center;
				return;
			}
			if (data.Length == 0) {
				var textLabel = _element.Entity.AddChild("Text").AttachComponent<TextLabel>();
				textLabel.Text.Value = Engine.localisationManager.GetLocalString("Programs.SessionsFinder.NoSessionsFound");
				textLabel.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
				textLabel.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
				textLabel.VerticalAlignment.Value = RVerticalAlignment.Center;
				textLabel.HorizontalAlignment.Value = RHorizontalAlignment.Center;
				return;
			}
			var _grid = _element.Entity.AddChild("Grid").AttachComponent<GridContainer>();
			_grid.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			_grid.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
			_grid.Columns.Value = 2;
			foreach (var item in data) {
				var sessionelement = _grid.Entity.AddChild(item.SessionName).AttachComponent<Button>();
				sessionelement.MinSize.Value = new Vector2i(250, 220);
				var Icon = sessionelement.Entity.AddChild("Icon").AttachComponent<TextureRect>();
				Icon.InputFilter.Value = RInputFilter.Ignore;
				Icon.ExpandedMode.Value = RExpandedMode.IgnoreSize;
				Icon.StrechMode.Value = RStrechMode.KeepAspectCenter;
				Icon.MaxOffset.Value = new Vector2f(0, -20);
				var recture = Icon.Entity.AttachComponent<StaticTexture>();
				Icon.Texture.Target = recture;
				recture.url.Value = item.ThumNailURL;
				var textEdit = sessionelement.Entity.AddChild("text").AttachComponent<TextLabel>();
				textEdit.Text.Value = item.SessionName;
				textEdit.InputFilter.Value = RInputFilter.Ignore;
				textEdit.VerticalAlignment.Value = RVerticalAlignment.Bottom;
				textEdit.HorizontalAlignment.Value = RHorizontalAlignment.Center;
				var delCal = sessionelement.Entity.AttachComponent<DelegateCall>();
				sessionelement.Pressed.Target = delCal.CallDelegate;
				delCal.action = () => {
					Engine.worldManager.JoinNewWorld(item.ID, World.FocusLevel.Focused, item.SessionName);
					CloseProgram();
				};
			}
		}
	}
}
