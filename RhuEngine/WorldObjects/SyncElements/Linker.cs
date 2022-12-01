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
			if (Target == val) {
				_linked = val;
			}
			else {
				if (Linked) {
					Unlink();
				}
				_linked = val;
			}
		}
		public void RemoveLinkLocation() {
			_linked = null;
		}
		public void SetLinkerTarget(ILinkerMember<T> Target) {
			base.Target = Target;
		}
		public override void Bind() {
			if (Target != null) {
				Link();
			}
		}

		private void Link() {
			if (Linked) {
				Unlink();
			}
			Target.Link(this);
		}
		private void Unlink() {
			_linked?.KillLink();
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
