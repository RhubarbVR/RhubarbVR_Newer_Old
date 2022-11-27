using System;
using System.Collections.Generic;
using System.Text;

using MessagePack;

namespace RhuEngine.AssetSystem
{
	public enum FileType : byte
	{
		Unknown,
		Mesh,
		Entity,
		World,
		Avatar,
	}

	[MessagePackObject]
	public sealed class RhubarbFile
	{
		[Key(0)]
		public FileType FileType { get; set; }
		[Key(1)]
		public DateTimeOffset CreationData { get; set; }
		[Key(3)]
		public string Creator { get; set; }
		[Key(4)]
		public string Name { get; set; }
		[Key(5)]
		public byte[] Data { get; set; }
	}
}
