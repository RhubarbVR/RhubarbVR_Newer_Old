using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhuEngine.Wasm
{
	public interface IBinaryAsset: IDisposable
	{
		public Stream CreateStream();
	}
}
