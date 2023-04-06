using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhuEngine.WorldObjects
{
	public partial class ConstArrayThree<T> : ConstArrayTwo<T> where T : SyncObject, new()
	{
		public readonly T Three;
		public override int Length => 3;

		public override T this[int index]
		{
			get {
				if(index == 2) {
					return Three;
				}
				return base[index];
			}
		}
	}
}
