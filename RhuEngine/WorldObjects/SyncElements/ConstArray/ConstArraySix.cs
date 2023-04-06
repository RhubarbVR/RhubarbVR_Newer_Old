using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhuEngine.WorldObjects
{
	public partial class ConstArraySix<T> : ConstArrayFive<T> where T : SyncObject, new()
	{
		public readonly T Six;
		public override int Length => 6;

		public override T this[int index]
		{
			get {
				if(index == 5) {
					return Six;
				}
				return base[index];
			}
		}
	}
}
