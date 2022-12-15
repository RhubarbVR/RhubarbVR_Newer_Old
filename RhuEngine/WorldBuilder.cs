using System;

using RhuEngine.Components;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using System.Collections.Generic;
using RNumerics;
using RhuEngine.Linker;
using System.Threading;
using System.Threading.Tasks;

namespace RhuEngine
{
	public static class WorldBuilder
	{

		public static void BuildDefaultWorld(this World world) {
			RLog.Info("Building Default World");
			var floor = world.RootEntity.AddChild("Floor");
			floor.position.Value = new Vector3f(0, 0, 0);
			var coloider = floor.AttachComponent<CylinderShape>();
			var (mesh, mit, render) = floor.AttachMeshWithMeshRender<CylinderMesh, UnlitMaterial>();
			mit.Transparency.Value = Transparency.Blend;
			mit.Tint.Value = new Colorf(10, 10, 10, 150);
			mesh.TopRadius.Value = 4;
			mesh.BaseRadius.Value = 3.5f;
			mesh.Height.Value = 0.25f;
			coloider.Radius.Value = 4;
			coloider.Height.Value = 0.25f;
			var spinningCubes = world.RootEntity.AddChild("SpinningCubes");
			spinningCubes.position.Value = new Vector3f(0, 0.5f, 0);
			AttachSpiningCubes(spinningCubes);
		}

		public static void BuildLocalWorld(this World world) {
			RLog.Info("Building Local World");
			var floor = world.RootEntity.AddChild("Floor");
			floor.position.Value = new Vector3f(0, 0, 0);
			var coloider = floor.AttachComponent<CylinderShape>();
			var (mesh, mit, render) = floor.AttachMeshWithMeshRender<CylinderMesh, UnlitMaterial>();
			mit.Transparency.Value = Transparency.Blend;
			mit.Tint.Value = new Colorf(10, 10, 10, 150);
			mesh.TopRadius.Value = 4;
			mesh.BaseRadius.Value = 3.5f;
			mesh.Height.Value = 0.25f;
			coloider.Radius.Value = 4;
			coloider.Height.Value = 0.5f;
			var spinningCubes = world.RootEntity.AddChild("SpinningCubes");
			spinningCubes.position.Value = new Vector3f(0, 0.5f, 0);
			AttachSpiningCubes(spinningCubes);
#if DEBUG


			var DebugStuff = floor.AddChild("DebugStuff");
			DebugStuff.position.Value = new Vector3f(-1.5f, 0f, -1f);

			var gizmo3D = DebugStuff.AddChild("Gizmo3D");
			gizmo3D.position.Value = new Vector3f(3, 1f, 0.1f);
			gizmo3D.rotation.Value = Quaternionf.CreateFromEuler(45, 5, 45);
			gizmo3D.AttachComponent<WorldGizmo3D>().SetUpWithEntity(gizmo3D);
			var (eemesh, _, _) = gizmo3D.AttachMeshWithMeshRender<TrivialBox3Mesh, UnlitMaterial>();
			eemesh.Extent.Value = new Vector3f(0.2f);

			var gizmo3D2 = gizmo3D.AddChild("Gizmo3D");
			gizmo3D2.position.Value = new Vector3f(1, 1f, 1f);
			gizmo3D2.AttachComponent<WorldGizmo3D>().SetUpWithEntity(gizmo3D2);
			var (eeemesh, _, _) = gizmo3D2.AttachMeshWithMeshRender<TrivialBox3Mesh, UnlitMaterial>();
			eeemesh.Extent.Value = new Vector3f(0.2f);

			var things = DebugStuff.AddChild("Thing");
			things.AttachMeshWithMeshRender<TrivialBox3Mesh, UnlitMaterial>();
			things.position.Value = Vector3f.Up * 5;
			things.AttachComponent<BoxShape>();
			things.AttachComponent<Grabbable>();
			things.AttachComponent<RigidBody>();

			var things2 = DebugStuff.AddChild("Thing");
			things2.AttachMeshWithMeshRender<TrivialBox3Mesh, UnlitMaterial>();
			things2.position.Value = Vector3f.Up * 6;
			things2.AttachComponent<BoxShape>();
			things2.AttachComponent<Grabbable>();
			things2.AttachComponent<RigidBody>();

			var SubviewPortCame = DebugStuff.AddChild("Camera");
			SubviewPortCame.position.Value = new Vector3f(4f, 2f, -2f);
			var subViewPOrtdatae = SubviewPortCame.AttachMeshWithMeshRender<TrivialBox3Mesh, UnlitMaterial>();
			var SubviewPorte = SubviewPortCame.AddChild("SubviewPort");
			var viewporte = SubviewPorte.AttachComponent<Viewport>();
			viewporte.OwnWorld3D.Value = false;
			viewporte.Size.Value *= 2;
			subViewPOrtdatae.Item2.MainTexture.Target = viewporte;
			var cameras = SubviewPorte.AddChild("Camera").AttachComponent<Camera3D>();
			cameras.Entity.rotation.Value = Quaternionf.Yawed180 * Quaternionf.Rolled180;


			var SubviewPortCam = DebugStuff.AddChild("Camera");
			SubviewPortCam.position.Value = new Vector3f(-1f, 1f, 1f);
			var subViewPOrtdata = SubviewPortCam.AttachMeshWithMeshRender<TrivialBox3Mesh, UnlitMaterial>();
			var SubviewPort = SubviewPortCam.AddChild("SubviewPort");
			var ee1 = SubviewPort.AddChild("test");
			ee1.AttachComponent<UIElement>();
			var ee2 = ee1.AddChild("test");
			ee2.AttachComponent<UIElement>();
			var ee3 = ee2.AddChild("test");
			ee3.position.Value = new Vector3f(10, 10, 10);
			ee3.AttachMeshWithMeshRender<Sphere3NormalizedCubeMesh, UnlitMaterial>();


			SubviewPort.position.Value = new Vector3f(-1.5f, 0f, -1f);
			var viewport = SubviewPort.AttachComponent<Viewport>();
			viewport.OwnWorld3D.Value = true;
			subViewPOrtdata.Item2.MainTexture.Target = viewport;

			var camera = SubviewPort.AddChild("Camera");
			var cam = camera.AttachComponent<Camera3D>();
			camera.position.Value = new Vector3f(0, 0, -5);
			camera.rotation.Value = Quaternionf.Yawed180;
			var trauns = SubviewPort.AddChild("trauns");
			trauns.AttachMeshWithMeshRender<Sphere3NormalizedCubeMesh, UnlitMaterial>();
			var e1 = trauns.AddChild("test");
			var e2 = e1.AddChild("test");
			var e3 = e2.AddChild("test");
			var e4 = e3.AddChild("test");
			var testElement = e4.AddChild("test");
			e4.position.Value = new Vector3f(1, 1, 1);
			var e5 = e4.AddChild("test");
			e5.AttachMeshWithMeshRender<Sphere3NormalizedCubeMesh, UnlitMaterial>();



			var TempComps = DebugStuff.AddChild("RenderComps");
			TempComps.position.Value = new Vector3f(0f, 3f, 4f);
			TempComps.AttachComponent<Light>();
			TempComps.AttachComponent<MeshRender>();
			TempComps.AttachComponent<Armature>();
			TempComps.AttachComponent<SkinnedMeshRender>();

			var Mits = DebugStuff.AddChild("DebugStuff");
			Mits.position.Value = new Vector3f(2f, 3f, -2f);
			Mits.scale.Value = new Vector3f(0.25f);
			var unlitmit = Mits.AddChild("Unlit");
			var (_, _, unliotRender) = unlitmit.AttachMeshWithMeshRender<Sphere3NormalizedCubeMesh, UnlitMaterial>();
			unliotRender.CastShadows.Value = ShadowCast.On;
			unliotRender.RecevieShadows.Value = true;
			unlitmit.AttachComponent<Grabbable>();
			unlitmit.AttachComponent<SphereShape>();

			var lights = DebugStuff.AddChild("Lights");
			lights.position.Value = new Vector3f(2f, 4f, -2f);

			var pointLight = lights.AddChild("PointLight");
			var pointLightmesh = pointLight.AttachMeshWithMeshRender<IcosphereMesh, UnlitMaterial>();
			pointLightmesh.Item1.Radius.Value = 0.05f;
			pointLightmesh.Item3.colorLinear.Value = Colorf.Plum;
			pointLight.AttachComponent<Grabbable>();
			pointLight.AttachComponent<SphereShape>().Radius.Value = 0.05f;
			var plight = pointLight.AddChild("Light");
			plight.position.Value = new Vector3f(0f, 0f, 0.1f);
			plight.AttachComponent<Light>().LightType.Value = RLightType.Point;

			var dirLight = lights.AddChild("DirLight");
			dirLight.position.Value = new Vector3f(1f, 0f, 0f);
			var dirLightmesh = dirLight.AttachMeshWithMeshRender<IcosphereMesh, UnlitMaterial>();
			dirLightmesh.Item1.Radius.Value = 0.05f;
			dirLightmesh.Item3.colorLinear.Value = Colorf.Violet;
			dirLight.AttachComponent<Grabbable>();
			dirLight.AttachComponent<SphereShape>().Radius.Value = 0.05f;
			var dlight = dirLight.AddChild("Light");
			dlight.position.Value = new Vector3f(0f, 0f, 0.1f);
			dlight.AttachComponent<Light>().LightType.Value = RLightType.Directional;

			var spotLight = lights.AddChild("SpotLight");
			spotLight.position.Value = new Vector3f(2f, 0f, 0f);
			var spotLightmesh = spotLight.AttachMeshWithMeshRender<IcosphereMesh, UnlitMaterial>();
			spotLightmesh.Item1.Radius.Value = 0.05f;
			spotLightmesh.Item3.colorLinear.Value = Colorf.Beige;
			spotLight.AttachComponent<Grabbable>();
			spotLight.AttachComponent<SphereShape>().Radius.Value = 0.05f;
			var slight = spotLight.AddChild("Light");
			slight.position.Value = new Vector3f(0f, 0f, 0.1f);
			slight.AttachComponent<Light>().LightType.Value = RLightType.Spot;

			var box = floor.AttachComponent<TrivialBox3Mesh>();
			var noTrans = floor.AttachComponent<UnlitMaterial>();
			noTrans.Transparency.Value = Transparency.None;
			var size = 10;
			Entity LastpowerCube = null;
			for (var y = 0; y < size; y++) {
				for (var a = 0; a < size; a++) {
					for (var i = 0; i < size; i++) {
						var PowerCube = DebugStuff.AddChild($"PowerCube{i}{a}{y}");
						PowerCube.position.Value = new Vector3f((i * 0.3f) - (size * 0.15), 0.7f + (y * 0.3f), -0.4f - (a * 0.3f));
						PowerCube.scale.Value = new Vector3f(0.15);
						if (LastpowerCube is not null) {
							PowerCube.SetParent(LastpowerCube);
						}
						AttachRender(box, noTrans, PowerCube, Colorf.RandomHue());
						PowerCube.AttachComponent<BoxShape>();
						PowerCube.AttachComponent<Grabbable>();
						LastpowerCube = PowerCube;
					}
				}
			}

			var testCubes = DebugStuff.AddChild("Test cubes");
			testCubes.position.Value = new Vector3f(2, 0.5f, -2);
			testCubes.scale.Value = new Vector3f(0.5f);

			var fontAtlis = testCubes.AddChild("Font Stuff");
			fontAtlis.AttachComponent<BoxShape>();
			fontAtlis.AttachComponent<Grabbable>();
			var data = fontAtlis.AttachMesh<TrivialBox3Mesh, UnlitMaterial>();

			var text = fontAtlis.AddChild("Text");
			text.position.Value = new Vector3f(0, 1.5f, 0);
			text.AttachComponent<TextLabel3D>();

			var text2 = fontAtlis.AddChild("Text2");
			text2.position.Value = new Vector3f(0, 2.5f, 0);
			var textRender = text2.AttachComponent<TextLabel3D>();
			textRender.Text.Value = "This is another\nBit of Text \nwith Billboard";
			textRender.Billboard.Value = RBillboardOptions.Enabled;
			var text8 = fontAtlis.AddChild("Text8");
			text8.position.Value = new Vector3f(1, 0, 0);
			text8.AttachComponent<TextLabel3D>().Text.Value = "<color=red>Wa<colorblue>Trains<size=50>Trains";
			var textureStuff = testCubes.AddChild("Texture Stuff");
			var dfg = textureStuff.AddChild("DFG-Noise");
			dfg.position.Value = new Vector3f(2, 0, 0);
			var (dfgMesh, dfgMat, dfgRender) = dfg.AttachMeshWithMeshRender<TrivialBox3Mesh, UnlitMaterial>();
			var noiseComp = dfg.AttachComponent<NoiseTexture>();
			dfgMat.MainTexture.Target = noiseComp;
			dfg.AttachComponent<Grabbable>();
			dfg.AttachComponent<BoxShape>();


			var dfg2 = textureStuff.AddChild("DFG-Checker");
			dfg2.position.Value = new Vector3f(4, 0, 0);
			var (dfgMesh2, dfgMat2, dfgRender2) = dfg2.AttachMeshWithMeshRender<TrivialBox3Mesh, UnlitMaterial>();
			var CheckerComp = dfg2.AttachComponent<CheckerboardTexture>();
			dfgMat2.MainTexture.Target = CheckerComp;
			dfg2.AttachComponent<Grabbable>();
			dfg2.AttachComponent<BoxShape>();

			var dfg3 = textureStuff.AddChild("DFG-Voronoi");
			dfg3.position.Value = new Vector3f(6, 0, 0);
			var (dfgMesh3, dfgMat3, dfgRender3) = dfg3.AttachMeshWithMeshRender<TrivialBox3Mesh, UnlitMaterial>();
			var voronoiTexture = dfg3.AttachComponent<VoronoiTexture>();
			voronoiTexture.Tint.Value = Colorf.Magenta;
			voronoiTexture.StartingColor.Value = Colorf.Orange;
			dfgMat3.MainTexture.Target = voronoiTexture;
			dfg3.AttachComponent<Grabbable>();
			dfg3.AttachComponent<BoxShape>();

			var dfg4 = textureStuff.AddChild("DFG-Edge");
			dfg4.position.Value = new Vector3f(8, 0, 0);
			var (dfgMesh4, dfgMat4, dfgRender4) = dfg4.AttachMeshWithMeshRender<TrivialBox3Mesh, UnlitMaterial>();
			var edgeTexture = dfg4.AttachComponent<EdgeTexture>();
			edgeTexture.BackgroundColor.Value = Colorf.Magenta;
			edgeTexture.InnerColor.Value = Colorf.Orange;
			dfgMat4.MainTexture.Target = edgeTexture;
			dfg4.AttachComponent<Grabbable>();
			dfg4.AttachComponent<BoxShape>();

			var CheckSCale = textureStuff.AddChild("CheckSCale");
			CheckSCale.position.Value = new Vector3f(10, 0, 0);
			CheckSCale.scale.Value = new Vector3f(1, 2, 1);

			var CheckSCale2 = CheckSCale.AddChild("CheckSCale");
			CheckSCale2.AttachMeshWithMeshRender<TrivialBox3Mesh, UnlitMaterial>();
			CheckSCale2.AttachComponent<Grabbable>();
			CheckSCale2.AttachComponent<BoxShape>();


			RLog.Info("Built Debug Local World");

#else
			RLog.Info("Built Local World");
#endif
		}


		static readonly Random _random = new();
		static float NextFloat() {
			var buffer = new byte[4];
			_random.NextBytes(buffer);
			return BitConverter.ToSingle(buffer, 0);
		}

		public static void AttachSpiningCubes(Entity root) {
			var speed = 50f;
			var group1 = root.AddChild("group1");
			group1.AttachComponent<Spinner>().speed.Value = new Vector3f(speed, 0, 0);
			var group2 = root.AddChild("group2");
			group2.AttachComponent<Spinner>().speed.Value = new Vector3f(0, speed, 0);
			var group3 = root.AddChild("group3");
			group3.AttachComponent<Spinner>().speed.Value = new Vector3f(0, 0, speed);
			var group4 = root.AddChild("group4");
			group4.AttachComponent<Spinner>().speed.Value = new Vector3f(speed, speed, speed / 2);
			var group5 = root.AddChild("group5");
			group5.AttachComponent<Spinner>().speed.Value = new Vector3f(speed / 2, speed, speed);
			var group6 = root.AddChild("group6");
			group6.AttachComponent<Spinner>().speed.Value = new Vector3f(speed, 0, speed / 2);
			var group11 = root.AddChild("group1");
			group11.AttachComponent<Spinner>().speed.Value = new Vector3f(-speed, 0, 0);
			var group21 = root.AddChild("group2");
			group21.AttachComponent<Spinner>().speed.Value = new Vector3f(0, -speed, 0);
			var group31 = root.AddChild("group3");
			group31.AttachComponent<Spinner>().speed.Value = new Vector3f(0, 0, -speed);
			var group41 = root.AddChild("group4");
			group41.AttachComponent<Spinner>().speed.Value = new Vector3f(-speed, -speed / 2, 0);
			var group51 = root.AddChild("group5");
			group51.AttachComponent<Spinner>().speed.Value = new Vector3f(-speed / 2, -speed, speed);
			var group61 = root.AddChild("group6");
			group61.AttachComponent<Spinner>().speed.Value = new Vector3f(-speed, 0, -speed);


			var boxMesh = root.AttachComponent<TrivialBox3Mesh>();
			boxMesh.Extent.Value = new Vector3f(0.4f, 0.4f, 0.4f);
			var mit = root.AttachComponent<UnlitMaterial>();
			BuildGroup(boxMesh, mit, group1, Colorf.RhubarbGreen);
			BuildGroup(boxMesh, mit, group2, Colorf.RhubarbRed);
			BuildGroup(boxMesh, mit, group3, Colorf.RhubarbGreen);
			BuildGroup(boxMesh, mit, group4, Colorf.RhubarbRed);
			BuildGroup(boxMesh, mit, group5, Colorf.RhubarbGreen);
			BuildGroup(boxMesh, mit, group6, Colorf.RhubarbRed);
			BuildGroup(boxMesh, mit, group11, Colorf.RhubarbGreen);
			BuildGroup(boxMesh, mit, group21, Colorf.RhubarbRed);
			BuildGroup(boxMesh, mit, group31, Colorf.RhubarbGreen);
			BuildGroup(boxMesh, mit, group41, Colorf.RhubarbRed);
			BuildGroup(boxMesh, mit, group51, Colorf.RhubarbGreen);
			BuildGroup(boxMesh, mit, group61, Colorf.RhubarbRed);

		}

		public static void BuildGroup(TrivialBox3Mesh boxMesh, UnlitMaterial mit, Entity entity, Colorf color) {
			for (var i = 0; i < 6; i++) {
				var cubeHolder = entity.AddChild("CubeHolder");
				cubeHolder.rotation.Value = Quaternionf.CreateFromEuler(NextFloat() * 180, NextFloat() * 180, NextFloat() * 180);
				var cube = cubeHolder.AddChild("Cube");
				cube.position.Value = new Vector3f(0, 6, 0);
				cube.scale.Value = new Vector3f(0.5f, 0.5f, 0.5f);
				AttachRender(boxMesh, mit, cube, color);
			}
		}

		public static void AttachRender(TrivialBox3Mesh boxMesh, UnlitMaterial mit, Entity entity, Colorf color) {
			var meshRender = entity.AttachComponent<MeshRender>();
			meshRender.colorLinear.Value = color;
			meshRender.materials.Add().Target = mit;
			meshRender.mesh.Target = boxMesh;
		}
	}
}
