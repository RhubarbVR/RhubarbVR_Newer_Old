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
	[UpdateLevel(UpdateEnum.Normal)]
	public sealed class PrivateSpaceManager : Component
	{

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Lazer Leftlazer;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Lazer Rightlazer;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Entity DashMover;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UIElement RootScreenElement;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UIElement UserInterface;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public RawAssetProvider<RTexture2D> CurrsorTexture;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public TextureRect IconTexRender;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Viewport VRViewPort;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UserInterfaceManager UserInterfaceManager;

		private RhubarbAtlasSheet.RhubarbIcons _cursorIcon;
		public RhubarbAtlasSheet.RhubarbIcons CursorIcon
		{
			get => _cursorIcon; set {
				_cursorIcon = value;
				if (Engine.EngineLink.CanRender) {
					CurrsorTexture.LoadAsset(Engine.staticResources.IconSheet.GetElement(_cursorIcon));
				}
			}
		}

		public Colorf CursorColor
		{
			get => IconTexRender.Modulate.Value;
			set => IconTexRender.Modulate.Value = value;
		}

		protected override void OnAttach() {
			base.OnAttach();
			Task.Run(async () => {
				if (!Engine.EngineLink.SpawnPlayer) {
					return;
				}
				Entity user;
				while (true) {
					user = LocalUser.userRoot.Target?.Entity;
					if (user is not null) {
						break;
					}
					await Task.Delay(100);
				}
				var entLeftlazer = user.AddChild("LeftLazer");
				Leftlazer = entLeftlazer.AttachComponent<Lazer>();
				Leftlazer.Side.Value = Handed.Left;
				var entRightlazer = user.AddChild("RightLazer");
				Rightlazer = entRightlazer.AttachComponent<Lazer>();
				Rightlazer.Side.Value = Handed.Right;
			});
			DashMover = World.RootEntity.AddChild("TaskBarMover");
			DashMover.AttachComponent<UserInterfacePositioner>();
			var screen = World.RootEntity.AddChild("RootScreen");
			RootScreenElement = screen.AttachComponent<UIElement>();
			IconTexRender = screen.AddChild("Center Icon").AttachComponent<TextureRect>();
			IconTexRender.Entity.orderOffset.Value = -100;
			UserInterface = screen.AddChild("UserInterface").AttachComponent<UIElement>();
			UserInterfaceManager = DashMover.AttachComponent<UserInterfaceManager>();
			var size = new Vector2f(0.075f);
			IconTexRender.Min.Value = new Vector2f(0.5f, 0.5f) - (size / 2);
			IconTexRender.Max.Value = new Vector2f(0.5f, 0.5f) + (size / 2);
			IconTexRender.StrechMode.Value = RStrechMode.KeepAspectCenter;
			IconTexRender.IgnoreTextureSize.Value = true;
			Entity.AttachComponent<IsInVR>().isNotVR.Target = screen.enabled;
			IconTexRender.Texture.Target = CurrsorTexture = IconTexRender.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
			CursorIcon = RhubarbAtlasSheet.RhubarbIcons.Cursor;
			CursorColor = new Colorf(222, 222, 222, 240);
			VRViewPort = World.RootEntity.AddChild("VRViewPort").AttachComponent<Viewport>();
			VRViewPort.Enabled.Value = false;
			VRViewPort.Size.Value = new Vector2i(1920, 1080);
			VRViewPort.UpdateMode.Value = RUpdateMode.Always;
			UserInterfaceManager.PrivateSpaceManager = this;
			UserInterfaceManager.UserInterface = UserInterface;
			UserInterfaceManager.LoadInterface();
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			WorldManager.PrivateSpaceManager = this;
		}

		protected override void RenderStep() {
			if (!Engine.EngineLink.CanInput) {
				return;
			}

			if (InputManager.GetInputAction(InputTypes.VRChange).JustActivated() && Engine.EngineLink.LiveVRChange) {
				Engine.EngineLink.ChangeVR(!Engine.IsInVR);
			}

			var head = LocalUser.userRoot.Target?.head.Target;
			if (head != null) {
				if (!Engine.IsInVR && !Engine.inputManager.screenInput.MouseFree) {
					UpdateHeadLazer(head);
				}
				if (Engine.IsInVR) {
					if (Leftlazer is not null) {
						UpdateLazer(Leftlazer.Entity, Handed.Left, Leftlazer);
					}
					if (Rightlazer is not null) {
						UpdateLazer(Rightlazer.Entity, Handed.Right, Rightlazer);
					}
				}
				//Todo: fingerPos
				//UpdateTouch(RInput.Hand(Handed.Right).Wrist, 2, Handed.Right);
				//UpdateTouch(RInput.Hand(Handed.Left).Wrist, 1, Handed.Left);
			}
		}


		public bool RunTouchCastInWorld(uint handed, World world, Vector3f pos, ref Vector3f Frompos, ref Vector3f ToPos, Handed handedSide) {
			if (World is null) {
				return false;
			}
			try {
				if (world.PhysicsSim.RayTest(ref Frompos, ref ToPos, out var collider, out var hitnormal, out var hitpointworld)) {
					if (collider.CustomObject is PhysicsObject physicsObject) {
						World.DrawDebugSphere(Matrix.T(hitpointworld), Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 0, 0.5f));
						physicsObject.Touch(handed, hitnormal, hitpointworld, handedSide);
					}
					return true;
				}
			}
			catch {
			}
			return false;
		}


		public void UpdateTouch(Matrix pos, uint handed, Handed handedSide) {
			var Frompos = Matrix.T(Vector3f.AxisY * -0.07f) * pos;
			var ToPos = Matrix.T(Vector3f.AxisY * 0.03f) * pos;
			World.DrawDebugSphere(Frompos, Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 0, 0.5f));
			World.DrawDebugSphere(ToPos, Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 0, 0.5f));
			var vpos = pos.Translation;
			var vFrompos = Frompos.Translation;
			var vToPos = ToPos.Translation;
			if (RunTouchCastInWorld(handed, World, vpos, ref vFrompos, ref vToPos, handedSide)) {

			}
			else if (RunTouchCastInWorld(handed, World.worldManager.FocusedWorld, vpos, ref vFrompos, ref vToPos, handedSide)) {

			}
		}

		public bool RunLaserCastInWorld(World world, ref Vector3f headFrompos, ref Vector3f headToPos, uint touchUndex, float pressForce, float gripForces, Handed side, ref Vector3f hitPointWorld) {
			if (world.PhysicsSim.RayTest(ref headFrompos, ref headToPos, out var collider, out var hitnormal, out var hitpointworld)) {
				hitPointWorld = hitpointworld;
				if (collider.CustomObject is PhysicsObject physicsObject) {
					World.DrawDebugSphere(Matrix.T(hitpointworld), Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 0, 0.5f));
					physicsObject.Lazer(touchUndex, hitnormal, hitpointworld, pressForce, gripForces, side);
				}
				if (collider.CustomObject is IWorldObject syncObject) {
					if (InputManager.ObserverOpen.JustActivated() && !Engine.HasKeyboard) {
						if (syncObject.World.IsPersonalSpace) {
							return true;
						}
						var ReletiveEntity = syncObject.World.GetLocalUser()?.userRoot.Target?.head.Target ?? syncObject.World.RootEntity;
						var observer = (syncObject.World.GetLocalUser()?.userRoot.Target?.Entity.parent.Target ?? syncObject.World.RootEntity).AddChild("Observer");
						//observer.AttachComponent<ObserverWindow>().Observerd.Target = syncObject.GetClosedEntity();
						//observer.GlobalTrans = Matrix.T(-0.5f, -0.5f, -1) * ReletiveEntity.GlobalTrans;
					}
				}
				return true;
			}
			return false;
		}
		public bool DisableRightLaser;

		public bool DisableLeftLaser;

		public bool DisableHeadLaser;

		public void UpdateLazer(Entity heand, Handed handed, Lazer lazer) {
			if (handed == Handed.Left) {
				if (DisableLeftLaser) {
					return;
				}
			}
			else {
				if (DisableRightLaser) {
					return;
				}
			}
			var PressForce = Engine.inputManager.GetInputAction(InputTypes.Primary).HandedValue(handed);
			var GripForce = Engine.inputManager.GetInputAction(InputTypes.Grab).HandedValue(handed);
			var headPos = heand.GlobalTrans;
			var headFrompos = headPos;
			var headToPos = Matrix.T(Vector3f.AxisZ * -5) * headPos;
			World.DrawDebugSphere(headToPos, Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 0, 0.5f));
			var vheadFrompos = headFrompos.Translation;
			var vheadToPos = headToPos.Translation;
			var hitPrivate = false;
			var hitOverlay = false;
			var hitFocus = false;
			var hitPoint = Vector3f.Zero;
			if (RunLaserCastInWorld(World, ref vheadFrompos, ref vheadToPos, 10, PressForce, GripForce, handed, ref hitPoint)) {
				hitPrivate = true;
			}
			else if (RunLaserCastInWorld(World.worldManager.OverlayWorld, ref vheadFrompos, ref vheadToPos, 10, PressForce, GripForce, handed, ref hitPoint)) {
				hitOverlay = true;
			}
			else if (RunLaserCastInWorld(World.worldManager.FocusedWorld, ref vheadFrompos, ref vheadToPos, 10, PressForce, GripForce, handed, ref hitPoint)) {
				hitFocus = true;
			}
			lazer.HitFocus = hitFocus;
			lazer.HitOverlay = hitOverlay;
			lazer.HitPrivate = hitPrivate;
			lazer.HitPoint = hitPoint;
		}

		public Vector3f HeadLaserHitPoint;
		public bool HeadLazerHitPrivate;
		public bool HeadLazerHitOverlay;
		public bool HeadLazerHitFocus;

		public bool HeadLazerHitAny => HeadLazerHitFocus | HeadLazerHitOverlay | HeadLazerHitFocus;

		public void UpdateHeadLazer(Entity head) {
			if (DisableHeadLaser) {
				return;
			}
			var PressForce = Engine.inputManager.GetInputAction(InputTypes.Primary).HandedValue(Handed.Max);
			var GripForce = Engine.inputManager.GetInputAction(InputTypes.Grab).HandedValue(Handed.Max);
			var headPos = head.GlobalTrans;
			var headFrompos = headPos;
			var headToPos = Matrix.T(Vector3f.AxisZ * -5) * headPos;
			World.DrawDebugSphere(headToPos, Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 0, 0.5f));
			var vheadFrompos = headFrompos.Translation;
			var vheadToPos = headToPos.Translation;
			var hitPrivate = false;
			var hitOverlay = false;
			var hitFocus = false;
			var hitPoint = Vector3f.Zero;
			if (RunLaserCastInWorld(World, ref vheadFrompos, ref vheadToPos, 10, PressForce, GripForce, Handed.Max, ref hitPoint)) {
				hitPrivate = true;
			}
			else if (RunLaserCastInWorld(World, ref vheadFrompos, ref vheadToPos, 10, PressForce, GripForce, Handed.Max, ref hitPoint)) {
				hitOverlay = true;
			}
			else if (RunLaserCastInWorld(World.worldManager.FocusedWorld, ref vheadFrompos, ref vheadToPos, 10, PressForce, GripForce, Handed.Max, ref hitPoint)) {
				hitFocus = true;
			}
			HeadLaserHitPoint = hitPoint;
			HeadLazerHitPrivate = hitPrivate;
			HeadLazerHitOverlay = hitOverlay;
			HeadLazerHitFocus = hitFocus;
		}

		public void KeyBoardUpdate(Matrix openLocation) {

		}
	}
}
