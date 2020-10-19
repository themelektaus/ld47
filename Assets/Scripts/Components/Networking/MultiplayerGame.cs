using TNet;
using UnityEngine;
using S = UnityEngine.SceneManagement;

namespace MT.Packages.LD47
{
	public class MultiplayerGame : Singleton<MultiplayerGame>
	{
		public string sceneName = "Multiplayer Map 1";
		public bool spawnPlayer = false;

		// [SerializeField, ReadOnly] bool _isHosting = false;

		public int channelID { get; private set; }

		bool sceneLoaded;
		bool spawningPlayer;

		void Start() {
			TNManager.Connect("34.65.179.224", 5127);
		}

		void OnEnable() {
			TNManager.onConnect += OnConnect;
			TNManager.onJoinChannel += OnJoinChannel;
			// TNManager.onLeaveChannel += OnLeaveChannel;
		}

		void OnDisable() {
			sceneLoaded = false;
			TNManager.onConnect -= OnConnect;
			TNManager.onJoinChannel -= OnJoinChannel;
			// TNManager.onLeaveChannel -= OnLeaveChannel;
		}

		void OnConnect(bool success, string message) {
			if (!success) {
				Debug.LogError(message);
				return;
			}
			var uptime = System.TimeSpan.FromMilliseconds(TNManager.serverUptime);
			Debug.Log($"Server Uptime: {(int) uptime.TotalDays} days, {uptime.Hours} hours, {uptime.Minutes} minutes");
			TNManager.JoinChannel(1);
		}

		void OnJoinChannel(int channelID, bool success, string message) {
			this.channelID = channelID;
			if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null) {
				if (isHosting) {
					Debug.LogError("isHosting");
#if UNITY_EDITOR
					// Debug.Break();
#else
					// Application.Quit();
#endif
				}
			}
			if (S.SceneManager.GetSceneByName(sceneName).isLoaded) {
				sceneLoaded = true;
			} else {
				S.SceneManager.LoadSceneAsync(sceneName, S.LoadSceneMode.Additive).completed += x => sceneLoaded = true;
			}
		}

		// void OnLeaveChannel(int channelID) {
		// 	this.channelID = 0;
		// 	S.SceneManager.UnloadSceneAsync(sceneName).completed += x => sceneLoaded = false;
		// }

		public static bool isReady {
			get {
				if (instance.channelID == 0 || !instance.sceneLoaded) {
					return false;
				}
				if (TNManager.isConnected && TNManager.IsInChannel(instance.channelID)) {
					return true;
				}
				return false;
			}
		}

		public static bool isHosting {
			get {
				return TNManager.IsHosting(instance.channelID);
			}
		}

		public static bool isReadyAndHosting {
			get {
				if (isReady && isHosting) {
					return true;
				}
				return false;
			}
		}

		void Update() {
			if (!isReady) {
				return;
			}
			if (spawnPlayer && !spawningPlayer) {
				if (FindObjectOfType<PlayerController>()) {
					spawnPlayer = false;
				} else {
					spawningPlayer = true;
					SpawnPlayer();
				}
			}
		}

		public static void SpawnPlayer() {
			Vector3 position;
			var playerSpawnArea = GameObject.FindGameObjectWithTag("Player Spawn Area");
			if (playerSpawnArea) {
				if (playerSpawnArea.TryGetComponent<BoxCollider2D>(out var boxCollider)) {
					position = new Vector3(
						Random.Range(boxCollider.bounds.min.x, boxCollider.bounds.max.x),
						Random.Range(boxCollider.bounds.min.y, boxCollider.bounds.max.y),
						playerSpawnArea.transform.position.z
					);
				} else {
					position = playerSpawnArea.transform.position;
				}
			} else {
				position = Vector3.zero;
			}
			TNManager.Instantiate(instance.channelID, nameof(RCC_CreatePlayer), "Player", false, position);
		}

		[RCC]
		public static GameObject RCC_CreatePlayer(GameObject prefab, Vector3 position) {
			var gameObject = prefab.Instantiate();
			if (gameObject.TryGetComponent<Player>(out var player)) {
				player.SetPosition(position);
				if (player.tno.isMine) {
					player.attractor.state = Attractor.State.Normal;
					gameObject.AddComponent<PlayerController>();
					if (Camera.main.TryGetComponent<SimpleInterpolation>(out var interpolation)) {
						interpolation.target = gameObject.transform;
					}
					instance.spawnPlayer = false;
					instance.spawningPlayer = false;
				}
			}
			return gameObject;
		}
	}
}