using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using RhuEngine.WorldObjects.ECS;

using SharedModels;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.Components.PrivateSpace;
using RhuEngine.Input.XRInput;
using System.Reflection;
using System.Security.Policy;
using System.ComponentModel.DataAnnotations;

namespace RhuEngine.Components
{
	[PrivateSpaceOnly]
	[UpdateLevel(UpdateEnum.Normal)]
	public sealed class LazerVisual : Component
	{
		public bool HitPrivate;
		public bool HitOverlay;
		public bool HitFocus;

		[NoLoad, NoSave, NoShow, NoSync]
		public PhysicsObject hitPhysicsObject;

		public bool HitAny => HitPrivate | HitOverlay | HitFocus;

		public Vector3f HitPoint;

		public readonly Sync<bool> Locked;
		public readonly Sync<Handed> Side;

		public readonly SyncRef<CurvedTubeMesh> Mesh;
		public readonly SyncRef<UnlitMaterial> Material;
		public readonly SyncRef<Sprite3D> Currsor;
		public readonly SyncRef<Entity> Render;

		public RhubarbAtlasSheet.RhubarbIcons CurrsorIcon
		{
			set {
				if (Currsor.Target is null) {
					return;
				}
				Currsor.Target.Frame.Value = (int)value;
			}
		}

		protected override void OnAttach() {
			base.OnAttach();
			var render = World.RootEntity.AddChild("LazerRender");
			Render.Target = render;
			var sprite = Currsor.Target = render.AddChild("Currsor").AttachComponent<Sprite3D>();
			sprite.texture.Target = World.RootEntity.GetFirstComponentOrAttach<IconsTex>();
			sprite.HFrames.Value = 26;
			sprite.VFrames.Value = 7;
			sprite.PixelSize.Value = 0.00005f;
			sprite.NoDepthTest.Value = true;
			sprite.RenderPriority.Value += 101;
			sprite.Billboard.Value = RBillboardOptions.Enabled;
			sprite.FixedSize.Value = true;
			var data = render.AttachMeshWithMeshRender<CurvedTubeMesh, UnlitMaterial>();
			data.Item1.Radius.Value /= 2;
			data.Item1.StartHandle.Value /= 35;
			data.Item1.EndHandle.Value /= 35;
			data.Item2.Transparency.Value = Transparency.Blend;
			data.Item2.DullSided.Value = true;
			sprite.Moduluate.Value = data.Item2.Tint.Value = new Colorf(0.95f, 0.95f, 0.95f, 0.95f);
			Mesh.Target = data.Item1;
			Material.Target = data.Item2;
			RenderThread.ExecuteOnStartOfFrame(() => {
				RenderThread.ExecuteOnEndOfFrame(() => {
					RenderThread.ExecuteOnStartOfFrame(() => {
						data.Item2._material.NoDepthTest = true;
						data.Item2._material.Material.RenderPriority += 100;
					});
				});
			});
		}

		protected override void Step() {
			base.Step();
			Render.Target.enabled.Value = Engine.EngineLink.InVR;
			if (!Engine.EngineLink.InVR) {
				return;
			}
			RUpdateManager.ExecuteOnStartOfUpdate(() => {
				var pos = InputManager.XRInputSystem.GetHand(Side.Value)?[TrackerPos.Aim];
				Entity.rotation.Value = pos.Rotation;
				Entity.position.Value = pos.Position;
				Mesh.Target.Startpoint.Value = Entity.GlobalTrans.Translation;
			});
			if (Material.Target is not null && Currsor.Target is not null) {
				Currsor.Target.Moduluate.Value = Locked
					? (Material.Target.Tint.Value = new Colorf(1f, 0.95f, 1f, 0.95f))
					: (Material.Target.Tint.Value = new Colorf(0.95f, 0.95f, 0.95f, 0.95f));
			}
			RenderThread.ExecuteOnStartOfFrame(() => {
				if (Mesh.Target is null) {
					return;
				}
				var globalPos = Entity.GlobalTrans;
				Mesh.Target.Endpoint.Value = (HitAny | Locked) ? (Vector3d)HitPoint : (globalPos.Rotation * new Vector3d(0, 0, -100));
				Mesh.Target.EndHandle.Value = Mesh.Target.Endpoint.Value;
				var lenth = -Mesh.Target.Endpoint.Value.Distance(Mesh.Target.Startpoint.Value);
				Mesh.Target.StartHandle.Value = (globalPos.Rotation * new Vector3d(0, 0, lenth / 2)) + globalPos.Translation;

				if (Currsor.Target is null) {
					return;
				}

				Currsor.Target.Entity.enabled.Value = HitAny | Locked;
				Currsor.Target.Entity.GlobalTrans = Matrix.T(HitPoint);
			});
		}
	}
}
