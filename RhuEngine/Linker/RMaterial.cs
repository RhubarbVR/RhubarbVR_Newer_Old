using System;
using System.Collections.Generic;
using System.Text;

using RNumerics;

using static System.Net.Mime.MediaTypeNames;

namespace RhuEngine.Linker
{
	public interface IRMaterial : IDisposable
	{
		public int GetRenderPriority();
		public void SetRenderPriority(int renderPri);
	}


	public interface IUnlitMaterial : IRMaterial
	{
		public bool NoDepthTest { set; }

		public bool DullSided { set; }

		public Transparency Transparency { set; }

		public RTexture2D Texture { set; }

		public Colorf Tint { set; }
	}


	public class RUnlitMaterial : RMaterial
	{
		public IUnlitMaterial UnlitMaterial => (IUnlitMaterial)Inst;

		public static Type Instance { get; set; }

		public RUnlitMaterial() : this(null) {

		}

		public RUnlitMaterial(IRMaterial target) : base(target) {
			if (typeof(RUnlitMaterial) == GetType()) {
				Inst = target ?? (IUnlitMaterial)Activator.CreateInstance(Instance);
			}
		}

		public bool NoDepthTest
		{
			set => UnlitMaterial.NoDepthTest = value;
		}

		public bool DullSided
		{
			set => UnlitMaterial.DullSided = value;
		}


		public Transparency Transparency
		{
			set => UnlitMaterial.Transparency = value;
		}


		public RTexture2D Texture
		{
			set => UnlitMaterial.Texture = value;
		}


		public Colorf Tint
		{
			set => UnlitMaterial.Tint = value;
		}

	}

	public class RMaterial : IDisposable
	{
		public int RenderPriority
		{
			set {
				lock (this) {
					Inst.SetRenderPriority(value);
				}
			}
			get {
				lock (this) {
					return Inst.GetRenderPriority();
				}
			}
		}

		public IRMaterial Inst { get; set; }

		public RMaterial(IRMaterial target) {
			if (typeof(RMaterial) == GetType()) {
				Inst = target;
			}
		}

		public event Action<RMaterial> OnDispose;

		public void Dispose() {
			OnDispose?.Invoke(this);
			Inst?.Dispose();
			Inst = null;
			GC.SuppressFinalize(this);
		}
	}
}
