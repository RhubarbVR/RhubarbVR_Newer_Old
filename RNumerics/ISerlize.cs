using System.IO;

namespace RNumerics
{
	public interface ISerlize
	{
		public void Serlize(BinaryWriter binaryWriter);
		public void DeSerlize(BinaryReader binaryReader);
	}

	public interface ISerlize<T> : ISerlize
	{

	}
}
