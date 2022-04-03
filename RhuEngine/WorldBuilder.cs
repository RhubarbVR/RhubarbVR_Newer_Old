using System;

using RhuEngine.Components;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.Components.ScriptNodes;
using System.Collections.Generic;
using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine
{
	public static class WorldBuilder
	{
		public static void BuildUITest(Entity entity) {
			entity.position.Value = new Vector3f(0, 1, 0);
			var pannel = entity.AddChild("PannelRoot");
			// TODO add back with ui
			//pannel.AttachComponent<UIWindow>();
			//var button = pannel.AddChild("Button").AttachComponent<UIButton>();
			var script = pannel.AttachComponent<RhuScript>();
			//button.onClick.Target = script.CallMainMethod;
			//Hello World with number
			//var method = ScriptNodeBuidlers.GetScriptNodes(typeof(RhuScript))[0].GetNodeMethods("InfoLog")[0];
			//var tostring = ScriptNodeBuidlers.GetNodeMethods(typeof(RhuScriptStatics), "ToString")[0];
			//method.Prams[0] = tostring;
			//tostring.Prams[0] = new ScriptNodeConst(10);
			//normal hello world
			var method = ScriptNodeBuidlers.GetScriptNodes(typeof(RhuScript))[0].GetNodeMethods("InfoLog")[0];
			method.Prams[0] = new ScriptNodeConst("Hi there is has been changed");
			//Hello Word
			//var method = ScriptNodeBuidlers.GetScriptNodes(typeof(RhuScript))[0].GetNodeMethods("InfoLog")[0];
			//Test for stack overflow
			//var method = ScriptNodeBuidlers.GetScriptNodes(typeof(RhuScript))[0].GetNodeMethods("CallMainMethod")[0];
			//Test for fields
			//var method = ScriptNodeBuidlers.GetScriptNodes(typeof(RhuScript))[0].GetNodeMethods("InfoLog")[0];
			//var tostring = ScriptNodeBuidlers.GetNodeMethods(typeof(RhuScriptStatics), "ToString")[0];
			//method.Prams[0] = tostring;
			//tostring.Prams[0] = ScriptNodeBuidlers.GetScriptNodes(typeof(RhuScript))[0].GetNodeFieldsRead()[0];
			//var method = ScriptNodeBuidlers.GetScriptNodes(typeof(RhuScript))[0].GetNodeMethods("InfoLog")[0];
			//var tostring = ScriptNodeBuidlers.GetNodeMethods(typeof(RhuScriptStatics), "ToString")[0];
			//method.Prams[0] = tostring;
			//var inc = 2;
			//var add = ScriptNodeBuidlers.GetNodeMethods(typeof(RhuScriptStatics), "Add")[0];
			//inc++;
			//var e = AddNodeSpawn(new ScriptNodeMethod[1] {add},ref inc);
			//var length = 8;
			//for (var i = 0; i < length; i++) {
			//	e = AddNodeSpawn(e, ref inc);
			//}
			//Console.WriteLine("Nodes" + e.Length);
			//foreach (var item in e) {
			//	item.Prams[0] = new ScriptNodeConst(1);
			//	inc++;
			//	item.Prams[1] = new ScriptNodeConst(1);
			//	inc++;
			//}
			//Console.WriteLine($"Nodes are {inc}");
			//tostring.Prams[0] = add;
			script.MainMethod = method;
			var ScripEditor = entity.AddChild("ScripEditor");
			ScripEditor.position.Value = new Vector3f(0, -0.1f, 0);
			// TODO add back with ui
			//var VisualScriptBuilder = ScripEditor.AttachComponent<VisualScriptBuilder>();
			//VisualScriptBuilder.script.Target = script;
		}

		public static ScriptNodeMethod[] AddNodeSpawn(ScriptNodeMethod[] scriptNodeMethod,ref int inc) {
			var e = new List<ScriptNodeMethod>();
			foreach (var item in scriptNodeMethod) {
				item.Prams[0] = ScriptNodeBuidlers.GetNodeMethods(typeof(RhuScriptStatics), "Add")[0];
				inc++;
				item.Prams[1] = ScriptNodeBuidlers.GetNodeMethods(typeof(RhuScriptStatics), "Add")[0];
				inc++;
				e.Add((ScriptNodeMethod)item.Prams[0]);
				e.Add((ScriptNodeMethod)item.Prams[1]);
			}
			return e.ToArray();
		}

		public static void BuildLocalWorld(this World world) {
			RLog.Info("Building Local World");
			BuildUITest(world.RootEntity.AddChild("UITest"));
			var floor = world.RootEntity.AddChild("Floor");
			floor.position.Value = new Vector3f(0, 0, 0);
			var (mesh, _, render) = floor.AttachMeshWithMeshRender<TrivialBox3Mesh, UnlitShader>();
			render.colorLinear.Value = new Colorf(0.9f, 0.9f, 0.9f);
			mesh.Extent.Value = new Vector3f(10, 0.01f, 10);
			var spinningCubes = world.RootEntity.AddChild("SpinningCubes");
			spinningCubes.position.Value = new Vector3f(0, 0.5f, 0);
			AttachSpiningCubes(spinningCubes);
			RLog.Info("Built Local World");
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


			var shader = root.GetFirstComponentOrAttach<UnlitClipShader>();
			var boxMesh = root.AttachComponent<TrivialBox3Mesh>();
			boxMesh.Extent.Value = new Vector3f(0.4f, 0.4f, 0.4f);
			var mit = root.AttachComponent<DynamicMaterial>();
			mit.shader.Target = shader;
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

		public static void BuildGroup(TrivialBox3Mesh boxMesh, DynamicMaterial mit, Entity entity, Colorf color) {
			for (var i = 0; i < 6; i++) {
				var cubeHolder = entity.AddChild("CubeHolder");
				cubeHolder.rotation.Value = Quaternionf.CreateFromEuler(NextFloat() * 180, NextFloat() * 180, NextFloat() * 180);
				var cube = cubeHolder.AddChild("Cube");
				cube.position.Value = new Vector3f(0, 2, 0);
				cube.scale.Value = new Vector3f(0.5f, 0.5f, 0.5f);
				AttachRender(boxMesh, mit, cube, color);
			}
		}

		public static void AttachRender(TrivialBox3Mesh boxMesh, DynamicMaterial mit, Entity entity, Colorf color) {
			var meshRender = entity.AttachComponent<MeshRender>();
			meshRender.colorLinear.Value = color;
			meshRender.materials.Add().Target = mit;
			meshRender.mesh.Target = boxMesh;
		}
	}
}
