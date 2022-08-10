using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using System;

namespace RhuEngine.Components
{
	[NotLinkedRenderingComponent]
	[Category(new string[] { "UI" })]
	public class UICanvas : RenderingComponent
	{

		[OnChanged(nameof(UpdatePyhsicsMesh))]
		public readonly Sync<Vector3f> scale;

		[Default(false)]
		[OnChanged(nameof(UpdatePyhsicsMesh))]
		public readonly Sync<bool> TopOffset;

		[Default(3f)]
		[OnChanged(nameof(UpdatePyhsicsMesh))]
		public readonly Sync<float> TopOffsetValue;

		[Default(false)]
		[OnChanged(nameof(UpdatePyhsicsMesh))]
		public readonly Sync<bool> FrontBind;

		[Default(10)]
		[OnChanged(nameof(UpdatePyhsicsMesh))]
		public readonly Sync<int> FrontBindSegments;

		[Default(135f)]
		[OnChanged(nameof(UpdatePyhsicsMesh))]
		public readonly Sync<float> FrontBindAngle;

		[Default(7.5f)]
		[OnChanged(nameof(UpdatePyhsicsMesh))]
		public readonly Sync<float> FrontBindRadus;

		public RigidBodyCollider PhysicsCollider;
		public Matrix PhysicsColliderOffset = Matrix.Identity;
		public void UpdatePyhsicsMesh() {
			RWorld.ExecuteOnEndOfFrame(this, () => {
				var isJustBloxMesh = !(TopOffset.Value || FrontBind.Value);
				if (isJustBloxMesh) {
					var size = scale.Value / 10;
					PhysicsCollider = new RBoxShape(size / 2).GetCollider(World.PhysicsSim);
					PhysicsCollider.CustomObject = this;
					PhysicsCollider.Group = ECollisionFilterGroups.UI;
					PhysicsCollider.Mask = ECollisionFilterGroups.UI;
					PhysicsCollider.Active = Entity.IsEnabled;
					PhysicsColliderOffset = Matrix.T(size / 2);
				}
				else {
					var newMeshGen = new TrivialBox3Generator {
						Box = new Box3d(Vector3d.One / 2, Vector3d.One / 2)
					};
					var NewMesh = newMeshGen.Generate().MakeSimpleMesh();
					if (TopOffset.Value) {
						NewMesh.OffsetTop(TopOffsetValue.Value);
					}
					if (FrontBind.Value) {
						NewMesh = NewMesh.UIBind(FrontBindAngle.Value, FrontBindRadus.Value, FrontBindSegments.Value, scale);
					}
					NewMesh.Scale(scale.Value.x / 10, scale.Value.y / 10, scale.Value.z / 10);
					PhysicsCollider = new RRawMeshShape(NewMesh).GetCollider(World.PhysicsSim);
					PhysicsCollider.CustomObject = this;
					PhysicsCollider.Group = ECollisionFilterGroups.UI;
					PhysicsCollider.Mask = ECollisionFilterGroups.UI;
					PhysicsCollider.Active = Entity.IsEnabled;
					PhysicsColliderOffset = Matrix.Identity;
				}
			});
		}

		public override void OnAttach() {
			base.OnAttach();
			scale.Value = new Vector3f(16, 9, 1);
		}

		public override void OnLoaded() {
			base.OnLoaded();
			Entity.UIRect?.CanvasUpdate();
			UpdatePyhsicsMesh();
			Entity.EnabledChanged += Entity_EnabledChanged;
		}

		private void Entity_EnabledChanged() {
			if (PhysicsCollider is not null) {
				PhysicsCollider.Active = Entity.IsEnabled;
			}
		}

		public override void Render() {
			var transFormOfMesh = Entity.GlobalTrans;
			if (PhysicsCollider is not null) {
				PhysicsCollider.Matrix = transFormOfMesh;
			}
			Entity.UIRect?.RenderRect(transFormOfMesh);
		}

		public void ProcessHitTouch(uint handed, Vector3f hitnormal, Vector3f hitpointworld) {
		
		}
		public void ProcessHitLazer(uint touchUndex, Vector3f hitnormal, Vector3f hitpointworld, float pressForce, float gripForces, Handed side) {

		}
	}
}
