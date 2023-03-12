using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	public enum RGlobalIlluminationMode
	{
		Diabled,
		Static,
		Dynamic
	}
	public enum RLightMapScale
	{
		OneX,
		TwoX,
		FourX,
		EightX

	}
	public enum RFadeMode
	{
		Diabled,
		Self,
		Dependencies,
	}

	public abstract partial class GeometryInstance3D : VisualInstance3D
	{
		[Default(ShadowCast.Off)]
		public readonly Sync<ShadowCast> CastShadows;
		public readonly Sync<float> Transparency;
		public readonly Sync<float> ExtraCullMargin;
		public readonly Sync<bool> IgnoreOcclusionCulling;
		public readonly Sync<RGlobalIlluminationMode> GlobalIlluminationMode;
		public readonly Sync<RLightMapScale> LightMapScale;
		public readonly Sync<float> VisibilityBegin;
		public readonly Sync<float> VisibilityBeginMargin;
		public readonly Sync<float> VisibilityEnd;
		public readonly Sync<float> VisibilityEndMargin;
		public readonly Sync<RFadeMode> FadeMode;


	}
}
