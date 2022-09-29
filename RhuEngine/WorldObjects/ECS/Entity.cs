using System;
using System.Collections.Generic;
using System.Linq;
using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Components;

namespace RhuEngine.WorldObjects.ECS
{
	public sealed class Entity : SyncObject, IOffsetableElement, IWorldBoundingBox
	{
		private uint CompDepth => (_internalParent?.Depth + 1) ?? 0;

		public uint Depth { get; private set; }

		public readonly SyncObjList<Entity> children;
		[OnChanged(nameof(ParentChanged))]
		public readonly SyncRef<Entity> parent;
		[Default("Entity")]
		public readonly Sync<string> name;
		public override string Name => name.Value;

		[OnChanged(nameof(TransValueChange))]
		public readonly Sync<Vector3f> position;
		[OnChanged(nameof(TransValueChange))]
		public readonly Sync<Quaternionf> rotation;
		[OnChanged(nameof(TransValueChange))]
		public readonly Sync<Vector3f> scale;
		[OnChanged(nameof(OnOrderOffsetChange))]
		public readonly Sync<int> orderOffset;
		public int Offset => orderOffset.Value;
		[Default(true)]
		[OnChanged(nameof(OnEnableChange))]
		public readonly Sync<bool> enabled;

		[Default(true)]
		public readonly Sync<bool> persistence;

		public override bool Persistence => persistence.Value;

		[OnChanged(nameof(OnComponentChange))]
		public readonly SyncAbstractObjList<IComponent> components;
		[Exposed]
		public AxisAlignedBox3f Bounds
		{
			get {
				var box = AxisAlignedBox3f.Zero;
				foreach (var item in components) {
					if (item is IWorldBoundingBox boundingBox) {
						var scale = boundingBox.Bounds;
						scale.Scale(GlobalTrans.Scale);
						box = BoundsUtil.Combined(box, scale);
					}
				}
				foreach (Entity item in children.Cast<Entity>()) {
					var element = item.Bounds;
					element.Translate(item.GlobalTrans.Translation);
					element.Rotate(item.GlobalTrans.Rotation);
					element.Scale(GlobalTrans.Scale);
					box = BoundsUtil.Combined(box, element);
				}
				return box;
			}
		}


		public void DestroyChildren() {
			children.Clear();
		}



		public event Action<GrabbableHolder, bool, float> OnGrip;
		internal void CallOnGrip(GrabbableHolder obj, bool Laser, float gripForce) {
			OnGrip?.Invoke(obj, Laser, gripForce);
		}
		public event Action<uint, Vector3f, Vector3f, float, float, Handed> OnLazerPyhsics;

		internal void CallOnLazer(uint v, Vector3f hitnormal, Vector3f hitpointworld, float pressForce, float gripForce, Handed handed) {
			OnLazerPyhsics?.Invoke(v, hitnormal, hitpointworld, pressForce, gripForce, handed);
		}

		public event Action<uint, Vector3f, Vector3f, Handed> OnTouchPyhsics;

		internal void CallOnTouch(uint v, Vector3f hitnormal, Vector3f hitpointworld, Handed handedSide) {
			OnTouchPyhsics?.Invoke(v, hitnormal, hitpointworld, handedSide);
		}
		public void ParentDepthUpdate() {
			Depth = CompDepth;
			foreach (Entity child in children.Cast<Entity>()) {
				child.ParentDepthUpdate();
			}
		}

		internal void SetGlobalMatrixPysics(Matrix matrix) {
			var parentMatrix = Matrix.S(Vector3f.One);
			if (_internalParent != null) {
				parentMatrix = _internalParent.GlobalTrans;
			}
			var newLocal = matrix * parentMatrix.Inverse;
			newLocal.Decompose(out var newtranslation, out var newrotation, out var newscale);
			position.SetValueNoOnChange(newtranslation);
			rotation.SetValueNoOnChange(newrotation);
			scale.SetValueNoOnChange(newscale);
			_cachedGlobalMatrix = matrix;
			_cachedLocalMatrix = newLocal;
			GlobalTransformChange?.Invoke(this, false);
			foreach (var item in children.Cast<Entity>()) {
				item.GlobalTransMark(false);
			}
		}

		public override void Destroy() {
			if (IsRoot) {
				return;
			}
			base.Destroy();
		}

		[Exposed]
		public Entity GetChildByName(string v) {
			foreach (var child in children.Cast<Entity>()) {
				if (((Entity)child).name.Value == v) {
					return (Entity)child;
				}
			}
			return null;
		}

		private void OnComponentChange() {
			if (_hasUpdatingComponentSave != HasUpdatingComponent) {
				_hasUpdatingComponentSave = !_hasUpdatingComponentSave;
				UpdateEnableList();
			}
			UIRect = GetFirstComponent<UIRect>();
		}

		private void OnOrderOffsetChange() {
			OffsetChanged?.Invoke();
		}
		[Exposed]
		public IComponent AttachComponent(Type type) {
			if (!typeof(Component).IsAssignableFrom(type)) {
				throw new ArgumentException($"Type {type.GetFormattedName()} is not assignable to {typeof(Component).GetFormattedName()}");
			}
			var comp = components.Add(type);
			comp.RunAttach();
			return comp;
		}
		[Exposed]
		public T AttachComponent<T>(Type type) where T : class, IComponent {
			if (!typeof(T).IsAssignableFrom(type)) {
				throw new ArgumentException($"Type {type.GetFormattedName()} is not assignable to {typeof(T).GetFormattedName()}");
			}
			var comp = components.Add(type);
			comp.RunAttach();
			return (T)comp;
		}

		[Exposed]
		public T AttachComponent<T>() where T : Component, new() {
			var comp = components.Add<T>();
			comp.RunAttach();
			return comp;
		}
		public T AttachComponent<T>(Action<T> beforeAttach) where T : Component, new() {
			var comp = components.Add<T>();
			beforeAttach.Invoke(comp);
			comp.RunAttach();
			return comp;
		}

		[Exposed]
		public T GetFirstComponentOrAttach<T>() where T : Component, new() {
			foreach (var item in components) {
				if (typeof(T).IsAssignableFrom(item.GetType())) {
					return (T)item;
				}
			}
			return AttachComponent<T>();
		}
		[Exposed]
		public T GetFirstComponent<T>() where T : Component {
			foreach (var item in components) {
				if (typeof(T).IsAssignableFrom(item.GetType())) {
					return (T)item;
				}
			}
			return null;
		}
		[Exposed]
		public IEnumerable<T> GetAllComponents<T>() where T : Component {
			foreach (var item in components) {
				if (typeof(T).IsAssignableFrom(item.GetType())) {
					yield return (T)item;
				}
			}
		}
		[Exposed]
		public Matrix GlobalToLocal(Matrix point, bool Child = true) {
			var parentMatrix = Child ? GlobalTrans : parent.Target?.GlobalTrans ?? Matrix.Identity;
			var newLocal = point * parentMatrix.Inverse;
			return newLocal;
		}
		public void GlobalToLocal(Matrix point, bool Child, out Vector3f translation, out Quaternionf rotation, out Vector3f scale) {
			GlobalToLocal(point, Child).Decompose(out translation, out rotation, out scale);
		}
		[Exposed]
		public Vector3f GlobalPointToLocal(Vector3f point, bool Child = true) {
			GlobalToLocal(Matrix.T(point), Child, out var newTranslation, out _, out _);
			return newTranslation;
		}
		[Exposed]
		public Vector3f GlobalScaleToLocal(Vector3f Scale, bool Child = true) {
			GlobalToLocal(Matrix.S(Scale), Child, out _, out _, out var newScale);
			return newScale;
		}
		[Exposed]
		public Quaternionf GlobalRotToLocal(Quaternionf Rot, bool Child = true) {
			GlobalToLocal(Matrix.R(Rot), Child, out _, out var newRotation, out _);
			return newRotation;
		}
		[Exposed]
		public Matrix LocalToGlobal(Matrix point, bool Child = true) {
			return point * (Child ? GlobalTrans : _internalParent?.GlobalTrans ?? Matrix.Identity);
		}
		public void LocalToGlobal(Matrix point, bool Child, out Vector3f translation, out Quaternionf rotation, out Vector3f scale) {
			LocalToGlobal(point, Child).Decompose(out translation, out rotation, out scale);
		}
		[Exposed]
		public Quaternionf LocalRotToGlobal(Quaternionf Rot, bool Child = true) {
			LocalToGlobal(Matrix.R(Rot), Child, out _, out var newRotation, out _);
			return newRotation;
		}
		[Exposed]
		public Vector3f LocalScaleToGlobal(Vector3f scale, bool Child = true) {
			LocalToGlobal(Matrix.S(scale), Child, out _, out _, out var newScale);
			return newScale;
		}
		[Exposed]
		public Vector3f LocalPosToGlobal(Vector3f pos, bool Child = true) {
			LocalToGlobal(Matrix.T(pos), Child, out var newPos, out _, out _);
			return newPos;
		}



		[NoShow]
		[NoSave]
		[NoSync]
		[NoLoad]
		[UnExsposed]
		private Entity _internalParent;

		private Matrix _cachedGlobalMatrix = Matrix.S(1);

		private Matrix _cachedLocalMatrix = Matrix.S(1);

		public bool CheckIfParented(Entity entity) {
			return entity == this || (_internalParent?.CheckIfParented(entity) ?? false);
		}

		public bool parentEnabled = true;

		public event Action EnabledChanged;

		private bool _hasUpdatingComponentSave;
		[Exposed]
		public bool HasUpdatingComponent
		{
			get {
				if (components.Count == 0) {
					return false;
				}
				foreach (var item in components) {
					foreach (var aitem in item.GetType().GetCustomAttributes(true)) {
						if (typeof(UpdatingComponentAttribute).IsAssignableFrom(aitem.GetType())) {
							return true;
						}
					}
				}
				return false;
			}
		}

		private void UpdateEnableList() {
			if (IsRemoved || IsDestroying) {
				return;
			}
			foreach (var item in components) {
				((Component)item).ListObjectUpdate(IsEnabled);
			}
			if (IsEnabled && _hasUpdatingComponentSave) {
				try {
					World.RegisterUpdatingEntity(this);
				}
				catch { }
			}
			else {
				try {
					World.UnregisterUpdatingEntity(this);
				}
				catch { }
			}
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			UpdateEnableList();
			TransValueChange();
		}

		public void ParentEnabledChange(bool _parentEnabled) {
			if (!enabled.Value) {
				return;
			}

			if (_parentEnabled != parentEnabled) {
				parentEnabled = _parentEnabled;
				foreach (var entity in children.Cast<Entity>()) {
					((Entity)entity).ParentEnabledChange(_parentEnabled);
				}
			}
			EnabledChanged?.Invoke();
			UpdateEnableList();
		}
		private void OnEnableChange() {
			if (!enabled.Value && (World.RootEntity == this)) {
				enabled.Value = true;
			};
			foreach (var entity in children.Cast<Entity>()) {
				((Entity)entity).ParentEnabledChange(enabled.Value);
			}
			EnabledChanged?.Invoke();
			UpdateEnableList();
		}
		[Exposed]
		public bool IsEnabled => parentEnabled && enabled.Value && !IsDestroying && !IsRemoved;

		private void GoBackToOld() {
			if (parent.Target != _internalParent) {
				parent.Target = _internalParent;
			}
		}

		private bool IsParrent(Entity check) {
			return (check == this) || (_internalParent?.IsParrent(check) ?? false);
		}

		private void ParentChanged() {
			try {
				if (parent.Target is null) {
					return;
				}

				if (World.RootEntity == this) {
					GoBackToOld();
					return;
				}

				if (parent.Target == _internalParent) {
					return;
				}

				if (parent.Target.IsParrent(this)) {
					GoBackToOld();
					return;
				}
				if (World != parent.Target.World) {
					RLog.Warn("tried to set parent from another world");
					GoBackToOld();
					return;
				}
				if (_internalParent == null) {
					_internalParent = parent.Target;
					ParentDepthUpdate();
					TransValueChange();
					return;
				}
				if (parent.Target == null) {
					parent.Target = World.RootEntity;
					ParentDepthUpdate();
					return;
				}
				parent.Target.children.AddInternal(this);
				_internalParent.children.RemoveInternal(this);
				_internalParent = parent.Target;
				ParentDepthUpdate();
				ParentEnabledChange(_internalParent.IsEnabled);
				TransValueChange();
			}
			catch {

			}
		}
		[Exposed]
		public void SetParent(Entity entity, bool preserverGlobal = true, bool resetPos = false) {
			var mach = GlobalTrans;
			parent.Target = entity;
			if (preserverGlobal) {
				GlobalTrans = mach;
			}
			else if (resetPos) {
				GlobalTrans = entity.GlobalTrans;
			}
		}
		[Exposed]
		public bool IsRoot => World?.RootEntity == this;

		public event Action<Entity, bool> GlobalTransformChange;

		public event Action OffsetChanged;
		[Exposed]
		public Matrix GlobalTrans
		{
			get {
				if (_dirtyGlobal) {
					_cachedGlobalMatrix = LocalTrans * _internalParent?.GlobalTrans ?? Matrix.Identity;
					_dirtyGlobal = false;
				}
				return _cachedGlobalMatrix;
			}
			set {
				var parentMatrix = Matrix.S(Vector3f.One);
				if (_internalParent != null) {
					parentMatrix = _internalParent.GlobalTrans;
				}
				var newLocal = value * parentMatrix.Inverse;
				newLocal.Decompose(out var newtranslation, out var newrotation, out var newscale);
				position.SetValueNoOnChange(newtranslation);
				rotation.SetValueNoOnChange(newrotation);
				scale.SetValueNoOnChange(newscale);
				_cachedGlobalMatrix = value;
				_cachedLocalMatrix = newLocal;
				GlobalTransformChange?.Invoke(this, true);
				foreach (Entity item in children.Cast<Entity>()) {
					item.GlobalTransMark();
				}
			}
		}
		[Exposed]
		public Matrix LocalTrans
		{
			get {
				if (_dirtyLocal) {
					_cachedLocalMatrix = Matrix.TRS(position.Value, rotation.Value, scale.Value);
					_dirtyLocal = false;
				}
				return _cachedLocalMatrix;
			}
			set {
				var parentMatrix = Matrix.S(Vector3f.One);
				if (_internalParent != null) {
					parentMatrix = _internalParent.GlobalTrans;
				}
				value.Decompose(out var newtranslation, out var newrotation, out var newscale);
				position.SetValueNoOnChange(newtranslation);
				rotation.SetValueNoOnChange(newrotation);
				scale.SetValueNoOnChange(newscale);
				_cachedGlobalMatrix = value * parentMatrix;
				_cachedLocalMatrix = value;
				GlobalTransformChange?.Invoke(this, true);
				foreach (var item in children.Cast<Entity>()) {
					item.GlobalTransMark();
				}
			}
		}

		private bool _dirtyGlobal;
		private bool _dirtyLocal;

		private void TransValueChange() {
			if (IsRoot) {
				return;
			}
			_dirtyLocal = true;
			GlobalTransMark();
		}

		internal void GlobalTransMark(bool physics = true) {
			if (IsRoot) {
				return;
			}
			if (!_dirtyGlobal) {
				foreach (var child in children.Cast<Entity>()) {
					child.GlobalTransMark(physics);
				}
				GlobalTransformChange?.Invoke(this, physics);
				_dirtyGlobal = true;
			}
		}

		protected override void FirstCreation() {
			base.FirstCreation();
			rotation.Value = Quaternionf.Identity;
			scale.Value = Vector3f.One;
		}

		public void RenderStep() {
			foreach (var item in components) {
				var comp = (Component)item;
				comp.RunRenderStep(comp.Enabled.Value && IsEnabled);
			}
		}

		public void Step() {
			foreach (var item in components) {
				var comp = (Component)item;
				comp.RunStep(comp.Enabled.Value && IsEnabled);
			}
		}
		[Exposed]
		public Entity AddChild(string name = "Entity") {
			var entity = children.Add();
			entity.name.Value = name;
			return entity;
		}

		protected override void OnInitialize() {
			World.RegisterEntity(this);
			if (Parent?.Parent is Entity par) {
				parent.NetValue = par.Pointer;
				if (par.Depth >= 10000) {
					throw new Exception("Max Entity Depth Reached");
				}
			}
		}
		public override void Dispose() {
			base.Dispose();
			World.UnregisterEntity(this);
			if (HasUpdatingComponent) {
				World.UnregisterUpdatingEntity(this);
			}
		}
		[NoShow]
		[NoSave]
		[NoSync]
		[NoLoad]
		[UnExsposed]
		public UIRect UIRect { get; private set; }

		public Entity() {
		}
	}
}
