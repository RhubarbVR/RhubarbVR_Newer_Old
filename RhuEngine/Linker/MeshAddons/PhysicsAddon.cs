using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.WorldObjects;

using RNumerics;
using BepuPhysics;
using BepuUtilities.Memory;

namespace RhuEngine.Linker.MeshAddons
{
	public abstract class PhysicsAddon : RMeshAddon
	{
		public bool Loaded { get; protected set; }
		public BufferPool BufferPool => World.PhysicsSimulation.BufferPool;

		public abstract void Unload();

		public abstract void Load(IMesh mesh);
		
		public override void UpdateMesh() {
			try {
				if (RawMesh is null) {
					if (Loaded) {
						Loaded = false;
						Unload();
					}
					return;
				}
				if (Loaded) {
					Loaded = false;
					Unload();
				}
				Load(RawMesh);
				Loaded = true;
			}catch(Exception ex) {
				RLog.Err($"Mesh Physics Addon Update Failed Error:{ex}");
			}
		}

		public override void Dispose() {
			if(Loaded) {
				Loaded = false;
				Unload();
			}
			GC.SuppressFinalize(this);
		}
	}
}
