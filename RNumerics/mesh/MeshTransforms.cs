using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	public static class MeshTransforms
	{

		public static void Translate(in IDeformableMesh mesh, in Vector3d v) {
			Translate(mesh, v.x, v.y, v.z);
		}
		public static void Translate(in IDeformableMesh mesh, in double tx, in double ty, in double tz) {
			var NV = mesh.MaxVertexID;
			for (var vid = 0; vid < NV; ++vid) {
				if (mesh.IsVertex(vid)) {
					var v = mesh.GetVertex(vid);
					v.x += tx;
					v.y += ty;
					v.z += tz;
					mesh.SetVertex(vid, v);
				}
			}
		}


		public static Vector3d Rotate(in Vector3d pos, in Vector3d origin, in Quaternionf rotation) {
			var v = pos - origin;
			v = (Vector3d)(rotation * (Vector3f)v);
			v += origin;
			return v;
		}
		public static Frame3f Rotate(Frame3f f, in Vector3d origin, in Quaternionf rotation) {
			f.Rotate(rotation);
			f.Origin = (Vector3f)Rotate(f.Origin, origin, rotation);
			return f;
		}
		public static Frame3f Rotate(Frame3f f, in Vector3d origin, in Quaterniond rotation) {
			f.Rotate((Quaternionf)rotation);
			f.Origin = (Vector3f)Rotate(f.Origin, origin, rotation);
			return f;
		}
		public static void Rotate(in IDeformableMesh mesh, in Vector3d origin, in Quaternionf rotation) {
			var NV = mesh.MaxVertexID;
			for (var vid = 0; vid < NV; ++vid) {
				if (mesh.IsVertex(vid)) {
					var v = mesh.GetVertex(vid);
					v -= origin;
					v = (Vector3d)(rotation * (Vector3f)v);
					v += origin;
					mesh.SetVertex(vid, v);
				}
			}
		}


		public static Vector3d Rotate(in Vector3d pos, in Vector3d origin, in Quaterniond rotation) {
			return (rotation * (pos - origin)) + origin;
		}
		public static void Rotate(in IDeformableMesh mesh, in Vector3d origin, in Quaterniond rotation) {
			var bHasNormals = mesh.HasVertexNormals;
			var NV = mesh.MaxVertexID;
			for (var vid = 0; vid < NV; ++vid) {
				if (mesh.IsVertex(vid)) {
					var v = (rotation * (mesh.GetVertex(vid) - origin)) + origin;
					mesh.SetVertex(vid, v);
					if (bHasNormals) {
						mesh.SetVertexNormal(vid, (Vector3f)(rotation * mesh.GetVertexNormal(vid)));
					}
				}
			}
		}


		public static void Scale(in IDeformableMesh mesh, in Vector3d scale, in Vector3d origin) {
			var NV = mesh.MaxVertexID;
			for (var vid = 0; vid < NV; ++vid) {
				if (mesh.IsVertex(vid)) {
					var v = mesh.GetVertex(vid);
					v.x -= origin.x;
					v.y -= origin.y;
					v.z -= origin.z;
					v.x *= scale.x;
					v.y *= scale.y;
					v.z *= scale.z;
					v.x += origin.x;
					v.y += origin.y;
					v.z += origin.z;
					mesh.SetVertex(vid, v);
				}
			}
		}
		public static void Scale(in IDeformableMesh mesh, in double sx, in double sy, in double sz) {
			Scale(mesh, new Vector3d(sx, sy, sz), Vector3d.Zero);
		}
		public static void Scale(in IDeformableMesh mesh, in double s) {
			Scale(mesh, s, s, s);
		}

		///<summary>Map mesh *into* local coordinates of Frame </summary>
		public static void ToFrame(in IDeformableMesh mesh, in Frame3f f) {
			var NV = mesh.MaxVertexID;
			var bHasNormals = mesh.HasVertexNormals;
			for (var vid = 0; vid < NV; ++vid) {
				if (mesh.IsVertex(vid)) {
					var v = mesh.GetVertex(vid);
					var vf = f.ToFrameP( v);
					mesh.SetVertex(vid, vf);
					if (bHasNormals) {
						var n = mesh.GetVertexNormal(vid);
						var nf = f.ToFrameV(ref n);
						mesh.SetVertexNormal(vid, nf);
					}
				}
			}
		}

		/// <summary> Map mesh *from* local frame coordinates into "world" coordinates </summary>
		public static void FromFrame(in IDeformableMesh mesh, in Frame3f f) {
			var NV = mesh.MaxVertexID;
			var bHasNormals = mesh.HasVertexNormals;
			for (var vid = 0; vid < NV; ++vid) {
				if (mesh.IsVertex(vid)) {
					var vf = mesh.GetVertex(vid);
					var v = f.FromFrameP( vf);
					mesh.SetVertex(vid, v);
					if (bHasNormals) {
						var n = mesh.GetVertexNormal(vid);
						var nf = f.FromFrameV(ref n);
						mesh.SetVertexNormal(vid, nf);
					}
				}
			}
		}


		public static Vector3d ConvertZUpToYUp(in Vector3d v) {
			return new Vector3d(v.x, v.z, -v.y);
		}
		public static Vector3f ConvertZUpToYUp(in Vector3f v) {
			return new Vector3f(v.x, v.z, -v.y);
		}
		public static Frame3f ConvertZUpToYUp(in Frame3f f) {
			return new Frame3f(
				ConvertZUpToYUp(f.Origin),
				ConvertZUpToYUp(f.X),
				ConvertZUpToYUp(f.Y),
				ConvertZUpToYUp(f.Z));
		}
		public static void ConvertZUpToYUp(in IDeformableMesh mesh) {
			var NV = mesh.MaxVertexID;
			var bHasNormals = mesh.HasVertexNormals;
			for (var vid = 0; vid < NV; ++vid) {
				if (mesh.IsVertex(vid)) {
					var v = mesh.GetVertex(vid);
					mesh.SetVertex(vid, new Vector3d(v.x, v.z, -v.y));
					if (bHasNormals) {
						var n = mesh.GetVertexNormal(vid);
						mesh.SetVertexNormal(vid, new Vector3f(n.x, n.z, -n.y));
					}
				}
			}
		}

		public static Vector3d ConvertYUpToZUp(in Vector3d v) {
			return new Vector3d(v.x, -v.z, v.y);
		}
		public static Vector3f ConvertYUpToZUp(in Vector3f v) {
			return new Vector3f(v.x, -v.z, v.y);
		}
		public static Frame3f ConvertYUpToZUp(in Frame3f f) {
			return new Frame3f(
				ConvertYUpToZUp(f.Origin),
				ConvertYUpToZUp(f.X),
				ConvertYUpToZUp(f.Y),
				ConvertYUpToZUp(f.Z));
		}
		public static void ConvertYUpToZUp(in IDeformableMesh mesh) {
			var NV = mesh.MaxVertexID;
			var bHasNormals = mesh.HasVertexNormals;
			for (var vid = 0; vid < NV; ++vid) {
				if (mesh.IsVertex(vid)) {
					var v = mesh.GetVertex(vid);
					mesh.SetVertex(vid, new Vector3d(v.x, -v.z, v.y));
					if (bHasNormals) {
						var n = mesh.GetVertexNormal(vid);
						mesh.SetVertexNormal(vid, new Vector3f(n.x, -n.z, n.y));
					}
				}
			}
		}


		public static Vector3d FlipLeftRightCoordSystems(in Vector3d v) {
			return new Vector3d(v.x, v.y, -v.z);
		}
		public static Vector3f FlipLeftRightCoordSystems(in Vector3f v) {
			return new Vector3f(v.x, v.y, -v.z);
		}
		public static Frame3f FlipLeftRightCoordSystems(in Frame3f f) {
			throw new NotImplementedException("this doesn't work...frame becomes broken somehow?");
			//return new Frame3f(
			//    FlipLeftRightCoordSystems(f.Origin),
			//    f.X, f.Y, f.Z);
			//    //FlipLeftRightCoordSystems(f.X),
			//    //FlipLeftRightCoordSystems(f.Y),
			//    //FlipLeftRightCoordSystems(f.Z));
		}
		public static void FlipLeftRightCoordSystems(in IDeformableMesh mesh) {
			var NV = mesh.MaxVertexID;
			for (var vid = 0; vid < NV; ++vid) {
				if (mesh.IsVertex(vid)) {
					var v = mesh.GetVertex(vid);
					v.z = -v.z;
					mesh.SetVertex(vid, v);

					if (mesh.HasVertexNormals) {
						var n = mesh.GetVertexNormal(vid);
						n.z = -n.z;
						mesh.SetVertexNormal(vid, n);
					}
				}
			}
			throw new Exception("argh don't want this in IDeformableMesh...but then for SimpleMesh??");
		}




		public static void VertexNormalOffset(in IDeformableMesh mesh, in double offsetDistance) {
			var NV = mesh.MaxVertexID;
			for (var vid = 0; vid < NV; ++vid) {
				if (mesh.IsVertex(vid)) {
					var newPos = mesh.GetVertex(vid) + (offsetDistance * (Vector3d)mesh.GetVertexNormal(vid));
					mesh.SetVertex(vid, newPos);
				}
			}
		}


		/// <summary>
		/// Apply TransformF to vertices of mesh
		/// </summary>
		public static void PerVertexTransform(in IDeformableMesh mesh, in Func<Vector3d, Vector3d> TransformF) {
			var NV = mesh.MaxVertexID;
			for (var vid = 0; vid < NV; ++vid) {
				if (mesh.IsVertex(vid)) {
					var newPos = TransformF(mesh.GetVertex(vid));
					mesh.SetVertex(vid, newPos);
				}
			}
		}
		public static void PerVertexTransform(in IDeformableMesh mesh, in Func<Vector3d, Vector3f, Vector3d> TransformF) {
			var NV = mesh.MaxVertexID;
			for (var vid = 0; vid < NV; ++vid) {
				if (mesh.IsVertex(vid)) {
					var newPos = TransformF(mesh.GetVertex(vid), mesh.GetVertexNormal(vid));
					mesh.SetVertex(vid, newPos);
				}
			}
		}


		/// <summary>
		/// Apply TransformF to vertices and normals of mesh
		/// </summary>
		public static void PerVertexTransform(in IDeformableMesh mesh, in Func<Vector3d, Vector3f, Vector3dTuple2> TransformF) {
			var NV = mesh.MaxVertexID;
			for (var vid = 0; vid < NV; ++vid) {
				if (mesh.IsVertex(vid)) {
					var newPN = TransformF(mesh.GetVertex(vid), mesh.GetVertexNormal(vid));
					mesh.SetVertex(vid, newPN.v0);
					mesh.SetVertexNormal(vid, (Vector3f)newPN.v1);
				}
			}
		}


		/// <summary>
		/// Apply Transform to vertices and normals of mesh
		/// </summary>
		public static void PerVertexTransform(in IDeformableMesh mesh, in TransformSequence xform) {
			var NV = mesh.MaxVertexID;
			if (mesh.HasVertexNormals) {
				for (var vid = 0; vid < NV; ++vid) {
					if (mesh.IsVertex(vid)) {
						mesh.SetVertex(vid, xform.TransformP(mesh.GetVertex(vid)));
						mesh.SetVertexNormal(vid, (Vector3f)xform.TransformV(mesh.GetVertexNormal(vid)));
					}
				}
			}
			else {
				for (var vid = 0; vid < NV; ++vid) {
					if (mesh.IsVertex(vid)) {
						mesh.SetVertex(vid, xform.TransformP(mesh.GetVertex(vid)));
					}
				}
			}
		}



		/// <summary>
		/// Apply TransformF to subset of vertices of mesh
		/// </summary>
		public static void PerVertexTransform(in IDeformableMesh mesh, in IEnumerable<int> vertices, in Func<Vector3d, int, Vector3d> TransformF) {
			foreach (var vid in vertices) {
				if (mesh.IsVertex(vid)) {
					var newPos = TransformF(mesh.GetVertex(vid), vid);
					mesh.SetVertex(vid, newPos);
				}
			}
		}

		/// <summary>
		/// Apply TransformF to subset of mesh vertices defined by MapV[vertices] 
		/// </summary>
		public static void PerVertexTransform(in IDeformableMesh mesh, in IEnumerable<int> vertices, in Func<int, int> MapV, in Func<Vector3d, int, int, Vector3d> TransformF) {
			foreach (var vid in vertices) {
				var map_vid = MapV(vid);
				if (mesh.IsVertex(map_vid)) {
					var newPos = TransformF(mesh.GetVertex(map_vid), vid, map_vid);
					mesh.SetVertex(map_vid, newPos);
				}
			}
		}


		/// <summary>
		/// Apply TransformF to subset of mesh vertices defined by MapV[vertices] 
		/// </summary>
		public static void PerVertexTransform(in IDeformableMesh targetMesh, in IDeformableMesh sourceMesh, in int[] mapV, in Func<Vector3d, int, int, Vector3d> TransformF) {
			foreach (var vid in sourceMesh.VertexIndices()) {
				var map_vid = mapV[vid];
				if (targetMesh.IsVertex(map_vid)) {
					var newPos = TransformF(targetMesh.GetVertex(map_vid), vid, map_vid);
					targetMesh.SetVertex(map_vid, newPos);
				}
			}
		}



	}
}
