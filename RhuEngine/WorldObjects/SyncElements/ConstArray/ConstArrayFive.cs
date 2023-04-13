using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhuEngine.WorldObjects
{
	public partial class ConstArrayFive<T> : ConstArrayFour<T> where T : SyncObject, new()
	{
		public readonly T Five;
		public override int Length => 5;

		public override T this[int index] => index == 4 ? Five : base[index];
	}
}
