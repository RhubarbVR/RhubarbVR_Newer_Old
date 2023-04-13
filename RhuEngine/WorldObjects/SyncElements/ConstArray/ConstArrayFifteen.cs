using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhuEngine.WorldObjects
{
	public partial class ConstArrayFifteen<T> : ConstArrayFourteen<T> where T : SyncObject, new()
	{
		public readonly T Fifteen;
		public override int Length => 15;

		public override T this[int index] => index == 14 ? Fifteen : base[index];
	}
}
