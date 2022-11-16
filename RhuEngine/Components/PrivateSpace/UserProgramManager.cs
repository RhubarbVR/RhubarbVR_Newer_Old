using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using RhuEngine.WorldObjects.ECS;

using SharedModels;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using RhuEngine.WorldObjects;
using System.IO;
using System.Reflection;

namespace RhuEngine.Components
{
	[PrivateSpaceOnly]
	public sealed class UserProgramManager : Component
	{

		public OverlayProgram OverlayProgram { get; private set; }


		protected override void OnLoaded() {
			base.OnLoaded();
			Engine.worldManager.PrivateSpaceManager._ProgramManager = this;
			if (!Engine.EngineLink.CanRender) {
				return;
			}

			Engine.netApiManager.Client.OnLogin += Client_OnLogin;
			Engine.netApiManager.Client.OnLogout += Client_OnLogout;
			Engine.netApiManager.Client.HasGoneOfline += Client_HasGoneOfline;
			Engine.netApiManager.Client.HasGoneOnline += Client_HasGoneOnline;

			OverlayProgram = OpenOnePrivateOpenProgram<OverlayProgram>();
			if (!Engine.netApiManager.Client.IsOnline) {
				Client_HasGoneOfline();
				return;
			}
			if (Engine.netApiManager.Client.IsLogin) {
				Client_HasGoneOnline();
			}
			else {
				Client_HasGoneOfline();
			}
		}

		private void Client_HasGoneOnline() {
			OpenOnePrivateOpenProgram<LoginProgram>();
		}

		private void Client_HasGoneOfline() {
			OpenOnePrivateOpenProgram<LoginProgram>();
		}

		private void Client_OnLogout() {
			OpenOnePrivateOpenProgram<LoginProgram>();
		}

		private void Client_OnLogin(RhubarbCloudClient.Model.PrivateUser obj) {
			GetProgram<LoginProgram>()?.CloseProgram();
		}

		public Program this[int index] => Programs[index];

		public readonly List<Program> Programs = new();
		public readonly List<ProgramWindow> ProgramWindows = new();

		public T GetProgram<T>() where T : Program, new() {
			foreach (var item in Programs) {
				if (item is T data) {
					return data;
				}
			}
			return null;
		}

		public T OpenOnePrivateOpenProgram<T>(object[] args = null, Stream file = null, string mimetype = null, string ex = null) where T : Program, new() {
			foreach (var item in Programs) {
				if (item is T data) {
					return data;
				}
			}
			return PrivateOpenProgram<T>(args, file, mimetype, ex);
		}

		public Program OpenProgram(Type programType, object[] args = null, Stream file = null, string mimetype = null, string ex = null) {
			return programType is null
				? null
				: programType.GetCustomAttribute<PrivateSpaceOnlyAttribute>() is not null
				? OpenProgram(programType, WorldManager.PrivateOverlay, args, file, mimetype, ex)
				: programType.GetCustomAttribute<OverlayOnlyAttribute>() is not null
				? OpenProgram(programType, WorldManager.OverlayWorld, args, file, mimetype, ex)
				: OpenProgram(programType, WorldManager.FocusedWorld, args,file,mimetype,ex);
		}

		public Program OpenProgram(Type programType,World world, object[] args = null, Stream file = null, string mimetype = null, string ex = null) {
			var program = world.RootEntity.AddChild(programType.Name).AttachComponent<Program>(programType);
			program.StartProgram(args, file, mimetype, ex);
			return program;
		}

		public T OpenProgram<T>(World world, object[] args = null, Stream file = null, string mimetype = null, string ex = null) where T : Program, new() {
			var program = world.RootEntity.AddChild(typeof(T).Name).AttachComponent<T>();
			program.StartProgram(args, file, mimetype, ex);
			return program;
		}

		public T PrivateOpenProgram<T>(object[] args = null, Stream file = null, string mimetype = null, string ex = null) where T : Program, new() {
			return OpenProgram<T>(WorldManager.PrivateOverlay, args, file, mimetype, ex);
		}

		public T OverlayOpenProgram<T>(object[] args = null, Stream file = null, string mimetype = null, string ex = null) where T : Program, new() {
			return OpenProgram<T>(WorldManager.OverlayWorld, args, file, mimetype, ex);
		}

		public T FocusedOpenProgram<T>(object[] args = null, Stream file = null, string mimetype = null, string ex = null) where T : Program, new() {
			return OpenProgram<T>(WorldManager.FocusedWorld, args, file, mimetype, ex);
		}

		internal void LoadProgramWindow(ProgramWindow program) {
			ProgramWindows.Add(program);
			Entity.AddChild().AttachComponent<PrivateSpaceWindow>().InitPrivateSpaceWindow(program);
		}

		internal void UnLoadProgramWindow(ProgramWindow program) {
			ProgramWindows.Remove(program);
		}

		internal void LoadProgram(Program program) {
			Programs.Add(program);
		}

		internal void UnLoadProgram(Program program) {
			Programs.Remove(program);
		}
	}
}
