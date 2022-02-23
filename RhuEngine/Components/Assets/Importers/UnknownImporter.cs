using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Importers" })]
	public class UnknownImporter : Importer
	{
		public string path_url;

		public bool isUrl;

		public byte[] rawData;
		public override void Import(string path_url, bool isUrl, byte[] rawData) {
			this.path_url = path_url;
			this.rawData = rawData;
			this.isUrl = isUrl;
		}
	}
}
