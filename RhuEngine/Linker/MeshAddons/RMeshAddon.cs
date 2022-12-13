using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.WorldObjects;

using RNumerics;

namespace RhuEngine.Linker.MeshAddons
{
	public abstract class RMeshAddon : IDisposable
	{
		public abstract string Name { get; }

		public RMesh Mesh { get; private set; }
		public World World { get; private set; }

		public IMesh RawMesh => Mesh.LoadedMesh;

		public int Refs { get; private set; }

		public void RemoveRef() {
			Refs--;
		}

		public void AddRef() {
			Refs++;
		}

		public virtual void Dispose() {
			GC.SuppressFinalize(this);
		}

		public void Init(RMesh rMesh, World world) {
			Mesh = rMesh;
			World = world;
			UpdateMesh();
		}

		public abstract void UpdateMesh();
	}
}
