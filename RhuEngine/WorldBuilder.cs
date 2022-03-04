using System;

using RhuEngine.Components;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

using World = RhuEngine.WorldObjects.World;
using RhuEngine.Components.ScriptNodes;
namespace RhuEngine
{
	public static class WorldBuilder
	{
		public static void BuildUITest(Entity entity) {
			entity.position.Value = new Vec3(0, 1, 0);
			var pannel = entity.AddChild("PannelRoot");
			pannel.AttachComponent<UIWindow>();
			var button = pannel.AddChild("Button").AttachComponent<UIButton>();
			var script = pannel.AttachComponent<RhuScript>();
			button.onClick.Target = script.CallMainMethod;
			//Hello World with number
			var method = ScriptNodeBuidlers.GetScriptNodes(typeof(RhuScript))[0].GetNodeMethods("InfoLog")[0];
			var tostring = ScriptNodeBuidlers.GetNodeMethods(typeof(RhuScriptStatics), "ToString")[0];
			method.Prams[0] = tostring;
			tostring.Prams[0] = new ScriptNodeConst(10);
			//normal hello world
			//var method = ScriptNodeBuidlers.GetScriptNodes(typeof(RhuScript))[0].GetNodeMethods("InfoLog")[0];
			//method.Prams[0] = new ScriptNodeConst("Hi there is has been changed");
			//Hello Word
			//var method = ScriptNodeBuidlers.GetScriptNodes(typeof(RhuScript))[0].GetNodeMethods("InfoLog")[0];
			//Test for stack overflow
			//var method = ScriptNodeBuidlers.GetScriptNodes(typeof(RhuScript))[0].GetNodeMethods("CallMainMethod")[0];
			script.MainMethod = method;
			var ScripEditor = entity.AddChild("ScripEditor");
			ScripEditor.position.Value = new Vec3(0, -0.1f, 0);
			var VisualScriptBuilder = ScripEditor.AttachComponent<VisualScriptBuilder>();
			VisualScriptBuilder.script.Target = script;
		}

		public static void BuildLocalWorld(this World world) {
			Log.Info("Building Local World");
			BuildUITest(world.RootEntity.AddChild("UITest"));
			var picMesh = world.RootEntity.AddChild("Floor");
			picMesh.position.Value = new Vec3(0, 0.25f, -0.5f);
			picMesh.rotation.Value = Quat.FromAngles(90, 0, 0);
			picMesh.scale.Value = new Vec3(3);
			var (pmesh, mit, prender) = picMesh.AttachMeshWithMeshRender<PlaneMesh, UnlitShader>();
			var textur = picMesh.AttachComponent<StaticTexture>();
			textur.url.Value = "https://cataas.com/cat/says/Base%20Url%20For%20RhubarbVR";
			mit.faceCull.Value = Cull.None;
			mit.SetPram("diffuse", textur);
			var floor = world.RootEntity.AddChild("Floor");
			floor.position.Value = new Vec3(0, 0, 0);
			var (mesh, _, render) = floor.AttachMeshWithMeshRender<CubeMesh, PBRShader>();
			render.colorLinear.Value = new Color(0.9f, 0.9f, 0.9f);
			mesh.dimensions.Value = new Vec3(10, 0.01f, 10);
			var spinningCubes = world.RootEntity.AddChild("SpinningCubes");
			spinningCubes.position.Value = new Vec3(0, 0.5f, 0);
			AttachSpiningCubes(spinningCubes, new Color(0.3f, 0.3f, 0.3f, 0.6f));
			Log.Info("Built Local World");
		}


		static readonly Random _random = new();
		static float NextFloat() {
			var buffer = new byte[4];
			_random.NextBytes(buffer);
			return BitConverter.ToSingle(buffer, 0);
		}

		public static void AttachSpiningCubes(Entity root, Color color) {
			var speed = 50f;
			var group1 = root.AddChild("group1");
			group1.AttachComponent<Spinner>().speed.Value = new Vec3(speed, 0, 0);
			var group2 = root.AddChild("group2");
			group2.AttachComponent<Spinner>().speed.Value = new Vec3(0, speed, 0);
			var group3 = root.AddChild("group3");
			group3.AttachComponent<Spinner>().speed.Value = new Vec3(0, 0, speed);
			var group4 = root.AddChild("group4");
			group4.AttachComponent<Spinner>().speed.Value = new Vec3(speed, speed, speed / 2);
			var group5 = root.AddChild("group5");
			group5.AttachComponent<Spinner>().speed.Value = new Vec3(speed / 2, speed, speed);
			var group6 = root.AddChild("group6");
			group6.AttachComponent<Spinner>().speed.Value = new Vec3(speed, 0, speed / 2);
			var group11 = root.AddChild("group1");
			group11.AttachComponent<Spinner>().speed.Value = new Vec3(-speed, 0, 0);
			var group21 = root.AddChild("group2");
			group21.AttachComponent<Spinner>().speed.Value = new Vec3(0, -speed, 0);
			var group31 = root.AddChild("group3");
			group31.AttachComponent<Spinner>().speed.Value = new Vec3(0, 0, -speed);
			var group41 = root.AddChild("group4");
			group41.AttachComponent<Spinner>().speed.Value = new Vec3(-speed, -speed / 2, 0);
			var group51 = root.AddChild("group5");
			group51.AttachComponent<Spinner>().speed.Value = new Vec3(-speed / 2, -speed, speed);
			var group61 = root.AddChild("group6");
			group61.AttachComponent<Spinner>().speed.Value = new Vec3(-speed, 0, -speed);


			var shader = root.GetFirstComponentOrAttach<UnlitClipShader>();
			var boxMesh = root.AttachComponent<CubeMesh>();
			boxMesh.dimensions.Value = new Vec3(0.4f, 0.4f, 0.4f);
			var mit = root.AttachComponent<DynamicMaterial>();
			mit.shader.Target = shader;
			BuildGroup(boxMesh, mit, group1, color);
			BuildGroup(boxMesh, mit, group2, color);
			BuildGroup(boxMesh, mit, group3, color);
			BuildGroup(boxMesh, mit, group4, color);
			BuildGroup(boxMesh, mit, group5, color);
			BuildGroup(boxMesh, mit, group6, color);
			BuildGroup(boxMesh, mit, group11, color);
			BuildGroup(boxMesh, mit, group21, color);
			BuildGroup(boxMesh, mit, group31, color);
			BuildGroup(boxMesh, mit, group41, color);
			BuildGroup(boxMesh, mit, group51, color);
			BuildGroup(boxMesh, mit, group61, color);

		}

		public static void BuildGroup(CubeMesh boxMesh, DynamicMaterial mit, Entity entity, Color color) {
			for (var i = 0; i < 6; i++) {
				var cubeHolder = entity.AddChild("CubeHolder");
				cubeHolder.rotation.Value = Quat.FromAngles(NextFloat() * 180, NextFloat() * 180, NextFloat() * 180);
				var cube = cubeHolder.AddChild("Cube");
				cube.position.Value = new Vec3(0, 2, 0);
				cube.scale.Value = new Vec3(0.5f, 0.5f, 0.5f);
				AttachRender(boxMesh, mit, cube, color);
			}
		}

		public static void AttachRender(CubeMesh boxMesh, DynamicMaterial mit, Entity entity, Color color) {
			var meshRender = entity.AttachComponent<MeshRender>();
			meshRender.colorLinear.Value = color;
			meshRender.materials.Add().Target = mit;
			meshRender.mesh.Target = boxMesh;
		}
	}
}
