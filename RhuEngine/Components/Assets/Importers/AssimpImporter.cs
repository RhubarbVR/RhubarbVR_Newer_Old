using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using System.IO;
using RhuEngine.Linker;
using Assimp;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Collections.Generic;
using RNumerics;
using System.Linq;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Importers" })]
	public sealed partial class AssimpImporter : Importer
	{
		AssimpContext _assimpContext;

		public readonly SyncRef<IValueSource<bool>> srgbTextures;

		private bool SrgbTextures => srgbTextures.Target?.Value ?? false;

		public override void BuildUI(Entity rootBox) {
			var checkBOx = rootBox.AddChild("CheckBox").AttachComponent<CheckBox>();
			checkBOx.Text.Value = "SRGB";
			srgbTextures.Target = checkBOx.ButtonPressed;
			checkBOx.ButtonPressed.Value = true;
			base.BuildUI(rootBox);
		}


		public class StringArrayEqualityComparer : IEqualityComparer<string[]>
		{
			public bool Equals(string[] x, string[] y) {
				if (x.Length != y.Length) {
					return false;
				}
				for (var i = 0; i < x.Length; i++) {
					if (x[i] != y[i]) {
						return false;
					}
				}
				return true;
			}

			public int GetHashCode(string[] obj) {
				var result = 17;
				for (var i = 0; i < obj.Length; i++) {
					unchecked {
						result = (result * 23) + obj[i].GetHashCode();
					}
				}
				return result;
			}
		}

		private class AssimpHolder
		{
			public Entity root;
			public Entity assetEntity;
			public Scene scene;

			public List<AssetProvider<RTexture2D>> textures = new();
			public List<ComplexMesh> meshes = new();
			public List<AssetProvider<RMaterial>> materials = new();
			public bool ReScale = true;
			public float TargetSize = 0.5f;
			public Dictionary<string, Entity> Nodes = new();
			public Dictionary<string, Armature> Armatures = new();
			public AxisAlignedBox3f BoundingBox = AxisAlignedBox3f.CenterZero;
			public List<(Entity, Node)> LoadMeshNodes = new();

			public AssimpHolder(Scene scene, Entity _root, Entity _assetEntity) {
				this.scene = scene;
				root = _root;
				assetEntity = _assetEntity;
			}

			public void CalculateOptimumBounds(ComplexMesh amesh, Entity entity) {
				var local = root.GlobalToLocal(entity.GlobalTrans);
				var mesh = BoundsUtil.Bounds(amesh.Vertices, (x) => x);
				mesh.Translate(local.Translation);
				mesh.Scale(local.Scale);
				BoundingBox = BoundsUtil.Combined(BoundingBox, mesh);
			}

			public void Rescale() {
				if (ReScale) {
					var size = BoundingBox.Extents;
					var largestSize = MathUtil.Max(size.x, size.y, size.z);
					root.scale.Value *= new Vector3f(TargetSize / largestSize);
				}
			}

			public void CalculateOptimumBounds(Entity entity) {
				var localPoint = root.GlobalPointToLocal(entity.GlobalTrans.Translation);
				BoundingBox = BoundsUtil.Combined(BoundingBox, new AxisAlignedBox3f { max = localPoint, min = localPoint });
			}
		}

		public override async Task ImportAsset() {
			try {
				Entity.rotation.Value *= Quaternionf.Pitched.Inverse;
				_assimpContext ??= new AssimpContext {
					Scale = .001f,
				};
				Scene scene;
				if (_importData.isUrl) {
					using var client = new HttpClient();
					using var response = await client.GetAsync(_importData.url_path);
					using var streamToReadFrom = await response.Content.ReadAsStreamAsync();
					scene = _assimpContext.ImportFileFromStream(streamToReadFrom);
				}
				else {
					scene = _importData.rawData is null ? _assimpContext.ImportFile(_importData.url_path) : _assimpContext.ImportFileFromStream(_importData.rawData);
				}
				if (scene is null) {
					RLog.Err("failed to Load Model Scene not loaded");
					return;
				}
				var root = Entity.AddChild("Root");
				var AssimpHolder = new AssimpHolder(scene, root, root.AddChild("Assets"));
				LoadTextures(AssimpHolder.assetEntity, AssimpHolder);
				LoadMaterials(AssimpHolder.assetEntity, AssimpHolder);
				LoadMesh(AssimpHolder.assetEntity, AssimpHolder);
				LoadNode(root, scene.RootNode, AssimpHolder);
				LoadLights(AssimpHolder.assetEntity, AssimpHolder);
				foreach (var item in AssimpHolder.LoadMeshNodes) {
					LoadMeshNode(item.Item1, item.Item2, AssimpHolder);
				}
				AssimpHolder.Rescale();
				RLog.Info("Done Loading Model");
				//LoadAnimations(AssimpHolder.assetEntity, AssimpHolder);
				//LoadCameras(AssimpHolder.assetEntity, AssimpHolder);

			}
			catch (Exception e) {
				RLog.Err($"Failed to Load Model Error {e}");
			}
		}

		private static void LoadNode(Entity ParrentEntity, Assimp.Node node, AssimpHolder scene) {
			RLog.Info($"Loaded Node {node.Name} Parrent {node.Parent?.Name ?? "NULL"}");
			var entity = ParrentEntity.AddChild(node.Name);
			entity.LocalTrans = Matrix.CreateFromAssimp(node.Transform);
			if (!scene.Nodes.ContainsKey(node.Name)) {
				scene.Nodes.Add(node.Name, entity);
			}
			if (node.HasChildren) {
				foreach (var item in node.Children) {
					LoadNode(entity, item, scene);
				}
			}
			scene.CalculateOptimumBounds(entity);
			if (node.HasMeshes) {
				scene.LoadMeshNodes.Add((entity, node));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
		private static void LoadMesh(Entity entity, AssimpHolder scene) {
			if (!scene.scene.HasMeshes) {
				RLog.Info($"No Meshes");
				return;
			}
			foreach (var item in scene.scene.Meshes) {
				var newMesh = new ComplexMesh(item);
				if (item.MorphMethod == MeshMorphingMethod.None && newMesh.MeshAttachments.Count != 0) {
					newMesh.MorphingMethod = RMeshMorphingMethod.VertexBlend;
				}
				RLog.Info($"New Mesh MeshName:{newMesh.MeshName} MorphingMethod:{newMesh.MorphingMethod} VertexCount:{newMesh.VertexCount}  MeshAttachmentsCount:{newMesh.MeshAttachments.Count}");
				scene.meshes.Add(newMesh);
				RLog.Info($"Loaded Mesh {item.Name}");
			}
		}

		private static void LoadMaterials(Entity entity, AssimpHolder scene) {
			if (!scene.scene.HasMaterials) {
				RLog.Info($"No Materials");
				return;
			}
			foreach (var item in scene.scene.Materials) {
				if (item.IsPBRMaterial) {
					//var mat = entity.AttachComponent<PBRMaterial>();
					//scene.materials.Add(mat);
					//if (item.HasShininess) {
					//	mat.Smoothness.Value = item.Shininess;
					//}
					//if (item.HasColorDiffuse) {
					//	mat.AlbedoTint.Value = new Colorf(item.ColorDiffuse.R, item.ColorDiffuse.G, item.ColorDiffuse.B, item.ColorDiffuse.A);
					//}
					//if (item.HasTextureDiffuse) {
					//	try {
					//		mat.DetailAlbedo.Target = scene.textures[item.TextureDiffuse.TextureIndex];
					//	}
					//	catch { }
					//}
					//if (item.HasTextureNormal) {
					//	try {
					//		mat.NormalMap.Target = scene.textures[item.TextureNormal.TextureIndex];
					//	}
					//	catch { }
					//}
					//if (item.HasTextureEmissive) {
					//	try {
					//		mat.EmissionTexture.Target = scene.textures[item.TextureEmissive.TextureIndex];
					//	}
					//	catch { }
					//}
					//if (item.HasColorDiffuse) {
					//	mat.AlbedoTint.Value = new Colorf(item.ColorDiffuse.R, item.ColorDiffuse.G, item.ColorDiffuse.B, item.ColorDiffuse.A);
					//}
					//if (item.HasTextureDiffuse) {
					//	try {
					//		mat.AlbedoTexture.Target = scene.textures[item.TextureDiffuse.TextureIndex];
					//	}
					//	catch { }
					//}
					RLog.Info($"Loaded PBR Material");
				}
				else {
					var mat = entity.AttachComponent<UnlitMaterial>();
					scene.materials.Add(mat);
					if (item.HasColorDiffuse) {
						mat.Tint.Value = new Colorf(item.ColorDiffuse.R, item.ColorDiffuse.G, item.ColorDiffuse.B, item.ColorDiffuse.A);
					}
					if (item.HasTextureDiffuse) {
						try {
							mat.MainTexture.Target = scene.textures[item.TextureDiffuse.TextureIndex];
						}
						catch { }
					}
					RLog.Info($"Loaded Unlit Material");
				}
			}
		}

		private static void LoadLights(Entity entity, AssimpHolder scene) {
			if (!scene.scene.HasLights) {
				RLog.Info($"No lights");
				return;
			}
			var lights = entity.AddChild("Lights");
			foreach (var item in scene.scene.Lights) {
				var ligh = scene.Nodes.ContainsKey(item.Name) ? scene.Nodes[item.Name] : lights.AddChild(item.Name);
				//Todo Fix light with model import
				//var lightcomp = ligh.AttachComponent<Light>();
				//lightcomp.LightType.Value = item.LightType switch {
				//	LightSourceType.Directional => RLightType.Directional,
				//	LightSourceType.Spot => RLightType.Spot,
				//	_ => RLightType.Point,
				//};
				//lightcomp.SpotAngle.Value = item.AngleInnerCone;
				//lightcomp.Color.Value = new RNumerics.Colorf(item.ColorDiffuse.R, item.ColorDiffuse.G, item.ColorDiffuse.B, 1);
			}
		}
		private void LoadTextures(Entity entity, AssimpHolder scene) {
			if (!scene.scene.HasTextures) {
				RLog.Info($"No Textures");
				return;
			}
			foreach (var item in scene.scene.Textures) {
				RLog.Info($"Loaded Texture {item.Filename}");
				if (item.HasCompressedData) {
					var newtexture = new ImageSharpTexture(new MemoryStream(item.CompressedData), SrgbTextures).CreateTextureAndDisposes();
					var textureURI = entity.World.CreateLocalAsset(newtexture);
					var tex = entity.AttachComponent<StaticTexture>();
					scene.textures.Add(tex);
					tex.url.Value = textureURI.ToString();
				}
				else if (item.HasNonCompressedData) {
					RLog.Err("not supported");
				}
				else {
					RLog.Err("Texture had no data to be found");
				}
			}
		}

		//private static void LoadAnimations(Entity entity, AssimpHolder scene) {
		//	if (!scene.scene.HasAnimations) {
		//		RLog.Info("No Animations");
		//		return;
		//	}
		//	RLog.Err("not supported");
		//}

		//private static void LoadCameras(Entity entity, AssimpHolder scene) {
		//	if (!scene.scene.HasCameras) {
		//		RLog.Info("No Cameras");
		//		return;
		//	}
		//}
		private static void LoadMeshNode(Entity entity, Node node, AssimpHolder scene) {
			ComplexMesh complexMesh = null;
			var mits = new List<int>();
			foreach (var item in node.MeshIndices) {
				var rMesh = scene.meshes[item];
				var amesh = scene.scene.Meshes[item];
				if (complexMesh is not null) {
					try {
						complexMesh.AddSubMesh(rMesh);
						mits.Add(amesh.MaterialIndex);
					}
					catch {
						AddMeshRender(entity, node, scene, rMesh, new int[] { amesh.MaterialIndex });
					}
				}
				else {
					mits.Add(amesh.MaterialIndex);
					complexMesh = rMesh;
				}
			}
			if (complexMesh is not null) {
				AddMeshRender(entity, node, scene, complexMesh, mits);
			}
		}

		private static void AddMeshRender(Entity entity, Node node, AssimpHolder scene, ComplexMesh amesh, IEnumerable<int> mits) {
			RLog.Info($"Added Mesh Render SubMeshesCount:{amesh.SubMeshes.Count} VertexCount:{amesh.VertexCount} MeshAttachmentsCount:{amesh.MeshAttachments.Count} MeshBlendMode:{amesh.MorphingMethod}");
			var rmesh = scene.assetEntity.AttachComponent<StaticMesh>();
			if (amesh is not null) {
				rmesh.url.Value = entity.World.CreateLocalAsset(amesh).ToString();
			}
			if (amesh.HasBones || amesh.HasMeshAttachments) {
				Armature armiturer;
				scene.Nodes.TryGetValue(amesh.Bones[0].BoneName, out var armitureEntity);
				if (armitureEntity?.parent.Target is not null) {
					armiturer = armitureEntity.parent.Target.GetFirstComponentOrAttach<Armature>();
					if(armiturer.ArmatureEntitys.Count < amesh.Bones.Count) {
						foreach (var bone in amesh.Bones.Skip(armiturer.ArmatureEntitys.Count)) {
							if (scene.Nodes.ContainsKey(bone.Name)) {
								armiturer.ArmatureEntitys.Add().Target = scene.Nodes[bone.Name];
							}
							else {
								RLog.Info($"Didn't FindNode for {bone.Name}");
								var ent = entity.AddChild(bone.Name);
								armiturer.ArmatureEntitys.Add().Target = ent;
							}
						}
					}
				}
				else {
					armiturer = entity.GetFirstComponentOrAttach<Armature>();
					if (armiturer.ArmatureEntitys.Count < amesh.Bones.Count) {
						foreach (var bone in amesh.Bones.Skip(armiturer.ArmatureEntitys.Count)) {
							if (scene.Nodes.ContainsKey(bone.Name)) {
								armiturer.ArmatureEntitys.Add().Target = scene.Nodes[bone.Name];
							}
							else {
								RLog.Info($"Didn't FindNode for {bone.Name}");
								var ent = entity.AddChild(bone.Name);
								armiturer.ArmatureEntitys.Add().Target = ent;
							}
						}
					}
				}
				var meshRender = entity.AttachComponent<SkinnedMeshRender>();
				meshRender.Armature.Target = armiturer;
				foreach (var boneMesh in amesh.MeshAttachments) {
					var newShape = meshRender.BlendShapes.Add();
					newShape.BlendName.Value = boneMesh.Name;
					RLog.Info($"Added ShapeKey {newShape.BlendName.Value}");
				}
				meshRender.mesh.Target = rmesh;
				foreach (var item in mits) {
					meshRender.materials.Add().Target = scene.materials[item];
				}
			}
			else {
				scene.CalculateOptimumBounds(amesh, entity);
				var meshRender = entity.AttachComponent<MeshRender>();
				meshRender.mesh.Target = rmesh;
				foreach (var item in mits) {
					meshRender.materials.Add().Target = scene.materials[item];
				}
			}
			RLog.Info($"Added MeshNode {node.Name}");
		}


	}
}
