using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhuEngine.WorldObjects
{
	public partial class ConstArrayFour<T> : ConstArrayThree<T> where T : SyncObject, new()
	{
		public readonly T Four;
		public override int Length => 4;

		public override T this[int index]
		{
			get {
				if(index == 3) {
					return Four;
				}
				return base[index];
			}
		}
	}
}
