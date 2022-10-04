using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Materials" })]
	public sealed class DynamicMaterial : AssetProvider<RMaterial>
	{
		[OnAssetLoaded(nameof(LoadMaterial))]
		public readonly AssetRef<RShader> shader;

		public readonly SyncAbstractObjList<PramInfo> Prams;

		[OnChanged(nameof(UpdateRenderOrder))]
		public readonly Sync<int> RenderOrderOffset;

		public void UpdateRenderOrder() {
			_material.RenderOrderOffset = RenderOrderOffset;
		}

		public void UpdatePram(string pram, object data) {
			if (_material is not null) {
				_material[pram] = data;
			}
		}

		public (PramInfo, int) GetParam(string name) {
			var index = 0;
			foreach (var item in Prams) {
				var data = (PramInfo)item;
				if (data.name.Value == name) {
					return (data, index);
				}
				index++;
			}
			return (null, 0);
		}

		public void SetPram(string name, object data) {
			try {
				GetParam(name).Item1?.SetValue(data);
			}
			catch { }
		}

		public abstract class PramInfo : SyncObject
		{
			public readonly Sync<string> name;
			public abstract object GetData();

			public void OnValueChangeed() {
				MaterialLoaded();
			}

			public abstract void SetValue(object newValue);

			public void MaterialLoaded() {
				try {
					var mit = (DynamicMaterial)Parent.Parent;
					mit.UpdatePram(name.Value, GetData());
				}
				catch { }
			}
		}
		[GenericTypeConstraint()]
		public class ValuePramInfo<T> : PramInfo
		{
			[OnChanged(nameof(OnValueChangeed))]
			public readonly Sync<T> value;

			public override void SetValue(object newValue) {
				try {
					value.Value = (T)newValue;
				}
				catch { }
			}


			public override object GetData() {
				return value.Value;
			}
		}

		public class TexPramInfo : PramInfo
		{
			[OnAssetLoaded(nameof(OnValueChangeed))]
			public readonly AssetRef<RTexture2D> value;

			public override void SetValue(object newValue) {
				try {
					value.Target = (AssetProvider<RTexture2D>)newValue;
				}
				catch { }
			}

			public override object GetData() {
				return value.Target is null ? RTexture2D.White : value.Asset ?? Engine.staticResources.Null;
			}
		}

		RMaterial _material;

		public bool TrySet(string name, object data) {
			try {
				var pram = GetParam(name);
				if (pram.Item1 is null) {
					return false;
				}
				pram.Item1.SetValue(data);        
				return true;
			}
			catch {
				RLog.Warn($"Failed to get pram {name}");
				return false;
			}
		}

		public Transparency Transparency
		{
			get => (Transparency)GetParam("TRANSPARENCY_RHUBARB_CUSTOM").Item1.GetData();
			set => TrySet("TRANSPARENCY_RHUBARB_CUSTOM", value);
		}
		public IAssetProvider<RTexture2D> MainTexture
		{
			get => (IAssetProvider<RTexture2D>) GetParam("MAINTEXTURE_RHUBARB_CUSTOM").Item1.GetData();
			set => TrySet("MAINTEXTURE_RHUBARB_CUSTOM", value);
		}
		public bool WireFrame
		{
			get => (bool)GetParam("WIREFRAME_RHUBARB_CUSTOM").Item1.GetData();
			set => TrySet("WIREFRAME_RHUBARB_CUSTOM", value);
		}
		private void LoadMaterial() {
			//if (!Engine.EngineLink.CanRender) {
			//	return;
			//}
			//Console.WriteLine("Load material");
			//if (shader.Asset is not null) {
			//	_material = new RMaterial(shader.Asset);
			//	foreach (var item in _material.GetAllParamInfo()) {
			//		var (pram, index) = GetParam(item.name);
			//		if (pram is not null) {
			//			switch (item.type) {
			//				case MaterialParam.Unknown:
			//					throw new Exception("Not supported");
			//				case MaterialParam.Float:
			//					if (pram.GetType() != typeof(ValuePramInfo<float>)) {
			//						Prams.DisposeAtIndex(index);
			//						pram = null;
			//					}
			//					break;
			//				case MaterialParam.Bool:
			//					if (pram.GetType() != typeof(ValuePramInfo<bool>)) {
			//						Prams.DisposeAtIndex(index);
			//						pram = null;
			//					}
			//					break;
			//				case MaterialParam.Vector2:
			//					if (pram.GetType() != typeof(ValuePramInfo<Vector2f>)) {
			//						Prams.DisposeAtIndex(index);
			//						pram = null;
			//					}
			//					break;
			//				case MaterialParam.Vector3:
			//					if (pram.GetType() != typeof(ValuePramInfo<Vector3f>)) {
			//						Prams.DisposeAtIndex(index);
			//						pram = null;
			//					}
			//					break;
			//				case MaterialParam.Vector4:
			//					if (pram.GetType() != typeof(ValuePramInfo<Vector4f>)) {
			//						Prams.DisposeAtIndex(index);
			//						pram = null;
			//					}
			//					break;
			//				case MaterialParam.Matrix:
			//					if (pram.GetType() != typeof(ValuePramInfo<Matrix>)) {
			//						Prams.DisposeAtIndex(index);
			//						pram = null;
			//					}
			//					break;
			//				case MaterialParam.Texture:
			//					if (pram.GetType() != typeof(TexPramInfo)) {
			//						Prams.DisposeAtIndex(index);
			//						pram = null;
			//					}
			//					break;
			//				case MaterialParam.Int:
			//					if (pram.GetType() != typeof(ValuePramInfo<int>)) {
			//						Prams.DisposeAtIndex(index);
			//						pram = null;
			//					}
			//					break;
			//				case MaterialParam.Int2:
			//					if (pram.GetType() != typeof(ValuePramInfo<Vector2i>)) {
			//						Prams.DisposeAtIndex(index);
			//						pram = null;
			//					}
			//					break;
			//				case MaterialParam.Int3:
			//					if (pram.GetType() != typeof(ValuePramInfo<Vector3i>)) {
			//						Prams.DisposeAtIndex(index);
			//						pram = null;
			//					}
			//					break;
			//				case MaterialParam.Int4:
			//					if (pram.GetType() != typeof(ValuePramInfo<Vector4i>)) {
			//						Prams.DisposeAtIndex(index);
			//						pram = null;
			//					}
			//					break;
			//				case MaterialParam.UInt:
			//					if (pram.GetType() != typeof(ValuePramInfo<uint>)) {
			//						Prams.DisposeAtIndex(index);
			//						pram = null;
			//					}
			//					break;
			//				case MaterialParam.UInt2:
			//					if (pram.GetType() != typeof(ValuePramInfo<Vector2u>)) {
			//						Prams.DisposeAtIndex(index);
			//						pram = null;
			//					}
			//					break;
			//				case MaterialParam.UInt3:
			//					if (pram.GetType() != typeof(ValuePramInfo<Vector3u>)) {
			//						Prams.DisposeAtIndex(index);
			//						pram = null;
			//					}
			//					break;
			//				case MaterialParam.UInt4:
			//					if (pram.GetType() != typeof(ValuePramInfo<Vector4u>)) {
			//						Prams.DisposeAtIndex(index);
			//						pram = null;
			//					}
			//					break;
			//				case MaterialParam.Cull:
			//					if (pram.GetType() != typeof(ValuePramInfo<Cull>)) {
			//						Prams.DisposeAtIndex(index);
			//						pram = null;
			//					}
			//					break;
			//				case MaterialParam.Transparency:
			//					if (pram.GetType() != typeof(ValuePramInfo<Transparency>)) {
			//						Prams.DisposeAtIndex(index);
			//						pram = null;
			//					}
			//					break;
			//				case MaterialParam.TexAddress:
			//					if (pram.GetType() != typeof(ValuePramInfo<TexAddress>)) {
			//						Prams.DisposeAtIndex(index);
			//						pram = null;
			//					}
			//					break;
			//				case MaterialParam.TexSample:
			//					if (pram.GetType() != typeof(ValuePramInfo<TexSample>)) {
			//						Prams.DisposeAtIndex(index);
			//						pram = null;
			//					}
			//					break;
			//				default:
			//					break;
			//			}
			//		}
			//		if(pram is null) {
			//			switch (item.type) {
			//				case MaterialParam.Unknown:
			//					throw new Exception("Not supported");
			//				case MaterialParam.Float:
			//					Prams.Add<ValuePramInfo<float>>().name.Value = item.name;
			//					break;
			//				case MaterialParam.Bool:
			//					Prams.Add<ValuePramInfo<bool>>().name.Value = item.name;
			//					break;
			//				case MaterialParam.Vector2:
			//					Prams.Add<ValuePramInfo<Vector2f>>().name.Value = item.name;
			//					break;
			//				case MaterialParam.Vector3:
			//					Prams.Add<ValuePramInfo<Vector3f>>().name.Value = item.name;
			//					break;
			//				case MaterialParam.Vector4:
			//					Prams.Add<ValuePramInfo<Vector4f>>().name.Value = item.name;
			//					break;
			//				case MaterialParam.Matrix:
			//					Prams.Add<ValuePramInfo<Matrix>>().name.Value = item.name;
			//					break;
			//				case MaterialParam.Texture:
			//					Prams.Add<TexPramInfo>().name.Value = item.name;
			//					break;
			//				case MaterialParam.Int:
			//					Prams.Add<ValuePramInfo<int>>().name.Value = item.name;
			//					break;
			//				case MaterialParam.Int2:
			//					Prams.Add<ValuePramInfo<Vector2i>>().name.Value = item.name;
			//					break;
			//				case MaterialParam.Int3:
			//					Prams.Add<ValuePramInfo<Vector3i>>().name.Value = item.name;
			//					break;
			//				case MaterialParam.Int4:
			//					Prams.Add<ValuePramInfo<Vector4i>>().name.Value = item.name;
			//					break;
			//				case MaterialParam.UInt:
			//					Prams.Add<ValuePramInfo<uint>>().name.Value = item.name;
			//					break;
			//				case MaterialParam.UInt2:
			//					Prams.Add<ValuePramInfo<Vector2u>>().name.Value = item.name;
			//					break;
			//				case MaterialParam.UInt3:
			//					Prams.Add<ValuePramInfo<Vector3u>>().name.Value = item.name;
			//					break;
			//				case MaterialParam.UInt4:
			//					Prams.Add<ValuePramInfo<Vector4u>>().name.Value = item.name;
			//					break;
			//				case MaterialParam.Cull:
			//					Prams.Add<ValuePramInfo<Cull>>().name.Value = item.name;
			//					break;
			//				case MaterialParam.Transparency:
			//					Prams.Add<ValuePramInfo<Transparency>>().name.Value = item.name;
			//					break;
			//				case MaterialParam.TexAddress:
			//					Prams.Add<ValuePramInfo<TexAddress>>().name.Value = item.name;
			//					break;
			//				case MaterialParam.TexSample:
			//					Prams.Add<ValuePramInfo<TexSample>>().name.Value = item.name;
			//					break;
			//				default:
			//					break;
			//			}
			//			switch (item.type) {
			//				case MaterialParam.Float:
			//					break;
			//				case MaterialParam.Vector4:
			//					Prams.Add<ValuePramInfo<Vector4f>>().name.Value = item.name;
			//					break;
			//				case MaterialParam.Vector3:
			//					Prams.Add<ValuePramInfo<Vector3f>>().name.Value = item.name;
			//					break;
			//				case MaterialParam.Vector2:
			//					Prams.Add<ValuePramInfo<Vector2f>>().name.Value = item.name;
			//					break;
			//				case MaterialParam.Matrix:
			//					Prams.Add<ValuePramInfo<Matrix>>().name.Value = item.name;
			//					break;
			//				case MaterialParam.Texture:
			//					break;
			//				default:
			//					break;
			//			}
			//			RLog.Info($"Material pram loaded. Name: {item.name} Type: {item.type}");
			//		}
			//	}
			//	foreach (var item in Prams) {
			//		((PramInfo)item).MaterialLoaded();
			//	}
			//	Load(_material);
			//	Console.WriteLine("Loaded material");
			//}
			//else {
			//	Console.WriteLine($"Shader target null {shader.Value.HexString()}");
			//}
		}

		public override void Dispose() {
			base.Dispose();
			_material?.Dispose();
			_material = null;
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			LoadMaterial();
		}
	}
}
