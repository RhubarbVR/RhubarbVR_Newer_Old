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
	[RemoveFromProgramList]
	[UpdateLevel(UpdateEnum.Normal)]
	public sealed class IsOnlineProgram : Program
	{
		public override string ProgramID => "OnlineScreen";

		public override Vector2i? Icon => new Vector2i(17,0);

		public override RTexture2D Texture => null;

		public override string ProgramName => "Programs.Offline.Name";

		public override bool LocalName => true;

		public float TimeLastChange = 10;

		protected override void Step() {
			base.Step();
			if(_uIText is null) {
				return;
			}
			TimeLastChange += RTime.Elapsedf;
			if (TimeLastChange < 5) {
				return;
			}
			TimeLastChange = 0;
			_uIText.Text.Value = RhubarbCloudClient.RhubarbLoadingFacts.GetRandomFact(Engine.localisationManager).Replace("<br />", "").Replace("<i class=\"bi bi-train-front\"></i>", "");
		}

		[Exposed]
		public void CheckIfOnline() {
			Task.Run(Engine.netApiManager.Client.Check);
		}
		UI3DText _uIText;
		public override void LoadUI(Entity uiRoot) {
			Engine.netApiManager.Client.HasGoneOnline += () => Close();
			window.CloseButton.Value = false;
			var ma = uiRoot.AttachComponent<UI3DRect>();
			var mit = window.MainMit.Target;
			var uiBuilder = new UI3DBuilder(uiRoot, mit, ma,true);
			uiBuilder.PushRectNoDepth(new Vector2f(0.1f, 0.3f), new Vector2f(0.9f, 0.9f));
			uiBuilder.PushRectNoDepth(new Vector2f(0f, 0.5f));
			uiBuilder.AddImg(World.RootEntity.GetFirstComponentOrAttach<RhubarbLogo>()).Item2.Transparency.Value = Transparency.Blend;
			uiBuilder.PopRect();
			uiBuilder.PushRectNoDepth(null,new Vector2f(1f, 0.5f));
			_uIText = uiBuilder.AddText("",null,2,1,null,true);
			uiBuilder.PopRect();
			uiBuilder.PopRect();
			uiBuilder.PushRectNoDepth(new Vector2f(0.1f, 0.1f), new Vector2f(0.9f, 0.3f));
			uiBuilder.AddButtonEventLabled("Programs.Offline.GoOnline", null, 2, 1, CheckIfOnline, null, null, true, 0.1f, 0.9f);
			uiBuilder.PopRect();
		}
	}
}
