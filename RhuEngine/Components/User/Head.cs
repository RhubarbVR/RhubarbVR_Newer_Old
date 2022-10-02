using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.PlayerInput)]
	[Category(new string[] { "User" })]
	public sealed class Head : Component
	{
		public readonly SyncRef<User> user;

		public readonly Linker<Vector3f> pos;

		public readonly Linker<Quaternionf> rot;

		public readonly Linker<Vector3f> scale;

		protected override void OnAttach() {
			pos.SetLinkerTarget(Entity.position);
			rot.SetLinkerTarget(Entity.rotation);
			scale.SetLinkerTarget(Entity.scale);
			if (!World.IsPersonalSpace) {
				return;
			}
			var invr = Entity.AttachComponent<IsInVR>();
			var entity = Entity.AddChild("Currsor");
			entity.position.Value = new Vector3f(0, 0, -0.1f);
			entity.scale.Value = new Vector3f(0.005f);
			entity.rotation.Value = Quaternionf.CreateFromEuler(0, 90, 0);
			invr.isNotVR.Target = entity.enabled;
			var (mesh,mrit,render) = entity.AttachMeshWithMeshRender<SpriteMesh, UnlitMaterial>();
			var colorer = entity.AttachComponent<UIColorAssign>();
			colorer.TargetColor.Target = render.colorLinear;
			colorer.ColorShif.Value = 1.9f;
			colorer.Alpha.Value = 0.9f;
			var ea = entity.AttachComponent<IconsTex>();
			var sprite = entity.AttachComponent<SpriteProvder>();
			sprite.Texture.Target = ea;
			mrit.MainTexture.Target = ea;
			mrit.Transparency.Value = Transparency.Blend;
			RenderThread.ExecuteOnEndOfFrame(() => mrit._material.NoDepthTest = true);
			mrit.RenderOrderOffset.Value = 1000000000;
			sprite.GridSize.Value = new Vector2i(26, 7);
			mesh.Dimensions.Value = new Vector2f(1.25f);
			mesh.Sprite.Target = sprite;
			mesh.SetPos(new Vector2i(16,2));
		}

		protected override void RenderStep() {
			if (!Engine.EngineLink.CanInput) {
				return;
			}
			if (World.IsPersonalSpace) {
				Entity.LocalTrans = Engine.IsInVR ? InputManager.HeadMatrix * RRenderer.CameraRoot.Inverse : InputManager.HeadMatrix;
			}
			else {
				if (user.Target is null) {
					return;
				}
				if (user.Target == LocalUser) {
					Entity.LocalTrans = Engine.IsInVR ? InputManager.HeadMatrix  * RRenderer.CameraRoot.Inverse : InputManager.HeadMatrix;
					user.Target.FindOrCreateSyncStream<SyncValueStream<Vector3f>>("HeadPos").Value = Entity.position.Value;
					user.Target.FindOrCreateSyncStream<SyncValueStream<Quaternionf>>("HeadRot").Value = Entity.rotation.Value;
				}
				else {
					var position = user.Target.FindSyncStream<SyncValueStream<Vector3f>>("HeadPos")?.Value ?? Vector3f.Zero;
					var rotation = user.Target.FindSyncStream<SyncValueStream<Quaternionf>>("HeadRot")?.Value ?? Quaternionf.Identity;
					Entity.LocalTrans = Matrix.TR(position, rotation);
				}
			}
		}
	}
}