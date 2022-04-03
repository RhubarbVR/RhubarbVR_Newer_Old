using System;
using System.Reflection;

using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;


namespace RhuEngine.WorldObjects
{
	public class AssetRef<T> : SyncRef<IAssetProvider<T>>, IAssetRef, IWorldObject where T : class
	{
		public T Asset => base.Target?.Value;

		public event Action<T> LoadChange;

		public void LoadedCall(T newAsset) {
			LoadChange?.Invoke(newAsset);
		}

		public override void Bind() {
			if (base.Target is null) {
				return;
			}
			base.Target.OnAssetLoaded += LoadedCall;
			if (base.Target.Loaded) {
				LoadedCall(Target.Value);
			}
		}

		public override void Unbind() {
			if (base.Target is not null) {
				base.Target.OnAssetLoaded -= LoadedCall;
			}
		}

		public override void OnLoaded() {
			base.OnLoaded();
			if (base.Target is null) {
				return;
			}
			base.Target.OnAssetLoaded += LoadedCall;
			if (base.Target.Loaded) {
				LoadedCall(base.Target.Value);
			}
		}


		public void BindMethod(string name, object obje) {
			var method = obje.GetType().GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (method is null) {
				RLog.Err($"Method not found {name}");
			}
			else {
				var prams = method.GetParameters();
				if (prams.Length == 0) {
					LoadChange += (obj) => method.Invoke(obje, new object[0] { });
				}
				else if (prams[0].ParameterType == typeof(T)) {
					LoadChange += (obj) => method.Invoke(obje, new object[1] { obj });
				}
				else {
					RLog.Err($"Cannot use method {name} on type {GetType().GetFormattedName()}");
				}
			}
		}
	}
}
