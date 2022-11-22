using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using RhubarbCloudClient;

using RhuEngine.Commads;
using RhuEngine.Components.PrivateSpace;
using RhuEngine.Components.PrivateSpace.Programs.OverlayDialogues;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

using TextCopy;

using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;

namespace RhuEngine.Components
{
	[PrivateSpaceOnly]
	public sealed class FileExplorerProgram : PrivateSpaceProgram
	{
		public override RhubarbAtlasSheet.RhubarbIcons IconFind => RhubarbAtlasSheet.RhubarbIcons.Folder;

		public override string ProgramNameLocName => "Programs.FileExplorer.Name";

		private UIElement _elements;
		private LineEdit _lineEdit;
		private Button _forwaredButton;
		private Button _backButton;
		private Button _backDirButton;

		private Button _changeLayoutButton;
		private BoxContainer _sideBarVisual;
		private IFolder CurrentFolder { get; set; }

		private bool _isGridLayout = false;

		private void UpdateFolder(IFolder folder) {
			CurrentFolder = folder;
			CurrentFolder?.Refresh();
			PathDataUpdate();
			UpdateCenterUI();
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
			refreshButton.Text.Value = "↻";
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

			Refresh();
		}
		private void UpdateCenterUI() {
			_elements.Entity.DestroyChildren();
			if (CurrentFolder is null) {
				//Build this pc


				return;
			}
			var allFolders = CurrentFolder.Folders;
			var allFiles = CurrentFolder.Files;
			if (_isGridLayout) {
				var currentIndex = 0;
				foreach (var item in allFolders) {

					currentIndex++;
				}
				foreach (var item in allFiles) {

					currentIndex++;
				}
			}
			else {
				var scrollRoot = _elements.Entity.AddChild("ScrollRoot").AttachComponent<ScrollContainer>();
				scrollRoot.ClipContents.Value = true;
				scrollRoot.VerticalScrollBar.Value = RScrollBarVisibility.Disable;
				var box = scrollRoot.Entity.AddChild("list").AttachComponent<BoxContainer>();
				box.Vertical.Value = true;
				var boxCon = box.Entity.AddChild("top").AttachComponent<BoxContainer>();
				var nameLabel = boxCon.Entity.AddChild("Name").AttachComponent<TextLabel>();
				nameLabel.HorizontalAlignment.Value = RHorizontalAlignment.Left;
				nameLabel.MinSize.Value = new Vector2i(300, 0);
				nameLabel.TextSize.Value = 20;
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

				var bottom = box.Entity.AddChild("bottom").AttachComponent<BoxContainer>();
				bottom.Vertical.Value = true;
				foreach (var item in allFolders) {

				}

				foreach (var item in allFiles) {

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
			var Desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			var MyPictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
			var MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var MyVideos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
			var MyMusic = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
			AddSideButton(Engine.localisationManager.GetLocalString("Programs.FileExplorer.SpecialFolder.Desktop"), () => NavToPath(Desktop));
			AddSideButton(Engine.localisationManager.GetLocalString("Programs.FileExplorer.SpecialFolder.MyPictures"), () => NavToPath(MyPictures));
			AddSideButton(Engine.localisationManager.GetLocalString("Programs.FileExplorer.SpecialFolder.MyDocuments"), () => NavToPath(MyDocuments));
			AddSideButton(Engine.localisationManager.GetLocalString("Programs.FileExplorer.SpecialFolder.MyVideos"), () => NavToPath(MyVideos));
			AddSideButton(Engine.localisationManager.GetLocalString("Programs.FileExplorer.SpecialFolder.MyMusic"), () => NavToPath(MyMusic));
			var thisPcLoc = Engine.localisationManager.GetLocalString("Programs.FileExplorer.ThisPC");
			AddSideButton(thisPcLoc, () => NavToPath(thisPcLoc));

			foreach (var item in Engine.fileManager.Drives) {
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
