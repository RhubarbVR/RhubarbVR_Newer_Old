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

		public void Load(A data) {
			Value = data;
			Loaded = data != null;
			OnAssetLoaded?.Invoke(data);
		}

		public bool Loaded { get; private set; } = false;

		public override void Dispose() {
			Load(null);
			base.Dispose();
		}
	}
}
