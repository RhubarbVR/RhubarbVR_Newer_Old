//using RhuEngine.WorldObjects;
//using RhuEngine.WorldObjects.ECS;

//using RNumerics;
//using RhuEngine.Linker;

//namespace RhuEngine.Components
//{
//	[Category(new string[] { "UI" })]
//	public class UIRect : Component
//	{
//		public Sync<bool> ToCanvas;

//		public Sync<Vector2f> Min;

//		public Sync<Vector2f> Max;

//		[NoSave][NoShow][NoSync][NoLoad][NoSyncUpdate]
//		public UICanvas Canvas { get; internal set; }

//		[NoSave][NoShow][NoSync][NoLoad][NoSyncUpdate]
//		public UIRect ParentRect => Entity.parent.Target?.UIRect;


//		public override void OnAttach() {
//			base.OnAttach();
//			Min.Value = Vector2f.Zero;
//			Max.Value = Vector2f.One;
//		}

//		public override void OnLoaded() {
//			base.OnLoaded();
//			Entity.UIRect = Entity.GetFirstComponent<UIRect>();
//			Entity.components.Changed += RegisterUIList;
//		}

//		private void RegisterUIList(IChangeable obj) {

//		}

//		public void Render() {
//			var renderHole = ToCanvas.Value || ParentRect is null || Canvas is not null;
//			foreach (Entity item in Entity.children) {
//				item.UIRect?.Render();
//			}
//		}

//		public override void Dispose() {
//			base.Dispose();
//			Entity.UIRect = Entity.GetFirstComponent<UIRect>();
//			Entity.components.Changed -= RegisterUIList;
//		}
//	}
//}
