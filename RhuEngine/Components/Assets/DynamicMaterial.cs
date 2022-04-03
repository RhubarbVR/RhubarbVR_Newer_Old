using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets" })]
	public class DynamicMaterial : AssetProvider<RMaterial>
	{
		[OnAssetLoaded(nameof(LoadMaterial))]
		public AssetRef<RShader> shader;

		[OnChanged(nameof(UpdateValues))]
		[Default(DepthTest.Less)]
		public Sync<DepthTest> depthTest;

		[OnChanged(nameof(UpdateValues))]
		[Default(true)]
		public Sync<bool> depthWrite;

		[OnChanged(nameof(UpdateValues))]
		[Default(Cull.Back)]
		public Sync<Cull> faceCull;

		[OnChanged(nameof(UpdateValues))]
		public Sync<int> queueOffset;

		[OnChanged(nameof(UpdateValues))]
		[Default(Transparency.None)]
		public Sync<Transparency> transparency;

		[OnChanged(nameof(UpdateValues))]
		public Sync<bool> wireframe;

		public SyncAbstractObjList<PramInfo> Prams;

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
				GetParam(name).Item1.SetValue(data);
			}
			catch { }
		}

		public abstract class PramInfo : SyncObject
		{
			public Sync<string> name;
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

		public class ValuePramInfo<T> : PramInfo
		{
			[OnChanged(nameof(OnValueChangeed))]
			public Sync<T> value;

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
			public AssetRef<RTexture2D> value;

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
		private void LoadMaterial() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			Console.WriteLine("Load material");
			if (shader.Asset is not null) {
				_material = new RMaterial(shader.Asset);
				foreach (var item in _material.GetAllParamInfo()) {
					if (item.name != "color") {
						var (pram, index) = GetParam(item.name);
						if (pram is not null) {
							switch (item.type) {
								case MaterialParam.Float:
									if (pram.GetType() != typeof(ValuePramInfo<float>)) {
										Prams.RemoveAtIndex(index);
										pram = null;
									}
									break;
								case MaterialParam.Color128:
									if (pram.GetType() != typeof(ValuePramInfo<Colorb>)) {
										Prams.RemoveAtIndex(index);
										pram = null;
									}
									break;
								case MaterialParam.Vector4:
									if (pram.GetType() != typeof(ValuePramInfo<Vector4f>)) {
										Prams.RemoveAtIndex(index);
										pram = null;
									}
									break;
								case MaterialParam.Vector3:
									if (pram.GetType() != typeof(ValuePramInfo<Vector3f>)) {
										Prams.RemoveAtIndex(index);
										pram = null;
									}
									break;
								case MaterialParam.Vector2:
									if (pram.GetType() != typeof(ValuePramInfo<Vector2f>)) {
										Prams.RemoveAtIndex(index);
										pram = null;
									}
									break;
								case MaterialParam.Matrix:
									if (pram.GetType() != typeof(ValuePramInfo<Matrix>)) {
										Prams.RemoveAtIndex(index);
										pram = null;
									}
									break;
								case MaterialParam.Texture:
									if (pram.GetType() != typeof(TexPramInfo)) {
										Prams.RemoveAtIndex(index);
										pram = null;
									}
									break;
								default:
									break;
							}
						}
						if (pram is null) {
							switch (item.type) {
								case MaterialParam.Float:
									Prams.Add<ValuePramInfo<float>>().name.Value = item.name;
									break;
								case MaterialParam.Color128:
									Prams.Add<ValuePramInfo<Colorb>>().name.Value = item.name;
									break;
								case MaterialParam.Vector4:
									Prams.Add<ValuePramInfo<Vector4f>>().name.Value = item.name;
									break;
								case MaterialParam.Vector3:
									Prams.Add<ValuePramInfo<Vector3f>>().name.Value = item.name;
									break;
								case MaterialParam.Vector2:
									Prams.Add<ValuePramInfo<Vector2f>>().name.Value = item.name;
									break;
								case MaterialParam.Matrix:
									Prams.Add<ValuePramInfo<Matrix>>().name.Value = item.name;
									break;
								case MaterialParam.Texture:
									Prams.Add<TexPramInfo>().name.Value = item.name;
									break;
								default:
									break;
							}
							RLog.Info($"Material pram loaded. Name: {item.name} Type: {item.type}");
						}
					}
				}
				foreach (var item in Prams) {
					((PramInfo)item).MaterialLoaded();
				}
				UpdateValues();
				Load(_material);
				Console.WriteLine("Loaded material");
			}
			else {
				Console.WriteLine($"Shader target null {shader.Value.HexString()}");
			}
		}

		private void UpdateValues() {
			if (_material is null) {
				return;
			}
			_material.DepthTest = depthTest.Value;
			_material.DepthWrite = depthWrite.Value;
			_material.FaceCull = faceCull.Value;
			_material.QueueOffset = queueOffset.Value;
			_material.Transparency = transparency.Value;
			_material.Wireframe = wireframe.Value;
		}

		public override void OnAttach() {
			base.OnAttach();
			LoadMaterial();
		}

		public override void OnLoaded() {
			base.OnLoaded();
		}
	}
}
