using System;

using RhuEngine.Components.PrivateSpace;
using RhuEngine.Linker;
using RhuEngine.Managers;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

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
		public UserProgramManager _ProgramManager;

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

		public GrabbableHolderCombiner Head;
		public GrabbableHolderCombiner Left;
		public GrabbableHolderCombiner Right;

		public IWorldObject GetHolderRefGrabbable
		{
			get {
				return Head.HolderReferenWithGrabed is not null
					? Head.HolderReferenWithGrabed
					: Right.HolderReferenWithGrabed is not null ? Right.HolderReferenWithGrabed : Left.HolderReferenWithGrabed is not null ? Left.HolderReferenWithGrabed : null;
			}
		}

		public IWorldObject GetHolderRef
		{
			get {
				return Head.HolderReferen is not null
					? Head.HolderReferen
					: Right.HolderReferen is not null ? Right.HolderReferen : Left.HolderReferen is not null ? Left.HolderReferen : null;
			}
		}

		public GrabbableHolderCombiner GetGrabbableHolder(Handed handed) {
			return handed switch {
				Handed.Left => Left,
				Handed.Right => Right,
				Handed.Max => Head,
				_ => throw new NotSupportedException(),
			};
		}


		public void BuildLazers() {
			var user = LocalUser.userRoot.Target?.Entity;
			var entLeftlazer = user.AddChild("LeftLazer");
			Leftlazer = entLeftlazer.AttachComponent<Lazer>();
			Leftlazer.Side.Value = Handed.Left;
			var entRightlazer = user.AddChild("RightLazer");
			Rightlazer = entRightlazer.AttachComponent<Lazer>();
			Rightlazer.Side.Value = Handed.Right;
		}

		public bool isOnScreen = true;

		[Exposed]
		public void ScreenInputExited() {
			isOnScreen = false;
		}


		[Exposed]
		public void ScreenInputEntered() {
			isOnScreen = true;
		}

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Entity KeyboardEntity;

		private void BuildKeyboard() {
			KeyboardEntity = DashMover.AddChild("Keybord");
			KeyboardEntity.AttachComponent<Grabbable>();
			KeyboardEntity.AttachComponent<VirtualKeyboard>();
			KeyboardEntity.enabled.Value = false;
		}

		protected override void OnAttach() {
			base.OnAttach();
			Head = new GrabbableHolderCombiner(Handed.Max, WorldManager);
			Left = new GrabbableHolderCombiner(Handed.Left, WorldManager);
			Right = new GrabbableHolderCombiner(Handed.Right, WorldManager);
			DashMover = World.RootEntity.AddChild("TaskBarMover");
			DashMover.AttachComponent<UserInterfacePositioner>();
			BuildKeyboard();
			var screen = World.RootEntity.AddChild("RootScreen");
			RootScreenElement = screen.AttachComponent<UIElement>();
			var events = screen.AttachComponent<UIInputEvents>();
			events.InputEntered.Target = ScreenInputEntered;
			events.InputExited.Target = ScreenInputExited;
			IconTexRender = screen.AddChild("Center Icon").AttachComponent<TextureRect>();
			IconTexRender.InputFilter.Value = RInputFilter.Pass;
			InputManager.screenInput.MouseStateUpdate += (newState) => IconTexRender.Entity.enabled.Value = !newState;
			IconTexRender.Entity.orderOffset.Value = -100;
			UserInterface = screen.AddChild("UserInterface").AttachComponent<UIElement>();
			UserInterface.InputFilter.Value = RInputFilter.Pass;
			UserInterface.Enabled.Value = false;
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
			VRViewPort.TransparentBG.Value = true;
			VRViewPort.Size.Value = new Vector2i(1920, 1080);
			VRViewPort.UpdateMode.Value = RUpdateMode.Always;

			// It is here in our hearts

			//var e = Engine.windowManager.CreateNewWindow(1920, 1080);
			//e.WaitOnLoadedIn((win) => {
			//	win.Transparent = true;
			//	win.SizeChanged += () => {
			//		VRViewPort.Size.Value = win.Size;
			//	};
			//	win.Viewport = VRViewPort;
			//});

			UserInterfaceManager._PrivateSpaceManager = this;
			UserInterfaceManager.UserInterface = UserInterface;
			UserInterfaceManager.LoadInterface();
			_ProgramManager = Entity.AttachComponent<UserProgramManager>();
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			WorldManager.PrivateSpaceManager = this;
		}

		private double _contextMenuHoldTime = 0;
		private bool _updateDashState;
		private Handed _contextHand;

		public void WorldCycling() {
			WorldManager.WorldCycling();
		}

		protected override void RenderStep() {
			if (!Engine.EngineLink.CanInput) {
				return;
			}

			if (InputManager.GetInputAction(InputTypes.VRChange).JustActivated() && Engine.EngineLink.LiveVRChange && !Engine.HasKeyboard) {
				Engine.EngineLink.ChangeVR(!Engine.IsInVR);
			}

			if (InputManager.GetInputAction(InputTypes.ChangeWorld).JustActivated() && !Engine.HasKeyboard) {
				WorldCycling();
			}

			if (InputManager.GetInputAction(InputTypes.ContextMenu).JustActivated() && !Engine.HasKeyboard) {
				_contextMenuHoldTime = 0;
				_updateDashState = false;
				var mainContextHandValue = InputManager.GetInputAction(InputTypes.ContextMenu).HandedValue(Handed.Max);
				_contextHand = Handed.Max;
				if (mainContextHandValue < InputManager.GetInputAction(InputTypes.ContextMenu).HandedValue(Handed.Left)) {
					mainContextHandValue = InputManager.GetInputAction(InputTypes.ContextMenu).HandedValue(Handed.Left);
					_contextHand = Handed.Left;
				}
				if (mainContextHandValue < InputManager.GetInputAction(InputTypes.ContextMenu).HandedValue(Handed.Right)) {
					mainContextHandValue = InputManager.GetInputAction(InputTypes.ContextMenu).HandedValue(Handed.Right);
					_contextHand = Handed.Right;
				}
			}
			if (InputManager.GetInputAction(InputTypes.ContextMenu).Activated() && !Engine.HasKeyboard) {
				_contextMenuHoldTime += RTime.Elapsed;
			}
			if (InputManager.GetInputAction(InputTypes.ContextMenu).JustDeActivated() && !_updateDashState && !Engine.HasKeyboard) {
				_contextMenuHoldTime = 0;
				_updateDashState = false;
#if DEBUG
				RLog.Info($"Toggle Context Menu {_contextHand}");
#endif
			}
			if (!_updateDashState) {
				//Update hand progress

			}
			if (_contextMenuHoldTime >= 0.5f && !_updateDashState) {
				_updateDashState = true;
				UserInterfaceManager.ToggleDash();
			}

			var head = LocalUser.userRoot.Target?.head.Target;
			if (head != null) {
				if (!Engine.IsInVR) {
					if (isOnScreen) {
						UpdateHeadLazer(head);
					}
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
					if (collider.CustomObject is CanvasMesh uIComponent) {
						World.DrawDebugSphere(Matrix.T(hitpointworld), Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 0, 0.5f));
						uIComponent.ProcessHitTouch(handed, hitnormal, hitpointworld, handedSide);
					}
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

		public bool RunLaserCastInWorld(World world, ref Vector3f headFrompos, ref Vector3f headToPos, uint touchUndex, float pressForce, float gripForces, Handed side, ref Vector3f hitPointWorld, ref RCursorShape rCursorShape) {
			if (world.PhysicsSim.RayTest(ref headFrompos, ref headToPos, out var collider, out var hitnormal, out var hitpointworld)) {
				hitPointWorld = hitpointworld;
				if (collider.CustomObject is CanvasMesh uIComponent) {
					World.DrawDebugSphere(Matrix.T(hitpointworld), Vector3f.Zero, new Vector3f(0.005f), new Colorf(1, 1, 0, 0.5f));
					uIComponent.ProcessHitLazer(touchUndex, hitnormal, hitpointworld, pressForce, gripForces, side);
					rCursorShape = uIComponent.InputInterface.Target?.RCursorShape ?? RCursorShape.Arrow;
				}
				if (collider.CustomObject is PhysicsObject physicsObject) {
					World.DrawDebugSphere(Matrix.T(hitpointworld), Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 0, 0.5f));
					physicsObject.Lazer(touchUndex, hitnormal, hitpointworld, pressForce, gripForces, side);
					rCursorShape = physicsObject.CursorShape.Value;
				}
				return true;
			}
			return false;
		}

		public static RhubarbAtlasSheet.RhubarbIcons GetIcon(RCursorShape rCursorShape) {
			switch (rCursorShape) {
				//case RCursorShape.Ibeam:
				//	return RhubarbAtlasSheet.RhubarbIcons.Ibeam;
				case RCursorShape.PointingHand:
					return RhubarbAtlasSheet.RhubarbIcons.CursorCircle;
				//case RCursorShape.Cross:
				//	break;
				//case RCursorShape.Wait:
				//	break;
				//case RCursorShape.Busy:
				//	break;
				//case RCursorShape.Drag:
				//	break;
				//case RCursorShape.CanDrop:
				//	break;
				//case RCursorShape.Forbidden:
				//	break;
				//case RCursorShape.Vsize:
				//	break;
				//case RCursorShape.Hsize:
				//	break;
				//case RCursorShape.Bdiagsize:
				//	break;
				//case RCursorShape.Fdiagsize:
				//	break;
				case RCursorShape.Move:
					return RhubarbAtlasSheet.RhubarbIcons.CursorMove;
				//case RCursorShape.Vsplit:
				//	break;
				//case RCursorShape.Hsplit:
				//	break;
				//case RCursorShape.Help:
				//	break;
				default:
					return RhubarbAtlasSheet.RhubarbIcons.Cursor;
			}
		}

		public void UpdateLazer(Entity heand, Handed handed, Lazer lazer) {
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
			var currsor = RCursorShape.Arrow;
			if (RunLaserCastInWorld(World, ref vheadFrompos, ref vheadToPos, 10, PressForce, GripForce, handed, ref hitPoint, ref currsor)) {
				hitPrivate = true;
			}
			else if (RunLaserCastInWorld(World.worldManager.OverlayWorld, ref vheadFrompos, ref vheadToPos, 10, PressForce, GripForce, handed, ref hitPoint, ref currsor)) {
				hitOverlay = true;
			}
			else if (RunLaserCastInWorld(World.worldManager.FocusedWorld, ref vheadFrompos, ref vheadToPos, 10, PressForce, GripForce, handed, ref hitPoint, ref currsor)) {
				hitFocus = true;
			}
			lazer.CurrsorIcon = GetIcon(currsor);
			Engine.inputManager.MouseSystem.SetCurrsor(currsor);
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

		public event Action OnDrop;

		public event Action OnGrip;

		internal void HolderDrop() {
			OnDrop?.Invoke();
		}
		internal void HolderGrip() {
			OnGrip?.Invoke();
		}


		public void UpdateHeadLazer(Entity head) {
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
			var currsor = RCursorShape.Arrow;
			if (RunLaserCastInWorld(World, ref vheadFrompos, ref vheadToPos, 10, PressForce, GripForce, Handed.Max, ref hitPoint, ref currsor)) {
				hitPrivate = true;
			}
			else if (RunLaserCastInWorld(WorldManager.OverlayWorld, ref vheadFrompos, ref vheadToPos, 10, PressForce, GripForce, Handed.Max, ref hitPoint, ref currsor)) {
				hitOverlay = true;
			}
			else if (RunLaserCastInWorld(World.worldManager.FocusedWorld, ref vheadFrompos, ref vheadToPos, 10, PressForce, GripForce, Handed.Max, ref hitPoint, ref currsor)) {
				hitFocus = true;
			}
			CursorIcon = GetIcon(currsor);
			RootScreenElement.CursorShape.Value = currsor;
			UserInterface.CursorShape.Value = currsor;
			HeadLaserHitPoint = hitPoint;
			HeadLazerHitPrivate = hitPrivate;
			HeadLazerHitOverlay = hitOverlay;
			HeadLazerHitFocus = hitFocus;
		}

		public void KeyBoardUpdate(Matrix openLocation) {
			var wasAlreadyOpen = KeyboardEntity.enabled.Value;
			KeyboardEntity.enabled.Value = Engine.HasKeyboard && Engine.IsInVR;
			if (!wasAlreadyOpen) {
				KeyboardEntity.LocalTrans = Matrix.TR(new Vector3f(0, -0.5, -0.5), Quaternionf.CreateFromEuler(0, -10, 0));
			}
		}

		public event Action OnUpdateHolderReferen;
		internal void UpdateHolderReferen() {
			OnUpdateHolderReferen?.Invoke();
		}
	}
}
