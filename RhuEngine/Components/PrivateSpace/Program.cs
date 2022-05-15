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
	[PrivateSpaceOnly]
	public abstract class Program : Component
	{
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public TaskBar taskBar;

		public ProgramTaskBarItem taskBarItem;

		public abstract string ProgramID { get; }

		public abstract Vector2i? Icon { get; }

		public abstract RTexture2D Texture { get; }

		public abstract string ProgramName { get; }

		public void IntProgram() {
			World.DrawDebugText(taskBar.Entity.GlobalTrans, new Vector3f(0,1,-1), Vector3f.One, Colorf.Green,"Program Loaded", 5);
		}

		public void ClickedButton() {
			RLog.Info("Clicked button");
		}
	}
}
