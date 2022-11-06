using System;
using System.Collections.Generic;
using System.Text;

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
		public bool Visible { get => Inst.Visible; set=>Inst.Visible = value; }
		public RMaterial Material { get => Inst.Material; set => Inst.Material = value; }
		public void Dispose() {
			Inst.Dispose();
		}
	}


	public interface IRMesh: IDisposable
	{
		public void LoadMeshData(IMesh mesh);
		public void LoadMeshToRender();
		public void Init(RMesh rMesh);
	}

	public class RMesh
	{
		public AxisAlignedBox3f BoundingBox { get; private set; }

		public bool Dynamic { get; private set; }

		public static Type Instance { get; set; }

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
			if (!Dynamic) {
				BoundingBox = BoundsUtil.Bounds(mesh.VertexIndices(), (x) => (Vector3f)mesh.GetVertex(x));
			}
			RenderThread.ExecuteOnStartOfFrame(Inst.LoadMeshToRender);
		}

	}
}
