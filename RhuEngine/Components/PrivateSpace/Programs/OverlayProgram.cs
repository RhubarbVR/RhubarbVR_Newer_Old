using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
	[PrivateSpaceOnly]
	[UpdateLevel(UpdateEnum.Normal)]
	public sealed class OverlayProgram : PrivateSpaceProgram
	{
		public override RhubarbAtlasSheet.RhubarbIcons IconFind => RhubarbAtlasSheet.RhubarbIcons.MissingFile;

		public override string ProgramNameLocName => "Overlay";

		public override void StartProgram(Stream file = null, string mimetype = null, string ex = null, params object[] args) {
			if (Engine.windowManager.MainWindow is null) {
				return;
			}
			Engine.windowManager.MainWindow.FilesDropped += MainWindow_FilesDropped;
		}

		private void MainWindow_FilesDropped(string[] obj) {
			obj = obj.SelectMany(DirToFiles).ToArray();
			if (obj.Length == 0) {
				return;
			}
			else if (obj.Length == 1) {
				OpenFile(obj[0]);
			}
			else {
				OpenFiles(obj);
			}
		}

		private IEnumerable<string> DirToFiles(string obj) {
			if (Directory.Exists(obj)) {
				foreach (var item in Directory.GetFiles(obj)) {
					yield return item;
				}
			}
			else {
				yield return obj;
			}
		}

		public void OpenFiles(string[] files) {
			if (files.Length == 0) {
				return;
			}
			MimeTypeManagment.TryGetMimeType(files[0], out var mimeType);
			var ex = Path.GetExtension(files[0]);
			var areAllTheSame = files.All(x => {
				MimeTypeManagment.TryGetMimeType(x, out var othermimeType);
				return othermimeType == mimeType;
			});
			if (areAllTheSame) {
				BatchOpenSameFiles(files, mimeType, ex);
			}
			else {
				OpenDialogue<Multi_Open_WithDialogue>().LoadImport(files);
			}
		}


		private void BatchOpenSameFiles(string[] files, string mimeType, string ex) {
			foreach (var filePath in files) {
				if (!File.Exists(filePath)) {
					return;
				}
			}
			OpenDialogue<Batch_Open_WithDialogue>().LoadImport(files, mimeType, ex);

		}

		public void OpenFile(string filePath) {
			if (!File.Exists(filePath)) {
				return;
			}
			MimeTypeManagment.TryGetMimeType(filePath, out var mimeType);
			var ex = Path.GetExtension(filePath);
			OpenDialogue<Open_WithDialogue>().LoadImport(filePath, null, mimeType, ex);
		}

		public void OpenURI(Uri uri) {
			OpenNoFile("nofile/uri", null, null, uri);
		}

		public void OpenNoFile(string mimeType, Stream stream, string ex, params object[] args) {
			OpenDialogue<Open_WithDialogue>().LoadImport(null, stream, mimeType, ex, args);
		}

		public T OpenDialogue<T>(RhubarbAtlasSheet.RhubarbIcons rhubarbIcons = RhubarbAtlasSheet.RhubarbIcons.File) where T : OverlayDialogue, new() {
			var name = typeof(T).Name.Replace("Dialogue", "").Replace("_", " ");
			var newWindow = AddWindow(name, Engine.staticResources.IconSheet.GetElement(rhubarbIcons), false, true);
			newWindow.SizePixels = new Vector2i(320,350);
			newWindow.CenterWindowIntoView();
			var dialog = newWindow.Entity.AttachComponent<T>();
			newWindow.OnClosedWindow += dialog.Close;
			dialog.programWindow = newWindow;
			dialog.Opened();
			Engine.worldManager.PrivateSpaceManager.UserInterfaceManager.OpenCloseDash = true;
			return dialog;
		}

		protected override void Step() {
			base.Step();
			if (InputManager.KeyboardSystem.IsKeyDown(Key.Ctrl) && InputManager.KeyboardSystem.IsKeyJustDown(Key.V)) {
				var clipBoardText = ClipboardService.GetText();
				// Todo add raw img support
				RLog.Info("ClipBoardText:" + clipBoardText ?? "NULL");
				if (clipBoardText != null) {
					if (File.Exists(clipBoardText)) {
						OpenFile(clipBoardText);
					}
					else if (Directory.Exists(clipBoardText)) {
						OpenFiles(new string[] { clipBoardText }.SelectMany(DirToFiles).ToArray());
					}
					else if (!Engine.HasKeyboard) {
						var text = Path.GetTempFileName() + ".txt";
						var tempFile = File.CreateText(text);
						tempFile.Write(clipBoardText);
						TempFiles.AddTempFile(text);
						OpenFile(text);
					}
				}
			}
		}

	}
}
