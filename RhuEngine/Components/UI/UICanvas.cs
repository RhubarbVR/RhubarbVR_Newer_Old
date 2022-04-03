//using RhuEngine.WorldObjects;
//using RhuEngine.WorldObjects.ECS;

//using RNumerics;
//using RhuEngine.Linker;

//namespace RhuEngine.Components
//{
//	[Category(new string[] { "UI" })]
//	public class UICanvas : RenderingComponent
//	{
//		public override void OnAttach() {
//			base.OnAttach();
//		}

//		public void RenderUI() {
//			if (Entity.UIRect is null) {
//				return;
//			}
//			var uiRect = Entity.UIRect;
//			uiRect.Canvas = this;
//			//uiRect.Render(null,Entity.GlobalTrans);
//		}
//	}
//}
