using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components.PrivateSpace
{
	[OpenMany]
	public sealed class FileExplorerProgram : Program
	{
		public override string ProgramID => "FileExplorer";

		public override Vector2i? Icon => new Vector2i(1, 0);

		public override RTexture2D Texture => null;

		public override string ProgramName => "Programs.FileExplorer.Name";

		public override bool LocalName => true;

		public override void LoadUI(Entity uiRoot) {
			var ma = uiRoot.AttachComponent<UI3DRect>();
			var mit = window.MainMit.Target;
			var uiBuilder = new UI3DBuilder(uiRoot, mit, ma, true);
			uiBuilder.PushRect(null, null, 0);
			uiBuilder.PushRect(new Vector2f(0, 1), new Vector2f(1, 1), 0);
			uiBuilder.SetOffsetMinMax(new Vector2f(0, -1));
			BuildTopHeader(uiBuilder);
			uiBuilder.PopRect();
			uiBuilder.PushRect(new Vector2f(0, 1), new Vector2f(1, 1), 0);
			uiBuilder.SetOffsetMinMax(new Vector2f(0, -2), new Vector2f(0, -1));
			BuildBottomHeader(uiBuilder);
			uiBuilder.PopRect();
			uiBuilder.PushRect(new Vector2f(0), new Vector2f(0, 1), 0);
			uiBuilder.SetOffsetMinMax(new Vector2f(0), new Vector2f(3.5, -2));
			BuildLeftBar(uiBuilder);
			uiBuilder.PopRect();
			uiBuilder.PushRect(new Vector2f(0), new Vector2f(1), 0);
			uiBuilder.SetOffsetMinMax(new Vector2f(3.5, 0), new Vector2f(0, -2));
			BuildMainAria(uiBuilder);
			uiBuilder.PopRect();
			uiBuilder.PopRect();
		}

		public void BuildTopHeader(UI3DBuilder uIBuilder) {
			var text = uIBuilder.AddTextEditor("Programs.FileExplorer.Path", 0.2f, 0.8f, "", 0.1f, null, 1.9f, 1, false);
			text.Item1.HorizontalAlien.Value = EHorizontalAlien.Left;
			uIBuilder.SetOffsetMinMax(new Vector2f(1.1f, 0.1f), new Vector2f(-0.1f));
			uIBuilder.PopRect();
			uIBuilder.PushRect(new Vector2f(0), new Vector2f(0, 1));
			uIBuilder.SetOffsetMinMax(new Vector2f(0f), new Vector2f(1, 0));
			uIBuilder.AddSprit(new Vector2i(1, 0), new Vector2i(1, 0), window.IconMit.Target, window.IconSprite.Target);
			uIBuilder.PopRect();
			uIBuilder.PopRect();
		}

		public void BuildBottomHeader(UI3DBuilder uIBuilder) {
			uIBuilder.PushRect(new Vector2f(0), new Vector2f(0, 1), 0);
			uIBuilder.SetOffsetMinMax(new Vector2f(), new Vector2f(3.5, 0));
			var text = uIBuilder.AddTextEditor("Common.Search", 0.2f, 0.8f, "", 0.1f, null, 1.9f, 1, false);
			text.Item1.HorizontalAlien.Value = EHorizontalAlien.Left;
			uIBuilder.SetOffsetMinMax(new Vector2f(1.1f, 0.1f), new Vector2f(-0.1f));
			uIBuilder.PopRect();
			uIBuilder.PushRect(new Vector2f(0), new Vector2f(0, 1));
			uIBuilder.SetOffsetMinMax(new Vector2f(0f), new Vector2f(1, 0));
			uIBuilder.AddSprit(new Vector2i(15, 1), new Vector2i(15, 1), window.IconMit.Target, window.IconSprite.Target);
			uIBuilder.PopRect();
			uIBuilder.PopRect();
			uIBuilder.PopRect();
			uIBuilder.PushRect(new Vector2f(0), new Vector2f(1), 0);
			uIBuilder.SetOffsetMinMax(new Vector2f(3.5, 0), new Vector2f(0));

			//Trash
			uIBuilder.PushRect(new Vector2f(1, 0), new Vector2f(1), 0);
			uIBuilder.SetOffsetMinMax(new Vector2f(-1, 0), new Vector2f(0));
			uIBuilder.PushRect(new Vector2f(0.1), new Vector2f(0.9f), 0);
			uIBuilder.AddButton(false, 0.2f);
			uIBuilder.PushRect();
			uIBuilder.AddSprit(new Vector2i(24, 1), new Vector2i(24, 1), window.IconMit.Target, window.IconSprite.Target);
			uIBuilder.PopRect();
			uIBuilder.PopRect();
			uIBuilder.PopRect();

			//New folder
			uIBuilder.PushRect(new Vector2f(1, 0), new Vector2f(1), 0);
			uIBuilder.SetOffsetMinMax(new Vector2f(-2, 0), new Vector2f(-1, 0));
			uIBuilder.PushRect(new Vector2f(0.1), new Vector2f(0.9f), 0);
			uIBuilder.AddButton(false, 0.2f);
			uIBuilder.PushRect();
			uIBuilder.AddSprit(new Vector2i(8, 1), new Vector2i(8, 1), window.IconMit.Target, window.IconSprite.Target);
			uIBuilder.PopRect();
			uIBuilder.PopRect();
			uIBuilder.PopRect();
			uIBuilder.PopRect();

			//Share
			uIBuilder.PushRect(new Vector2f(1, 0), new Vector2f(1), 0);
			uIBuilder.SetOffsetMinMax(new Vector2f(-3, 0), new Vector2f(-2, 0));
			uIBuilder.PushRect(new Vector2f(0.1), new Vector2f(0.9f), 0);
			uIBuilder.AddButton(false, 0.2f);
			uIBuilder.PushRect();
			uIBuilder.AddSprit(new Vector2i(14, 3), new Vector2i(14, 3), window.IconMit.Target, window.IconSprite.Target);
			uIBuilder.PopRect();
			uIBuilder.PopRect();
			uIBuilder.PopRect();
			uIBuilder.PopRect();

			//Fav
			uIBuilder.PushRect(new Vector2f(1, 0), new Vector2f(1), 0);
			uIBuilder.SetOffsetMinMax(new Vector2f(-4, 0), new Vector2f(-3, 0));
			uIBuilder.PushRect(new Vector2f(0.1), new Vector2f(0.9f), 0);
			uIBuilder.AddButton(false, 0.2f);
			uIBuilder.PushRect();
			uIBuilder.AddSprit(new Vector2i(20, 2), new Vector2i(20, 2), window.IconMit.Target, window.IconSprite.Target);
			uIBuilder.PopRect();
			uIBuilder.PopRect();
			uIBuilder.PopRect();
			uIBuilder.PopRect();

			//Undo
			uIBuilder.PushRect(new Vector2f(1, 0), new Vector2f(1), 0);
			uIBuilder.SetOffsetMinMax(new Vector2f(-5, 0), new Vector2f(-4, 0));
			uIBuilder.PushRect(new Vector2f(0.1), new Vector2f(0.9f), 0);
			uIBuilder.AddButton(false, 0.2f);
			uIBuilder.PushRect();
			uIBuilder.AddSprit(new Vector2i(13, 1), new Vector2i(13, 1), window.IconMit.Target, window.IconSprite.Target);
			uIBuilder.PopRect();
			uIBuilder.PopRect();
			uIBuilder.PopRect();
			uIBuilder.PopRect();
			uIBuilder.PopRect();

			//List
			uIBuilder.PushRect(new Vector2f(1, 0), new Vector2f(1), 0);
			uIBuilder.SetOffsetMinMax(new Vector2f(-6, 0), new Vector2f(-5, 0));
			uIBuilder.PushRect(new Vector2f(0.1), new Vector2f(0.9f), 0);
			uIBuilder.AddCheckBox(new Vector2i(1, 4), new Vector2i(1, 4), new Vector2i(1, 6), new Vector2i(1, 6), window.IconMit.Target, window.IconSprite.Target, 1.6f, 1);
			uIBuilder.PopRect();
			uIBuilder.PopRect();
			uIBuilder.PopRect();

			uIBuilder.PopRect();
		}

		public void BuildLeftBar(UI3DBuilder uIBuilder) {
			uIBuilder.AddRectangle(1.5f);
		}
		public void BuildMainAria(UI3DBuilder uIBuilder) {
			uIBuilder.AttachChildRect<CuttingUI3DRect>(null, null, 0);
			var scroller = uIBuilder.AttachComponentToStack<UI3DScrollInteraction>();
			var grid = uIBuilder.AttachChildRect<UI3DGrid>();
			scroller.OnScroll.Target = grid.Scroll;

			for (var i = 0; i < 9; i++) {
				uIBuilder.PushRect(null, null, 0);
				uIBuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f));
				var button = uIBuilder.AttachComponentToStack<UI3DButtonInteraction>();
				uIBuilder.AddRectangle(0.1f);
				uIBuilder.PushRect(new Vector2f(0, 0.2f), new Vector2f(1));
				uIBuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f), 0.01f);
				uIBuilder.AddRectangle(0.2f);
				uIBuilder.PopRect();
				uIBuilder.PopRect();
				uIBuilder.PushRect(new Vector2f(0), new Vector2f(1, 0.2f),0);
				uIBuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f), 0.1f);
				uIBuilder.AddText($"This is file {i + 1}/9", null, 1.9f, 1, null, true);
				uIBuilder.PopRect();
				uIBuilder.PopRect();
				uIBuilder.PopRect();
				uIBuilder.PopRect();
			}
			uIBuilder.PopRect();
			uIBuilder.PopRect();
		}
	}
}
