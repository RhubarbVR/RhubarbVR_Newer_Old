using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhuEngine.Components;
using RhuEngine.Linker;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class ArmatureLink : WorldPositionLinked<Armature, Skeleton3D>
	{
		public override string ObjectName => "Armature";

		public override void StartContinueInit() {
			LinkedComp.ArmatureEntitys.Changed += ArmatureEntitys_Changed;
		}

		private void ArmatureEntitys_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RUpdateManager.ExecuteOnEndOfFrame(this, () => {
				node.ClearBones();
				var count = LinkedComp.ArmatureEntitys.Count;
				for (var i = 0; i < count; i++) {
					node.AddBone(count.ToString());
					node.SetBonePosePosition(i, Vector3.Zero);
					node.SetBonePoseRotation(i, Quaternion.Identity);
					node.SetBonePoseScale(i, Vector3.One);
				}
			});
		}
	}
}
