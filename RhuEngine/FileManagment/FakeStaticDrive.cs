using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RhuEngine
{
	public sealed class FakeStaticDrive : IDrive
	{
		public FakeStaticDrive(Engine engine) {
			Engine = engine;
			Root = new FakeStaticFolder(this);
		}

		public string Path => "Static";

		public string Name { get => Engine.localisationManager.GetLocalString("Start.AllPrograms"); set => _ = value; }

		public long UsedBytes => 0;

		public long TotalBytes => 0;

		public IFolder Root { get; }

		public Engine Engine { get; }
	}
}
