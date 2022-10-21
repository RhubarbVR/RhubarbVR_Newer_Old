using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using RhuEngine.WorldObjects.ECS;

using SharedModels;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using RhuEngine.WorldObjects;
using RhuEngine.Components.PrivateSpace;
using RhuEngine.Input.XRInput;

namespace RhuEngine.Components
{
	[PrivateSpaceOnly]
	[UpdateLevel(UpdateEnum.Normal)]
	public sealed class Lazer : Component
	{
		public bool HitPrivate;
		public bool HitOverlay;
		public bool HitFocus;

		public bool HitAny => HitPrivate | HitOverlay | HitFocus;

		public Vector3f HitPoint;


		public readonly Sync<Handed> Side;

		public readonly SyncRef<CurvedTubeMesh> Mesh;
		public readonly SyncRef<UnlitMaterial> Material;
		public readonly SyncRef<Sprite3D> Currsor;

		protected override void OnAttach() {
			base.OnAttach();
			var sprite = Currsor.Target = Entity.AddChild("Currsor").AttachComponent<Sprite3D>();
			sprite.texture.Target = World.RootEntity.GetFirstComponentOrAttach<IconsTex>();
			sprite.HFrames.Value = 26;
			sprite.VFrames.Value = 7;
			sprite.Frame.Value = (int)RhubarbAtlasSheet.RhubarbIcons.Cursor;
			sprite.PixelSize.Value = 0.00005f;
			sprite.NoDepthTest.Value = true;
			sprite.RenderPriority.Value = int.MaxValue;
			sprite.Billboard.Value = RBillboardOptions.Enabled;
			sprite.FixedSize.Value = true;
			var data = Entity.AttachMeshWithMeshRender<CurvedTubeMesh, UnlitMaterial>();
			data.Item1.StartHandle.Value /= 10;
			data.Item1.EndHandle.Value /= 10;
			Mesh.Target = data.Item1;
			Material.Target = data.Item2;
			RenderThread.ExecuteOnStartOfFrame(() => {
				RenderThread.ExecuteOnEndOfFrame(() => {
					RenderThread.ExecuteOnStartOfFrame(() => {
						data.Item2._material.NoDepthTest = true;
						data.Item2._material.Material.RenderPriority = int.MaxValue;
					});
				});
			});
		}

		protected override void Step() {
			base.Step();
			Entity.enabled.Value = Engine.EngineLink.InVR;
			if (!Engine.EngineLink.InVR) {
				return;
			}
			var pos = InputManager.XRInputSystem.GetHand(Side.Value)?[TrackerPos.Aim];
			Entity.rotation.Value = pos.Rotation;
			Entity.position.Value = pos.Position;


			if (Mesh.Target is null) {
				return;
			}
			if (HitAny) {
				Mesh.Target.Endpoint.Value = Entity.GlobalPointToLocal(HitPoint);
			}
			else {
				Mesh.Target.Endpoint.Value = new Vector3d(0, 0, -100);
			}
			if (Currsor.Target is null) {
				return;
			}

			Currsor.Target.Entity.enabled.Value = HitAny;
			Currsor.Target.Entity.GlobalTrans = Matrix.T(HitPoint);
		}
	}
}
