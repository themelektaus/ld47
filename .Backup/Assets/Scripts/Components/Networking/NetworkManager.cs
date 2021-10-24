using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Flags = System.Reflection.BindingFlags;
#endif

namespace MT.Packages.LD47
{
	public partial class NetworkManager : Mirror.NetworkManager
	{
		public static NetworkManager instance;

		public static List<MonoBehaviour> entities = new List<MonoBehaviour>();

		public static Character GetClosestCharacter(byte ringIndex, Vector2 position, bool includeBots, float minDistance = 0) {
			return entities
				.Where(x => x is Character)
				.Select(x => x as Character)
				.Where(x => x.isInGameAndAlive && x.GetRingIndex() == ringIndex)
				.Where(x => includeBots || !x.isBot)
				.Select(character => {
					var magnitude = ((Vector2) character.transform.position - position).magnitude;
					return (character, magnitude);
				})
				.OrderBy(x => x.magnitude)
				.Where(x => minDistance == 0 || x.magnitude <= minDistance)
				.FirstOrDefault()
				.character;
		}

		public static Enemy GetClosestAttackingEnemy(byte ringIndex, Vector2 position, float maxDistance = 0) {
			return entities
				.Where(x => x is Enemy)
				.Select(x => x as Enemy)
				.Where(x => x.isReadyAndAlive && x.state == Enemy.State.Attack && x.GetRingIndex() == ringIndex)
				.Select(enemy => {
					var magnitude = ((Vector2) enemy.transform.position - position).magnitude;
					return (enemy, magnitude);
				})
				.OrderBy(x => x.magnitude)
				.Where(x => maxDistance == 0 || x.magnitude <= maxDistance)
				.FirstOrDefault()
				.enemy;
		}

		public static void Register(MonoBehaviour entity) {
			entities.Add(entity);
		}

		public static void Unregister(MonoBehaviour entity) {
			entities.Remove(entity);
			if (entity is Character character) {
				Utility.SendToClients((Pool_Message_DisableAll) character.netId, true);
			}
		}

		public bool isHost;

		Animator gameStateMachine;

		public override void Awake() {
			if (!this.SetSingleton(ref instance)) {
				return;
			}
			instance = this;
			gameStateMachine = GetComponent<Animator>();
			AudioSystem.AudioLibrary.forcedOwner = this;
			base.Awake();
		}

		public override void OnStartServer() {
			base.OnStartServer();
			RegisterServerMessageHandlers();
		}

		public override void OnStartClient() {
			base.OnStartClient();
			RegisterClientMessageHandlers();
		}

		public void RespawnPlayer() {
			if (CharacterController.instance && CharacterController.instance.character.isDead) {
				CharacterController.instance.Spawn();
			}
		}

		public void SetTrigger(string name) {
			gameStateMachine.SetTrigger(name);
		}

		[Server]
		public void Server_SendCharacterSkins() {
			foreach (var character in entities.Where(x => x is Character).Select(x => x as Character)) {
				if (character.animator.TryGetComponent(out CharacterSkin skin)) {
					character.ClientRpc_ApplySkin(skin.data);
				}
			}
		}

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
		public void RegisterServerMessageHandlers() {
";
			foreach (var messageType in messageTypes) {
				if (messageType.GetMethod("OnServerReceive", Flags.Public | Flags.Static) != null) {
					contents += $"			NetworkServer.RegisterHandler<{messageType.Name}>({messageType.Name}.OnServerReceive);\r\n";
				}
			}
			contents += @"		}
		
		public void RegisterClientMessageHandlers() {
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
}