using System;


namespace RNumerics
{
	public interface IMatrix
	{
		int Rows { get; }
		int Columns { get; }
		Index2i Size { get; }

		void Set(in int r,in int c, in double value);

		double this[in int r,in int c] { get; set; }
	}
}
