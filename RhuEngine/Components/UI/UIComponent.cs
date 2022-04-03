//using RhuEngine.WorldObjects;
//using RhuEngine.WorldObjects.ECS;

//using RNumerics;
//using RhuEngine.Linker;

//namespace RhuEngine.Components
//{
//	[Category(new string[] { "UI" })]
//	public abstract class UIComponent : Component
//	{
//		public NTMesh3 MainMesh { get; set; }
//		NTMesh3 CachedCut { get; set; }
//		NTMesh3 CachedDeform { get; set; }

//		bool WillRender { get; set; }
//		[NoSave]
//		[NoShow]
//		[NoSync]
//		[NoLoad]
//		[NoSyncUpdate]
//		public UIRect Rect => Entity.UIRect;

//		public abstract void ProcessBaseMesh();

//		public void ProcessCut() {
//			CachedCut = new NTMesh3();
//			CachedCut.Copy(MainMesh);
//			//Run cut here
//		}

//		public void ProcessDeform() {
//			CachedDeform = new NTMesh3();
//			CachedDeform.Copy(CachedCut);
//			//Run Deform here
//		}

//		public void ProcessHole() {

//		}

//		public void RenderUI() {
//			if (!WillRender) {
//				return;
//			}
//		}
//	}
//}
