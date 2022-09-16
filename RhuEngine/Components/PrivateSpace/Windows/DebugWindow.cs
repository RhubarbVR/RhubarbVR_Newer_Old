//using System;
//using System.Collections.Generic;
//using System.Text;

//using RhuEngine.Managers;
//using RhuEngine.WorldObjects;

//using RNumerics;
//using RhuEngine.Linker;

//namespace RhuEngine.Components.PrivateSpace.Windows
//{
//	public class DebugWindow:Window
//	{
//		public override bool? OnLogin => null;

//		public override string Name => "Debug";



//		public bool ShowVolumes = false;

//		public override void Update() {
//			Hierarchy.Push(Matrix.S(0.5f));
//			UI.WindowBegin("    ===---===     Debug Window     ===---===", ref windowPose, new Vec2(0.4f, 0));
//			CloseDraw();
//			var e = ShowVolumes;
//			UI.Toggle("ShowVolumes", ref e);
//			if(e != ShowVolumes) {
//				UI.ShowVolumes = ShowVolumes= e;
//			}
//			UI.Text();
//			var serverindex = 0;
//			foreach (var item in WorldManager.FocusedWorld?.relayServers) {
//				UI.Text($"RelayServer{serverindex} Connections{item.peers.Count} latency{item.latency}");
//				serverindex++;
//			}
//			UI.WindowEnd();
//			Hierarchy.Pop();
//		}
//		public DebugWindow(Engine engine, WorldManager worldManager, WorldObjects.World world) :base(engine,worldManager,world) {
//		}
//	}
//}
