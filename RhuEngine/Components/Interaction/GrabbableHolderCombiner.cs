using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using System;
using RhuEngine.Managers;

namespace RhuEngine.Components
{

	public sealed class GrabbableHolderCombiner
	{
		public Handed Source { get; }
		public WorldManager WorldManager { get; }

		private GrabbableHolder GetGrabbableHolderFromWorld(World world) {
			return Source switch {
				Handed.Left => world.LeftGrabbableHolder,
				Handed.Right => world.RightGrabbableHolder,
				_ => world.HeadGrabbableHolder,
			};
		}

		public GrabbableHolder PrivateGrabbableHolder => GetGrabbableHolderFromWorld(WorldManager.PrivateOverlay);

		public GrabbableHolderCombiner(Handed handed, WorldManager worldManager) {
			WorldManager = worldManager;
			Source = handed;
		}

		public void DeleteGrabObjects() {
			foreach (var item in WorldManager.worlds) {
				GetGrabbableHolderFromWorld(item)?.DeleteGrabObjects();
			}
		}
		public bool IsAnyLaserGrabbed
		{
			get {
				foreach (var item in WorldManager.worlds) {
					if (GetGrabbableHolderFromWorld(item)?.IsAnyLaserGrabbed ?? false) {
						return true;
					}
				}
				return false;
			}
		}
		public bool CanDestroyAnyGabbed
		{
			get {
				foreach (var item in WorldManager.worlds) {
					if (GetGrabbableHolderFromWorld(item)?.CanDestroyAnyGabbed ?? false) {
						return true;
					}
				}
				return false;
			}
		}

		public IWorldObject HolderReferenWithGrabed
		{
			get {
				foreach (var item in WorldManager.worlds) {
					if (GetGrabbableHolderFromWorld(item)?.HolderReferen is not null) {
						return GetGrabbableHolderFromWorld(item)?.HolderReferen;
					}
					else if ((GetGrabbableHolderFromWorld(item)?.GrabbedObjects?.Count ?? 0) == 1) {
						return GetGrabbableHolderFromWorld(item).GrabbedObjects[0];
					}
				}
				return null;
			}
		}

		public IWorldObject HolderReferen
		{
			get {
				foreach (var item in WorldManager.worlds) {
					if (GetGrabbableHolderFromWorld(item)?.HolderReferen is not null) {
						return GetGrabbableHolderFromWorld(item)?.HolderReferen;
					}
				}
				return null;
			}
		}

		public void UpdateHolderReferen() {
			WorldManager.PrivateSpaceManager.UpdateHolderReferen();
		}


		public bool Gripping => PrivateGrabbableHolder.gripping;
		public bool GrippingLastFrame => PrivateGrabbableHolder.grippingLastFrame;


	}
}
