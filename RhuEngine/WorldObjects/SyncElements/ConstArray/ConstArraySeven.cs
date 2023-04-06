using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhuEngine.WorldObjects
{
	public partial class ConstArraySeven<T> : ConstArraySix<T> where T : SyncObject, new()
	{
		public readonly T Seven;
		public override int Length => 7;

		public override T this[int index]
		{
			get {
				if(index == 6) {
					return Seven;
				}
				return base[index];
			}
		}
	}
}
