using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
// using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using Flags = System.Reflection.BindingFlags;
#endif

namespace MT.Packages.LD47
{
	public partial class NetworkManager : Mirror.NetworkManager
	{
		public static List<Player> players = new List<Player>();

		public static Player GetClosestPlayer(Vector2 position) {
			return players
				.Where(x => x.isInGameAndAlive)
				.Select(player => {
					var sqrMagnitude = ((Vector2) player.transform.position - position).sqrMagnitude;
					return (player, sqrMagnitude);
				})
				.OrderBy(x => x.sqrMagnitude)
				.FirstOrDefault()
				.player;
		}

		public static void RegisterPlayer(Player player) {
			players.Add(player);
		}

		public static void UnregisterPlayer(Player player) {
			players.Remove(player);
			NetworkServer.SendToReady(new Pool_Message_DisableAll {
				ownerID = player.netId
			});
		}

		public static void Respawn() {
			PlayerController.instance.player.Respawn();
		}

		// static IEnumerable<System.Type> GetTypes<T>() {
		// 	return
		// 		from assembly in System.AppDomain.CurrentDomain.GetAssemblies()
		// 		from type in assembly.GetTypes()
		// 		where typeof(T).IsAssignableFrom(type)
		// 		select type;
		// }

		// static System.Type[] poolObjectMessageTypes;

		// public override void Awake() {
		//	poolObjectMessageTypes = GetTypes<PoolObjectMessage>().ToArray();
		//	base.Awake();
		// }

		public override void Awake() {
			base.Awake();
			var startMenu = GameObject.FindGameObjectWithTag("Start Menu");
			if (SceneManager.sceneCount == 1) {
				SceneManager.LoadScene(1, LoadSceneMode.Additive);
			} else {
				if (startMenu) {
					startMenu.SetActive(false);
				}
				StartClientNormal();
				return;
			}
#if UNITY_SERVER
			if (startMenu) {
				startMenu.SetActive(false);
			}
#elif !UNITY_EDITOR
			if (startMenu) {
				startMenu.SetActive(false);
			}
			networkAddress = "cloudbase.tk";
			StartClientAsBot();
#endif
		}

		public static bool botMode;

		public void StartClientNormal() {
			botMode = false;
			StartClient();
		}

		public void StartClientAsBot() {
			botMode = true;
			StartClient();
		}

		public override void OnClientDisconnect(NetworkConnection conn) {
			base.OnClientDisconnect(conn);
			this.LogError($"Connecting to {networkAddress} failed");
			if (networkAddress != "cloudbase.tk") {
				networkAddress = "cloudbase.tk";
				this.Log($"Trying again connecting to {networkAddress}...");
				StartClient();
			}
		}

		// public override void OnStartServer() {
		//	base.OnStartServer();
		//	foreach (var type in poolObjectMessageTypes) { }
		// }

#if UNITY_EDITOR
		public static void GenerateMessageRegistrations() {
			var messageTypes = typeof(NetworkManager).Assembly.GetTypes().Where(type => {
				if (type.Namespace != typeof(NetworkManager).Namespace) {
					return false;
				}
				if (type.IsAbstract) {
					return false;
				}
				if (!typeof(MessageBase).IsAssignableFrom(type)) {
					return false;
				}
				return true;
			}).ToArray();

			// var now = System.DateTime.Now;
			// var comment = $"Generated on {now:yyyy-MM-dd}, {now:HH:mm:ss}";

			var comment = "Generated file";
			string contents = $"// {comment}" + @"

using Mirror;

namespace MT.Packages.LD47
{
	public partial class NetworkManager : Mirror.NetworkManager
	{
		public override void OnStartServer() {
			base.OnStartServer();
";
			foreach (var messageType in messageTypes) {
				if (messageType.GetMethod("OnServerReceive", Flags.Public | Flags.Static) != null) {
					contents += $"			NetworkServer.RegisterHandler<{messageType.Name}>({messageType.Name}.OnServerReceive);\r\n";
				}
			}
			contents += @"		}
		
		public override void OnStartClient() {
			base.OnStartClient();
";
			foreach (var messageType in messageTypes) {
				if (messageType.GetMethod("OnClientReceive", Flags.Public | Flags.Static) != null) {
					contents += $"			NetworkClient.RegisterHandler<{messageType.Name}>({messageType.Name}.OnClientReceive);\r\n";
				}
			}

			contents += @"		}
	}
}";
			System.IO.File.WriteAllText(@"Assets\Scripts\Components\Networking\NetworkManager.Messages.cs", contents);
			AssetDatabase.Refresh();
		}
#endif
	}

#if UNITY_EDITOR
	[InitializeOnLoad]
	public static class NetworkManagerPlaymodeStateChanged
	{
		static NetworkManagerPlaymodeStateChanged() {
			EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged; ;
		}
		static void EditorApplication_playModeStateChanged(PlayModeStateChange state) {
			if (state == PlayModeStateChange.EnteredPlayMode) {
				NetworkManager.GenerateMessageRegistrations();
			}
		}
	}
	class NetworkManagerBuildProcessor : UnityEditor.Build.IPreprocessBuildWithReport
	{
		public int callbackOrder => 0;
		public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report) {
			NetworkManager.GenerateMessageRegistrations();
		}
	}
#endif

}