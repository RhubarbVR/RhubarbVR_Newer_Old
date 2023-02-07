namespace RhuEngine.WorldObjects
{
	public sealed class Linker<T> : SyncRef<ILinkerMember<T>>, ILinker, ISyncMember, IChangeable
	{
		public Linker() { }

		public T LinkedValue
		{
			get => Target.Value;
			set {
				if (Target is null) {
					return;
				}
				Target.Value = value;
			}
		}

		private ILinkable _linked;

		public bool Linked => _linked != null;

		public void SetLinkLocation(ILinkable val) {
			_linked = val;
		}
		public void RemoveLinkLocation() {
			_linked = null;
		}
		public void SetLinkerTarget(ILinkerMember<T> Target) {
			base.Target = Target;
		}
		public override void Bind() {
			Link();
		}

		private void Link() {
			Unlink();
			Target?.Link(this);
		}
		private void Unlink() {
			_linked?.KillLink();
			_linked = null;
		}

		public override void Dispose() {
			Unlink();
			base.Dispose();
		}

		public void SetValue(object value) {
			if (Linked) {
				LinkedValue = (T)value;
			}
		}
	}
}
