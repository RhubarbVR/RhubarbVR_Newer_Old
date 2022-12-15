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

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer" })]
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

		[Default(GizmoMode.All)]
		public readonly Sync<GizmoMode> Mode;

		public readonly Sync<bool> MultiSelect;

		[Exposed]
		public void ActionAddGizmo(Entity targetEntity) {
			if (MultiSelect) {
				AddGizmo(targetEntity);
			}
			else {
				if (Gizmos.Count >= 1) {
					RemoveGizmo(Gizmos[0].Target);
				}
				AddGizmo(targetEntity);
			}
		}

		public void AddGizmo(Entity targetEntity) {
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
		public void ToggleAction(bool stateChane) {
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

		public override void StartProgram(Stream file = null, string mimetype = null, string ex = null, params object[] args) {
			var tool = AddToolBar(IconFind);
			var boxCon = tool.Entity.AddChild().AttachComponent<BoxContainer>();
			var driver = Drive.Target = Entity.AttachComponent<ValueMultiDriver<GizmoMode>>();
			driver.source.Target = Mode;
			var pos = _pos.Target = boxCon.Entity.AddChild().AttachComponent<Button>();
			var posTexture = pos.Entity.AttachComponent<SingleIconTex>();
			posTexture.Icon.Value = RhubarbAtlasSheet.RhubarbIcons.Translate;
			pos.Icon.Target = posTexture;
			pos.MinSize.Value = new Vector2i(35);
			pos.IconAlignment.Value = RButtonAlignment.Center;
			pos.ExpandIcon.Value = true;
			pos.FocusMode.Value = RFocusMode.None;
			pos.ToggleMode.Value = true;
			pos.Toggled.Target = ToggleAction;
			pos.ButtonPressed.Value = true;

			var rot = _rot.Target = boxCon.Entity.AddChild().AttachComponent<Button>();
			var rotTexture = rot.Entity.AttachComponent<SingleIconTex>();
			rotTexture.Icon.Value = RhubarbAtlasSheet.RhubarbIcons.Rotation;
			rot.Icon.Target = rotTexture;
			rot.MinSize.Value = new Vector2i(35);
			rot.IconAlignment.Value = RButtonAlignment.Center;
			rot.ExpandIcon.Value = true;
			rot.FocusMode.Value = RFocusMode.None;
			rot.ToggleMode.Value = true;
			rot.ButtonPressed.Value = true;

			var scale = _scale.Target = boxCon.Entity.AddChild().AttachComponent<Button>();
			var scaleTexture = scale.Entity.AttachComponent<SingleIconTex>();
			scaleTexture.Icon.Value = RhubarbAtlasSheet.RhubarbIcons.Scale;
			scale.Icon.Target = scaleTexture;
			scale.MinSize.Value = new Vector2i(35);
			scale.IconAlignment.Value = RButtonAlignment.Center;
			scale.ExpandIcon.Value = true;
			scale.FocusMode.Value = RFocusMode.None;
			scale.ToggleMode.Value = true;
			scale.ButtonPressed.Value = true;

		}
	}
}
