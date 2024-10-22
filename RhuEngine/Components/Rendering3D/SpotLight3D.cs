﻿using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Linq;

namespace RhuEngine.Components
{

	[Category(new string[] { "Rendering3D" })]
	public sealed partial class SpotLight3D : Light3D
	{
		[Default(5f)]
		public readonly Sync<float> Range;

		[Default(1f)]
		public readonly Sync<float> Attenuation;

		[Default(45f)]
		public readonly Sync<float> Angle;

		[Default(1f)]
		public readonly Sync<float> AngleAttenuation;
	}
}
