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
	public sealed class PublicSessionsProgram : Program
	{
		public override string ProgramID => "PublicSessions";

		public override Vector2i? Icon => new Vector2i(16,1);

		public override RTexture2D Texture => null;

		public override string ProgramName => "Programs.PublicSessions.Name";

		public override bool LocalName => true;
		private UI3DBuilder _uiBuilder;
		[Exposed]
		public void JoinSession(string id,string name) {
			if(Guid.TryParse(id, out var data)) {
				WorldManager.JoinNewWorld(data, World.FocusLevel.Focused, name);
			}
		}

		[Exposed]
		public void Refresh() {
			Task.Run(async () => {
				if(_uiBuilder is null) {
					return;
				}
				var sessions = await Engine.netApiManager.Client.GetTopPublicSessions();
				RLog.Info("Loaded " + sessions.Length);
				_uiBuilder.CurretRectEntity.DestroyChildren();
				foreach (var item in sessions) {
					_uiBuilder.PushRectNoDepth();
					_uiBuilder.PushRect(null, null, 0);
					_uiBuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f));
					var button = _uiBuilder.AttachComponentToStack<UI3DButtonInteraction>();
					var buttonEvent = _uiBuilder.AttachComponentToStack<ButtonEventManager>();
					button.ButtonEvent.Target = buttonEvent.Call;
					var sessionJoin = _uiBuilder.AttachComponentToStack<AddTwoValuePram<string, string>>();
					sessionJoin.FirstValue.Value = item.ID.ToString();
					sessionJoin.SecondValue.Value = item.SessionName;
					buttonEvent.Click.Target = sessionJoin.Call;
					sessionJoin.Target.Target = JoinSession;
					_uiBuilder.AddRectangle(0.1f);
					_uiBuilder.PushRect(new Vector2f(0, 0.2f), new Vector2f(1));
					_uiBuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f), 0.01f);
					_uiBuilder.AddRectangle(0.2f);
					_uiBuilder.PopRect();
					_uiBuilder.PopRect();
					_uiBuilder.PushRect(new Vector2f(0), new Vector2f(1, 0.2f), 0);
					_uiBuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f), 0.1f);
					_uiBuilder.AddText(item.SessionName, null, 1.9f, 1, null, true);
					_uiBuilder.PopRect();
					_uiBuilder.PopRect();
					_uiBuilder.PopRect();
					_uiBuilder.PopRect();
					_uiBuilder.PopRect();
				}
			});
		}

		public override void LoadUI(Entity uiRoot) {
			var ma = uiRoot.AttachComponent<UI3DRect>();
			var mit = window.MainMit.Target;
			_uiBuilder = new UI3DBuilder(uiRoot, mit, ma,true);
			_uiBuilder.PushRect();
			_uiBuilder.PushRectNoDepth(new Vector2f(0,0.9f));
			_uiBuilder.AddButtonEventLabled("Common.Refresh", null, 2, 1, Refresh,null,null,true,0.1f,0.9f);
			_uiBuilder.PopRect();
			_uiBuilder.PushRectNoDepth(null,new Vector2f(1f,0.9f));
			_uiBuilder.AttachChildRect<CuttingUI3DRect>(null, null, 0);
			var scroller = _uiBuilder.AttachComponentToStack<UI3DScrollInteraction>();
			var grid = _uiBuilder.AttachChildRect<UI3DGrid>();
			scroller.OnScroll.Target = grid.Scroll;
			Refresh();
		}
	}
}
