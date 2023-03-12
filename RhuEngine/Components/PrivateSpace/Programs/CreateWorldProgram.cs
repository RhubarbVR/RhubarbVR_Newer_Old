using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Assimp;

using DataModel.Enums;

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
	public sealed partial class CreateWorldProgram : PrivateSpaceProgram
	{
		private OptionButton _optionsAccess;

		public override RhubarbAtlasSheet.RhubarbIcons IconFind => RhubarbAtlasSheet.RhubarbIcons.AddWorld;

		public override string ProgramNameLocName => "Programs.CreateNewWorld.Name";

		public Stream _file;
		private LineEdit _nameEdit;
		private SpinBox _maxUsers;
		private CheckBox _isHidden;
		private OptionButton _optionsWorldType;
		private CheckBox _isLocal;

		public override void StartProgram(Stream file = null, string mimetype = null, string ex = null, params object[] args) {
			var Window = AddWindow();
			_file = file;
			if (file is not null) {
				Window.Title.Value = Engine.localisationManager.GetLocalString("Programs.CreateNewWorld.NewSession.Name");
			}

			var scroll = Window.Entity.AddChild("Scroll").AttachComponent<ScrollContainer>();

			var rootbox = scroll.Entity.AddChild("Box").AttachComponent<BoxContainer>();
			rootbox.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
			rootbox.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			rootbox.Vertical.Value = true;

			var header = rootbox.Entity.AddChild("Text").AttachComponent<TextLabel>();
			header.Text.Value = Window.WindowTitle;
			header.TextSize.Value = 31;
			header.VerticalAlignment.Value = RVerticalAlignment.Center;
			header.HorizontalAlignment.Value = RHorizontalAlignment.Center;
			header.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;

			var box = rootbox.Entity.AddChild("Box").AttachComponent<BoxContainer>();
			box.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
			box.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;

			box.Entity.AddChild("LeftPad").AttachComponent<UIElement>().MinSize.Value = new RNumerics.Vector2i(20);
			var leftBox = box.Entity.AddChild("leftBox").AttachComponent<BoxContainer>();
			leftBox.Vertical.Value = true;
			leftBox.Alignment.Value = RBoxContainerAlignment.Center;
			leftBox.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
			leftBox.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			leftBox.Entity.AddChild("Pad").AttachComponent<UIElement>().MinSize.Value = new RNumerics.Vector2i(10);

			_nameEdit = leftBox.Entity.AddChild("Text").AttachComponent<LineEdit>();
			_nameEdit.PlaceholderText.Value = "World Name";
			_nameEdit.Text.Value = Engine.netApiManager.Client.IsLogin
				? $"{Engine.netApiManager.Client.User.UserName}'s New world"
				: $"{Environment.MachineName}'s New world";

			leftBox.Entity.AddChild("Pad").AttachComponent<UIElement>().MinSize.Value = new RNumerics.Vector2i(10);

			_maxUsers = leftBox.Entity.AddChild("MaxUsers").AttachComponent<SpinBox>();
			_maxUsers.MaxValue.Value = ushort.MaxValue;
			_maxUsers.MinValue.Value = 1;
			_maxUsers.Value.Value = 16;
			_maxUsers.StepValue.Value = 1;

			_isHidden = leftBox.Entity.AddChild("Hidden").AttachComponent<CheckBox>();
			_isHidden.Text.Value = "Hidden";

			box.Entity.AddChild("CenterPad").AttachComponent<UIElement>().MinSize.Value = new RNumerics.Vector2i(10);

			var rightBox = box.Entity.AddChild("rightBox").AttachComponent<BoxContainer>();
			rightBox.Vertical.Value = true;
			rightBox.Alignment.Value = RBoxContainerAlignment.Center;
			rightBox.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
			rightBox.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			rightBox.Entity.AddChild("Pad").AttachComponent<UIElement>().MinSize.Value = new RNumerics.Vector2i(10);


			_optionsAccess = rightBox.Entity.AddChild("Options").AttachComponent<OptionButton>();
			_optionsAccess.Selected.Value = 1;
			var privates = _optionsAccess.Items.Add();
			privates.Text.Value = "Private";
			privates.Id.Value = 0;

			var Public = _optionsAccess.Items.Add();
			Public.Text.Value = "Public";
			Public.Id.Value = 1;

			var Friends = _optionsAccess.Items.Add();
			Friends.Text.Value = "Friends";
			Friends.Id.Value = 2;

			var Followers = _optionsAccess.Items.Add();
			Followers.Text.Value = "Followers";
			Followers.Id.Value = 16;

			var KnownPeople = _optionsAccess.Items.Add();
			KnownPeople.Text.Value = "KnownPeople";
			KnownPeople.Id.Value = (int)AccessLevel.KnownPeople;

			rightBox.Entity.AddChild("CenterPad").AttachComponent<UIElement>().MinSize.Value = new RNumerics.Vector2i(10);

			_optionsWorldType = rightBox.Entity.AddChild("WorldType").AttachComponent<OptionButton>();
			var blank = _optionsWorldType.Items.Add();
			blank.Text.Value = "Blank";
			blank.Id.Value = 0;

			var sdefault = _optionsWorldType.Items.Add();
			sdefault.Text.Value = "Default";
			sdefault.Id.Value = 1;
			_optionsWorldType.Selected.Value = 1;

			_isLocal = rightBox.Entity.AddChild("Local World").AttachComponent<CheckBox>();
			_isLocal.Text.Value = "Local World";
			if (!Engine.netApiManager.Client.IsLogin) {
				_isLocal.ButtonPressed.Value = true;
				_isLocal.Disabled.Value = true;
			}

			box.Entity.AddChild("rightPad").AttachComponent<UIElement>().MinSize.Value = new RNumerics.Vector2i(20);

			var button = rootbox.Entity.AddChild("Bottom").AttachComponent<Button>();
			button.Alignment.Value = RButtonAlignment.Center;
			button.Text.Value = Window.Title.Value;
			button.HorizontalFilling.Value = RFilling.ShrinkCenter;
			button.MinSize.Value = new RNumerics.Vector2i(200, 50);
			button.Pressed.Target = CreateWorld;
			rootbox.Entity.AddChild("rightPad").AttachComponent<UIElement>().MinSize.Value = new RNumerics.Vector2i(20);

		}

		[Exposed]
		public void CreateWorld() {
			if (_file is null) {
				var world = WorldManager.CreateNewWorld(World.FocusLevel.Focused, _nameEdit.Text.Value, (DataModel.Enums.AccessLevel)_optionsAccess.Selected.Value, (int)_maxUsers.Value.Value, _isHidden.ButtonPressed.Value, _isLocal.ButtonPressed.Value);
				if(_optionsWorldType.Selected.Value == 1) {
					world.BuildDefaultWorld();
				}
				CloseProgram();
			}
			else {
				//var world = WorldManager.load

			}

		}
	}
}
