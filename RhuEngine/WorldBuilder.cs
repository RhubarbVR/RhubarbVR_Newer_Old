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
			var (mesh, mit, render) = floor.AttachMeshWithMeshRender<CylinderMesh, UnlitMaterial>();
			mit.Transparency.Value = Transparency.Blend;
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
#if DEBUG || UNITY
			var DebugStuff = floor.AddChild("DebugStuff");
			DebugStuff.position.Value = new Vector3f(-1.5f, 0f, -1f);

			var box = floor.AttachComponent<TrivialBox3Mesh>();
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
						AttachRender(box, mit, PowerCube, Colorf.RandomHue());
						PowerCube.AttachComponent<BoxShape>();
						PowerCube.AttachComponent<Grabbable>();
						LastpowerCube = PowerCube;
					}
				}
			}
			var testCubes = DebugStuff.AddChild("Test cubes");
			testCubes.position.Value = new Vector3f(2, 0.5f,-2);
			testCubes.scale.Value = new Vector3f(0.5f);

			var fontAtlis = testCubes.AddChild("Font Stuff");
			fontAtlis.AttachComponent<BoxShape>();
			fontAtlis.AttachComponent<Grabbable>();
			var data = fontAtlis.AttachMesh<TrivialBox3Mesh, UnlitMaterial>();
			data.Item2.MainTexture.Target = fontAtlis.AttachComponent<FontAtlasTexture>();
			
			var text = fontAtlis.AddChild("Text");
			text.position.Value = new Vector3f(-0.5f, 1.5f, 0);
			text.AttachComponent<WorldText>();

			var textureStuff = testCubes.AddChild("Texture Stuff");
			var dfg = textureStuff.AddChild("DFG-Noise");
			dfg.position.Value = new Vector3f(2, 0, 0);
			var (dfgMesh, dfgMat, dfgRender) = dfg.AttachMeshWithMeshRender<TrivialBox3Mesh, UnlitMaterial>();
			var noiseComp = dfg.AttachComponent<NoiseTexture>();
			dfgMat.MainTexture.Target = noiseComp;
			dfg.AttachComponent<Grabbable>();
			dfg.AttachComponent<BoxShape>();

			var dfg2 = textureStuff.AddChild("DFG-UV");
			dfg2.position.Value = new Vector3f(4, 0, 0);
			var (dfgMesh2, dfgMat2, dfgRender2) = dfg2.AttachMeshWithMeshRender<TrivialBox3Mesh, UnlitMaterial>();
			var UVComp = dfg2.AttachComponent<UVTexture>();
			dfgMat2.MainTexture.Target = UVComp;
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


			//Build Debug Man
			var debugMan = world.RootEntity.AddChild("DebugMan");
			var debugManMit = debugMan.AttachComponent<UnlitMaterial>();
			debugMan.position.Value = new Vector3f(1f, 1.5f, -1f);
			debugMan.scale.Value = new Vector3f(0.2f);

			var debugManBody = debugMan.AddChild("body");
			var bodyRender = debugManBody.AttachMesh<TrivialBox3Mesh>(debugManMit);
			bodyRender.Extent.Value = new Vector3f(1.5f,2f,1f)/2f;
			debugManBody.AttachComponent<BoxShape>().boxHalfExtent.Value = bodyRender.Extent.Value;

			var debugManHead = debugManBody.AddChild("Head");
			debugManHead.position.Value = new Vector3f(0, 2, 0);
			var sphere = debugManHead.AttachMesh<Sphere3NormalizedCubeMesh>(debugManMit);
			debugManHead.AttachComponent<SphereShape>();

			var debugManUpperLeftArm = debugManBody.AddChild("UpperLeftArm");
			debugManUpperLeftArm.position.Value = new Vector3f(-1.6f, .8f, 0);
			var upperLeftArmRender = debugManUpperLeftArm.AttachMesh<TrivialBox3Mesh>(debugManMit);
			upperLeftArmRender.Extent.Value = new Vector3f(1f, .5f, .5f) / 2f;
			debugManUpperLeftArm.AttachComponent<BoxShape>().boxHalfExtent.Value = upperLeftArmRender.Extent.Value;

			var debugManLowerLeftArm = debugManUpperLeftArm.AddChild("LowerLeftArm");
			debugManLowerLeftArm.position.Value = new Vector3f(-1.4f, 0, 0);
			var LowerLeftArmRender = debugManLowerLeftArm.AttachMesh<TrivialBox3Mesh>(debugManMit);
			LowerLeftArmRender.Extent.Value = new Vector3f(1f, .5f, .5f) / 2f;
			debugManLowerLeftArm.AttachComponent<BoxShape>().boxHalfExtent.Value = LowerLeftArmRender.Extent.Value;

			var debugManLeftHand = debugManLowerLeftArm.AddChild("leftHand");
			debugManLeftHand.position.Value = new Vector3f(-.8f, 0, 0);
			var debugManLeftHandRender = debugManLeftHand.AttachMesh<TrivialBox3Mesh>(debugManMit);
			debugManLeftHandRender.Extent.Value = new Vector3f(0.5f, 0.3f, 0.5f) / 2f;
			debugManLeftHand.AttachComponent<BoxShape>().boxHalfExtent.Value = debugManLeftHandRender.Extent.Value;




			var debugManUpperRightArm = debugManBody.AddChild("UpperRightArm");
			debugManUpperRightArm.position.Value = new Vector3f(1.6f, .8f, 0);
			var upperRightArmRender = debugManUpperRightArm.AttachMesh<TrivialBox3Mesh>(debugManMit);
			upperRightArmRender.Extent.Value = new Vector3f(1f, .5f, .5f) / 2f;
			debugManUpperRightArm.AttachComponent<BoxShape>().boxHalfExtent.Value = upperRightArmRender.Extent.Value;

			var debugManLowerRightArm = debugManUpperRightArm.AddChild("LowerRightArm");
			debugManLowerRightArm.position.Value = new Vector3f(1.4f, 0, 0);
			var LowerRightArmRender = debugManLowerRightArm.AttachMesh<TrivialBox3Mesh>(debugManMit);
			LowerRightArmRender.Extent.Value = new Vector3f(1f, .5f, .5f) / 2f;
			debugManLowerRightArm.AttachComponent<BoxShape>().boxHalfExtent.Value = LowerRightArmRender.Extent.Value;

			var debugManRightHand = debugManLowerRightArm.AddChild("RightHand");
			debugManRightHand.position.Value = new Vector3f(.8f, 0, 0);
			var debugManRightHandRender = debugManRightHand.AttachMesh<TrivialBox3Mesh>(debugManMit);
			debugManRightHandRender.Extent.Value = new Vector3f(0.5f, 0.3f, 0.5f) / 2f;
			debugManRightHand.AttachComponent<BoxShape>().boxHalfExtent.Value = debugManRightHandRender.Extent.Value;

			var debugManUpperLeftLeg = debugManBody.AddChild("UpperLeftLeg");
			debugManUpperLeftLeg.position.Value = new Vector3f(-.6f, -2.1f, 0);
			var upperLeftLegRender = debugManUpperLeftLeg.AttachMesh<TrivialBox3Mesh>(debugManMit);
			upperLeftLegRender.Extent.Value = new Vector3f(.5f, 1.3f, .5f) / 2f;
			debugManUpperLeftLeg.AttachComponent<BoxShape>().boxHalfExtent.Value = upperLeftLegRender.Extent.Value;

			var debugManLowerLeftLeg = debugManUpperLeftLeg.AddChild("LowerLeftLeg");
			debugManLowerLeftLeg.position.Value = new Vector3f(0, -1.7f, 0);
			var lowerLeftLegRender = debugManLowerLeftLeg.AttachMesh<TrivialBox3Mesh>(debugManMit);
			lowerLeftLegRender.Extent.Value = new Vector3f(.5f, 1.3f, .5f) / 2f;
			debugManLowerLeftLeg.AttachComponent<BoxShape>().boxHalfExtent.Value = lowerLeftLegRender.Extent.Value;

			var debugManLeftFoot = debugManLowerLeftLeg.AddChild("LeftFoot");
			debugManLeftFoot.position.Value = new Vector3f(0, -0.9f, 0.25f);
			var leftFootRender = debugManLeftFoot.AttachMesh<TrivialBox3Mesh>(debugManMit);
			leftFootRender.Extent.Value = new Vector3f(.5f, .4f, 1) / 2f;
			debugManLeftFoot.AttachComponent<BoxShape>().boxHalfExtent.Value = leftFootRender.Extent.Value;

			var debugManUpperRightLeg = debugManBody.AddChild("UpperRightLeg");
			debugManUpperRightLeg.position.Value = new Vector3f(.6f, -2.1f, 0);
			var upperRightLegRender = debugManUpperRightLeg.AttachMesh<TrivialBox3Mesh>(debugManMit);
			upperRightLegRender.Extent.Value = new Vector3f(.5f, 1.3f, .5f) / 2f;
			debugManUpperRightLeg.AttachComponent<BoxShape>().boxHalfExtent.Value = upperRightLegRender.Extent.Value;

			var debugManLowerRightLeg = debugManUpperRightLeg.AddChild("LowerRightLeg");
			debugManLowerRightLeg.position.Value = new Vector3f(0, -1.7f, 0);
			var lowerRightLegRender = debugManLowerRightLeg.AttachMesh<TrivialBox3Mesh>(debugManMit);
			lowerRightLegRender.Extent.Value = new Vector3f(.5f, 1.3f, .5f) / 2f;
			debugManLowerRightLeg.AttachComponent<BoxShape>().boxHalfExtent.Value = lowerRightLegRender.Extent.Value;

			var debugManRightFoot = debugManLowerRightLeg.AddChild("RightFoot");
			debugManRightFoot.position.Value = new Vector3f(0, -0.9f, 0.25f);
			var RightFootRender = debugManRightFoot.AttachMesh<TrivialBox3Mesh>(debugManMit);
			RightFootRender.Extent.Value = new Vector3f(.5f, .4f, 1) / 2f;
			debugManRightFoot.AttachComponent<BoxShape>().boxHalfExtent.Value = RightFootRender.Extent.Value;

			//IKloadeding
			var IkManager = debugMan.AttachComponent<IKManager>();

			var bodyBone = debugManBody.AttachComponent<IKBone>();
			bodyBone.Radius.Value = .75f;
			bodyBone.Height.Value = 2f;
			var headBone = debugManHead.AttachComponent<IKBone>();
			headBone.Radius.Value = 0.4f;
			headBone.Height.Value = 0.8f;
			
			var upperLeftArmBone = debugManUpperLeftArm.AttachComponent<IKBone>();
			debugManUpperLeftArm.rotation.Value = Quaternionf.AxisAngleR(new Vector3f(0, 0, 0.5f), MathUtil.HALF_P_IF);
			var lowerLeftArmBone = debugManLowerLeftArm.AttachComponent<IKBone>();
			var upperRightArmBone = debugManUpperRightArm.AttachComponent<IKBone>();
			debugManUpperRightArm.rotation.Value = Quaternionf.AxisAngleR(new Vector3f(0, 0, -0.5f), MathUtil.HALF_P_IF);
			var lowerRightArmBone = debugManLowerRightArm.AttachComponent<IKBone>();

			var leftHandBone = debugManLeftHand.AttachComponent<IKBone>();
			leftHandBone.Height.Value = .5f;
			leftHandBone.Radius.Value = .2f;
			var rightHandBone = debugManRightHand.AttachComponent<IKBone>();
			rightHandBone.Height.Value = .5f;
			rightHandBone.Radius.Value = .2f;


			var upperLeftLegBone = debugManUpperLeftLeg.AttachComponent<IKBone>();
			upperLeftLegBone.Height.Value = 1.3f;
			var lowerLeftLegBone = debugManLowerLeftLeg.AttachComponent<IKBone>();
			lowerLeftLegBone.Height.Value = 1.3f;

			var leftFootBone = debugManLeftFoot.AttachComponent<IKBone>();
			leftFootBone.Height.Value = 1f;
			debugManLeftFoot.rotation.Value = Quaternionf.AxisAngleR(new Vector3f(-0.5f,0,0), MathUtil.HALF_P_IF);

			var upperRightLegBone = debugManUpperRightLeg.AttachComponent<IKBone>();
			upperRightLegBone.Height.Value = 1.3f;
			var lowerRightLegBone = debugManLowerRightLeg.AttachComponent<IKBone>();
			lowerRightLegBone.Height.Value = 1.3f;

			var rightFootBone = debugManRightFoot.AttachComponent<IKBone>();
			rightFootBone.Height.Value = 1f;
			debugManRightFoot.rotation.Value = Quaternionf.AxisAngleR(new Vector3f(-0.5f, 0, 0), MathUtil.HALF_P_IF);

			rightFootBone.MoveMentSpace.Target = leftFootBone.MoveMentSpace.Target = debugMan;
			lowerRightLegBone.MoveMentSpace.Target = lowerLeftLegBone.MoveMentSpace.Target = debugMan;
			upperRightLegBone.MoveMentSpace.Target = upperLeftLegBone.MoveMentSpace.Target = debugMan;


			leftHandBone.MoveMentSpace.Target = rightHandBone.MoveMentSpace.Target = debugMan;
			lowerLeftArmBone.MoveMentSpace.Target = lowerRightArmBone.MoveMentSpace.Target = debugMan;
			upperLeftArmBone.MoveMentSpace.Target = upperRightArmBone.MoveMentSpace.Target = debugMan;
			headBone.MoveMentSpace.Target = debugMan;
			bodyBone.MoveMentSpace.Target = debugMan;



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
