using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Components;
using RhuEngine.Linker;

namespace RhuEngine
{
	public sealed class FakeStaticProgramFile : FileBase
	{
		public FakeStaticProgramFile(IDrive drive, IFolder parrent,Type program) {
			Drive = drive;
			Parrent = parrent;
			_program = program;
			var (ProgramName, icon) = program.GetProgramInfo();
			Name = ProgramName;
			Texture = icon;
		}

		private readonly Type _program;

		public override string Name { get; set; }

		public override IFolder Parrent { get; }

		public override DateTimeOffset CreationDate => DateTimeOffset.MinValue;

		public override DateTimeOffset LastEdit => DateTimeOffset.MinValue;

		public override long SizeInBytes => 0;

		public override IDrive Drive { get; }

		public override RTexture2D Texture { get; }

		public override string Type => "Program";

		public override void Open() {
			try {
				Drive.Engine.worldManager.PrivateSpaceManager.ProgramManager.OpenProgram(_program);
			}
			catch { }
		}
	}
}
