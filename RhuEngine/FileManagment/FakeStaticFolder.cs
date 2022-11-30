using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Components;

namespace RhuEngine
{
	public sealed class FakeStaticFolder : FolderBase
	{
		public FakeStaticFolder(FakeStaticDrive drive) {
			Drive = drive;
		}

		public override string Name { get => Drive.Name; set => _ = value; }

		public override IFolder Parrent => null;

		public override DateTimeOffset CreationDate => DateTimeOffset.MinValue;

		public override DateTimeOffset LastEdit => DateTimeOffset.MinValue;

		public override IDrive Drive { get; }

		public override IFile[] Files => AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => !x.IsAbstract).Where(x => x.IsAssignableTo(typeof(Program))).Where(x => x.GetCustomAttribute<ProgramHideAttribute>(true) is null).Select(x => new FakeStaticProgramFile(Drive, this, x)).ToArray();

		public override IFolder[] Folders => Array.Empty<IFolder>();

		public override Task Refresh() {
			return Task.CompletedTask;
		}
	}
}
