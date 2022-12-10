namespace RhuEngine.WorldObjects
{
	public sealed class DynamicLinker : SyncRef<ILinkable>, ILinker, ISyncMember
	{
		public DynamicLinker() { }

		public object LinkedValue
		{
			get => Target.Object;
			set => Target.Object = value;
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
		public void SetLinkerTarget(ILinkable Target) {
			base.Target = Target;
		}
		public override void OnChanged() {
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
	}
}
