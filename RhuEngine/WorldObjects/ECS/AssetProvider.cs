using System;

namespace RhuEngine.WorldObjects.ECS
{
	public interface IAssetProvider<A> : ISyncObject where A : class
	{
		public event Action<A> OnAssetLoaded;

		public A Value { get; }

		public bool Loaded { get; }

	}
	public abstract class AssetProvider<A> : Component, IAssetProvider<A> where A : class
	{
		public event Action<A> OnAssetLoaded;

		public A Value { get; private set; }

		public virtual bool AutoDisposes => true;

		public void Load(A data) {
			if(AutoDisposes && Value is IDisposable disposable) {
				disposable.Dispose();
			}
			Value = data;
			Loaded = data != null;
			OnAssetLoaded?.Invoke(data);
		}

		public bool Loaded { get; private set; } = false;

		public override void Dispose() {
			IsDestroying= true;
			Load(null);
			base.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
