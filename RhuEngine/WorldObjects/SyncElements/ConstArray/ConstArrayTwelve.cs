using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhuEngine.WorldObjects
{
	public partial class ConstArrayTwelve<T> : ConstArrayEleven<T> where T : SyncObject, new()
	{
		public readonly T Twelve;

		public override int Length => 12;

		public override T this[int index]
		{
			get {
				if(index == 11) {
					return Twelve;
				}
				return base[index];
			}
		}
	}
}
