using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine
{
	public abstract class FolderBase : IFolder
	{
		public abstract string Name { get; set; }

		public string Path => System.IO.Path.Combine((Parrent?.Path ?? Drive?.Path??"NULL://"), Name);

		public abstract IFolder Parrent { get; }

		public abstract DateTimeOffset CreationDate { get; }

		public abstract DateTimeOffset LastEdit { get; }

		public abstract IDrive Drive { get; }

		public abstract IFile[] Files { get; }

		public abstract IFolder[] Folders { get; }

	}
}
