using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using RhuEngine.Linker;

namespace RhuEngine
{
	public sealed class SystemFile : FileBase
	{
		public SystemFile(string path, IDrive drive, IFolder parrent) {
			_path = path;
			Drive = drive;
			Parrent = parrent;
		}
		public SystemFile(string path, IDrive drive) {
			_path = path;
			Drive = drive;
			var parrent = Directory.GetParent(path);
			if (parrent.FullName == System.IO.Path.GetPathRoot(parrent.FullName)) {
				Parrent = null;
				return;
			}
			Parrent = new SystemFolder(parrent.FullName, drive);
		}

		private readonly string _path;
		public override string Name { get => System.IO.Path.GetFileName(_path); set => File.Move(_path, System.IO.Path.Combine(Directory.GetParent(_path).FullName, value)); }

		public override IFolder Parrent { get; }

		public override DateTimeOffset CreationDate => File.GetCreationTimeUtc(_path);

		public override DateTimeOffset LastEdit => File.GetLastWriteTimeUtc(_path);

		public override IDrive Drive { get; }

		public override long SizeInBytes => new FileInfo(_path).Length;
		public override RTexture2D Texture => Drive.Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.File);

		public override string Type => MimeTypeManagment.GetMimeType(_path);

		public override void Open() {
			Drive?.Engine?.worldManager?.PrivateSpaceManager?.ProgramManager?.OverlayProgram?.OpenFile(_path);
		}
	}
}
