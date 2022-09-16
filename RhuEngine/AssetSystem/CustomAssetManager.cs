using System;
using System.Collections.Generic;
using System.Text;

using MessagePack;

using RNumerics;

using SharedModels.GameSpecific;

namespace RhuEngine.AssetSystem
{
	[Union(0, typeof(RhubarbAsset<ComplexMesh>))]
	public interface IRhubarbAsset
	{
		public string Name { get; set; }
	}
	[MessagePackObject]
	public class RhubarbAsset<T>: IRhubarbAsset
	{
		[Key(0)]
		public T Asset;
		[Key(1)]
		public string name;
		[IgnoreMember]
		public string Name { get =>name; set => name = value; }
		public RhubarbAsset(T asset,string assetName) {
			Asset = asset;
			name = assetName;
		}
		public RhubarbAsset() {

		}
	}

	public static class CustomAssetManager
	{
		public static T GetCustomAsset<T>(byte[] data) {
			return ((RhubarbAsset<T>)Serializer.Read<IRhubarbAsset>(data)).Asset;
		}

		public static byte[] SaveAsset<T>(T asset,string assetName) {
			return Serializer.Save<IRhubarbAsset>(new RhubarbAsset<T>(asset, assetName));
		}
	}
}
