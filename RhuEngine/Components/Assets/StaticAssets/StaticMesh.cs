using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using SharedModels.GameSpecific;
using RhuEngine.AssetSystem;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/StaticAssets" })]
	public sealed class StaticMesh : StaticAsset<RMesh>
	{
		public override void LoadAsset(byte[] data) {
			try {
				if (!Engine.EngineLink.CanRender) {
					return;
				}
				Load(null);
				Load(new RMesh(CustomAssetManager.GetCustomAsset<ComplexMesh>(data), false));
			}
			catch(Exception err) {
				RLog.Err($"Failed to load Static Mesh Error {err}");
			}
		}
	}
}
