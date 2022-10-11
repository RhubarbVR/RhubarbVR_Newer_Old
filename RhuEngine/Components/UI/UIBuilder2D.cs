using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	public class UIBuilder2D
	{
		public Stack<UIElement> Elements = new();

		public Stack<Entity> Entities = new();

		public Entity Entity
		{
			get => Entities.Peek();
			set => Entities.Push(value);
		}

		public UIElement Element
		{
			get => Elements.Peek();
			set => Elements.Push(value);
		}

		public UIBuilder2D(Entity entity) {
			Elements.Push(entity.GetFirstComponentOrAttach<UIElement>());
			Entities.Push(entity);
		}

		public UIBuilder2D(UIElement uIElement) {
			Elements.Push(uIElement);
			Entities.Push(uIElement.Entity);
		}

		public Entity PushEntity(string elements = "UIElement") {
			return Entity = Entity.AddChild(elements);
		}

		public T PushElement<T>(string elements = null) where T : UIElement, new() {
			elements ??= typeof(T).GetFormattedName();
			var trains = PushEntity(elements).AttachComponent<T>();
			Element = trains;
			return trains;
		}

		public T GetElement<T>() where T : UIElement {
			return Element is T data ? data : null;
		}


		public void Pop() {
			PopEntity();
			PopUIElement();
		}

		public void PopEntity() {
			Entities.Pop();
		}
		public void PopUIElement() {
			Elements.Pop();
		}
		public bool ClipContents
		{
			get => Element.ClipContents.Value;
			set => Element.ClipContents.Value = value;
		}
		public Vector2i MinSize
		{
			get => Element.MinSize.Value;
			set => Element.MinSize.Value = value;
		}
		public RLayoutDir LayoutDir
		{
			get => Element.LayoutDir.Value;
			set => Element.LayoutDir.Value = value;
		}
		public Vector2f Min
		{
			get => Element.Min.Value;
			set => Element.Min.Value = value;
		}
		public Vector2f Max
		{
			get => Element.Max.Value;
			set => Element.Max.Value = value;
		}
		public Vector2f MinOffset
		{
			get => Element.MinOffset.Value;
			set => Element.MinOffset.Value = value;
		}
		public Vector2f MaxOffset
		{
			get => Element.MaxOffset.Value;
			set => Element.MaxOffset.Value = value;
		}
		public RGrowHorizontal GrowHorizontal
		{
			get => Element.GrowHorizontal.Value;
			set => Element.GrowHorizontal.Value = value;
		}
		public RGrowVertical GrowVertical
		{
			get => Element.GrowVertical.Value;
			set => Element.GrowVertical.Value = value;
		}
		public float Rotation
		{
			get => Element.Rotation.Value;
			set => Element.Rotation.Value = value;
		}
		public Vector2f Scale
		{
			get => Element.Scale.Value;
			set => Element.Scale.Value = value;
		}
		public Vector2f PivotOffset
		{
			get => Element.PivotOffset.Value;
			set => Element.PivotOffset.Value = value;
		}
		public RFilling HorizontalFilling
		{
			get => Element.HorizontalFilling.Value;
			set => Element.HorizontalFilling.Value = value;
		}
		public RFilling VerticalFilling
		{
			get => Element.VerticalFilling.Value;
			set => Element.VerticalFilling.Value = value;
		}
		public float StretchRatio
		{
			get => Element.StretchRatio.Value;
			set => Element.StretchRatio.Value = value;
		}
		public bool AutoTranslate
		{
			get => Element.AutoTranslate.Value;
			set => Element.AutoTranslate.Value = value;
		}
		public RInputFilter InputFilter
		{
			get => Element.InputFilter.Value;
			set => Element.InputFilter.Value = value;
		}
		public bool ForceScrollEventPassing
		{
			get => Element.ForceScrollEventPassing.Value;
			set => Element.ForceScrollEventPassing.Value = value;
		}
		public RCursorShape CursorShape
		{
			get => Element.CursorShape.Value;
			set => Element.CursorShape.Value = value;
		}
	}
}
