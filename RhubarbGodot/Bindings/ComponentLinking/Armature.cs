using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhuEngine;
using RhuEngine.Components;
using RhuEngine.Linker;

using RNumerics;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class ArmatureLink : WorldPositionLinked<Armature, Skeleton3D>
	{
		public override string ObjectName => "Armature";

		public override void StartContinueInit() {
			LinkedComp.ArmatureEntitys.Changed += ArmatureEntitys_Changed;
			ArmatureEntitys_Changed(null);
		}

		public override void Render() {
			base.Render();
			var count = Math.Min(LinkedComp.ArmatureEntitys.Count, node.GetBoneCount());
			for (var i = 0; i < count; i++) {
				var entity = LinkedComp.ArmatureEntitys[i].Target;
				if (entity is null) {
					node.SetBonePosePosition(i, Vector3.Zero);
					node.SetBonePoseRotation(i, Quaternion.Identity);
					node.SetBonePoseScale(i, Vector3.One);
				}
				else {
					var localToMesh = LinkedComp.Entity.GlobalToLocal(entity.GlobalTrans);
					var casted = localToMesh.CastPosMatrix();
					node.SetBonePosePosition(i, casted.Origin);
					node.SetBonePoseRotation(i, casted.Basis.GetRotationQuaternion());
					node.SetBonePoseScale(i, casted.Basis.Scale);
				}
			}
		}

		private void ArmatureEntitys_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(this, () => {
				node.ClearBones();
				var count = LinkedComp.ArmatureEntitys.Count;
				for (var i = 0; i < count; i++) {
					node.AddBone(i.ToString());
				}
			});
		}
	}
}
