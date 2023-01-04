using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Assimp;

using DataModel.Enums;

using NYoutubeDL.Options;

using RhubarbCloudClient;

using RhuEngine.Commads;
using RhuEngine.Components.PrivateSpace;
using RhuEngine.Components.PrivateSpace.Programs.OverlayDialogues;
using RhuEngine.Components.UI;
using RhuEngine.Input.XRInput;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

using static System.Formats.Asn1.AsnWriter;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer" })]
	[UpdateLevel(UpdateEnum.Normal)]
	public sealed class DevToolsProgram : Program
	{
		public readonly SyncObjList<SyncRef<WorldGizmo3D>> Gizmos;

		public static RhubarbAtlasSheet.RhubarbIcons IconFind => RhubarbAtlasSheet.RhubarbIcons.Mouse;

		public static string ProgramNameLocName => "Programs.DevTools.Name";

		public override RTexture2D ProgramIcon => EngineHelpers.MainEngine.staticResources.IconSheet.GetElement(IconFind);

		public override string ProgramName => EngineHelpers.MainEngine.localisationManager.GetLocalString(ProgramNameLocName);

		public readonly SyncRef<Button> _pos;
		public readonly SyncRef<Button> _rot;
		public readonly SyncRef<Button> _scale;
		public readonly SyncRef<ValueMultiDriver<GizmoMode>> Drive;
		public readonly SyncRef<Button> _single;
		public readonly SyncRef<Button> _multi;

		[Default(GizmoMode.All)]
		public readonly Sync<GizmoMode> Mode;

		public readonly Sync<bool> MultiSelect;

		protected override void Step() {
			UpdateData(Handed.Left);
			UpdateData(Handed.Right);
			UpdateData(Handed.Max);
		}

		public void UpdateData(Handed handed) {
			if (!InputManager.GetInputAction(InputTypes.Secondary).HandedJustActivated(handed)) {
				return;
			}
			var hittingObject = PrivateSpaceManager.GetLazerHitObject(handed);
			if (hittingObject is null) {
				if (!MultiSelect) {
					for (var i = Gizmos.Count - 1; i >= 0; i--) {
						RemoveGizmo(Gizmos[i].Target);
					}
				}
				return;
			}
			if (hittingObject?.World != World) {
				return;
			}
			ActionAddGizmo(hittingObject.Entity);
		}

		[Exposed]
		public void ActionAddGizmo(Entity targetEntity) {
			if (MultiSelect) {
				AddGizmo(targetEntity);
			}
			else {
				for (var i = Gizmos.Count - 1; i >= 0; i--) {
					RemoveGizmo(Gizmos[i].Target);
				}
				AddGizmo(targetEntity);
			}
		}

		public void AddGizmo(Entity targetEntity) {
			foreach (SyncRef<WorldGizmo3D> item in Gizmos) {
				if (item.Target?.TargetEntity.Target == targetEntity) {
					return;
				}
			}
			var gizmo = Entity.AddChild("Gizmo").AttachComponent<WorldGizmo3D>();
			gizmo.SetUpWithEntity(targetEntity);
			Gizmos.Add().Target = gizmo;
			if (Drive.Target is not null) {
				Drive.Target.drivers.Add().Target = gizmo.Mode;
			}
		}

		public void RemoveGizmo(WorldGizmo3D worldGizmo) {
			if (worldGizmo is null) {
				return;
			}
			for (var i = 0; i < Gizmos.Count; i++) {
				if (Gizmos[i].Target == worldGizmo) {
					Gizmos[i].Destroy();
					break;
				}
			}
			if (Drive.Target is null) {
				return;
			}
			for (var i = 0; i < Drive.Target.drivers.Count; i++) {
				if (Drive.Target.drivers[i].Target == worldGizmo.Mode) {
					Drive.Target.drivers[i].Destroy();
					break;
				}
			}
			worldGizmo.Entity.Destroy();
		}

		[Exposed]
		public void ToggleMultiAction(bool stateChane) {
			var multi = _multi.Target?.ButtonPressed.Value ?? false;
			var single = _single.Target?.ButtonPressed.Value ?? false;
			if (!(multi | single)) {
				if (_multi.Target is not null) {
					_multi.Target.ButtonPressed.Value = MultiSelect.Value;
				}
				if (_single.Target is not null) {
					_single.Target.ButtonPressed.Value = !MultiSelect.Value;
				}
			}
			else {
				if (multi & single) {
					MultiSelect.Value = !MultiSelect.Value;
					if (_multi.Target is not null) {
						_multi.Target.ButtonPressed.Value = MultiSelect.Value;
					}
					if (_single.Target is not null) {
						_single.Target.ButtonPressed.Value = !MultiSelect.Value;
					}
				}
				else {
					if (_multi.Target is not null) {
						_multi.Target.ButtonPressed.Value = MultiSelect.Value;
					}
					if (_single.Target is not null) {
						_single.Target.ButtonPressed.Value = !MultiSelect.Value;
					}
				}
			}
		}

		[Exposed]
		public void ToggleModeAction(bool stateChane) {
			var pos = _pos.Target?.ButtonPressed.Value ?? false;
			var rot = _rot.Target?.ButtonPressed.Value ?? false;
			var scale = _scale.Target?.ButtonPressed.Value ?? false;
			if (!(pos | rot | scale)) {
				if (_pos.Target is not null) {
					_pos.Target.ButtonPressed.Value = Mode.Value.HasFlag(GizmoMode.Position);
				}
				if (_rot.Target is not null) {
					_rot.Target.ButtonPressed.Value = Mode.Value.HasFlag(GizmoMode.Rotation);
				}
				if (_scale.Target is not null) {
					_scale.Target.ButtonPressed.Value = Mode.Value.HasFlag(GizmoMode.Scale);
				}
			}
			else {
				var currentPos = GizmoMode.None;
				if (pos) {
					currentPos |= GizmoMode.Position;
				}
				if (rot) {
					currentPos |= GizmoMode.Rotation;
				}
				if (scale) {
					currentPos |= GizmoMode.Scale;
				}
				Mode.Value = currentPos;
			}
		}


		public void Window3DInVR(ViewPortProgramWindow viewPortProgramWindow) {
			if (!Engine.IsInVR) {
				PrivateSpaceManager.UserInterfaceManager.OpenCloseDash = true;
				return;
			}
			viewPortProgramWindow?.PrivateSpaceWindow?.PopoutIntoWorld();
			viewPortProgramWindow?.PrivateSpaceWindow?.MinimizeWindow();
		}

		[Exposed]
		public void SpawnObject(Spawn.SpawnObject spawnObject) {
			RLog.Info($"SpawnObject: {spawnObject}");
			var newEntity = LocalUser?.userRoot.Target?.Entity?.InternalParent?.AddChild(spawnObject.ToString());
			if (newEntity is null) {
				CloseWindowWithTag("CreateNEW");
				return;
			}
			newEntity.GlobalTrans = Matrix.T(Vector3f.Forward * 0.5f) * (LocalUser?.userRoot.Target?.head.Target?.GlobalTrans ?? Matrix.Identity);
			switch (spawnObject) {
				case Spawn.SpawnObject.cube:
					newEntity.AttachMeshWithMeshRender<TrivialBox3Mesh, UnlitMaterial>();
					newEntity.AttachComponent<BoxShape>();
					break;
				case Spawn.SpawnObject.sphere:
					newEntity.AttachMeshWithMeshRender<Sphere3NormalizedCubeMesh, UnlitMaterial>();
					newEntity.AttachComponent<SphereShape>();
					break;
				case Spawn.SpawnObject.arrow:
					var arrow = newEntity.AttachMeshWithMeshRender<ArrowMesh, UnlitMaterial>();
					newEntity.AttachComponent<MeshShape>().TargetMesh.Target = arrow.Item1;
					break;
				case Spawn.SpawnObject.capsule:
					newEntity.AttachMeshWithMeshRender<CapsuleMesh, UnlitMaterial>();
					newEntity.AttachComponent<CapsuleShape>();
					break;
				case Spawn.SpawnObject.cone:
					var cone = newEntity.AttachMeshWithMeshRender<ConeMesh, UnlitMaterial>();
					newEntity.AttachComponent<MeshShape>().TargetMesh.Target = cone.Item1;
					break;
				case Spawn.SpawnObject.cylinder:
					newEntity.AttachMeshWithMeshRender<CylinderMesh, UnlitMaterial>();
					newEntity.AttachComponent<CylinderShape>();
					break;
				case Spawn.SpawnObject.icosphere:
					newEntity.AttachMeshWithMeshRender<IcosphereMesh, UnlitMaterial>();
					newEntity.AttachComponent<SphereShape>();
					break;
				case Spawn.SpawnObject.mobiusstrip:
					var mobiusstrip = newEntity.AttachMeshWithMeshRender<MobiusStripMesh, UnlitMaterial>();
					newEntity.AttachComponent<MeshShape>().TargetMesh.Target = mobiusstrip.Item1;
					break;
				case Spawn.SpawnObject.circle:
					var circle = newEntity.AttachMeshWithMeshRender<CircleMesh, UnlitMaterial>();
					newEntity.AttachComponent<MeshShape>().TargetMesh.Target = circle.Item1;
					break;
				case Spawn.SpawnObject.rectangle:
					var rectangle = newEntity.AttachMeshWithMeshRender<RectangleMesh, UnlitMaterial>();
					newEntity.AttachComponent<MeshShape>().TargetMesh.Target = rectangle.Item1;
					break;
				case Spawn.SpawnObject.torus:
					var torus = newEntity.AttachMeshWithMeshRender<TorusMesh, UnlitMaterial>();
					newEntity.AttachComponent<MeshShape>().TargetMesh.Target = torus.Item1;
					break;
				case Spawn.SpawnObject.triangle:
					var triangle = newEntity.AttachMeshWithMeshRender<TriangleMesh, UnlitMaterial>();
					newEntity.AttachComponent<MeshShape>().TargetMesh.Target = triangle.Item1;
					break;
				default:
					break;
			}
			AddGizmo(newEntity);
			OpenInspectorOnObject(newEntity);
			CloseWindowWithTag("CreateNEW");
		}

		[Exposed]
		public void OpenCreateNew() {
			var window = AddWindow(Engine.localisationManager.GetLocalString("Programs.DevTools.CreateNew.Name"), null, false);
			window.Tag.Value = "CreateNEW";
			var iconTex = window.Entity.AttachComponent<SingleIconTex>();
			window.SizePixels -= new Vector2i(60, 60);
			window.Pos += new Vector2f(0, 60);
			iconTex.Icon.Value = RhubarbAtlasSheet.RhubarbIcons.AddObject;
			window.IconTexture.Target = iconTex;
			var scrollCon = window.Entity.AddChild("Scroll").AttachComponent<ScrollContainer>();
			var box = scrollCon.Entity.AddChild().AttachComponent<BoxContainer>();
			box.Vertical.Value = true;
			box.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
			box.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			foreach (Spawn.SpawnObject item in Enum.GetValues(typeof(Spawn.SpawnObject))) {
				var creteNewButton = box.Entity.AddChild(item.ToString()).AttachComponent<Button>();
				creteNewButton.Alignment.Value = RButtonAlignment.Center;
				creteNewButton.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
				creteNewButton.Text.Value = Engine.localisationManager.GetLocalString($"Programs.DevTools.CreateNew.{item}");
				var eventMan = creteNewButton.Entity.AttachComponent<AddSingleValuePram<Spawn.SpawnObject>>();
				eventMan.Target.Target = SpawnObject;
				eventMan.Value.Value = item;
				creteNewButton.Pressed.Target = eventMan.Call;
			}
			Window3DInVR(window);
		}

		[Exposed]
		public void OpenInspectorOnObject(ISyncObject target) {
			if (target is null) {
				return;
			}
			var window = AddWindow(Engine.localisationManager.GetLocalString("Programs.DevTools.Inspector.Name"), null, false);
			window.Tag.Value = "Inspector";
			var iconTex = window.Entity.AttachComponent<SingleIconTex>();
			window.SizePixels -= new Vector2i(0, 60);
			window.Pos += new Vector2f(0, 60);
			window.IconTexture.Target = iconTex;
			iconTex.Icon.Value = RhubarbAtlasSheet.RhubarbIcons.List;
			if (target is Entity targetEntity) {
				window.Entity.AttachComponent<EntityInspector>().TargetObject.Target = targetEntity;
			}
			else {
				var scroll = window.Entity.AddChild("Scroll").AttachComponent<ScrollContainer>();
				scroll.Entity.AddChild("Inspect").AttachComponent<WorldObjectInspector>().TargetObject.Target = target;
			}
			Window3DInVR(window);
		}

		[Exposed]
		public void OpenInspector() {
			OpenInspectorOnObject((Gizmos.Cast<SyncRef<WorldGizmo3D>>().FirstOrDefault()?.Target?.TargetEntity.Target) ?? World.RootEntity);
		}

		public enum DevWindows
		{
			Inspector,
			CreateNew,
			DevWindow,

		}

		[Exposed]
		public void SpawnDevWindow(DevWindows windows) {
			CloseWindowWithTag("devWindows");
			switch (windows) {
				case DevWindows.Inspector:
					OpenInspector();
					break;
				case DevWindows.CreateNew:
					OpenCreateNew();
					break;
				case DevWindows.DevWindow:
					OpenDevWindow();
					break;
				default:
					break;
			}
		}
		[Exposed]
		public void OpenDevWindow() {
			var window = AddWindow(Engine.localisationManager.GetLocalString("Programs.DevTools.DevWindow.Name"), null, false);
			window.Tag.Value = "devWindows";
			var iconTex = window.Entity.AttachComponent<SingleIconTex>();
			window.SizePixels -= new Vector2i(60, 60);
			window.Pos += new Vector2f(0, 60);
			iconTex.Icon.Value = RhubarbAtlasSheet.RhubarbIcons.AddObject;
			window.IconTexture.Target = iconTex;
			var scrollCon = window.Entity.AddChild("Scroll").AttachComponent<ScrollContainer>();
			var box = scrollCon.Entity.AddChild().AttachComponent<BoxContainer>();
			box.Vertical.Value = true;
			box.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
			box.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			foreach (DevWindows item in Enum.GetValues(typeof(DevWindows))) {
				var creteNewButton = box.Entity.AddChild(item.ToString()).AttachComponent<Button>();
				creteNewButton.Alignment.Value = RButtonAlignment.Center;
				creteNewButton.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
				creteNewButton.Text.Value = Engine.localisationManager.GetLocalString($"Programs.DevTools.{item}.Name");
				var eventMan = creteNewButton.Entity.AttachComponent<AddSingleValuePram<DevWindows>>();
				eventMan.Target.Target = SpawnDevWindow;
				eventMan.Value.Value = item;
				creteNewButton.Pressed.Target = eventMan.Call;
			}
			Window3DInVR(window);
		}

		public override void StartProgram(Stream file = null, string mimetype = null, string ex = null, params object[] args) {
			var tool = AddToolBar(IconFind);
			var boxCon = tool.Entity.AddChild().AttachComponent<BoxContainer>();
			var driver = Drive.Target = Entity.AttachComponent<ValueMultiDriver<GizmoMode>>();
			driver.source.Target = Mode;

			boxCon.Entity.AddChild().AttachComponent<Panel>().MinSize.Value = new Vector2i(5, 15);

			var pos = _pos.Target = boxCon.Entity.AddChild().AttachComponent<Button>();
			var posTexture = pos.Entity.AttachComponent<SingleIconTex>();
			posTexture.Icon.Value = RhubarbAtlasSheet.RhubarbIcons.Translate;
			pos.Icon.Target = posTexture;
			pos.MinSize.Value = new Vector2i(55);
			pos.IconAlignment.Value = RButtonAlignment.Center;
			pos.ExpandIcon.Value = true;
			pos.FocusMode.Value = RFocusMode.None;
			pos.ToggleMode.Value = true;
			pos.Toggled.Target = ToggleModeAction;
			pos.ButtonPressed.Value = true;

			var rot = _rot.Target = boxCon.Entity.AddChild().AttachComponent<Button>();
			var rotTexture = rot.Entity.AttachComponent<SingleIconTex>();
			rotTexture.Icon.Value = RhubarbAtlasSheet.RhubarbIcons.Rotation;
			rot.Icon.Target = rotTexture;
			rot.MinSize.Value = new Vector2i(55);
			rot.IconAlignment.Value = RButtonAlignment.Center;
			rot.ExpandIcon.Value = true;
			rot.FocusMode.Value = RFocusMode.None;
			rot.ToggleMode.Value = true;
			rot.ButtonPressed.Value = true;
			rot.Toggled.Target = ToggleModeAction;

			var scale = _scale.Target = boxCon.Entity.AddChild().AttachComponent<Button>();
			var scaleTexture = scale.Entity.AttachComponent<SingleIconTex>();
			scaleTexture.Icon.Value = RhubarbAtlasSheet.RhubarbIcons.Scale;
			scale.Icon.Target = scaleTexture;
			scale.MinSize.Value = new Vector2i(55);
			scale.IconAlignment.Value = RButtonAlignment.Center;
			scale.ExpandIcon.Value = true;
			scale.FocusMode.Value = RFocusMode.None;
			scale.ToggleMode.Value = true;
			scale.ButtonPressed.Value = true;
			scale.Toggled.Target = ToggleModeAction;

			boxCon.Entity.AddChild().AttachComponent<Panel>().MinSize.Value = new Vector2i(5, 15);

			var multi = _multi.Target = boxCon.Entity.AddChild().AttachComponent<Button>();
			var multiTexture = multi.Entity.AttachComponent<SingleIconTex>();
			multiTexture.Icon.Value = RhubarbAtlasSheet.RhubarbIcons.MultiSelect;
			multi.Icon.Target = multiTexture;
			multi.MinSize.Value = new Vector2i(55);
			multi.IconAlignment.Value = RButtonAlignment.Center;
			multi.ExpandIcon.Value = true;
			multi.FocusMode.Value = RFocusMode.None;
			multi.ToggleMode.Value = true;
			multi.ButtonPressed.Value = true;
			multi.Toggled.Target = ToggleMultiAction;

			var single = _single.Target = boxCon.Entity.AddChild().AttachComponent<Button>();
			var singleTexture = single.Entity.AttachComponent<SingleIconTex>();
			singleTexture.Icon.Value = RhubarbAtlasSheet.RhubarbIcons.SingleSelect;
			single.Icon.Target = singleTexture;
			single.MinSize.Value = new Vector2i(55);
			single.IconAlignment.Value = RButtonAlignment.Center;
			single.ExpandIcon.Value = true;
			single.FocusMode.Value = RFocusMode.None;
			single.ToggleMode.Value = true;
			single.ButtonPressed.Value = true;
			single.Toggled.Target = ToggleMultiAction;

			boxCon.Entity.AddChild().AttachComponent<Panel>().MinSize.Value = new Vector2i(5, 15);

			var openWindow = boxCon.Entity.AddChild().AttachComponent<Button>();
			var openWindowTexture = openWindow.Entity.AttachComponent<SingleIconTex>();
			openWindowTexture.Icon.Value = RhubarbAtlasSheet.RhubarbIcons.OpenInspectorWindow;
			openWindow.Icon.Target = openWindowTexture;
			openWindow.MinSize.Value = new Vector2i(55);
			openWindow.IconAlignment.Value = RButtonAlignment.Center;
			openWindow.ExpandIcon.Value = true;
			openWindow.FocusMode.Value = RFocusMode.None;
			openWindow.Pressed.Target = OpenInspector;

			var openDevWindow = boxCon.Entity.AddChild().AttachComponent<Button>();
			var openDevWindowTexture = openDevWindow.Entity.AttachComponent<SingleIconTex>();
			openDevWindowTexture.Icon.Value = RhubarbAtlasSheet.RhubarbIcons.OpenDevWindow;
			openDevWindow.Icon.Target = openDevWindowTexture;
			openDevWindow.MinSize.Value = new Vector2i(55);
			openDevWindow.IconAlignment.Value = RButtonAlignment.Center;
			openDevWindow.ExpandIcon.Value = true;
			openDevWindow.FocusMode.Value = RFocusMode.None;
			openDevWindow.Pressed.Target = OpenDevWindow;


			var openCreateWindow = boxCon.Entity.AddChild().AttachComponent<Button>();
			var openCreateWindowTexture = openCreateWindow.Entity.AttachComponent<SingleIconTex>();
			openCreateWindowTexture.Icon.Value = RhubarbAtlasSheet.RhubarbIcons.AddObject;
			openCreateWindow.Icon.Target = openCreateWindowTexture;
			openCreateWindow.MinSize.Value = new Vector2i(55);
			openCreateWindow.IconAlignment.Value = RButtonAlignment.Center;
			openCreateWindow.ExpandIcon.Value = true;
			openCreateWindow.FocusMode.Value = RFocusMode.None;
			openCreateWindow.Pressed.Target = OpenCreateNew;


			boxCon.Entity.AddChild().AttachComponent<Panel>().MinSize.Value = new Vector2i(5, 15);

		}
	}
}
