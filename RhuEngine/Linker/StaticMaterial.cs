using System;
using System.Collections.Generic;
using System.Text;

using RNumerics;

namespace RhuEngine.Linker
{

	public enum BasicRenderMode
	{
		Opaque,
		CutOut,
		Transparent
	}

	public interface IUnlitMaterial : IStaticMaterial
	{
		public bool NoDepthTest { set; }

		public bool DullSided { set; }

		public Transparency Transparency { set; }

		public RTexture2D Texture { set; }

		public Colorf Tint { set; }
	}
	public interface IStaticMaterialManager
	{
		public IUnlitMaterial CreateUnlitMaterial();
	}


	public static class StaticMaterialManager
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "<Pending>")]
		public static IStaticMaterialManager Instanances;

		public static T GetMaterial<T>() where T : IStaticMaterial {
			return typeof(T) == typeof(IUnlitMaterial) ? (T)Instanances.CreateUnlitMaterial() : default;
		}
	}

	public interface IStaticMaterial : IDisposable
	{
		public RMaterial Material { get; }

	}


	public abstract class StaticMaterialBase<T> : IStaticMaterial
	{
		public RMaterial Material { get; } = new(null);

		public T YourData;

		public void UpdateMaterial(T rMaterial) {
			YourData = rMaterial;
			Material.Target = rMaterial;
		}

		public void Dispose() {
			Material.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
