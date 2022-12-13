using System;
using System.Collections.Generic;
using System.Text;

using BepuPhysics.Constraints;

using RhuEngine.Linker.MeshAddons;
using RhuEngine.WorldObjects;

using RNumerics;
namespace RhuEngine.Linker
{
	public interface IRTempQuad : IDisposable
	{
		void Init(RTempQuad text);
		public Matrix Pos { get; set; }
		public bool Visible { get; set; }
		public RMaterial Material { get; set; }
	}

	public sealed class RTempQuad : IDisposable
	{

		public static Type Instance { get; set; }

		public IRTempQuad Inst { get; set; }

		public RTempQuad() {
			Inst = (IRTempQuad)Activator.CreateInstance(Instance);
			Inst.Init(this);

		}
		public Matrix Pos { get => Inst.Pos; set => Inst.Pos = value; }
		public bool Visible { get => Inst.Visible; set => Inst.Visible = value; }
		public RMaterial Material { get => Inst.Material; set => Inst.Material = value; }
		public void Dispose() {
			Inst.Dispose();
		}
	}


	public interface IRMesh : IDisposable
	{
		public void LoadMeshData(IMesh mesh);
		public void LoadMeshToRender();
		public void Init(RMesh rMesh);
	}

	public class RMesh : IDisposable
	{
		public AxisAlignedBox3f BoundingBox { get; private set; }

		public bool Dynamic { get; private set; }

		public static Type Instance { get; set; }

		private readonly HashSet<RMeshAddon> _rMeshAddons = new();

		public IMesh LoadedMesh { get; private set; }

		public IRMesh Inst { get; private set; }

		public RMesh(IRMesh rMesh, bool dynamic) {
			Dynamic = dynamic;
			Inst = rMesh ?? (IRMesh)Activator.CreateInstance(Instance);
			Inst.Init(this);
		}

		public RMesh(IMesh mesh, bool dynamic) : this((IRMesh)null, dynamic) {
			LoadMesh(mesh);
		}

		public void LoadMesh(IMesh mesh) {
			LoadedMesh = mesh;
			Inst.LoadMeshData(mesh);
			BoundingBox = BoundsUtil.Bounds(mesh.VertexIndices(), (x) => (Vector3f)mesh.GetVertex(x));
			foreach (var item in _rMeshAddons) {
				item.UpdateMesh();
			}
			RenderThread.ExecuteOnStartOfFrame(Inst.LoadMeshToRender);
		}

		private T AddMeshAddon<T>(World world) where T : RMeshAddon, new() {
			var newMesh = new T();
			newMesh.Init(this, world);
			_rMeshAddons.Add(newMesh);
			world.IsDisposeing += (world) => {
				_rMeshAddons.Remove(newMesh);
				newMesh.Dispose();
			};
			return newMesh;
		}

		public T GetMeshAddon<T>(World world) where T : RMeshAddon, new() {
			foreach (var item in _rMeshAddons) {
				if (item is T data) {
					if (data.World == world) {
						return data;
					}
				}
			}
			return AddMeshAddon<T>(world);
		}

		public void Dispose() {
			Inst?.Dispose();
			Inst = null;
			foreach (var item in _rMeshAddons) {
				item.Dispose();
			}
			_rMeshAddons.Clear();
		}
	}
}
