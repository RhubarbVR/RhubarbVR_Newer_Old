using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhuEngine.WorldObjects
{
	public partial class ConstArrayTwo<T> : ConstArrayOne<T> where T : SyncObject
	{
		public readonly T Two;

		public override int Length => 2;

		public override T this[int index]
		{
			get {
				if(index == 1) {
					return Two;
				}
				return base[index];
			}
		}
	}
}
