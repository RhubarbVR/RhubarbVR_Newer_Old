using System;
using System.Linq;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Importers" })]
	public sealed class UnknownImporter : Importer {
		public string path_url;

		public bool isUrl;

		public byte[] rawData;

		//public SyncRef<UIWindow> Window;

		public readonly SyncRef<Entity> UI;

		public override void Import(string path_url, bool isUrl, byte[] rawData) {
			this.path_url = path_url;
			this.rawData = rawData;
			this.isUrl = isUrl;
			var importers = from e in AppDomain.CurrentDomain.GetAssemblies()
							from t in e.GetTypes()
							where !t.IsAbstract
							where typeof(Importer).IsAssignableFrom(t)
							where t != typeof(UnknownImporter)
							select t;
			// TODO UI added back
			//var window = Entity.AttachComponent<UIWindow>();
			//Entity.rotation.Value *= Quaternionf.CreateFromYawPitchRoll(90,0,180);
			//Window.Target = window;
			//window.Text.Value = "Importer";
			//window.WindowType.Value = UIWin.Normal;
			//UI.Target = Entity.AddChild("UI");
			//foreach (var item in importers) {
			//	var button = UI.Target.AttachComponent<UIButton>();
			//	button.Text.Value = item.Name.Remove(item.Name.Length - 8);
			//	var dataHolder = UI.Target.AttachComponent<AddSingleValuePram<string>>();
			//	dataHolder.Value.Value = item.FullName;
			//	dataHolder.Target.Target = RunImport;
			//	button.onClick.Target = dataHolder.Call;
			//}
		}

		[Exposed]
		public void RunImport(string data) {
			var e = Type.GetType(data);
			if(e != null) {
				if (typeof(Importer).IsAssignableFrom(e)) {
					var importer = Entity.AttachComponent<Importer>(e);
					importer.Import(path_url, isUrl, rawData);
				}
			}
			Entity.rotation.Value *= Quaternionf.CreateFromEuler(90, 0, 180).Inverse;
			//Window.Target?.Destroy();
			UI.Target?.Destroy();
			Destroy();
		}

	}
}
