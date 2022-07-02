using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using RhuEngine.Physics;
using BEPUik;
using System;

namespace RhuEngine.Components.Transform.IK.IKConstraints
{
	public abstract class IKConstraint<T>: Component where T : IKConstraint
	{
		public T LoadedValue { get; private set; }

		public abstract T LoadIn();


	}
}
