using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using System.IO;
using StereoKit;
using Assimp;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Collections.Generic;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Importers" })]
	public class AssimpImporter : Importer
	{
		public static bool IsValidImport(string path) {
			path = path.ToLower();
			return
				path.EndsWith(".fbx") ||
				path.EndsWith(".dea") ||
				path.EndsWith(".gltf") || path.EndsWith(".glb") ||
				path.EndsWith(".blend") ||
				path.EndsWith(".3ds") ||
				path.EndsWith(".ase") ||
				path.EndsWith(".obj") ||
				path.EndsWith(".ifc") ||
				path.EndsWith(".xgl") || path.EndsWith(".zgl") ||
				path.EndsWith(".ply") ||
				path.EndsWith(".dxf") ||
				path.EndsWith(".lwo") ||
				path.EndsWith(".lws") ||
				path.EndsWith(".lxo") ||
				path.EndsWith(".stl") ||
				path.EndsWith(".x") ||
				path.EndsWith(".ac") ||
				path.EndsWith(".ms3d") ||
				path.EndsWith(".cob") || path.EndsWith(".scn") ||
				path.EndsWith(".bvh") ||
				path.EndsWith(".csm") ||
				path.EndsWith(".mdl") ||
				path.EndsWith(".md2") ||
				path.EndsWith(".md3") ||
				path.EndsWith(".pk3") ||
				path.EndsWith(".mdc") ||
				path.EndsWith(".md5") ||
				path.EndsWith(".smd") || path.EndsWith(".vta") ||
				path.EndsWith(".ogex") ||
				path.EndsWith(".b3d") ||
				path.EndsWith(".q3d") ||
				path.EndsWith(".q3s") ||
				path.EndsWith(".nff") ||
				path.EndsWith(".off") ||
				path.EndsWith(".raw") ||
				path.EndsWith(".ter") ||
				path.EndsWith(".hmp") ||
				path.EndsWith(".ndo");
		}

		AssimpContext _assimpContext;

		private class AssimpHolder
		{
			public Entity root;
			public Entity assetEntity;
			public Scene scene;

			public List<AssetProvider<Tex>> textures = new();

			public AssimpHolder(Scene scene,Entity _root,Entity _assetEntity) {
				this.scene = scene;
				root = _root;
				assetEntity = _assetEntity;
			}
		}

		public async Task ImportAsync(string path_url, bool isUrl, byte[] rawData) {
			try {
				_assimpContext ??= new AssimpContext();
				Scene scene;
				if (isUrl) {
					using var client = new HttpClient();
					using var response = await client.GetAsync(path_url);
					using var streamToReadFrom = await response.Content.ReadAsStreamAsync();
					scene = _assimpContext.ImportFileFromStream(streamToReadFrom);
				}
				else {
					scene = rawData is null ? _assimpContext.ImportFile(path_url) : _assimpContext.ImportFileFromStream(new MemoryStream(rawData));
				}
				if (scene is null) {
					Log.Err("failed to Load Model Scene not loaded");
					return;
				}
				var root = Entity.AddChild("Root");
				var AssimpHolder = new AssimpHolder(scene,root,root.AddChild("Assets"));
				LoadMesh(AssimpHolder.assetEntity, AssimpHolder);
				LoadMaterials(AssimpHolder.assetEntity, AssimpHolder);
				Loadights(AssimpHolder.assetEntity, AssimpHolder);
				LoadTextures(AssimpHolder.assetEntity, AssimpHolder);
				LoadAnimations(AssimpHolder.assetEntity, AssimpHolder);
				LoadCameras(AssimpHolder.assetEntity, AssimpHolder);
				LoadLight(AssimpHolder.assetEntity, AssimpHolder);
				LoadNode(root, scene.RootNode,AssimpHolder);
			}catch(Exception e) {
				Log.Err($"failed to Load Model Error {e}");
			}
		}

		private static void LoadNode(Entity ParrentEntity,Assimp.Node node, AssimpHolder scene) {
			Log.Info($"Loaded Node {node.Name} Parrent {node.Parent?.Name??"NULL"}");
			var entity = ParrentEntity.AddChild(node.Name);
			entity.LocalTrans = node.Transform.CastToNormal();
			if (node.HasChildren) {
				foreach (var item in node.Children) {
					LoadNode(entity, item, scene);
				}
			}
			LoadMeshNode(entity, node, scene);
		}

		private static void LoadMesh(Entity entity, AssimpHolder scene) {
			if (!scene.scene.HasMeshes) {
				Log.Info($"No Meshes");
				return;
			}
		}

		private static void LoadMaterials(Entity entity, AssimpHolder scene) {
			if (!scene.scene.HasMaterials) {
				Log.Info($"No Materials");
				return;
			}
			foreach (var item in scene.scene.Materials) {
			}
		}

		private static void Loadights(Entity entity, AssimpHolder scene) {
			if (!scene.scene.HasLights) {
				Log.Info($"No lights");
				return;
			}
			foreach (var item in scene.scene.Lights) {
				//Need to add light component
			}
		}
		private static void LoadTextures(Entity entity, AssimpHolder scene) {
			if (!scene.scene.HasTextures) {
				Log.Info($"No Textures");
				return;
			}
			foreach (var item in scene.scene.Textures) {
				Log.Info($"Loaded Texture {item.Filename}");
				if (item.HasCompressedData) {
					var newuri = entity.World.LoadLocalAsset(item.CompressedData, item.Filename + item.CompressedFormatHint);
					var tex = entity.AttachComponent<StaticTexture>();
					scene.textures.Add(tex);
					tex.url.Value = newuri.ToString();
				}
				else if (item.HasNonCompressedData) {
					Log.Err("not supported");
				}
				else {
					Log.Err("Texture had no data to be found");
				}
			}
		}

		private static void LoadAnimations(Entity entity, AssimpHolder scene) {
			if (!scene.scene.HasAnimations) {
				Log.Info("No Animations");
				return;
			}
			Log.Err("not supported");
		}

		private static void LoadCameras(Entity entity, AssimpHolder scene) {
			if (!scene.scene.HasCameras) {
				Log.Info("No Cameras");
				return;
			}
		}
		private static void LoadLight(Entity entity, AssimpHolder scene) {
			if (!scene.scene.HasLights) {
				Log.Info("No Lights");
				return;
			}
		}

		private static void LoadMeshNode(Entity entity, Assimp.Node node, AssimpHolder scene) {
			if (!node.HasMeshes) {
				return;
			}
			foreach (var item in node.MeshIndices) {
			}
		}

		public override void Import(string path_url, bool isUrl, byte[] rawData) {
			ImportAsync(path_url,isUrl,rawData).ConfigureAwait(false);
		}
	}
}
