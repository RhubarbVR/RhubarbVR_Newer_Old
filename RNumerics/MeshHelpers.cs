using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using RNumerics;

public static class MeshHelpers
{
	private static bool SameSide(in Vector3 p1, in Vector3 p2, in Vector3 A, in Vector3 B) {
		var cp1 = Vector3.Cross(Vector3.Subtract(B, A), Vector3.Subtract(p1, A));
		var cp2 = Vector3.Cross(Vector3.Subtract(B, A), Vector3.Subtract(p2, A));
		return Vector3.Dot(cp1, cp2) >= 0;

	}

	public static int InsideTry(this IMesh mesh,in Vector3 point) {
		return InsideTry(mesh, new Vector3f(point.X, point.Y, point.Z));
	}

	public static int InsideTry(this IMesh mesh,in Vector3f point) {
		return InsideTry(mesh, new Vector3d(point.x, point.y, point.z));
	}

	public static int InsideTry(this IMesh mesh, in Vector3d point) {
		var e = mesh.RenderIndices().ToArray();
		for (var i = 0; i < e.Length; i += 3) {
			var point1 = mesh.GetVertex(e[i]);
			var point2 = mesh.GetVertex(e[i + 1]);
			var point3 = mesh.GetVertex(e[i + 2]);

			if(point1 == point2 || point1 == point3 || point2 == point3) {
				continue;
			}

			var testPointStep1 = point - point1;
			var point1Step1 = point1 - point1;
			var point2Step1 = point2 - point1;
			var point3Step1 = point3 - point1;

			var stestPointStep1 = new Vector3((float)testPointStep1.x, (float)testPointStep1.y, (float)testPointStep1.z);
			var spoint1Step1 = new Vector3((float)point1Step1.x, (float)point1Step1.y, (float)point1Step1.z);
			var spoint2Step1 = new Vector3((float)point2Step1.x, (float)point2Step1.y, (float)point2Step1.z);
			var spoint3Step1 = new Vector3((float)point3Step1.x, (float)point3Step1.y, (float)point3Step1.z);

			var P = stestPointStep1;

			Vector3 A = spoint1Step1, B = spoint2Step1, C = spoint3Step1;
			if (SameSide(P, A, B, C) && SameSide(P, B, A, C) && SameSide(P, C, A, B)) {
				var vc1 = Vector3.Cross(Vector3.Subtract(A, B), Vector3.Subtract(A, C));
				if (Math.Abs(Vector3.Dot(Vector3.Subtract(A, P), vc1)) <= .01f) {
					return i / 3;
				}
			}
		}
		return -1;
	}

}
