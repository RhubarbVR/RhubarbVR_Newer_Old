//using System;
//using System.Collections.Generic;
//using System.Text;

//using RhuEngine.Managers;

//using RNumerics;
//using RhuEngine.Linker;

//namespace RhuEngine.Components.PrivateSpace.Windows
//{
//	public abstract class Window
//	{
//		public abstract bool? OnLogin { get; }
//		public Engine Engine { get; }
//		public WorldManager WorldManager { get; }
//		public WorldObjects.World World { get; }

//		public abstract string Name { get; }
//		public Window(Engine engine, WorldManager worldManager, WorldObjects.World world) {
//			Engine = engine;
//			WorldManager = worldManager;
//			World = world;
//		}

//		public Pose windowPose = new(-.2f, 0.2f, -0.2f, Quat.LookDir(1, 0, 1));


//		private bool _isOpen;

//		public bool IsOpen
//		{
//			get => _isOpen;
//			set {
//				_isOpen = value;
//				if (_isOpen) {
//					OnOpen();
//				}
//				else {
//					OnClose();
//				}
//			}
//		}
//		public virtual void OnOpen() {

//		}
//		public virtual void OnClose() {

//		}
//		public virtual void Update() {

//		}

//		public void CloseDraw() {
//			UI.PushTint(new Color(1f, 0f, 0f));
//			if (UI.ButtonAt("X",new Vec3(0.2f, Engine.UISettings.padding * 3, 0), new Vec2(Engine.UISettings.padding * 3))) {
//				IsOpen = false;
//			}
//			UI.PopTint();
//		}

//	}
//}
