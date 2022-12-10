using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace RNumerics
{
	/// <summary>
	/// Convenience functions for working with arrays. 
	///    - Math functions on arrays of floats/doubles
	///    - "automatic" conversion from IEnumerable<T> (via introspection)
	///    - byte[] conversions
	///    - zlib compress/decompress byte[] buffers
	/// </summary>
	public static class BufferUtil
	{
		static public void SetVertex3(in double[] v, in int i, in double x, in double y, in double z) {
			v[3 * i] = x;
			v[(3 * i) + 1] = y;
			v[(3 * i) + 2] = z;
		}
		static public void SetVertex3(in float[] v, in int i, in float x, in float y, in float z) {
			v[3 * i] = x;
			v[(3 * i) + 1] = y;
			v[(3 * i) + 2] = z;
		}

		static public void SetVertex2(in double[] v, in int i, in double x, in double y) {
			v[2 * i] = x;
			v[(2 * i) + 1] = y;
		}
		static public void SetVertex2(in float[] v, in int i, in float x, in float y) {
			v[2 * i] = x;
			v[(2 * i) + 1] = y;
		}

		static public void SetTriangle(in int[] v, in int i, in int a, in int b, in int c) {
			v[3 * i] = a;
			v[(3 * i) + 1] = b;
			v[(3 * i) + 2] = c;
		}


		static public double Dot(in double[] a, in double[] b) {
			double dot = 0;
			for (var i = 0; i < a.Length; ++i) {
				dot += a[i] * b[i];
			}

			return dot;
		}

		static public void MultiplyAdd(in double[] dest, in double multiply, in double[] add) {
			for (var i = 0; i < dest.Length; ++i) {
				dest[i] += multiply * add[i];
			}
		}

		static public void MultiplyAdd(in double[] dest, in double[] multiply, in double[] add) {
			for (var i = 0; i < dest.Length; ++i) {
				dest[i] += multiply[i] * add[i];
			}
		}

		static public double MultiplyAdd_GetSqrSum(in double[] dest, in double multiply, in double[] add) {
			double sum = 0;
			for (var i = 0; i < dest.Length; ++i) {
				dest[i] += multiply * add[i];
				sum += dest[i] * dest[i];
			}
			return sum;
		}

		static public double DistanceSquared(in double[] a, in double[] b) {
			double sum = 0;
			for (var i = 0; i < a.Length; ++i) {
				sum += (a[i] - b[i]) * (a[i] - b[i]);
			}

			return sum;
		}



		static public void ParallelDot(double[] a, double[][] b, double[][] result) {
			int N = a.Length, count = b.Length;
			GParallel.BlockStartEnd(0, N - 1, (i0, i1) => {
				for (var i = i0; i <= i1; i++) {
					for (var j = 0; j < count; ++j) {
						result[j][i] = a[i] * b[j][i];
					}
				}
			}, 1000);
		}


		static public double[][] AllocNxM(in int N, in int M) {
			var d = new double[N][];
			for (var k = 0; k < N; ++k) {
				d[k] = new double[M];
			}

			return d;
		}

		static public double[][] InitNxM(in int N, in int M, in double[][] init) {
			var d = AllocNxM(N, M);
			for (var k = 0; k < N; ++k) {
				Array.Copy(init[k], d[k], M);
			}

			return d;
		}


		/// <summary>
		/// Count number of elements in array (or up to max_i) that pass FilterF test
		/// </summary>
		static public int CountValid<T>(in T[] data, in Func<T, bool> FilterF, in int max_i = -1) {
			var n = (max_i == -1) ? data.Length : max_i;
			var valid = 0;
			for (var i = 0; i < n; ++i) {
				if (FilterF(data[i])) {
					valid++;
				}
			}
			return valid;
		}

		/// <summary>
		/// shifts elements of array (or up to max_i) that pass FilterF to front of list,
		/// and returns number that passed
		/// </summary>
		static public int FilterInPlace<T>(in T[] data, in Func<T, bool> FilterF, in int max_i = -1) {
			var N = (max_i == -1) ? data.Length : max_i;
			var k = 0;
			for (var i = 0; i < N; ++i) {
				if (FilterF(data[i])) {
					data[k++] = data[i];
				}
			}
			return k;
		}

		/// <summary>
		/// return a new array containing only elements (or up to max_i) that pass FilterF test
		/// </summary>
		static public T[] Filter<T>(in T[] data, in Func<T, bool> FilterF, in int max_i = -1) {
			var n = (max_i == -1) ? data.Length : max_i;
			var valid = CountValid(data, FilterF);
			if (valid == 0) {
				return null;
			}

			var result = new T[valid];
			var k = 0;
			for (var i = 0; i < n; ++i) {
				if (FilterF(data[i])) {
					result[k++] = data[i];
				}
			}
			return result;
		}




		/// <summary>
		/// convert input set into Vector3d.
		/// Supports packed list of float/double tuples, list of Vector3f/Vector3d
		/// </summary>
		static public Vector3d[] ToVector3d<T>(in IEnumerable<T> values) {
			var N = values.Count();
			var k = 0;
			var j = 0;

			var t = typeof(T);
			Vector3d[] result;
			if (t == typeof(float)) {
				N /= 3;
				result = new Vector3d[N];
				var valuesf = values as IEnumerable<float>;
				foreach (var f in valuesf) {
					result[k][j++] = f;
					if (j == 3) {
						j = 0;
						k++;
					}
				}
			}
			else if (t == typeof(double)) {
				N /= 3;
				result = new Vector3d[N];
				var valuesd = values as IEnumerable<double>;
				foreach (var f in valuesd) {
					result[k][j++] = f;
					if (j == 3) {
						j = 0;
						k++;
					}
				}
			}
			else if (t == typeof(Vector3f)) {
				result = new Vector3d[N];
				var valuesvf = values as IEnumerable<Vector3f>;
				foreach (var v in valuesvf) {
					result[k++] = v;
				}
			}
			else if (t == typeof(Vector3d)) {
				result = new Vector3d[N];
				var valuesvd = values as IEnumerable<Vector3d>;
				foreach (var v in valuesvd) {
					result[k++] = v;
				}
			}
			else {
				throw new NotSupportedException("ToVector3d: unknown type " + t.ToString());
			}

			return result;
		}



		/// <summary>
		/// convert input set into Vector3f.
		/// Supports packed list of float/double tuples, list of Vector3f/Vector3d
		/// </summary>
		static public Vector3f[] ToVector3f<T>(in IEnumerable<T> values) {
			var N = values.Count();
			var k = 0;
			var j = 0;

			var t = typeof(T);
			Vector3f[] result;
			if (t == typeof(float)) {
				N /= 3;
				result = new Vector3f[N];
				var valuesf = values as IEnumerable<float>;
				foreach (var f in valuesf) {
					result[k][j++] = f;
					if (j == 3) {
						j = 0;
						k++;
					}
				}
			}
			else if (t == typeof(double)) {
				N /= 3;
				result = new Vector3f[N];
				var valuesd = values as IEnumerable<double>;
				foreach (var f in valuesd) {
					result[k][j++] = (float)f;
					if (j == 3) {
						j = 0;
						k++;
					}
				}
			}
			else if (t == typeof(Vector3f)) {
				result = new Vector3f[N];
				var valuesvf = values as IEnumerable<Vector3f>;
				foreach (var v in valuesvf) {
					result[k++] = v;
				}
			}
			else if (t == typeof(Vector3d)) {
				result = new Vector3f[N];
				var valuesvd = values as IEnumerable<Vector3d>;
				foreach (var v in valuesvd) {
					result[k++] = (Vector3f)v;
				}
			}
			else {
				throw new NotSupportedException("ToVector3d: unknown type " + t.ToString());
			}

			return result;
		}




		/// <summary>
		/// convert input set into Index3i.
		/// Supports packed list of int tuples, list of Vector3i/Index3i
		/// </summary>
		static public Index3i[] ToIndex3i<T>(in IEnumerable<T> values) {
			var N = values.Count();
			var k = 0;
			var j = 0;

			var t = typeof(T);
			Index3i[] result;
			if (t == typeof(int)) {
				N /= 3;
				result = new Index3i[N];
				var valuesi = values as IEnumerable<int>;
				foreach (var i in valuesi) {
					result[k][j++] = i;
					if (j == 3) {
						j = 0;
						k++;
					}
				}
			}
			else if (t == typeof(Index3i)) {
				result = new Index3i[N];
				var valuesvi = values as IEnumerable<Index3i>;
				foreach (var v in valuesvi) {
					result[k++] = v;
				}
			}
			else if (t == typeof(Vector3i)) {
				result = new Index3i[N];
				var valuesvi = values as IEnumerable<Vector3i>;
				foreach (var v in valuesvi) {
					result[k++] = v;
				}
			}
			else {
				throw new NotSupportedException("ToVector3d: unknown type " + t.ToString());
			}

			return result;
		}



		/// <summary>
		/// convert byte array to int array
		/// </summary>
		static public int[] ToInt(in byte[] buffer) {
			var sz = sizeof(int);
			var Nvals = buffer.Length / sz;
			var v = new int[Nvals];
			for (var i = 0; i < Nvals; i++) {
				v[i] = BitConverter.ToInt32(buffer, i * sz);
			}
			return v;
		}


		/// <summary>
		/// convert byte array to short array
		/// </summary>
		static public short[] ToShort(in byte[] buffer) {
			var sz = sizeof(short);
			var Nvals = buffer.Length / sz;
			var v = new short[Nvals];
			for (var i = 0; i < Nvals; i++) {
				v[i] = BitConverter.ToInt16(buffer, i * sz);
			}
			return v;
		}


		/// <summary>
		/// convert byte array to double array
		/// </summary>
		static public double[] ToDouble(in byte[] buffer) {
			var sz = sizeof(double);
			var Nvals = buffer.Length / sz;
			var v = new double[Nvals];
			for (var i = 0; i < Nvals; i++) {
				v[i] = BitConverter.ToDouble(buffer, i * sz);
			}
			return v;
		}


		/// <summary>
		/// convert byte array to float array
		/// </summary>
		static public float[] ToFloat(in byte[] buffer) {
			var sz = sizeof(float);
			var Nvals = buffer.Length / sz;
			var v = new float[Nvals];
			for (var i = 0; i < Nvals; i++) {
				v[i] = BitConverter.ToSingle(buffer, i * sz);
			}
			return v;
		}


		/// <summary>
		/// convert byte array to VectorArray3d
		/// </summary>
		static public VectorArray3d ToVectorArray3d(in byte[] buffer) {
			var sz = sizeof(double);
			var Nvals = buffer.Length / sz;
			var Nvecs = Nvals / 3;
			var v = new VectorArray3d(Nvecs);
			for (var i = 0; i < Nvecs; i++) {
				var x = BitConverter.ToDouble(buffer, 3 * i * sz);
				var y = BitConverter.ToDouble(buffer, ((3 * i) + 1) * sz);
				var z = BitConverter.ToDouble(buffer, ((3 * i) + 2) * sz);
				v.Set(i, x, y, z);
			}
			return v;
		}



		/// <summary>
		/// convert byte array to VectorArray2f
		/// </summary>
		static public VectorArray2f ToVectorArray2f(in byte[] buffer) {
			var sz = sizeof(float);
			var Nvals = buffer.Length / sz;
			var Nvecs = Nvals / 2;
			var v = new VectorArray2f(Nvecs);
			for (var i = 0; i < Nvecs; i++) {
				var x = BitConverter.ToSingle(buffer, 2 * i * sz);
				var y = BitConverter.ToSingle(buffer, ((2 * i) + 1) * sz);
				v.Set(i, x, y);
			}
			return v;
		}

		/// <summary>
		/// convert byte array to VectorArray3f
		/// </summary>
		static public VectorArray3f ToVectorArray3f(in byte[] buffer) {
			var sz = sizeof(float);
			var Nvals = buffer.Length / sz;
			var Nvecs = Nvals / 3;
			var v = new VectorArray3f(Nvecs);
			for (var i = 0; i < Nvecs; i++) {
				var x = BitConverter.ToSingle(buffer, 3 * i * sz);
				var y = BitConverter.ToSingle(buffer, ((3 * i) + 1) * sz);
				var z = BitConverter.ToSingle(buffer, ((3 * i) + 2) * sz);
				v.Set(i, x, y, z);
			}
			return v;
		}




		/// <summary>
		/// convert byte array to VectorArray3i
		/// </summary>
		static public VectorArray3i ToVectorArray3i(in byte[] buffer) {
			var sz = sizeof(int);
			var Nvals = buffer.Length / sz;
			var Nvecs = Nvals / 3;
			var v = new VectorArray3i(Nvecs);
			for (var i = 0; i < Nvecs; i++) {
				var x = BitConverter.ToInt32(buffer, 3 * i * sz);
				var y = BitConverter.ToInt32(buffer, ((3 * i) + 1) * sz);
				var z = BitConverter.ToInt32(buffer, ((3 * i) + 2) * sz);
				v.Set(i, x, y, z);
			}
			return v;
		}


		/// <summary>
		/// convert byte array to IndexArray4i
		/// </summary>
		static public IndexArray4i ToIndexArray4i(in byte[] buffer) {
			var sz = sizeof(int);
			var Nvals = buffer.Length / sz;
			var Nvecs = Nvals / 4;
			var v = new IndexArray4i(Nvecs);
			for (var i = 0; i < Nvecs; i++) {
				var a = BitConverter.ToInt32(buffer, 4 * i * sz);
				var b = BitConverter.ToInt32(buffer, ((4 * i) + 1) * sz);
				var c = BitConverter.ToInt32(buffer, ((4 * i) + 2) * sz);
				var d = BitConverter.ToInt32(buffer, ((4 * i) + 3) * sz);
				v.Set(i, a, b, c, d);
			}
			return v;
		}


		/// <summary>
		/// convert int array to bytes
		/// </summary>
		static public byte[] ToBytes(in int[] array) {
			var result = new byte[array.Length * sizeof(int)];
			Buffer.BlockCopy(array, 0, result, 0, result.Length);
			return result;
		}

		/// <summary>
		/// convert short array to bytes
		/// </summary>
		static public byte[] ToBytes(in short[] array) {
			var result = new byte[array.Length * sizeof(short)];
			Buffer.BlockCopy(array, 0, result, 0, result.Length);
			return result;
		}

		/// <summary>
		/// convert float array to bytes
		/// </summary>
		static public byte[] ToBytes(in float[] array) {
			var result = new byte[array.Length * sizeof(float)];
			Buffer.BlockCopy(array, 0, result, 0, result.Length);
			return result;
		}

		/// <summary>
		/// convert double array to bytes
		/// </summary>
		static public byte[] ToBytes(in double[] array) {
			var result = new byte[array.Length * sizeof(double)];
			Buffer.BlockCopy(array, 0, result, 0, result.Length);
			return result;
		}




		/// <summary>
		/// Compress a byte buffer using Deflate/ZLib compression. 
		/// </summary>
		static public byte[] CompressZLib(in byte[] buffer, in bool bFast) {
			var ms = new MemoryStream();
			var zip = new DeflateStream(ms, bFast ? CompressionLevel.Fastest : CompressionLevel.Optimal, true);
			zip.Write(buffer, 0, buffer.Length);
			zip.Close();
			ms.Position = 0;

			var compressed = new byte[ms.Length];
			ms.Read(compressed, 0, compressed.Length);

			var zBuffer = new byte[compressed.Length + 4];
			Buffer.BlockCopy(compressed, 0, zBuffer, 4, compressed.Length);
			Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, zBuffer, 0, 4);
			return zBuffer;
		}


		/// <summary>
		/// Decompress a byte buffer that has been compressed using Deflate/ZLib compression
		/// </summary>
		static public byte[] DecompressZLib(in byte[] zBuffer) {
			var ms = new MemoryStream();
			var msgLength = BitConverter.ToInt32(zBuffer, 0);
			ms.Write(zBuffer, 4, zBuffer.Length - 4);

			var buffer = new byte[msgLength];

			ms.Position = 0;
			var zip = new DeflateStream(ms, CompressionMode.Decompress);
			zip.Read(buffer, 0, buffer.Length);

			return buffer;
		}


	}




	/// <summary>
	/// utility class for porting C++ code that uses this kind of idiom:
	///    T * ptr = &array[i];
	///    ptr[k] = value
	/// </summary>
	public struct ArrayAlias<T>
	{
		public T[] Source;
		public int Index;

		public ArrayAlias(in T[] source, in int i) {
			Source = source;
			Index = i;
		}

		public T this[in int i]
		{
			get => Source[Index + i];
			set => Source[Index + i] = value;
		}
	}


}
