using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;

namespace RhuEngine
{

	public interface IFile
	{
		public string Name { get; set; }
		public string Path { get; }
		public IFolder Parrent { get; }
		public DateTimeOffset CreationDate { get; }
		public DateTimeOffset LastEdit { get; }
		public long SizeInBytes { get; }
		public IDrive Drive { get; }
		public RTexture2D Texture { get; }
		public void Open();
	}
}
