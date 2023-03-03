using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GDExtension;

using RhuEngine;
using RhuEngine.Components;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class GodotCamera3D : WorldPositionLinked<RhuEngine.Components.Camera3D, GDExtension.Camera3D>
	{
		public override bool GoToEngineRoot => false;
		public override string ObjectName => "Camera3D";

		public override void StartContinueInit() {
			LinkedComp.KeepWidth.Changed += KeepWidth_Changed;
			LinkedComp.RenderMask.Changed += RenderMask_Changed;
			LinkedComp.HOffset.Changed += HOffset_Changed;
			LinkedComp.VOffset.Changed += VOffset_Changed;
			LinkedComp.DopplerTracking.Changed += DopplerTracking_Changed;
			LinkedComp.Current.Changed += Current_Changed;
			LinkedComp.Near.Changed += Near_Changed;
			LinkedComp.Far.Changed += Far_Changed;
			LinkedComp.Projection.Changed += PRojection_Changed;
			LinkedComp.Perspective_Fov.Changed += Perspective_Fov_Changed;
			LinkedComp.Orthongonal_Frustum_Size.Changed += Orthongonal_Frustum_Size_Changed;
			LinkedComp.Frustum_Offset.Changed += Frustum_Offset_Changed;
			KeepWidth_Changed(null);
			RenderMask_Changed(null);
			HOffset_Changed(null);
			VOffset_Changed(null);
			DopplerTracking_Changed(null);
			Current_Changed(null);
			Near_Changed(null);
			Far_Changed(null);
			PRojection_Changed(null);
			Perspective_Fov_Changed(null);
			Orthongonal_Frustum_Size_Changed(null);
			Frustum_Offset_Changed(null);
		}

		private void Frustum_Offset_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.FrustumOffset = new Vector2(LinkedComp.Frustum_Offset.Value.x, LinkedComp.Frustum_Offset.Value.y));
		}

		private void Orthongonal_Frustum_Size_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Size = LinkedComp.Orthongonal_Frustum_Size.Value);
		}

		private void Perspective_Fov_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Fov = LinkedComp.Perspective_Fov.Value);
		}

		private void PRojection_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Projection = LinkedComp.Projection.Value switch { RProjectionMode.Orthongonal => GDExtension.Camera3D.ProjectionType.Orthogonal, RProjectionMode.Frustum => GDExtension.Camera3D.ProjectionType.Frustum, _ => GDExtension.Camera3D.ProjectionType.Perspective, });
		}

		private void Far_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Far = LinkedComp.Far.Value);
		}

		private void Near_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Near = LinkedComp.Near.Value);
		}

		private void Current_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Current = LinkedComp.Current.Value);
		}

		private void DopplerTracking_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.DopplerTrackingValue = LinkedComp.DopplerTracking.Value switch { RDopplerEffect.Idle => GDExtension.Camera3D.DopplerTracking.IdleStep, RDopplerEffect.Physics => GDExtension.Camera3D.DopplerTracking.PhysicsStep, _ => GDExtension.Camera3D.DopplerTracking.Disabled, });
		}

		private void VOffset_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.VOffset = LinkedComp.VOffset.Value);
		}

		private void HOffset_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.HOffset = LinkedComp.HOffset.Value);
		}

		private void RenderMask_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.CullMask = (uint)LinkedComp.RenderMask.Value);
		}

		private void KeepWidth_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.KeepAspectValue = LinkedComp.KeepWidth.Value ? GDExtension.Camera3D.KeepAspect.Width : GDExtension.Camera3D.KeepAspect.Height);
		}
	}
}
