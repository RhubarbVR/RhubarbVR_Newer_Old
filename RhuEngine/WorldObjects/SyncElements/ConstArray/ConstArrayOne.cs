using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhuEngine.WorldObjects
{
	public interface IArray<T> 
	{
		int Length { get; }
		T this[int i] { get; }
	}

	public partial class ConstArrayOne<T> : SyncObject, IArray<T> where T : SyncObject, new()
	{
		public readonly T One;

		public virtual int Length => 1;

		public virtual T this[int index]
		{
			get {
				return index == 0 ? One : null;
			}
		}
	}
}
