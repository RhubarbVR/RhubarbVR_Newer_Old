using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhuEngine;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.Components;
using RNumerics;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Commads
{
	public class Spawn : Command
	{
		public enum SpawnObject {
			none,
			cube,
			sphere,
			arrow,
			capsule,
			cone,
			cylinder,
			icosphere,
			mobiusstrip,
			circle,
			rectangle,
			torus,
			triangle,
		}
		public override string HelpMsg => "Spawn Object";

		public override Task RunCommand() {
			if (args.Length == 1) {
				Console.WriteLine("Need Object To Spawn");
				foreach (var item in Enum.GetNames(typeof(SpawnObject))) {
					Console.WriteLine(item);
				}

				return Task.CompletedTask;
			}
			var spawntype = (SpawnObject)Enum.Parse(typeof(SpawnObject), args[1], true);
			var enty = Manager.Engine.worldManager.FocusedWorld.GetLocalUser().userRoot.Target?.head.Target ?? Manager.Engine.worldManager.FocusedWorld.RootEntity;
			var AttachEntiyTo = Manager.Engine.worldManager.FocusedWorld.RootEntity.AddChild(spawntype.ToString());
			AttachEntiyTo.GlobalTrans = Matrix.TS(0, 0, -3,0.25f) * enty.GlobalTrans;
			AttachEntiyTo.AttachComponent<Grabbable>();
			IAssetProvider<RMesh> meshe;
			void AttachMeshCollider() {
				AttachEntiyTo.AttachComponent<MeshShape>().TargetMesh.Target = meshe;
			}
			switch (spawntype) {
				case SpawnObject.sphere:
					(_, _) = AttachEntiyTo.AttachMesh<Sphere3NormalizedCubeMesh, UnlitMaterial>();
					AttachEntiyTo.AttachComponent<SphereShape>();
					break;
				case SpawnObject.arrow:
					(meshe, _) = AttachEntiyTo.AttachMesh<ArrowMesh, UnlitMaterial>();
					AttachMeshCollider();
					break;
				case SpawnObject.capsule:
					(_, _) = AttachEntiyTo.AttachMesh<CapsuleMesh, UnlitMaterial>();
					AttachEntiyTo.AttachComponent<CapsuleShape>();
					break;
				case SpawnObject.cone:
					(meshe, _) = AttachEntiyTo.AttachMesh<ConeMesh, UnlitMaterial>();
					AttachMeshCollider();
					break;
				case SpawnObject.cylinder:
					(_, _) = AttachEntiyTo.AttachMesh<CylinderMesh, UnlitMaterial>();
					AttachEntiyTo.AttachComponent<CylinderShape>();
					break;
				case SpawnObject.icosphere:
					(_, _) = AttachEntiyTo.AttachMesh<IcosphereMesh, UnlitMaterial>();
					AttachEntiyTo.AttachComponent<SphereShape>();
					break;
				case SpawnObject.mobiusstrip:
					(meshe, _) = AttachEntiyTo.AttachMesh<MobiusStripMesh, UnlitMaterial>();
					AttachMeshCollider();
					break;
				case SpawnObject.circle:
					(meshe, _) = AttachEntiyTo.AttachMesh<CircleMesh, UnlitMaterial>();
					AttachMeshCollider();
					break;
				case SpawnObject.rectangle:
					(meshe, _) = AttachEntiyTo.AttachMesh<RectangleMesh, UnlitMaterial>();
					AttachMeshCollider();
					break;
				case SpawnObject.torus:
					(meshe, _) = AttachEntiyTo.AttachMesh<TorusMesh, UnlitMaterial>();
					AttachMeshCollider();
					break;
				case SpawnObject.triangle:
					(meshe, _) = AttachEntiyTo.AttachMesh<TriangleMesh, UnlitMaterial>();
					AttachMeshCollider();
					break;
				default:
					(_, _) = AttachEntiyTo.AttachMesh<TrivialBox3Mesh, UnlitMaterial>();
					AttachEntiyTo.AttachComponent<BoxShape>();
					break;
			}

			return Task.CompletedTask;
		}
	}
}
