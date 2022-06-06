using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.WorldObjects;

using RNumerics;

namespace RhuEngine.Components.PrivateSpace
{
	public class WorldTaskBarItem: ITaskBarItem {
		public string ExtraText { get; set; }

		public World target;

		public WorldTaskBarItem(World world) {
			target = world;
		}

		public bool ShowOpenFlag => target.Focus == World.FocusLevel.Focused;

		public string ID => "World" + target.SessionID.Value;

		public Vector2i? Icon => new Vector2i(0,4);

		public RTexture2D Texture => null;

		public string Name => target.SessionName.Value;

		public bool LocalName => false;

		public void Clicked() {
			if (target.Focus != World.FocusLevel.Focused) {
				target.Focus = World.FocusLevel.Focused;
			}
		}
	}

	public class ProgramTaskBarItem:ITaskBarItem
	{
		public string ExtraText { get; set; }

		public Program Program;

		public Type ProgramType;

		public TaskBar TaskBar;

		public ProgramTaskBarItem(TaskBar taskBar,Program program = null){
			TaskBar = taskBar;
			Program = program;
			if (program != null) {
				ProgramType = program.GetType();
				ShowOpenFlag = true;
				ID = program.ProgramID;
				Icon = program.Icon;
				Texture = program.Texture;
				Name = program.ProgramName;
				LocalName = program.LocalName;
			}
		}
		public ProgramTaskBarItem(TaskBar taskBar, Type programLink) {
			TaskBar = taskBar;
			if (typeof(Program).IsAssignableFrom(programLink)) {
				var program = (Program)Activator.CreateInstance(programLink);
				ShowOpenFlag = false;
				ID = program.ProgramID + ".0";
				Icon = program.Icon;
				Texture = program.Texture;
				Name = program.ProgramName;
				ProgramType = programLink;
				LocalName = program.LocalName;
			}
			else {
				throw new Exception("Not a program");
			}
		}
		public bool ShowOpenFlag { get; set; }

		public string ID { get; set; }

		public Vector2i? Icon { get; set; }

		public RTexture2D Texture { get; set; }

		public string Name { get; set; }
		public bool LocalName { get; set; }

		public void Clicked() {
			if(Program is null) {
				TaskBar.OpenProgramForced(ID,ProgramType);
			}
			else {
				Program.ClickedButton();
			}
		}
	}

	public interface ITaskBarItem
	{
		public string ExtraText { get; set; }
		public bool ShowOpenFlag { get; }
		
		public string ID { get; }

		public Vector2i? Icon { get; }

		public RTexture2D Texture { get; }

		public string Name { get; }

		public bool LocalName { get; }
		public void Clicked();
	}
}
