using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RhuEngine
{
	public sealed class SystemFolder : FolderBase
	{
		public SystemFolder(string path, IDrive drive, IFolder parrent) {
			_path = path;
			Drive = drive;
			Parrent = parrent;
		}
		private readonly string _path;
		public override string Name
		{
			get => Parrent is null ? (Drive?.Name) : System.IO.Path.GetDirectoryName(_path);
			set {
				if (Parrent is null) {
					if (Drive is null) {
						return;
					}
					Drive.Name = value;
					return;
				}
				Directory.Move(_path, System.IO.Path.Combine(Directory.GetParent(_path).FullName, value));
			}
		}

		public override IFolder Parrent { get; }

		public override DateTimeOffset CreationDate => Directory.GetCreationTimeUtc(_path);

		public override DateTimeOffset LastEdit => Directory.GetLastWriteTimeUtc(_path);

		public override IDrive Drive { get; }

		public override IFile[] Files => Directory.GetFiles(_path).Select(x => new SystemFile(x, Drive, this)).ToArray();

		public override IFolder[] Folders => Directory.GetFiles(_path).Select(x => new SystemFolder(x, Drive, this)).ToArray();

	}
}
