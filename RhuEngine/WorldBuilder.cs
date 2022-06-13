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

		public static void BuildLocalWorld(this World world) {
			RLog.Info("Building Local World");
			var floor = world.RootEntity.AddChild("Floor");
			floor.position.Value = new Vector3f(0, 0, 0);

			
			//Todo: fix problem with RigidBody
			//var rigbody = PowerCube.AttachComponent<RigidBody>();
			//rigbody.PhysicsObject.Target = boxshape;
			var coloider = floor.AttachComponent<CylinderShape>();
			var (mesh, mit, render) = floor.AttachMeshWithMeshRender<CylinderMesh, UnlitShader>();
			mit.Transparency = Transparency.Blend;
			var colorFollower = floor.AttachComponent<UIColorAssign>();
			colorFollower.Alpha.Value = 0.75f;
			colorFollower.Color.Value = UIColorAssign.ColorSelection.Primary;
			colorFollower.ColorShif.Value = 0.1f;
			colorFollower.TargetColor.Target = render.colorLinear;
			mesh.TopRadius.Value = 4;
			mesh.BaseRadius.Value = 3.5f;
			mesh.Height.Value = 0.25f;
			coloider.boxHalfExtent.Value = new Vector3d(8, 0.25f, 8)/2;
			var spinningCubes = world.RootEntity.AddChild("SpinningCubes");
			spinningCubes.position.Value = new Vector3f(0, 0.5f, 0);
			AttachSpiningCubes(spinningCubes);
			var box = floor.AttachComponent<TrivialBox3Mesh>();
			var size = 10;
			Entity LastpowerCube = null;
			for (var y = 0; y < size; y++) {
				for (var a = 0; a < size; a++) {
					for (var i = 0; i < size; i++) {
						var PowerCube = world.RootEntity.AddChild($"PowerCube{i}{a}{y}");
						PowerCube.position.Value = new Vector3f((i * 0.3f) - (size * 0.15), 1.7f + (y * 0.3f), -0.4f - (a * 0.3f));
						PowerCube.scale.Value = new Vector3f(0.15);
						if (LastpowerCube is not null) {
							PowerCube.SetParent(LastpowerCube);
						}
						AttachRender(box, mit, PowerCube, Colorf.RandomHue());
						var boxshape = PowerCube.AttachComponent<BoxShape>();
						PowerCube.AttachComponent<Grabbable>();
						LastpowerCube = PowerCube;
					}
				}
			}
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
				cube.position.Value = new Vector3f(0, 6, 0);
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
