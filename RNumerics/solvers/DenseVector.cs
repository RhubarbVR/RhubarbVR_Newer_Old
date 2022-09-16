using System;

namespace RNumerics
{
	public sealed class DenseVector
	{
		public DenseVector(in int N) {
			Buffer = new double[N];
			Array.Clear(Buffer, 0, Buffer.Length);
			Size = N;
		}


		public void Set(in int i, in double value) {
			Buffer[i] = value;
		}


		public int Size { get; }
		public int Length => Size;

		public double this[in int i]
		{
			get => Buffer[i];
			set => Buffer[i] = value;
		}

		public double[] Buffer { get; }


		public double Dot(in DenseVector v2) {
			return Dot(v2.Buffer);
		}
		public double Dot(in double[] v2) {
			if (v2.Length != Size) {
				throw new Exception("DenseVector.Dot: incompatible lengths");
			}

			double sum = 0;
			for (var k = 0; k < v2.Length; ++k) {
				sum += Buffer[k] * v2[k];
			}

			return sum;
		}

	}
}