using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RhuEngine
{
	public sealed class SystemFile : FileBase
	{
		public SystemFile(string path, IDrive drive, IFolder parrent) {
			_path = path;
			Drive = drive;
			Parrent = parrent;
		}
		private readonly string _path;
		public override string Name { get => System.IO.Path.GetDirectoryName(_path); set => File.Move(_path, System.IO.Path.Combine(Directory.GetParent(_path).FullName, value)); }

		public override IFolder Parrent { get; }

		public override DateTimeOffset CreationDate => File.GetCreationTimeUtc(_path);

		public override DateTimeOffset LastEdit => File.GetLastWriteTimeUtc(_path);

		public override IDrive Drive { get; }

		public override long SizeInBytes => new FileInfo(Path).Length;

		public override void Open() {
			throw new NotImplementedException();
		}
	}
}
