using System;

using RhuEngine.Components;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.Components.ScriptNodes;
using System.Collections.Generic;
using RNumerics;
using RhuEngine.Linker;
using System.Threading;

namespace RhuEngine
{
	public static class WorldBuilder
	{
		private static Entity AttachImage(this Entity parrent, IAssetProvider<RShader> shader, Vector2f min, Vector2f max, Colorf color,IAssetProvider<RTexture2D> assetProvider) {
			var child = parrent.AddChild("childEliment");
			var rectTwo = child.AttachComponent<UIRect>();
			rectTwo.AnchorMin.Value = min;
			rectTwo.AnchorMax.Value = max;
			var img = child.AttachComponent<UIImage>();
			img.Tint.Value = color;
			var mit = child.AttachComponent<DynamicMaterial>();
			mit.shader.Target = shader;
			mit.SetPram("diffuse", assetProvider);
			img.Material.Target = mit;
			img.Texture.Target = assetProvider;
			return child;
		}

		private static Entity AttachText(this Entity parrent, Vector2f min, Vector2f max, Colorf color,string text) {
			var child = parrent.AddChild("childEliment");
			var rectTwo = child.AttachComponent<UIRect>();
			rectTwo.AnchorMin.Value = min;
			rectTwo.AnchorMax.Value = max;
			var img = child.AttachComponent<UIText>();
			img.Text.Value = text;
			img.StartingColor.Value = color;
			return child;
		}
		private static Entity AttachList(this Entity parrent, DynamicMaterial mit, Vector2f min, Vector2f max, Colorf color) {
			var child = parrent.AddChild("Cut");
			var rectTwo = child.AttachComponent<CuttingUIRect>();
			rectTwo.AnchorMin.Value = min;
			rectTwo.AnchorMax.Value = max;
			var img = child.AttachComponent<UIRectangle>();
			img.Tint.Value = color;
			img.Material.Target = mit;
			var list = child.AddChild("List");
			var scroll = list.AttachComponent<UIScrollInteraction>();
			var rectthrere = list.AttachComponent<VerticalList>();
			rectthrere.Depth.Value = 0f;
			scroll.OnScroll.Target = rectthrere.Scroll;
			return list;
		}
		private static Entity AttachRectangle(this Entity parrent, DynamicMaterial mit,Vector2f min, Vector2f max, Colorf color) {
			var child = parrent.AddChild("childEliment");
			var rectTwo = child.AttachComponent<UIRect>();
			rectTwo.AnchorMin.Value = min;
			rectTwo.AnchorMax.Value = max;
			var img = child.AttachComponent<UIRectangle>();
			img.Tint.Value = color;
			img.Material.Target = mit;
			return child;
		}
		public static void BuildUITest(Entity entity) {
			entity.position.Value = new Vector3f(0, 1, 0);
			var pannel = entity.AddChild("PannelRoot");
			var shader = entity.World.RootEntity.GetFirstComponentOrAttach<UnlitClipShader>();
			var mit = pannel.AttachComponent<DynamicMaterial>();
			mit.shader.Target = shader;
			var rectone = pannel.AttachComponent<UIRect>();
			var canvas = pannel.AttachComponent<UICanvas>();
			var rectTwo = pannel.AttachComponent<UIRect>();
			var img = pannel.AttachComponent<UIRectangle>();
			img.Tint.Value = Colorf.Black;
			img.Material.Target = mit;
			var texture = pannel.AttachComponent<StaticTexture>();
			texture.url.Value = "https://cdn.discordapp.com/attachments/222386596236886026/963193866645831680/Screenshot_20220411-234823_Reddit.jpg";
			////pannel.AttachRectangle(mit, new Vector2f(0.25f), new Vector2f(0.75f), Colorf.Blue)
			////.AttachRectangle(mit, new Vector2f(0.25f), new Vector2f(0.75f), Colorf.Red)
			//////.AttachRectangle(mit, new Vector2f(0.25f), new Vector2f(0.75f), Colorf.Yellow)
			//////.AttachRectangle(mit, new Vector2f(0.25f), new Vector2f(0.75f), Colorf.Gold)
			//////.AttachRectangle(mit, new Vector2f(0.25f), new Vector2f(0.75f), Colorf.Green)
			//////.AttachRectangle(mit, new Vector2f(0.25f), new Vector2f(0.75f), Colorf.Grey)
			////.AttachImage(shader, new Vector2f(0.25f), new Vector2f(0.75f), Colorf.White, texture);
			///
			var rect = pannel.AttachRectangle(mit, new Vector2f(0.25f, 0f), new Vector2f(1f), Colorf.RhubarbGreen);
			var button = rect.AttachRectangle(mit, new Vector2f(0.25f), new Vector2f(0.75f), Colorf.RhubarbRed);
			button.AttachText(Vector2f.Zero, Vector2f.One, Colorf.White, "<colorred>Button<color=blue><size=20> Can go clicky<color=yellow> clicky<size5> So Click");
			button.AttachComponent<UIButtonInteraction>();
			var listRoot = pannel.AttachList(mit, new Vector2f(0f), new Vector2f(0.25f, 1f), Colorf.Blue);
			void AttachListElement(Entity root,string text) {
				var e = root.AttachRectangle(mit, new Vector2f(0f, 0f), new Vector2f(1f, 0.2f), Colorf.RhubarbRed);
				e.AttachImage(shader, new Vector2f(0.5, 0), new Vector2f(1f), Colorf.White, texture);
				e.AttachText(new Vector2f(0), new Vector2f(0.5f, 1), Colorf.White,text);
			}
			for (var i = 0; i < 15; i++) {
				AttachListElement(listRoot,$"Element {i}");
			}
			listRoot.GetFirstComponent<UIRect>().RegUpdateUIMeshes();
			//pannel.AttachRectangle(mit, new Vector2f(0.25f), new Vector2f(0.75f), Colorf.Red);
			//// TODO add back with ui
			////pannel.AttachComponent<UIWindow>();
			////var button = pannel.AddChild("Button").AttachComponent<UIButton>();
			//var script = pannel.AttachComponent<RhuScript>();
			////button.onClick.Target = script.CallMainMethod;
			////Hello World with number
			////var method = ScriptNodeBuidlers.GetScriptNodes(typeof(RhuScript))[0].GetNodeMethods("InfoLog")[0];
			////var tostring = ScriptNodeBuidlers.GetNodeMethods(typeof(RhuScriptStatics), "ToString")[0];
			////method.Prams[0] = tostring;
			////tostring.Prams[0] = new ScriptNodeConst(10);
			////normal hello world
			//var method = ScriptNodeBuidlers.GetScriptNodes(typeof(RhuScript))[0].GetNodeMethods("InfoLog")[0];
			//method.Prams[0] = new ScriptNodeConst("Hi there is has been changed");
			////Hello Word
			////var method = ScriptNodeBuidlers.GetScriptNodes(typeof(RhuScript))[0].GetNodeMethods("InfoLog")[0];
			////Test for stack overflow
			////var method = ScriptNodeBuidlers.GetScriptNodes(typeof(RhuScript))[0].GetNodeMethods("CallMainMethod")[0];
			////Test for fields
			////var method = ScriptNodeBuidlers.GetScriptNodes(typeof(RhuScript))[0].GetNodeMethods("InfoLog")[0];
			////var tostring = ScriptNodeBuidlers.GetNodeMethods(typeof(RhuScriptStatics), "ToString")[0];
			////method.Prams[0] = tostring;
			////tostring.Prams[0] = ScriptNodeBuidlers.GetScriptNodes(typeof(RhuScript))[0].GetNodeFieldsRead()[0];
			////var method = ScriptNodeBuidlers.GetScriptNodes(typeof(RhuScript))[0].GetNodeMethods("InfoLog")[0];
			////var tostring = ScriptNodeBuidlers.GetNodeMethods(typeof(RhuScriptStatics), "ToString")[0];
			////method.Prams[0] = tostring;
			////var inc = 2;
			////var add = ScriptNodeBuidlers.GetNodeMethods(typeof(RhuScriptStatics), "Add")[0];
			////inc++;
			////var e = AddNodeSpawn(new ScriptNodeMethod[1] {add},ref inc);
			////var length = 8;
			////for (var i = 0; i < length; i++) {
			////	e = AddNodeSpawn(e, ref inc);
			////}
			////Console.WriteLine("Nodes" + e.Length);
			////foreach (var item in e) {
			////	item.Prams[0] = new ScriptNodeConst(1);
			////	inc++;
			////	item.Prams[1] = new ScriptNodeConst(1);
			////	inc++;
			////}
			////Console.WriteLine($"Nodes are {inc}");
			////tostring.Prams[0] = add;
			//script.MainMethod = method;
			//var ScripEditor = entity.AddChild("ScripEditor");
			//ScripEditor.position.Value = new Vector3f(0, -0.1f, 0);
			//// TODO add back with ui
			////var VisualScriptBuilder = ScripEditor.AttachComponent<VisualScriptBuilder>();
			////VisualScriptBuilder.script.Target = script;
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
