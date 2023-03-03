using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GDExtension;

using RhuEngine;
using RhuEngine.Components;
using RhuEngine.Linker;

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
			var count = System.Math.Min(LinkedComp.ArmatureEntitys.Count, node.GetBoneCount());
			for (var i = 0; i < count; i++) {
				var entity = LinkedComp.ArmatureEntitys[i].Target;
				if (entity is null) {
					node.SetBonePosePosition(i, Vector3.Zero);
					node.SetBonePoseRotation(i, Quaternion.Identity);
					node.SetBonePoseScale(i, Vector3.One);
				}
				else {
					var entitPos = entity.GlobalTrans * LinkedComp.Entity.GlobalTrans.Inverse;
					entitPos.Decompose(out var pos, out var rot, out var scale);
					node.SetBonePosePosition(i, new Vector3(pos.x, pos.y, pos.z));
					node.SetBonePoseRotation(i, new Quaternion(rot.x, rot.y, rot.z, rot.w));
					node.SetBonePoseScale(i, new Vector3(scale.x, scale.y, scale.z));
				}
			}
		}

		private void ArmatureEntitys_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(this, () => {
				node.ClearBones();
				var count = LinkedComp.ArmatureEntitys.Count;
				for (var i = 0; i < count; i++) {
					node.AddBone(i.ToString());
					var entity = LinkedComp.ArmatureEntitys[i].Target;
					if (entity is null) {
						node.SetBonePosePosition(i, Vector3.Zero);
						node.SetBonePoseRotation(i, Quaternion.Identity);
						node.SetBonePoseScale(i, Vector3.One);
					}
					else {
						var entitPos = entity.GlobalTrans * LinkedComp.Entity.GlobalTrans.Inverse;
						entitPos.Decompose(out var pos, out var rot, out var scale);
						node.SetBonePosePosition(i, new Vector3(pos.x, pos.y, pos.z));
						node.SetBonePoseRotation(i, new Quaternion(rot.x, rot.y, rot.z, rot.w));
						node.SetBonePoseScale(i, new Vector3(scale.x, scale.y, scale.z));
					}

				}
			});
		}
	}
}
