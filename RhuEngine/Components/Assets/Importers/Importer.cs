using System.IO;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	public abstract class Importer:Component
	{
		public abstract void Import(string path_url, bool isUrl, byte[] rawData);
	}
}
