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

		public override void RunCommand() {
			if(args.Length == 1) {
				Console.WriteLine("Need Object To Spawn");
				return;
			}
			var spawntype = (SpawnObject)Enum.Parse(typeof(SpawnObject), args[1], true);
			var enty = Manager.Engine.worldManager.FocusedWorld.GetLocalUser().userRoot.Target?.head.Target ?? Manager.Engine.worldManager.FocusedWorld.RootEntity;
			var AttachEntiyTo = Manager.Engine.worldManager.FocusedWorld.RootEntity.AddChild(spawntype.ToString());
			AttachEntiyTo.GlobalTrans = Matrix.TS(0, 0, -3,0.25f) * enty.GlobalTrans;
			AttachEntiyTo.AttachComponent<Grabbable>();
			IAssetProvider<RMesh> meshe;
			switch (spawntype) {
				case SpawnObject.sphere:
					(meshe, _) = AttachEntiyTo.AttachMesh<Sphere3NormalizedCubeMesh, UnlitMaterial>();
					break;
				case SpawnObject.arrow:
					(meshe, _) = AttachEntiyTo.AttachMesh<ArrowMesh, UnlitMaterial>();
					break;
				case SpawnObject.capsule:
					(meshe, _) = AttachEntiyTo.AttachMesh<CapsuleMesh, UnlitMaterial>();
					break;
				case SpawnObject.cone:
					(meshe, _) = AttachEntiyTo.AttachMesh<ConeMesh, UnlitMaterial>();
					break;
				case SpawnObject.cylinder:
					(meshe, _) = AttachEntiyTo.AttachMesh<CylinderMesh, UnlitMaterial>();
					break;
				case SpawnObject.icosphere:
					(meshe, _) = AttachEntiyTo.AttachMesh<IcosphereMesh, UnlitMaterial>();
					break;
				case SpawnObject.mobiusstrip:
					(meshe, _) = AttachEntiyTo.AttachMesh<MobiusStripMesh, UnlitMaterial>();
					break;
				case SpawnObject.circle:
					(meshe, _) = AttachEntiyTo.AttachMesh<CircleMesh, UnlitMaterial>();
					break;
				case SpawnObject.rectangle:
					(meshe, _) = AttachEntiyTo.AttachMesh<RectangleMesh, UnlitMaterial>();
					break;
				case SpawnObject.torus:
					(meshe, _) = AttachEntiyTo.AttachMesh<TorusMesh, UnlitMaterial>();
					break;
				case SpawnObject.triangle:
					(meshe, _) = AttachEntiyTo.AttachMesh<TriangleMesh, UnlitMaterial>();
					break;
				default:
					(meshe, _) = AttachEntiyTo.AttachMesh<TrivialBox3Mesh, UnlitMaterial>();
					break;
			}
			AttachEntiyTo.AttachComponent<ConvexMeshShape>().TargetMesh.Target = meshe;
		}
	}
}
