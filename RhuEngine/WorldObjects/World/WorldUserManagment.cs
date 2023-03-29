using System;
using System.Linq;
using System.Threading.Tasks;

using RhuEngine.Components;
using RhuEngine.Datatypes;
using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.WorldObjects
{
	public sealed partial class World
	{
		public ushort LocalUserID { get; private set; } = 1;

		public User GetUserFromID(Guid id) {
			for (var i = 0; i < Users.Count; i++) {
				if (Users[i].userID.Value == id.ToString()) {
					return Users[i];
				}
			}
			return null;
		}

		private void LoadUserIn(Peer peer) {
			if (peer.User is not null) {
				RLog.Info("Peer already has peer");
			}
			else {
				var user = GetUserFromID(peer.UserID);
				if (user == null) {
					RLog.Info($"User built from peer {Users.Count + 1}");
					var userid = (ushort)(Users.Count + 1);
					var pos = 176u;
					user = Users.AddWithCustomRefIds(false, false, () => {
						lock (_buildRefIDLock) {
							var netPointer = NetPointer.BuildID(pos, userid);
							pos++;
							return netPointer;
						}
					});
					user.userID.Value = peer.UserID.ToString();
					user.CurrentPeer = peer;
					peer.User = user;
				}
				else {
					RLog.Info($"User found from peer UserID:{peer.UserID}");
					if (user.CurrentPeer == peer) {
						RLog.Info("Already bond to user");
						return;
					}
					if ((user.CurrentPeer?.NetPeer?.ConnectionState ?? LiteNetLib.ConnectionState.Disconnected) == LiteNetLib.ConnectionState.Connected) {
						RLog.Err("User already loaded can only join a world once");
						if (!peer.IsRelay) { 
							peer.NetPeer.Disconnect();
						}
					}
					else {
						user.CurrentPeer = peer;
						peer.User = user;
					}
				}
			}
		}
		[Exposed]
		public User GetMasterUser() {
			return IsNetworked ? Users[MasterUser] : Users[0];
		}

		public ushort GetUserID(User user) {
			return (ushort)(Users.IndexOf(user) + 1);
		}

		[Exposed]
		public User GetHostUser() {
			return Users[0];
		}
		[Exposed]
		public User GetLocalUser() {
			return Users is null ? null : LocalUserID <= 0 ? null : (LocalUserID - 1) < Users.Count ? Users[LocalUserID - 1] : null;
		}
		[PrivateSpaceOnly]
		public sealed partial class RawMeshAsset : ProceduralMesh
		{

			public override void ComputeMesh() {
			}
		}

		public void DrawDebugMesh(IMesh mesha, Matrix matrix, Colorf colorf, float drawTime = 1) {
			if (DebugVisuals) {
				RUpdateManager.ExecuteOnStartOfUpdate(() => {
					var mesh = RootEntity.GetFirstComponentOrAttach<RawMeshAsset>();
					var comp = RootEntity.GetFirstComponent<UnlitMaterial>();
					if (comp is null) {
						comp = RootEntity.AttachComponent<UnlitMaterial>();
						comp.Transparency.Value = Transparency.Blend;
					}
					var debugcube = RootEntity.AddChild("DebugCube");
					var meshrender = debugcube.AttachComponent<MeshRender>();
					meshrender.colorLinear.Value = colorf;
					meshrender.materials.Add().Target = comp;
					meshrender.mesh.Target = mesh;
					mesh.GenMesh(mesha);
					meshrender.Entity.GlobalTrans = matrix;
					Task.Run(async () => {
						await Task.Delay((int)(1000 * drawTime));
						debugcube.Destroy();
					});
				});
			}
		}


		public void DrawDebugCube(Matrix matrix, Vector3f pos, Vector3d scale, Colorf colorf, float drawTime = 1) {
			DrawDebugCube(matrix, pos, (Vector3f)scale, colorf, drawTime);
		}
		public void DrawDebugCube(Matrix matrix, Vector3f pos, Vector3f scale, Colorf colorf, float drawTime = 1) {
			if (DebugVisuals) {
				RUpdateManager.ExecuteOnStartOfUpdate(() => {
					var mesh = RootEntity.GetFirstComponentOrAttach<TrivialBox3Mesh>();
					var comp = RootEntity.GetFirstComponent<UnlitMaterial>();
					if (comp is null) {
						comp = RootEntity.AttachComponent<UnlitMaterial>();
						comp.Transparency.Value = Transparency.Blend;
					}
					var debugcube = RootEntity.AddChild("DebugCube");
					var meshrender = debugcube.AttachComponent<MeshRender>();
					meshrender.colorLinear.Value = colorf;
					meshrender.materials.Add().Target = comp;
					meshrender.mesh.Target = mesh;
					meshrender.Entity.GlobalTrans = Matrix.TS(pos, scale * 2.01f) * matrix;
					Task.Run(async () => {
						await Task.Delay((int)(1000 * drawTime));
						debugcube.Destroy();
					});
				});
			}
		}

		public void DrawDebugText(Matrix matrix, Vector3f pos, Vector3f scale, Colorf colorf, object text, float drawTime = 1, bool lookAtLocal = true) {
			if (DebugVisuals) {
				RUpdateManager.ExecuteOnStartOfUpdate(() => {
					var debugcube = RootEntity.AddChild("DebugText");
					if (lookAtLocal) {
						var lookAt = debugcube.AttachComponent<LookAtValue>();
						var userNodePos = debugcube.AttachComponent<UserBodyNodeTransform>();
						userNodePos.Pos.SetLinkerTarget(lookAt.LookAtPoint);
					}
					var meshrender = debugcube.AttachComponent<TextLabel3D>();
					meshrender.Modulate.Value = colorf;
					meshrender.Text.Value = text.ToString();
					meshrender.Entity.GlobalTrans = Matrix.TS(pos, scale) * matrix;
					Task.Run(async () => {
						await Task.Delay((int)(1000 * drawTime));
						debugcube.Destroy();
					});
				});
			}
		}

		public void DrawDebugSphere(Matrix matrix, Vector3f pos, Vector3d scale, Colorf colorf, float drawTime = 1) {
			DrawDebugSphere(matrix, pos, (Vector3f)scale, colorf, drawTime);
		}
		private bool DebugVisuals => Engine.DebugVisuals;


		public void DrawDebugSphere(Matrix matrix, Vector3f pos, Vector3f scale, Colorf colorf, float drawTime = 1) {
			if (DebugVisuals) {
				RUpdateManager.ExecuteOnStartOfUpdate(() => {
					var mesh = worldManager.PrivateOverlay.RootEntity.GetFirstComponentOrAttach<Sphere3NormalizedCubeMesh>();
					var comp = worldManager.PrivateOverlay.RootEntity.GetFirstComponent<UnlitMaterial>();
					if (comp is null) {
						comp = worldManager.PrivateOverlay.RootEntity.AttachComponent<UnlitMaterial>();
						comp.Transparency.Value = Transparency.Blend;
					}
					var debugcube = worldManager.PrivateOverlay.RootEntity.AddChild("DebugCube");
					var meshrender = debugcube.AttachComponent<MeshRender>();
					meshrender.colorLinear.Value = colorf;
					meshrender.materials.Add().Target = comp;
					meshrender.mesh.Target = mesh;
					meshrender.Entity.GlobalTrans = Matrix.TS(pos, scale * 2.01f) * matrix;
					Task.Run(async () => {
						await Task.Delay((int)(1000 * drawTime));
						debugcube.Destroy();
					});
				});
			}
		}

	}
}
