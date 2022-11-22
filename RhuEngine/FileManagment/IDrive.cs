using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine
{
	public interface IDrive
	{
		public Engine Engine { get; }
		public string Path { get; }
		public string Name { get; set; }
		public long UsedBytes { get; }
		public long TotalBytes { get; }
		public IFolder Root { get; }
	}
}
