using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using RhubarbCloudClient;

using RhuEngine.Commads;
using RhuEngine.Components.PrivateSpace.Programs.OverlayDialogues;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

using TextCopy;

namespace RhuEngine.Components
{
	[ProgramOpenWith("*")]
	[Category(new string[] { "Assets/Importers" })]
	[ProgramHide]
	public sealed class ImportProgram : Program
	{

		public static RhubarbAtlasSheet.RhubarbIcons IconFind => RhubarbAtlasSheet.RhubarbIcons.AddFile;

		public static string ProgramNameLocName => "Common.ImportFile";

		public override RTexture2D ProgramIcon => EngineHelpers.MainEngine.staticResources.IconSheet.GetElement(IconFind);

		public override string ProgramName => EngineHelpers.MainEngine.localisationManager.GetLocalString(ProgramNameLocName);

		private static (Button, Action) AddSelectionButton(Entity attachTo, string text, Action action) {
			var button = attachTo.AddChild(text).AttachComponent<Button>();
			button.Text.Value = text;
			button.Alignment.Value = RButtonAlignment.Center;
			button.Pressed.Target = action;
			button.MinSize.Value = new Vector2i(100);
			return (button, action);
		}

		private bool _hasImportData = false;

		private (string url_path, bool isUrl, Stream rawData) _importData;

		public readonly SyncRef<BoxContainer> scroll;

		public override void StartProgram(Stream file = null, string mimetype = null, string ex = null, params object[] args) {
			_hasImportData = true;
			var path = ex;
			if (args is not null && args.Length == 1 && args[0] is string @string) {
				path = @string;
			}
			var isUri = Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out var uri);
			if (isUri && uri.Scheme == "file") {
				isUri = false;
			}
			_importData = (path, isUri, file);
			var window = AddWindowWithIcon(IconFind);
			window.SizePixels = new Vector2i(320, 350);
			window.CenterWindowIntoView();

			var scroll = this.scroll.Target = window.Entity.AddChild("Scroll").AttachComponent<ScrollContainer>().Entity.AddChild("Back").AttachComponent<BoxContainer>();
			scroll.Vertical.Value = true;
			scroll.Alignment.Value = RBoxContainerAlignment.Center;
			scroll.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
			scroll.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			var text = scroll.Entity.AddChild("Top").AttachComponent<TextLabel>();
			text.Text.Value = "Import";
			text.TextSize.Value = 20;
			text.MinSize.Value = new Vector2i(200, 100);
			var box = scroll.Entity.AddChild("Bottom").AttachComponent<BoxContainer>();
			box.Alignment.Value = RBoxContainerAlignment.Center;
			box.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
			box.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			var grid = box.Entity.AddChild("Grid").AttachComponent<GridContainer>();
			grid.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Fill;
			grid.VerticalFilling.Value = RFilling.ShrinkCenter | RFilling.Fill;
			grid.Columns.Value = 2;
			var root = grid.Entity;
			var gettype = ImportStatics.GetFileTypes(path, mimetype);
			if ((gettype & ImportStatics.FileTypes.Text) != ImportStatics.FileTypes.None) {
				var button = AddSelectionButton(root, "Text", OpenText);
				if (gettype == ImportStatics.FileTypes.Text) {
					button.Item2?.Invoke();
				}
			}
			if ((gettype & ImportStatics.FileTypes.Image) != ImportStatics.FileTypes.None) {
				var button = AddSelectionButton(root, "Texture", OpenTexture);
				if (gettype == ImportStatics.FileTypes.Image) {
					button.Item2?.Invoke();
				}
			}
			if ((gettype & ImportStatics.FileTypes.Mesh) != ImportStatics.FileTypes.None) {
				var button = AddSelectionButton(root, "Model", OpenMesh);
				if (gettype == ImportStatics.FileTypes.Mesh) {
					button.Item2?.Invoke();
				}
			}
			if ((gettype & ImportStatics.FileTypes.Video) != ImportStatics.FileTypes.None) {
				var button = AddSelectionButton(root, "Video", OpenVideo);
				if (gettype == ImportStatics.FileTypes.Video) {
					button.Item2?.Invoke();
				}
			}
			if ((gettype & ImportStatics.FileTypes.Audio) != ImportStatics.FileTypes.None) {
				var button = AddSelectionButton(root, "Audio", OpenAudio);
				if (gettype == ImportStatics.FileTypes.Audio) {
					button.Item2?.Invoke();
				}
			}
			if ((gettype & ImportStatics.FileTypes.Font) != ImportStatics.FileTypes.None) {
				var button = AddSelectionButton(root, "Font", OpenFont);
				if (gettype == ImportStatics.FileTypes.Font) {
					button.Item2?.Invoke();
				}
			}
		}

		private T BuildAttachEntity<T>() where T : Importer, new() {
			var parent = LocalUser.userRoot.Target?.Entity.InternalParent ?? World.RootEntity;
			var newEntity = parent.AddChild(typeof(T).Name);
			var import = newEntity.AttachComponent<T>();
			if (LocalUser is not null) {
				newEntity.GlobalTrans = Matrix.TR(Vector3f.Forward * 0.35f, Quaternionf.Pitched) * LocalUser.userRoot.Target?.head.Target?.GlobalTrans ?? Matrix.Identity;
			}
			import.LoadImportData(_importData.url_path, _importData.isUrl, _importData.rawData, this);
			var box = scroll.Target.Entity.AddChild().AttachComponent<BoxContainer>();
			box.Alignment.Value = RBoxContainerAlignment.Center;
			box.Vertical.Value = true;
			import.BuildUI(box.Entity);
			return import;
		}

		[Exposed]
		public void OpenText() {
			if (!_hasImportData) { return; }
			if (scroll.Target is null) {
				return;
			}
			scroll.Target.Entity.DestroyChildren();
			//Todo Load text importer
		}
		[Exposed]
		public void OpenVideo() {
			if (!_hasImportData) { return; }
			if (scroll.Target is null) {
				return;
			}
			scroll.Target.Entity.DestroyChildren();
			BuildAttachEntity<VideoImporter>();
		}

		[Exposed]
		public void OpenMesh() {
			if (!_hasImportData) { return; }
			if (scroll.Target is null) {
				return;
			}
			scroll.Target.Entity.DestroyChildren();
			BuildAttachEntity<AssimpImporter>();
		}
		[Exposed]
		public void OpenFont() {
			if (!_hasImportData) { return; }
			if (scroll.Target is null) {
				return;
			}
			scroll.Target.Entity.DestroyChildren();
			//Todo Load font importer
		}

		[Exposed]
		public void OpenAudio() {
			if (!_hasImportData) { return; }
			if (scroll.Target is null) {
				return;
			}
			scroll.Target.Entity.DestroyChildren();
			//Todo Load audio importer
		}

		[Exposed]
		public void OpenTexture() {
			if (!_hasImportData) { return; }
			if (scroll.Target is null) {
				return;
			}
			scroll.Target.Entity.DestroyChildren();
			BuildAttachEntity<TextureImporter>();
		}

	}
}
