using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

using Newtonsoft.Json.Linq;

using RhuEngine.Linker;
using RhuEngine.Plugin;

namespace RhuEngine.Managers
{
	public sealed class PluginManager : IManager
	{
		public sealed class Plugin
		{
			public string TargetInfoFile => Path.Combine(PluginFolder, "info" + ".json");

			public string TargetMainFile => Path.Combine(PluginFolder, PluginName + ".dll");

			public string PluginName => Path.GetFileName(PluginFolder);

			public string PluginFolder;

			public bool IsLoaded => _assembly is not null;

			public IPlugin BasePlugin { get; private set; }

			private Assembly _assembly;

			private AssemblyLoadContext _loadContext;

			public Version Version { get; private set; }
			public string CompatibilityString { get; private set; }
			public string FriendlyName { get; private set; }
			public string[] Authors { get; private set; }

			public bool LoadInfo() {
				try {
					var jobject = JObject.Parse(File.ReadAllText(TargetInfoFile));
					var compatibilityString = jobject.GetValue("compatibilityString")?.ToObject<string>();
					var version = jobject.GetValue("version")?.ToObject<string>();
					var name = jobject.GetValue("name")?.ToObject<string>();
					var authors = jobject.GetValue("authors")?.ToObject<string[]>();
					if (version is null || name is null || authors is null) {
						return false;
					}
					if (!Version.TryParse(version, out var ver)) {
						return false;
					}
					Version = ver;
					Authors = authors;
					FriendlyName = name;
					CompatibilityString = compatibilityString;
				}
				catch {
					return false;
				}
				return true;
			}

			public bool CheckIfValid() {
				if (!File.Exists(TargetMainFile)) {
					return false;
				}
				if (!File.Exists(TargetInfoFile)) {
					return false;
				}
				if (!LoadInfo()) {
					return false;
				}
				return true;
			}

			public bool LoadPlugin() {
				if (!CheckIfValid()) {
					RLog.Err($"Failed to load Plugin {PluginName} not Valid");
					return false;
				}
				_loadContext = new AssemblyLoadContext(PluginName, true);
				try {
					_assembly = _loadContext.LoadFromAssemblyPath(TargetMainFile);
				}
				catch (Exception e) {
					RLog.Err($"Failed to load Plugin {PluginName} Error: {e}");
				}
				if (_assembly is null) {
					_loadContext.Unload();
					_loadContext = null;
					BasePlugin = null;
					return false;
				}
				var trains = (from type in _assembly.GetTypes()
							  where type.IsAssignableTo(typeof(IPlugin))
							  select type).FirstOrDefault();
				if (trains is null) {
					_loadContext.Unload();
					_loadContext = null;
					BasePlugin = null;
					return false;
				}
				BasePlugin = (IPlugin)Activator.CreateInstance(trains);
				if (BasePlugin is null) {
					_loadContext.Unload();
					_loadContext = null;
					BasePlugin = null;
					return false;
				}
				return true;
			}


			public void UnLoadPlugin() {
				if (!IsLoaded) {
					RLog.Warn($"Tried to unload plugin {PluginName} when was already unloaded");
					return;
				}
				BasePlugin.Dispose();
				BasePlugin = null;
				_assembly = null;
				_loadContext.Unload();
				_loadContext = null;
			}
		}


		public static string PluginDir => Path.Combine(EngineHelpers.BaseDir, "Plugins");

		private Engine _engine;

		public void Dispose() {

		}

		public IEnumerable<Plugin> GetLoadedPlugins() {
			foreach (var item in _plugins.Values) {
				if (item.IsLoaded) {
					yield return item;
				}
			}
		}

		public readonly ConcurrentDictionary<string, Plugin> _plugins = new();

		public event Action PluginsRefreshed;

		/// <summary>
		/// Initializes the manager by passing an engine reference
		/// </summary>
		/// <param name="engine">The engine to reference</param>
		public void Init(Engine engine) {
			_engine = engine;
			RLog.Info("Loading Plugin Manager");
			if (!Directory.Exists(PluginDir)) {
				Directory.CreateDirectory(PluginDir);
			}
			RefreshPluginList(true);
		}

		public void RefreshPluginList(bool isStartUP = false) {
			foreach (var item in Directory.GetDirectories(PluginDir)) {
				if (!_plugins.TryGetValue(item, out var plugin)) {
					plugin = new Plugin {
						PluginFolder = item
					};
					if (!_plugins.TryAdd(item, plugin)) {
						throw new Exception("Failed to add plugin");
					}
				}
				RLog.Info($"Loading plugin info for {plugin.PluginName}");
				if (!plugin.CheckIfValid()) {
					RLog.Err($"Failed to Load Plugin {plugin.PluginName}");
					_plugins.Remove(item, out var _);
					continue;
				}
				if (!isStartUP) {
					continue;
				}
				if (!plugin.IsLoaded) {
					if (_engine.MainSettings.EnabledPlugins.Contains(plugin.PluginName)) {
						plugin.LoadPlugin();
					}
				}
			}
			PluginsRefreshed?.Invoke();
		}

		public void RenderStep() {
			foreach (var item in GetLoadedPlugins()) {
				try {
					item.BasePlugin.RenderStep();
				}
				catch (Exception e) {
					RLog.Err($"Failed to renderStep plugin {item.PluginName} Error: {e}");
				}
			}
		}

		public void Step() {
			foreach (var item in GetLoadedPlugins()) {
				try {
					item.BasePlugin.Step();
				}
				catch (Exception e) {
					RLog.Err($"Failed to step plugin {item.PluginName} Error: {e}");
				}
			}
		}
	}
}
