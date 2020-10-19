using System.Linq;
using TNet;
using G = System.Collections.Generic;

namespace MT.Packages.LD47
{
	public abstract class NetworkPool : TNBehaviour
	{
		protected static G.Dictionary<NetworkPoolObject, NetworkPool> instances;

		public static bool TryGet<T, S>(T poolObject, out S pool) where T : NetworkPoolObject where S : NetworkPool {
			if (instances == null) {
				instances = new G.Dictionary<NetworkPoolObject, NetworkPool>();
			}
			if (instances.ContainsKey(poolObject)) {
				pool = instances[poolObject] as S;
				return true;
			}
			UnityEngine.Debug.LogError($"object {typeof(T)} in pool {typeof(S)} not exists");
			pool = null;
			return false;
		}

		static bool HasZeroError(int playerID, int objectID) {
			if (playerID == 0) {
				UnityEngine.Debug.LogError("playerID == 0");
				return true;
			}
			if (objectID == 0) {
				UnityEngine.Debug.LogError("objectID == 0");
				return true;
			}
			return false;
		}

		static object[] ToParameters(int playerID, int objectID, object[] data) {
			return (new object[] { playerID, objectID }).Concat(data).ToArray();
		}

		static (int playerID, int objectID, object[] data) FromParameters(object[] data) {
			return ((int) data[0], (int) data[1], data.Skip(2).ToArray());
		}

		[UnityEngine.SerializeField] int channelID = 1;
		[UnityEngine.SerializeField] NetworkPoolObject prefab = null;
		[UnityEngine.SerializeField, UnityEngine.Range(1, 200)] int size = 50;
		[UnityEngine.Range(0, 1)] public float syncInterval = .15f;
		[UnityEngine.SerializeField, UnityEngine.Range(0, 10)] float maxLifetime = 3;

		[UnityEngine.SerializeField, ReadOnly] int remoteObjectsCount = 0;

		int lastID;
		Timer timer;

		readonly G.List<NetworkPoolObject> localObjects = new G.List<NetworkPoolObject>();
		readonly G.List<NetworkPoolObject> remoteObjects = new G.List<NetworkPoolObject>();

		void OnDestroy() {
			if (instances == null) {
				instances = new G.Dictionary<NetworkPoolObject, NetworkPool>();
			}
			if (!instances.ContainsKey(prefab)) {
				UnityEngine.Debug.LogWarning("Singleton not set");
				return;
			}
			if (this != instances[prefab]) {
				UnityEngine.Debug.LogWarning("Singleton is not this");
				return;
			}
			if (!Singleton<UnityEngine.MonoBehaviour>.singletons.Contains(this)) {
				UnityEngine.Debug.LogWarning("Singleton is not listed");
				return;
			}
			Singleton<UnityEngine.MonoBehaviour>.singletons.Remove(this);
			instances.Remove(prefab);
		}

		protected override void Awake() {
			if (instances == null) {
				instances = new G.Dictionary<NetworkPoolObject, NetworkPool>();
			}
			if (instances.ContainsKey(prefab)) {
				UnityEngine.Debug.LogWarning("Singleton already setted");
			} else {
				instances[prefab] = this;
				Singleton<UnityEngine.MonoBehaviour>.singletons.Add(this);
			}
			base.Awake();
			tno.channelID = channelID;
			timer = new Timer(syncInterval);
			localObjects.Clear();
			remoteObjects.Clear();
			for (int i = 0; i < size; i++) {
				var t = UnityEngine.Object.Instantiate(prefab);
				t.transform.parent = transform;
				t.pool = this;
				t.OnInit();
				t.OnRelease();
				localObjects.Add(t);
			}
		}

		void Update() {
			if (timer.Update()) {
				foreach (var o in localObjects) {
					if (o.playerID == 0) {
						continue;
					}
					tno.Send(2, Target.Others, ToParameters(o.playerID, o.objectID, o.PrepareData()));
				}
			}
			remoteObjects.RemoveAll(o => o.playerID == 0);
			remoteObjectsCount = remoteObjects.Count;
		}

		protected NetworkPoolObject Instantiate(params object[] data) {
			var playerID = TNManager.playerID;
			var objectID = ++lastID;
			var result = CreateInstance(playerID, objectID, data);
			tno.Send(1, Target.Others, ToParameters(playerID, objectID, data));
			return result;
		}

		NetworkPoolObject CreateInstance(int playerID, int objectID, object[] data) {
			foreach (var localObject in localObjects) {
				if (localObject.playerID == 0) {
					localObject.playerID = playerID;
					localObject.objectID = objectID;
					localObject.hasLifetime = maxLifetime > 0;
					localObject.lifetime = maxLifetime;
					localObject.OnInstantiate(data);
					localObject.Update();
					return localObject;
				}
			}
			throw new System.Exception();
		}

		public void Destroy(int playerID, int objectID) {
			if (HasZeroError(playerID, objectID)) {
				return;
			}
			foreach (var localObject in localObjects) {
				if (localObject.playerID == playerID && localObject.objectID == objectID) {
					tno.Send(3, Target.Others, playerID, objectID);
					localObject.playerID = 0;
					localObject.objectID = 0;
					return;
				}
			}
			throw new System.Exception();
		}

		[RFC(1)]
		protected void RFC_Add(object[] args) {
			(int playerID, int objectID, object[] data) = FromParameters(args);
			if (HasZeroError(playerID, objectID)) {
				return;
			}
			remoteObjects.Add(CreateInstance(playerID, objectID, data));
		}

		[RFC(2)]
		protected void RFC_Update(object[] args) {
			(int playerID, int objectID, object[] data) = FromParameters(args);
			if (HasZeroError(playerID, objectID)) {
				return;
			}
			foreach (var remoteObject in remoteObjects.Where(x => x.playerID == playerID && x.objectID == objectID)) {
				remoteObject.OnReceiveData(data);
			}
		}

		[RFC(3)]
		protected void RFC_Remove(int playerID, int objectID) {
			if (HasZeroError(playerID, objectID)) {
				return;
			}
			foreach (var remoteObject in remoteObjects) {
				if (remoteObject.playerID == playerID && remoteObject.objectID == objectID) {
					remoteObject.playerID = 0;
					remoteObject.objectID = 0;
					remoteObject.OnRelease();
				}
			}
		}
	}
}