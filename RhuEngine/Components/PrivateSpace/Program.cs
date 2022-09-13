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
using RhuEngine.Components.PrivateSpace;

namespace RhuEngine.Components
{
	public sealed class OpenManyAttribute : Attribute
	{

	}

	public sealed class RemoveFromProgramListAttribute : Attribute {

	}

	[PrivateSpaceOnly]
	public abstract class Program : Component
	{
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public TaskBar taskBar;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Window window;

		public ProgramTaskBarItem taskBarItem;

		public abstract string ProgramID { get; }

		public abstract Vector2i? Icon { get; }

		public abstract RTexture2D Texture { get; }

		public abstract string ProgramName { get; }

		public abstract bool LocalName { get; }

		public void IntProgram() {
			World.DrawDebugText(taskBar.Entity.GlobalTrans, new Vector3f(0,1,-1), Vector3f.One, Colorf.Green,"Program Loaded", 5);
			window = Entity.AttachComponent<Window>();
			window.OnMinimize.Target = Minimize;
			window.OnClose.Target = Close;
			window.PinChanged.Target = OnPin;
			window.NameValue.Value = ProgramName;
			if (LocalName) {
				var local = Entity.AttachComponent<StandardLocale>();
				local.TargetValue.Target = window.NameValue;
				local.Key.Value = ProgramName;
			}
			LoadUI(window.PannelRoot.Target);
			BringToMe();
		}

		public void BringToMe() {
			Entity.GlobalTrans = Matrix.TR(new Vector3f(0,0.15f,-0.85f),Quaternionf.CreateFromEuler(22.5f,0,0)) * taskBar.Entity.GlobalTrans;
		}

		public abstract void LoadUI(Entity uiRoot);

		public bool Minimized = false;
		[Exposed]
		public void Minimize() {
			window.Entity.enabled.Value = Minimized;
			Minimized = !Minimized;
		}

		[Exposed]
		public void OnPin(bool pin) {
			if (pin) {
				Entity.SetParent(taskBar.Entity);
			}
			else {
				Entity.SetParent(taskBar.ProgramsHolder);
			}
		}

		[Exposed]
		public void Close() {
			Entity.Destroy();
		}

		public override void Dispose() {
			taskBar.ProgramClose(this);
			base.Dispose();
		}

		private DateTime _lastClick = DateTime.UtcNow;

		public void ClickedButton() {
			if (Minimized) {
				Minimize();
			}
			else {
				if((DateTime.UtcNow - _lastClick).Seconds <= 1) {
					BringToMe();
				}
				_lastClick = DateTime.UtcNow;
			}
		}
	}
}
