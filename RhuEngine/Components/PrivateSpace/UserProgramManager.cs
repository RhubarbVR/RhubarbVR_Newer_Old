using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using RhuEngine.WorldObjects.ECS;

using SharedModels;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using System.IO;
using System.Reflection;
using Esprima.Ast;

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
			RenderThread.ExecuteOnStartOfFrame(() => {
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
			});
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
		public readonly List<ProgramToolBar> ProgramToolBars = new();

		public T GetProgram<T>() where T : Program, new() {
			foreach (var item in Programs) {
				if (item is T data) {
					return data;
				}
			}
			return null;
		}

		public T OpenOnePrivateOpenProgram<T>(Stream file = null, string mimetype = null, string ex = null, params object[] args) where T : Program, new() {
			foreach (var item in Programs) {
				if (item is T data) {
					if (data.programWindows.Count >= 1) {
						data[0].Maximize();
						data[0].CenterWindowIntoView();
					}
					return data;
				}
			}
			return PrivateOpenProgram<T>(file, mimetype, ex, args);
		}

		public Program OpenProgram(Type programType, object[] args = null, Stream file = null, string mimetype = null, string ex = null) {
			return programType is null
				? null
				: programType.GetCustomAttribute<PrivateSpaceOnlyAttribute>() is not null
				? OpenProgram(programType, WorldManager.PrivateOverlay, file, mimetype, ex, args)
				: programType.GetCustomAttribute<OverlayOnlyAttribute>() is not null
				? OpenProgram(programType, WorldManager.OverlayWorld, file, mimetype, ex, args)
				: OpenProgram(programType, WorldManager.FocusedWorld, file, mimetype, ex, args);
		}

		public static Program OpenProgram(Type programType, World world, Stream file = null, string mimetype = null, string ex = null, params object[] args) {
			var program = world.RootEntity.AddChild(programType.Name).AttachComponent<Program>(programType);
			program.StartProgram(file, mimetype, ex, args);
			return program;
		}

		public static T OpenProgram<T>(World world, Stream file = null, string mimetype = null, string ex = null, params object[] args) where T : Program, new() {
			var program = world.RootEntity.AddChild(typeof(T).Name).AttachComponent<T>();
			program.StartProgram(file, mimetype, ex, args);
			return program;
		}

		public T PrivateOpenProgram<T>(Stream file = null, string mimetype = null, string ex = null, params object[] args) where T : Program, new() {
			return OpenProgram<T>(WorldManager.PrivateOverlay, file, mimetype, ex, args);
		}

		public T OverlayOpenProgram<T>(Stream file = null, string mimetype = null, string ex = null, params object[] args) where T : Program, new() {
			return OpenProgram<T>(WorldManager.OverlayWorld, file, mimetype, ex, args);
		}

		public T FocusedOpenProgram<T>(Stream file = null, string mimetype = null, string ex = null, params object[] args) where T : Program, new() {
			return OpenProgram<T>(WorldManager.FocusedWorld, file, mimetype, ex, args);
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

		internal void LoadProgramToolBar(ProgramToolBar programToolBar) {
			ProgramToolBars.Add(programToolBar);
			Entity.AddChild().AttachComponent<PrivateSpaceToolBar>().InitPrivateSpaceToolBar(programToolBar);
		}
		internal void UnLoadProgramToolBar(ProgramToolBar programToolBar) {
			ProgramToolBars.Remove(programToolBar);
		}

	}
}
