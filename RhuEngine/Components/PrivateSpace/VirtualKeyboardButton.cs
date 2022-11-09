using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Components.PrivateSpace;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{

	[Category(new string[] { "Local" })]
	public class ToggleVirtualKeyboardButton : VirtualKeyboardButton
	{
		public readonly Sync<bool> SingleClick;
	}

	[Category(new string[] { "Local" })]
	public class VirtualKeyboardButton : BaseVirtualKeyboardButton
	{
		public sealed class KeyboardLayer : SyncObject
		{
			public readonly Sync<string> Label;

			public readonly Sync<string> stringClick;

			public readonly Sync<Key> keyCode;
		}

		public readonly SyncObjList<KeyboardLayer> keyboardLayer;
	}


	[Category(new string[] { "Local" })]
	public abstract class BaseVirtualKeyboardButton : Component
	{

		public readonly Sync<float> row;
		public readonly Sync<float> col;

		public readonly Sync<float> width;
		public readonly Sync<float> hight;
	}
}
