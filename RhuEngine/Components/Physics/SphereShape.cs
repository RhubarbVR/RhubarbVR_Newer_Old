﻿using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Physics" })]
	public sealed class SphereShape : PhysicsObject
	{
		[OnChanged(nameof(RebuildPysics))]
		[Default(0.50)]
		public readonly Sync<double> Radus;
		public override ColliderShape PysicsBuild() {
			return new RSphereShape(Radus);
		}
	}
}
