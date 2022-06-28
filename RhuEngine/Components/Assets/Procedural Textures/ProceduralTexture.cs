using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	public abstract class ProceduralTexture : AssetProvider<RTexture2D>
	{
		public abstract void Generate();

		public override void OnLoaded() 
		{
			ComputeTexture();
		}

		public void ComputeTexture() 
		{
			if (!Engine.EngineLink.CanRender) 
			{
				return;
			}

			RWorld.ExecuteOnEndOfFrame(this, () => {
				try {
					Generate();
				}
				catch (Exception e) {
#if DEBUG
					RLog.Err(e.ToString());
#endif
				}
			});
		}
	}
}
