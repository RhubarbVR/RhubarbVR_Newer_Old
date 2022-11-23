using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;

namespace RhuEngine
{
	public abstract class FileBase : IFile
	{
		public abstract string Name { get; set; }


		public string Path => System.IO.Path.Combine((Parrent?.Path ?? Drive?.Path ?? "NULL://"), Name);

		public abstract IFolder Parrent { get; }

		public abstract DateTimeOffset CreationDate { get; }

		public abstract DateTimeOffset LastEdit { get; }
		public abstract long SizeInBytes { get; }

		public abstract IDrive Drive { get; }

		public abstract RTexture2D Texture { get; }

		public abstract void Open();
	}
}
