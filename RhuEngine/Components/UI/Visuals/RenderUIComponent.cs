﻿using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using System.Collections.Generic;
using RhuEngine.Physics;

namespace RhuEngine.Components
{
	public abstract class RenderUIComponent : UIComponent
	{
		public SimpleMesh MainMesh { get; set; }
		public SimpleMesh ScrollMesh { get; set; }
		public SimpleMesh CutMesh { get; set; }
		public SimpleMesh RenderMesh { get; set; }

		public abstract RMaterial RenderMaterial { get; }
		public abstract Colorf RenderTint { get; }

		public abstract bool HasPhysics { get; }

		public Vector3f PhysicsPose;

		public RigidBodyCollider PhysicsCollider;

		public abstract void ProcessBaseMesh();

		public void ProcessMesh() {
			ProcessBaseMesh();
			Entity.UIRect?.UpdateMeshes();
		}

		public override void RenderTargetChange() {
			ProcessBaseMesh();
		}
		public override void Render(Matrix matrix) {
			//Never Runs
		}

		public void RenderScrollMesh(bool updateMesh = true) {
			if (MainMesh is null) {
				ProcessBaseMesh();
			}
			if (Rect.ScrollOffset == Vector3f.Zero) {
				ScrollMesh = MainMesh;
			}
			else {
				var scrollMesh = new SimpleMesh(MainMesh);
				scrollMesh.Translate(Rect.ScrollOffset);
				ScrollMesh = scrollMesh;
			}
			if (updateMesh) {
				Rect.UpdateMeshes();
			}
		}

		private bool _isCut;

		public override void CutElement(bool cut,bool update = true) {
			_isCut = cut;
			RenderCutMesh(update, update);
		}
		private void RunLoadPhysicsBox() {
			PhysicsCollider?.Remove();
			PhysicsCollider = null;
			if (Rect.Culling) {
				return;
			}
			var truemin = Rect.Min + Rect.ScrollOffset.Xy;
			var truemax = Rect.Max + Rect.ScrollOffset.Xy;
			var min = MathUtil.Max(Rect.CutZonesMin, truemin);
			var max = MathUtil.Min(Rect.CutZonesMax,truemax);
			var size = max - min;
			var collidersize = new Vector3d(size.x, size.y, Rect.Depth.Value) / 2;
			var pos = min + collidersize.Xy;
			PhysicsPose = new Vector3f((float)pos.x, (float)pos.y, Rect.StartPoint + (float)collidersize.z);
			PhysicsCollider = new RBoxShape(collidersize).GetCollider(World.PhysicsSim);
			World.DrawDebugCube(Rect.LastRenderPos,PhysicsPose, collidersize,new Colorf(0,1,1,0.2f),3); //For testing collider
			PhysicsCollider.CustomObject = this;
			PhysicsCollider.Group = ECollisionFilterGroups.UI;
			PhysicsCollider.Mask = ECollisionFilterGroups.UI;
			PhysicsCollider.Active = true;
		}
		private void RunLoadPhysicsMesh() {
			PhysicsCollider?.Remove();
			PhysicsCollider = null;
			PhysicsPose = Vector3f.Zero;
			PhysicsCollider = new RConvexMeshShape(CutMesh).GetCollider(World.PhysicsSim);
			PhysicsCollider.CustomObject = this;
			PhysicsCollider.Group = ECollisionFilterGroups.UI;
			PhysicsCollider.Mask = ECollisionFilterGroups.UI;
			PhysicsCollider.Active = true;
		}
		public void LoadPhysicsMesh() {
			RWorld.ExecuteOnEndOfFrame(BoxBased ? RunLoadPhysicsBox : RunLoadPhysicsMesh);
		}

		public virtual bool BoxBased => true;

		public void RenderMainMesh(bool updateMesh = true, bool PhysicsMesh = true) {
			var returnMesh = CutMesh;
			if (Rect.Canvas.TopOffset.Value) {
				returnMesh = returnMesh.Copy();
				returnMesh.OffsetTop(Rect.Canvas.TopOffsetValue.Value);
			}
			if (Rect.Canvas.FrontBind.Value) {
				returnMesh = returnMesh.UIBind(Rect.Canvas.FrontBindDist.Value, Rect.Canvas.FrontBindSegments.Value);
			}
			RenderMesh = returnMesh;
			if (PhysicsMesh) {
				LoadPhysicsMesh();
			}
			if (updateMesh) {
				Rect.UpdateMeshes();
			}
		}

		public void RenderCutMesh(bool updateMesh = true,bool PhysicsMesh = true) {
			if (ScrollMesh is null) {
				RenderScrollMesh(false);
			}
			var cutMesh = _isCut ? ScrollMesh.Cut(Rect.CutZonesMax, Rect.CutZonesMin) : ScrollMesh;
			CutMesh = cutMesh;
			RenderMainMesh(updateMesh,PhysicsMesh);
		}
	}
}
