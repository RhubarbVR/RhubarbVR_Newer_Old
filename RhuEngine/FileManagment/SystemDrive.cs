﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RhuEngine
{
	public sealed class SystemDrive : IDrive
	{
		public SystemDrive(DriveInfo driveInfo) {
			_driveInfo = driveInfo;
		}

		private readonly DriveInfo _driveInfo;

		public string Path => _driveInfo.RootDirectory.FullName;

		public string Name { get => _driveInfo.RootDirectory.FullName; set => throw new NotSupportedException(); }

		public long UsedBytes => _driveInfo.TotalSize - _driveInfo.TotalFreeSpace;

		public long TotalBytes => _driveInfo.TotalSize;

		public IFolder Root => new SystemFolder(Path, this, null);
	}
}
