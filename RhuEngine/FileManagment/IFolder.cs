using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Linker;

namespace RhuEngine
{
	public interface IFolder
	{
		public string Name { get; set; }
		public string Path { get; }
		public IFolder Parrent { get; }
		public DateTimeOffset CreationDate { get; }
		public DateTimeOffset LastEdit { get; }
		public IFile[] Files { get; }
		public IFolder[] Folders { get; }
		public IDrive Drive { get; }
		public RTexture2D Texture { get; }

		public Task Refresh();

	}
}
