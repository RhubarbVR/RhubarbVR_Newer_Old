using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Assimp;

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

using TextCopy;

using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;
using static Assimp.Metadata;

namespace RhuEngine.Components
{
	[PrivateSpaceOnly]
	[UpdateLevel(UpdateEnum.Normal)]
	public sealed class FileExplorerProgram : PrivateSpaceProgram
	{
		public override RhubarbAtlasSheet.RhubarbIcons IconFind => RhubarbAtlasSheet.RhubarbIcons.Folder;

		public override string ProgramNameLocName => "Programs.FileExplorer.Name";

		protected override void Step() {
			base.Step();
			if (_gridContainer is null) {
				return;
			}
			if (MainProgramWindow is null) {
				return;
			}
			var sizeNoSideBar = MainProgramWindow.SizePixels.x - 170;
			var amount = (int)Math.Floor(sizeNoSideBar / _gridSize);
			if (_gridContainer.Columns.Value != amount) {
				_gridContainer.Columns.Value = amount;
			}
		}

		private GridContainer _gridContainer;
		private float _gridSize;
		private UIElement _elements;
		private Entity _drop;
		private LineEdit _lineEdit;
		private Button _forwaredButton;
		private Button _backButton;
		private Button _backDirButton;

		private Button _changeLayoutButton;
		private BoxContainer _sideBarVisual;
		private IFolder CurrentFolder { get; set; }

		private bool _isGridLayout = true;

		private void UpdateFolder(IFolder folder) {
			CurrentFolder = folder;
			Refresh();
		}

		private void PathDataUpdate() {
			_backDirButton.Disabled.Value = CurrentFolder?.Parrent is null;
			_lineEdit.Text.Value = (CurrentFolder?.Path) ?? Engine.localisationManager.GetLocalString("Programs.FileExplorer.ThisPC");
		}

		private void UpdateFolderAddBack(IFolder folder) {
			if (folder == CurrentFolder) {
				Refresh();
				return;
			}
			_backList.Add(CurrentFolder);
			if (_backList.Count > 25) {
				_backList.RemoveAt(0);
			}
			_forwaredList.Clear();
			UpdateFolder(folder);
			UpdateRedoUndoButtons();
		}

		private readonly List<IFolder> _backList = new(25);
		private readonly List<IFolder> _forwaredList = new(25);

		public override void StartProgram(Stream file = null, string mimetype = null, string ex = null, params object[] args) {
			var root = AddWindow();
			var mainBox = root.Entity.AddChild("MainBox").AttachComponent<BoxContainer>();
			mainBox.Vertical.Value = true;

			var header = mainBox.Entity.AddChild("Headerroot").AttachComponent<UIElement>();
			header.MinSize.Value = new Vector2i(0, 45);
			header.Entity.AddChild().AttachComponent<Panel>();

			var headerBox = header.Entity.AddChild("headerBox").AttachComponent<BoxContainer>();
			headerBox.MinSize.Value = new Vector2i(0, 45);

			_backButton = headerBox.Entity.AddChild("Back").AttachComponent<Button>();
			_backButton.Text.Value = "<";
			_backButton.MinSize.Value = new Vector2i(40);
			_backButton.Pressed.Target = Back;
			_backButton.Disabled.Value = true;
			_backButton.Alignment.Value = RButtonAlignment.Center;
			_forwaredButton = headerBox.Entity.AddChild("Forward").AttachComponent<Button>();
			_forwaredButton.Text.Value = ">";
			_forwaredButton.MinSize.Value = new Vector2i(40);
			_forwaredButton.Disabled.Value = true;
			_forwaredButton.Pressed.Target = Forward;
			_forwaredButton.Alignment.Value = RButtonAlignment.Center;


			_changeLayoutButton = headerBox.Entity.AddChild("ChangeLayout").AttachComponent<Button>();
			_changeLayoutButton.Text.Value = "ch";
			_changeLayoutButton.MinSize.Value = new Vector2i(40);
			_changeLayoutButton.Pressed.Target = ChangeLayout;
			_changeLayoutButton.Alignment.Value = RButtonAlignment.Center;

			var refreshButton = headerBox.Entity.AddChild("refreshButton").AttachComponent<Button>();
			var refrech = headerBox.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
			refreshButton.Icon.Target = refrech;
			refrech.LoadAsset(Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.Refresh));
			refreshButton.ExpandIcon.Value = true;
			refreshButton.IconAlignment.Value = RButtonAlignment.Center;
			refreshButton.MinSize.Value = new Vector2i(40);
			refreshButton.Pressed.Target = Refresh;
			refreshButton.Alignment.Value = RButtonAlignment.Center;

			_backDirButton = headerBox.Entity.AddChild("BackDir").AttachComponent<Button>();
			_backDirButton.Text.Value = "<-";
			_backDirButton.MinSize.Value = new Vector2i(40);
			_backDirButton.Pressed.Target = BackDir;
			_backDirButton.Alignment.Value = RButtonAlignment.Center;

			_lineEdit = headerBox.Entity.AddChild("DirInput").AttachComponent<LineEdit>();
			_lineEdit.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			_lineEdit.TextSubmitted.Target = LineEditSubmit;
			_lineEdit.Alignment.Value = RHorizontalAlignment.Left;

			var secBox = mainBox.Entity.AddChild("secBox").AttachComponent<BoxContainer>();
			secBox.Alignment.Value = RBoxContainerAlignment.Center;
			secBox.VerticalFilling.Value = RFilling.Fill | RFilling.Expand;

			var sideBarScroll = secBox.Entity.AddChild("sideBarScroll").AttachComponent<ScrollContainer>();
			sideBarScroll.MinSize.Value = new Vector2i(150, 0);
			sideBarScroll.ClipContents.Value = true;
			_sideBarVisual = sideBarScroll.Entity.AddChild("boxOnSideBar").AttachComponent<BoxContainer>();
			_sideBarVisual.Vertical.Value = true;
			_sideBarVisual.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;

			var mainArea = secBox.Entity.AddChild("mainArea").AttachComponent<UIElement>();
			mainArea.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;

			mainArea.Entity.AddChild().AttachComponent<Panel>();
			mainArea.Entity.AddChild().AttachComponent<Panel>();


			_elements = mainArea.Entity.AddChild("Elements").AttachComponent<UIElement>();
			_drop = mainArea.Entity.AddChild("Drop");
			var element = _drop.AttachComponent<UIElement>();
			element.Max.Value = element.Min.Value = new Vector2f(0.5f);
			element.MinOffset.Value = new Vector2f(50, -50);
			element.MaxOffset.Value = new Vector2f(-50, 50);

			_drop.AttachComponent<ReferenceAccepter<Entity>>().Dropped.Target = SaveEntity;

			Refresh();
		}

		[Exposed]
		public void SaveEntity(Entity entity) {
			if (CurrentFolder is null) {
				return;
			}
			if (CurrentFolder is NetworkedFolder networked) {
				BuildSaveDialogue(entity, networked);
			}
			else if (CurrentFolder is SystemFolder system) {
				BuildExportDialogue(entity, system);
			}
		}

		private (Entity, LineEdit, ViewPortProgramWindow) BuildMainDialoguePart(string text, IFolder folder, Entity entity) {
			var name = Engine.localisationManager.GetLocalString(text + "Title");
			var window = AddWindow(name, null, false);
			window.SizePixels = new Vector2i(320, 350);
			window.CenterWindowIntoView();
			var center = window.Entity.AddChild("Scroll").AttachComponent<ScrollContainer>().Entity.AddChild("Center");
			var root = center.AttachComponent<BoxContainer>();
			root.Vertical.Value = true;
			root.Alignment.Value = RBoxContainerAlignment.Center;
			root.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			root.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
			var label = center.AddChild("Title").AttachComponent<TextLabel>();
			label.Text.Value = name;
			label.TextSize.Value = 20;
			var pathlabel = center.AddChild("Title").AttachComponent<TextLabel>();
			pathlabel.Text.Value = Engine.localisationManager.GetLocalString(text + "Path", entity.name.Value, folder.Path);
			pathlabel.TextSize.Value = 15;
			var namelabel = center.AddChild("Title").AttachComponent<TextLabel>();
			namelabel.Text.Value = Engine.localisationManager.GetLocalString("Common.Name");
			namelabel.TextSize.Value = 13;
			namelabel.MinSize.Value = new Vector2i(300, 0);
			namelabel.HorizontalAlignment.Value = RHorizontalAlignment.Left;
			namelabel.HorizontalFilling.Value = RFilling.ShrinkCenter;
			var nameChange = center.AddChild("nameChange").AttachComponent<LineEdit>();
			nameChange.Text.Value = entity.name.Value;
			nameChange.PlaceholderText.Value = Engine.localisationManager.GetLocalString("Common.Name");
			nameChange.HorizontalFilling.Value = RFilling.ShrinkCenter;
			nameChange.MinSize.Value = new Vector2i(300, 20);
			return (center, nameChange, window);
		}

		private void BuildSaveDialogue(Entity taget, NetworkedFolder networked) {
			var data = BuildMainDialoguePart("Programs.FileExplorer.SaveDialogue.", networked, taget);
			var buttons = data.Item1.AddChild("ButtonHolder").AttachComponent<BoxContainer>();
			buttons.Alignment.Value = RBoxContainerAlignment.Center;
			var cancle = buttons.Entity.AddChild("Cancle").AttachComponent<Button>();
			cancle.Text.Value = Engine.localisationManager.GetLocalString("Common.Cancel");
			var cancledel = cancle.Entity.AttachComponent<DelegateCall>();
			cancle.Pressed.Target = cancledel.CallDelegate;
			cancledel.action = () => data.Item3.Close();

			var ok = buttons.Entity.AddChild("ok").AttachComponent<Button>();
			ok.Text.Value = Engine.localisationManager.GetLocalString("Programs.FileExplorer.SaveDialogue.Save");
			var okdel = cancle.Entity.AttachComponent<DelegateCall>();
			ok.Pressed.Target = okdel.CallDelegate;
			okdel.action = () => {

			};
		}

		private void BuildExportDialogue(Entity taget, SystemFolder systemFolder) {
			var data = BuildMainDialoguePart("Programs.FileExplorer.ExportDialogue.", systemFolder, taget);
			var exportAssets = data.Item1.AddChild("AssetsExport").AttachComponent<CheckBox>();
			exportAssets.Text.Value = Engine.localisationManager.GetLocalString("Programs.FileExplorer.ExportDialogue.EmbedAssets");
			exportAssets.ButtonPressed.Value = true;
			var buttons = data.Item1.AddChild("ButtonHolder").AttachComponent<BoxContainer>();
			buttons.Alignment.Value = RBoxContainerAlignment.Center;
			var cancle = buttons.Entity.AddChild("Cancle").AttachComponent<Button>();
			cancle.Text.Value = Engine.localisationManager.GetLocalString("Common.Cancel");
			var cancledel = cancle.Entity.AttachComponent<DelegateCall>();
			cancle.Pressed.Target = cancledel.CallDelegate;
			cancledel.action = () => data.Item3.Close();

			var ok = buttons.Entity.AddChild("ok").AttachComponent<Button>();
			ok.Text.Value = Engine.localisationManager.GetLocalString("Programs.FileExplorer.ExportDialogue.Export");
			var okdel = cancle.Entity.AttachComponent<DelegateCall>();
			ok.Pressed.Target = okdel.CallDelegate;
			okdel.action = () => {

			};
		}

		private void UpdateCenterUI() {
			_elements.Entity.DestroyChildren();
			if (CurrentFolder is null) {
				//Build this pc
				_drop.enabled.Value = false;
				var scrollRoot = _elements.Entity.AddChild("ScrollRoot").AttachComponent<ScrollContainer>();
				scrollRoot.ClipContents.Value = true;
				var gridData = scrollRoot.Entity.AddChild("list").AttachComponent<GridContainer>();
				_gridContainer = gridData;
				_gridSize = 300;

				foreach (var item in Engine.fileManager.GetDrives()) {
					var buttonBase = gridData.Entity.AddChild(item.Name).AttachComponent<Button>();
					buttonBase.MinSize.Value = new Vector2i(300, 125);
					buttonBase.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
					var boxButtonCon = buttonBase.Entity.AddChild().AttachComponent<BoxContainer>();
					boxButtonCon.Alignment.Value = RBoxContainerAlignment.Center;
					var Icon = boxButtonCon.Entity.AddChild().AttachComponent<TextureRect>();
					Icon.MinSize.Value = new Vector2i(65);
					var asset = Icon.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
					Icon.Texture.Target = asset;
					Icon.ExpandedMode.Value = RExpandedMode.IgnoreSize;
					Icon.StrechMode.Value = RStrechMode.KeepAspectCenter;
					asset.LoadAsset(Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.Archive));

					var boxButtonCon2 = boxButtonCon.Entity.AddChild().AttachComponent<BoxContainer>();
					boxButtonCon2.Alignment.Value = RBoxContainerAlignment.Center;
					boxButtonCon2.Vertical.Value = true;
					var itemData = boxButtonCon2.Entity.AddChild("Name").AttachComponent<LineEdit>();
					itemData.Text.Value = item.Name;
					itemData.MinSize.Value = new Vector2i(190, 20);
					itemData.GrowVertical.Value = RGrowVertical.Both;
					itemData.VerticalFilling.Value = RFilling.ShrinkCenter;
					if (!(item.TotalBytes == item.UsedBytes && item.UsedBytes == 0)) {
						var ProgressitemData = boxButtonCon2.Entity.AddChild("Progress").AttachComponent<ProgressBar>();
						ProgressitemData.Value.Value = item.UsedBytes;
						ProgressitemData.MaxValue.Value = item.TotalBytes;
						ProgressitemData.MinSize.Value = itemData.MinSize.Value;
						ProgressitemData.GrowVertical.Value = RGrowVertical.Both;
						ProgressitemData.VerticalFilling.Value = RFilling.ShrinkCenter;
					}
					var Text = boxButtonCon2.Entity.AddChild("ProgressText").AttachComponent<TextLabel>();
					Text.VerticalAlignment.Value = RVerticalAlignment.Top;
					Text.MinSize.Value = itemData.MinSize.Value;
					Text.GrowVertical.Value = RGrowVertical.Both;
					Text.VerticalFilling.Value = RFilling.ShrinkCenter;
					Text.TextSize.Value = 15;
					Text.Text.Value = item.TotalBytes == item.UsedBytes && item.UsedBytes == 0
						? ""
						: Engine.localisationManager.GetLocalString("Programs.FileExplorer.FreeOf", FileSizeFormatter.FormatSize(item.TotalBytes - item.UsedBytes), FileSizeFormatter.FormatSize(item.TotalBytes));


					var Namedel = buttonBase.Entity.AttachComponent<DelegateCall>();
					itemData.TextSubmitted.Target = Namedel.CallDelegate;
					Namedel.action = () => {
						if (item.Name != itemData.Text.Value) {
							item.Name = itemData.Text.Value;
						}
					};


					var del = buttonBase.Entity.AttachComponent<DelegateCall>();
					buttonBase.Pressed.Target = del.CallDelegate;
					del.action = () => UpdateFolderAddBack(item.Root);
					boxButtonCon.InputFilter.Value = RInputFilter.Pass;
					Icon.InputFilter.Value = RInputFilter.Pass;
				}


				return;
			}
			_drop.enabled.Value = true;
			var allFolders = CurrentFolder.Folders;
			var allFiles = CurrentFolder.Files;
			if (_isGridLayout) {
				var scrollRoot = _elements.Entity.AddChild("ScrollRoot").AttachComponent<ScrollContainer>();
				scrollRoot.ClipContents.Value = true;
				var gridData = scrollRoot.Entity.AddChild("list").AttachComponent<GridContainer>();
				_gridContainer = gridData;
				_gridSize = 125;
				gridData.Columns.Value = 3;

				foreach (var item in allFolders) {
					var buttonBase = gridData.Entity.AddChild(item.Name).AttachComponent<Button>();
					buttonBase.MinSize.Value = new Vector2i(125);
					buttonBase.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
					var boxButtonCon = buttonBase.Entity.AddChild().AttachComponent<BoxContainer>();
					boxButtonCon.Vertical.Value = true;
					boxButtonCon.Alignment.Value = RBoxContainerAlignment.Center;
					var Icon = boxButtonCon.Entity.AddChild().AttachComponent<TextureRect>();
					Icon.MinSize.Value = new Vector2i(65);
					var asset = Icon.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
					Icon.Texture.Target = asset;
					Icon.ExpandedMode.Value = RExpandedMode.IgnoreSize;
					Icon.StrechMode.Value = RStrechMode.KeepAspectCenter;
					asset.LoadAsset(item.Texture);
					var itemData = boxButtonCon.Entity.AddChild("Name").AttachComponent<LineEdit>();
					itemData.Text.Value = item.Name;
					itemData.MinSize.Value = new Vector2i(100, 20);
					itemData.GrowVertical.Value = RGrowVertical.Both;
					itemData.VerticalFilling.Value = RFilling.ShrinkCenter;

					var Namedel = buttonBase.Entity.AttachComponent<DelegateCall>();
					itemData.TextSubmitted.Target = Namedel.CallDelegate;
					Namedel.action = () => {
						if (item.Name != itemData.Text.Value) {
							item.Name = itemData.Text.Value;
						}
					};


					var del = buttonBase.Entity.AttachComponent<DelegateCall>();
					buttonBase.Pressed.Target = del.CallDelegate;
					del.action = () => UpdateFolderAddBack(item);
					boxButtonCon.InputFilter.Value = RInputFilter.Pass;
					Icon.InputFilter.Value = RInputFilter.Pass;
				}
				foreach (var item in allFiles) {
					var buttonBase = gridData.Entity.AddChild(item.Name).AttachComponent<Button>();
					buttonBase.MinSize.Value = new Vector2i(125);
					buttonBase.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
					var boxButtonCon = buttonBase.Entity.AddChild().AttachComponent<BoxContainer>();
					boxButtonCon.Vertical.Value = true;
					boxButtonCon.Alignment.Value = RBoxContainerAlignment.Center;
					var Icon = boxButtonCon.Entity.AddChild().AttachComponent<TextureRect>();
					Icon.MinSize.Value = new Vector2i(65);
					var asset = Icon.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
					Icon.Texture.Target = asset;
					Icon.ExpandedMode.Value = RExpandedMode.IgnoreSize;
					Icon.StrechMode.Value = RStrechMode.KeepAspectCenter;
					asset.LoadAsset(item.Texture);
					var itemData = boxButtonCon.Entity.AddChild("Name").AttachComponent<LineEdit>();
					itemData.Text.Value = item.Name;
					itemData.MinSize.Value = new Vector2i(100, 20);
					itemData.GrowVertical.Value = RGrowVertical.Both;
					itemData.VerticalFilling.Value = RFilling.ShrinkCenter;

					var Namedel = buttonBase.Entity.AttachComponent<DelegateCall>();
					itemData.TextSubmitted.Target = Namedel.CallDelegate;
					Namedel.action = () => {
						if (item.Name != itemData.Text.Value) {
							item.Name = itemData.Text.Value;
						}
					};
					var del = buttonBase.Entity.AttachComponent<DelegateCall>();
					buttonBase.Pressed.Target = del.CallDelegate;
					del.action = () => item.Open();
					boxButtonCon.InputFilter.Value = RInputFilter.Pass;
					itemData.InputFilter.Value = RInputFilter.Pass;
					Icon.InputFilter.Value = RInputFilter.Pass;

				}
			}
			else {
				_gridContainer = null;
				var scrollRoot = _elements.Entity.AddChild("ScrollRoot").AttachComponent<ScrollContainer>();
				scrollRoot.ClipContents.Value = true;
				var box = scrollRoot.Entity.AddChild("list").AttachComponent<BoxContainer>();
				box.Vertical.Value = true;
				box.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;

				var boxCon = box.Entity.AddChild("top").AttachComponent<BoxContainer>();
				boxCon.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;

				var nameLabel = boxCon.Entity.AddChild("Name").AttachComponent<TextLabel>();
				nameLabel.HorizontalAlignment.Value = RHorizontalAlignment.Left;
				nameLabel.MinSize.Value = new Vector2i(300, 0);
				nameLabel.TextSize.Value = 20;
				nameLabel.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;

				var sizeName = nameLabel.Entity.AttachComponent<StandardLocale>();
				sizeName.TargetValue.Target = nameLabel.Text;
				sizeName.Key.Value = "Programs.FileExplorer.FileName";


				var typeLabel = boxCon.Entity.AddChild("Type").AttachComponent<TextLabel>();
				typeLabel.HorizontalAlignment.Value = RHorizontalAlignment.Left;
				typeLabel.MinSize.Value = new Vector2i(150, 0);
				var sizeType = typeLabel.Entity.AttachComponent<StandardLocale>();
				sizeType.TargetValue.Target = typeLabel.Text;
				sizeType.Key.Value = "Programs.FileExplorer.Type";
				typeLabel.TextSize.Value = 20;


				var sizeLabel = boxCon.Entity.AddChild("Size").AttachComponent<TextLabel>();
				sizeLabel.HorizontalAlignment.Value = RHorizontalAlignment.Left;
				sizeLabel.MinSize.Value = new Vector2i(150, 0);
				sizeLabel.TextSize.Value = 20;
				var sizetext = sizeLabel.Entity.AttachComponent<StandardLocale>();
				sizetext.TargetValue.Target = sizeLabel.Text;
				sizetext.Key.Value = "Programs.FileExplorer.Size";

				var bottomBOx = box.Entity.AddChild("bottom").AttachComponent<BoxContainer>();
				bottomBOx.Vertical.Value = true;
				foreach (var item in allFolders) {
					var buttonBase = bottomBOx.Entity.AddChild(item.Name).AttachComponent<Button>();
					buttonBase.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
					buttonBase.MinSize.Value = new Vector2i(0, 45);
					var boxButtonCon = buttonBase.Entity.AddChild().AttachComponent<BoxContainer>();
					boxButtonCon.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
					var Icon = boxButtonCon.Entity.AddChild().AttachComponent<TextureRect>();
					Icon.MinSize.Value = new Vector2i(45);
					var asset = Icon.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
					Icon.Texture.Target = asset;
					Icon.ExpandedMode.Value = RExpandedMode.IgnoreSize;
					Icon.StrechMode.Value = RStrechMode.KeepAspectCenter;
					asset.LoadAsset(item.Texture);
					var itemData = boxButtonCon.Entity.AddChild("Name").AttachComponent<LineEdit>();
					itemData.Text.Value = item.Name;
					itemData.MinSize.Value = new Vector2i(255, 0);
					itemData.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;

					var typeText = boxButtonCon.Entity.AddChild("Type").AttachComponent<TextLabel>();
					typeText.HorizontalAlignment.Value = RHorizontalAlignment.Right;
					typeText.MinSize.Value = new Vector2i(150, 0);
					typeText.Text.Value = "Folder";
					typeText.TextSize.Value = 17;
					typeText.AutowrapMode.Value = RAutowrapMode.Arbitrary;
					typeText.OverrunBehavior.Value = ROverrunBehavior.TrimEllipsis;
					var sizeText = boxButtonCon.Entity.AddChild("Size").AttachComponent<TextLabel>();
					sizeText.HorizontalAlignment.Value = RHorizontalAlignment.Right;
					sizeText.MinSize.Value = new Vector2i(150, 0);
					sizeText.TextSize.Value = 17;
					sizeText.Text.Value = "";
					sizeText.AutowrapMode.Value = RAutowrapMode.Arbitrary;
					sizeText.OverrunBehavior.Value = ROverrunBehavior.TrimEllipsis;
					var Namedel = buttonBase.Entity.AttachComponent<DelegateCall>();
					itemData.TextSubmitted.Target = Namedel.CallDelegate;
					Namedel.action = () => {
						if (item.Name != itemData.Text.Value) {
							item.Name = itemData.Text.Value;
						}
					};

					var del = buttonBase.Entity.AttachComponent<DelegateCall>();
					buttonBase.Pressed.Target = del.CallDelegate;
					del.action = () => UpdateFolderAddBack(item);
					boxButtonCon.InputFilter.Value = RInputFilter.Pass;
					itemData.InputFilter.Value = RInputFilter.Pass;
					Icon.InputFilter.Value = RInputFilter.Pass;

				}

				foreach (var item in allFiles) {
					var buttonBase = bottomBOx.Entity.AddChild(item.Name).AttachComponent<Button>();
					buttonBase.MinSize.Value = new Vector2i(0, 45);
					var boxButtonCon = buttonBase.Entity.AddChild().AttachComponent<BoxContainer>();
					boxButtonCon.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
					var Icon = boxButtonCon.Entity.AddChild().AttachComponent<TextureRect>();
					Icon.MinSize.Value = new Vector2i(45);
					var asset = Icon.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
					Icon.Texture.Target = asset;
					Icon.ExpandedMode.Value = RExpandedMode.IgnoreSize;
					Icon.StrechMode.Value = RStrechMode.KeepAspectCenter;
					asset.LoadAsset(item.Texture);

					var itemData = boxButtonCon.Entity.AddChild("Name").AttachComponent<LineEdit>();
					itemData.Text.Value = item.Name;
					itemData.MinSize.Value = new Vector2i(255, 0);
					itemData.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;

					var typeText = boxButtonCon.Entity.AddChild("Type").AttachComponent<TextLabel>();
					typeText.HorizontalAlignment.Value = RHorizontalAlignment.Right;
					typeText.MinSize.Value = new Vector2i(150, 0);
					typeText.Text.Value = item.Type;//Todo add fancy name
					typeText.TextSize.Value = 17;
					typeText.AutowrapMode.Value = RAutowrapMode.Arbitrary;
					typeText.OverrunBehavior.Value = ROverrunBehavior.TrimEllipsis;

					var sizeText = boxButtonCon.Entity.AddChild("Size").AttachComponent<TextLabel>();
					sizeText.HorizontalAlignment.Value = RHorizontalAlignment.Right;
					sizeText.MinSize.Value = new Vector2i(150, 0);
					sizeText.TextSize.Value = 17;
					sizeText.Text.Value = FileSizeFormatter.FormatSize(item.SizeInBytes);
					if (string.IsNullOrEmpty(sizeText.Text.Value)) {
						sizeLabel.Text.Value = "SizeError";
					}
					sizeText.AutowrapMode.Value = RAutowrapMode.Arbitrary;
					sizeText.OverrunBehavior.Value = ROverrunBehavior.TrimEllipsis;

					var Namedel = buttonBase.Entity.AttachComponent<DelegateCall>();
					itemData.TextSubmitted.Target = Namedel.CallDelegate;
					Namedel.action = () => {
						if (item.Name != itemData.Text.Value) {
							item.Name = itemData.Text.Value;
						}
					};
					var del = buttonBase.Entity.AttachComponent<DelegateCall>();
					buttonBase.Pressed.Target = del.CallDelegate;
					del.action = () => item.Open();
					boxButtonCon.InputFilter.Value = RInputFilter.Pass;
					itemData.InputFilter.Value = RInputFilter.Pass;
					Icon.InputFilter.Value = RInputFilter.Pass;
				}

			}
		}

		private void BuildSideBarDataList() {
			_sideBarVisual.Entity.DestroyChildren();
			void AddSideButton(string text, Action action) {
				var button = _sideBarVisual.Entity.AddChild(text).AttachComponent<Button>();
				button.Text.Value = text;
				button.Alignment.Value = RButtonAlignment.Left;
				button.MinSize.Value = new Vector2i(0, 42);
				var delcal = button.Entity.AttachComponent<DelegateCall>();
				delcal.action = action;
				button.Pressed.Target = delcal.CallDelegate;
			}
			try {
				var Desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
				AddSideButton(Engine.localisationManager.GetLocalString("Programs.FileExplorer.SpecialFolder.Desktop"), () => NavToPath(Path.GetFullPath(Desktop)));
			}
			catch {

			}
			try {
				var MyPictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
				AddSideButton(Engine.localisationManager.GetLocalString("Programs.FileExplorer.SpecialFolder.MyPictures"), () => NavToPath(Path.GetFullPath(MyPictures)));
			}
			catch {

			}
			try {
				var MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				AddSideButton(Engine.localisationManager.GetLocalString("Programs.FileExplorer.SpecialFolder.MyDocuments"), () => NavToPath(Path.GetFullPath(MyDocuments)));
			}
			catch {

			}
			try {
				var MyVideos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
				AddSideButton(Engine.localisationManager.GetLocalString("Programs.FileExplorer.SpecialFolder.MyVideos"), () => NavToPath(Path.GetFullPath(MyVideos)));
			}
			catch {

			}
			try {
				var MyMusic = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
				AddSideButton(Engine.localisationManager.GetLocalString("Programs.FileExplorer.SpecialFolder.MyMusic"), () => NavToPath(Path.GetFullPath(MyMusic)));
			}
			catch {

			}
			var thisPcLoc = Engine.localisationManager.GetLocalString("Programs.FileExplorer.ThisPC");
			AddSideButton(thisPcLoc, () => NavToPath(thisPcLoc));

			foreach (var item in Engine.fileManager.GetDrives()) {
				AddSideButton(item.Name, () => UpdateFolderAddBack(item.Root));
			}

		}

		private void UpdateRedoUndoButtons() {
			_forwaredButton.Disabled.Value = _forwaredList.Count <= 0;
			_backButton.Disabled.Value = _backList.Count <= 0;
		}

		[Exposed]
		public void Back() {
			if (_backList.Count <= 0) {
				UpdateRedoUndoButtons();
				return;
			}
			var lastData = _backList.Last();
			_backList.RemoveAt(_backList.Count - 1);
			_forwaredList.Add(CurrentFolder);
			UpdateFolder(lastData);
			UpdateRedoUndoButtons();
		}
		[Exposed]
		public void Forward() {
			if (_forwaredList.Count <= 0) {
				UpdateRedoUndoButtons();
				return;
			}
			var lastData = _forwaredList.Last();
			_forwaredList.RemoveAt(_forwaredList.Count - 1);
			_backList.Add(CurrentFolder);
			UpdateFolder(lastData);
			UpdateRedoUndoButtons();
		}
		[Exposed]
		public void BackDir() {
			UpdateFolderAddBack(CurrentFolder?.Parrent);
		}

		[Exposed]
		public void Refresh() {
			Task.Run(RefreshAsync);
		}

		private async Task RefreshAsync() {
			if (CurrentFolder is not null) {
				await CurrentFolder.Refresh();
			}
			await Engine.fileManager.ReloadAllDrivesAsync();
			BuildSideBarDataList();
			UpdateCenterUI();
			UpdateRedoUndoButtons();
			PathDataUpdate();
		}

		[Exposed]
		public void ChangeLayout() {
			_isGridLayout = !_isGridLayout;
			Refresh();
		}

		private void NavToPath(string path) {
			RLog.Info("Nav To Path Path:" + path);
			if (path.ToLower() == Engine.localisationManager.GetLocalString("Programs.FileExplorer.ThisPC").ToLower()) {
				UpdateFolderAddBack(null);
			}
			else {
				if (Engine.fileManager.TryGetDataFromPath(path, out var folder, out var file) && folder is not null) {
					UpdateFolderAddBack(folder);
				}
				else {
					file?.Open();
					Refresh();
				}
			}
		}

		[Exposed]
		public void LineEditSubmit() {
			NavToPath(_lineEdit.Text.Value);
		}

	}
}
