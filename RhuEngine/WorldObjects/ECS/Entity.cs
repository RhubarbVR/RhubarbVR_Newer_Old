﻿using System;
using System.Collections.Generic;
using System.Linq;
using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Components;

namespace RhuEngine.WorldObjects.ECS
{
	public class Entity : SyncObject, IOffsetableElement
	{
		public uint Depth => (_internalParent?.Depth + 1) ?? 0;

		public uint CachedDepth { get; private set; }

		public SyncObjList<Entity> children;
		[OnChanged(nameof(ParentChanged))]
		public SyncRef<Entity> parent;
		[Default("Entity")]
		public Sync<string> name;
		public override string Name => name.Value;

		[OnChanged(nameof(TransValueChange))]
		public Sync<Vector3f> position;
		[OnChanged(nameof(TransValueChange))]
		public Sync<Quaternionf> rotation;
		[OnChanged(nameof(TransValueChange))]
		public Sync<Vector3f> scale;
		[Default(true)]
		[OnChanged(nameof(OnEnableChange))]
		public Sync<bool> enabled;
		[OnChanged(nameof(OnOrderOffsetChange))]
		public Sync<int> orderOffset;
		public int Offset => orderOffset.Value;
		[OnChanged(nameof(OnComponentChange))]
		public SyncAbstractObjList<Component> components;

		[Default(true)]
		public Sync<bool> persistence;

		public override bool Persistence => persistence.Value;

		public void ParentDepthUpdate() {
			CachedDepth = Depth;
			foreach (var child in children) {
				((Entity)child).ParentDepthUpdate();
			}
		}

		private void OnComponentChange() {
			if (_hasUpdatingComponentSave != HasUpdatingComponent) {
				_hasUpdatingComponentSave = !_hasUpdatingComponentSave;
				UpdateEnableList();
			}
		}

		private void OnOrderOffsetChange() {
			OffsetChanged?.Invoke();
		}

		[Exsposed]
		public T AttachComponent<T>(Type type) where T : Component {
			if(!typeof(T).IsAssignableFrom(type)) {
				throw new ArgumentException($"Type {type.GetFormattedName()} is not assignable to {typeof(T).GetFormattedName()}");
			}
			var comp = components.Add(type);
			comp.OnAttach();
			return (T)comp;
		}

		[Exsposed]
		public T AttachComponent<T>() where T : Component, new() {
			var comp = components.Add<T>();
			comp.OnAttach();
			return comp;
		}
		public T AttachComponent<T>(Action<T> beforeAttach) where T : Component, new() {
			var comp = components.Add<T>();
			beforeAttach.Invoke(comp);
			comp.OnAttach();
			return comp;
		}

		[Exsposed]
		public T GetFirstComponentOrAttach<T>() where T : Component, new() {
			foreach (var item in components) {
				if (typeof(T).IsAssignableFrom(item.GetType())) {
					return (T)item;
				}
			}
			return AttachComponent<T>();
		}
		[Exsposed]
		public T GetFirstComponent<T>() where T : Component {
			foreach (var item in components) {
				if (typeof(T).IsAssignableFrom(item.GetType())) {
					return (T)item;
				}
			}
			return null;
		}
		[Exsposed]
		public IEnumerable<T> GetAllComponents<T>() where T : Component {
			foreach (var item in components) {
				if (typeof(T).IsAssignableFrom(item.GetType())) {
					yield return (T)item;
				}
			}
		}
		[Exsposed]
		public Matrix GlobalToLocal(Matrix point,bool Child = true) {
			var parentMatrix = Child ? GlobalTrans : parent.Target?.GlobalTrans ?? Matrix.Identity;
			var newLocal = point * parentMatrix.Inverse;
			return newLocal;
		}
		public void GlobalToLocal(Matrix point, bool Child, out Vector3f translation, out Quaternionf rotation, out Vector3f scale) {
			GlobalToLocal(point,Child).Decompose(out translation, out rotation, out scale);
		}
		[Exsposed]
		public Vector3f GlobalPointToLocal(Vector3f point, bool Child = true) {
			GlobalToLocal(Matrix.T(point),Child,out var newTranslation, out _, out _);
			return newTranslation;
		}
		[Exsposed]
		public Vector3f GlobalScaleToLocal(Vector3f Scale, bool Child = true) {
			GlobalToLocal(Matrix.S(Scale), Child, out _, out _, out var newScale);
			return newScale;
		}
		[Exsposed]
		public Quaternionf GlobalRotToLocal(Quaternionf Rot, bool Child = true) {
			GlobalToLocal(Matrix.R(Rot), Child, out _, out var newRotation, out _);
			return newRotation;
		}
		[Exsposed]
		public Matrix LocalToGlobal(Matrix point,bool Child = true) {
			return point * (Child ? GlobalTrans : _internalParent?.GlobalTrans ?? Matrix.Identity);
		}
		public void LocalToGlobal(Matrix point, bool Child, out Vector3f translation, out Quaternionf rotation, out Vector3f scale) {
			LocalToGlobal(point, Child).Decompose(out translation, out rotation, out scale);
		}
		[Exsposed]
		public Quaternionf LocalRotToGlobal(Quaternionf Rot, bool Child = true) {
			LocalToGlobal(Matrix.R(Rot),Child,out _, out var newRotation, out _);
			return newRotation;
		}
		[Exsposed]
		public Vector3f LocalScaleToGlobal(Vector3f scale, bool Child = true) {
			LocalToGlobal(Matrix.S(scale), Child, out _, out _, out var newScale);
			return newScale;
		}
		[Exsposed]
		public Vector3f LocalPosToGlobal(Vector3f pos, bool Child = true) {
			LocalToGlobal(Matrix.T(pos), Child, out var newPos, out _, out _);
			return newPos;
		}

		private Entity _internalParent;

		[NoShow]
		[NoSave]
		[NoSync]
		[NoLoad]
		public UIRect UIRect { get; private set; }

		public void SetUIRect(UIRect newrect) {
			var oldrec = UIRect;
			UIRect = newrect;
			UIRectUpdate?.Invoke(oldrec, UIRect);
		}


		public event Action<UIRect, UIRect> UIRectUpdate;


		private Matrix _cachedGlobalMatrix = Matrix.S(1);

		private Matrix _cachedLocalMatrix = Matrix.S(1);

		public bool CheckIfParented(Entity entity) {
			return entity == this || (_internalParent?.CheckIfParented(entity) ?? false);
		}
		public bool parentEnabled = true;

		public event Action EnabledChanged;

		private bool _hasUpdatingComponentSave;
		[Exsposed]
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
			if (IsEnabled) {
				foreach (var item in components) {
					((Component)item).AddListObject();
				}
			}
			else {
				foreach (var item in components) {
					((Component)item).RemoveListObject();
				}
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

		public override void OnLoaded() {
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
				foreach (var entity in children) {
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
			foreach (var entity in children) {
				((Entity)entity).ParentEnabledChange(enabled.Value);
			}
			EnabledChanged?.Invoke();
			UpdateEnableList();
		}
		[Exsposed]
		public bool IsEnabled => parentEnabled && enabled.Value;
		private void ParentChanged() {
			if (World.RootEntity == this) {
				return;
			}

			if (parent.Target == _internalParent) {
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
			if (World != parent.Target.World) {
				RLog.Warn("tried to set parent from another world");
				return;
			}
			if (!parent.Target.CheckIfParented(this)) {
				parent.Target.children.AddInternal(this);
				_internalParent.children.RemoveInternal(this);
				_internalParent = parent.Target;
				ParentDepthUpdate();
				ParentEnabledChange(_internalParent.IsEnabled);
				TransValueChange();
			}
			else {
				parent.Target = _internalParent;
			}
		}
		[Exsposed]
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
		[Exsposed]
		public bool IsRoot => World?.RootEntity == this;

		public event Action<Entity> GlobalTransformChange;

		public event Action OffsetChanged;
		[Exsposed]
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
				GlobalTransformChange?.Invoke(this);
				foreach (Entity item in children) {
					item.GlobalTransMark();
				}
			}
		}
		[Exsposed]
		public Matrix LocalTrans {
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
				GlobalTransformChange?.Invoke(this);
				foreach (Entity item in children) {
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

		internal void GlobalTransMark() {
			if (IsRoot) {
				return;
			}
			_dirtyGlobal = true;
			foreach (Entity item in children) {
				item.GlobalTransMark();
			}
		}

		public override void FirstCreation() {
			base.FirstCreation();
			rotation.Value = Quaternionf.Identity;
			scale.Value = Vector3f.One;
		}

		public void Step() {
			foreach (var item in components) {
				var comp = (Component)item;
				comp.AlwaysStep();
				if (comp.Enabled.Value) {
					comp.Step();
				}
			}
		}
		[Exsposed]
		public Entity AddChild(string name = "Entity") {
			var entity = children.Add();
			entity.parent.Target = this;
			entity.name.Value = name;
			return entity;
		}

		public override void OnInitialize() {
			World.RegisterEntity(this);
		}
		public override void Dispose() {
			base.Dispose();
			World.UnregisterEntity(this);
			World.UnregisterUpdatingEntity(this);
		}

		public Entity() {
		}
	}
}
