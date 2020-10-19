using UnityEngine;

namespace MT.Packages.LD47
{
	public abstract class NetworkPoolObject : MonoBehaviour
	{
		[ReadOnly] public NetworkPool pool;
		[ReadOnly] public int playerID;
		[ReadOnly] public int objectID;
		[ReadOnly] public bool hasLifetime;
		[ReadOnly] public float lifetime;

		public virtual void OnInit() { }
		protected virtual void OnUpdate() { }

		void Awake() {
			OnRelease();
		}

		public void Update() {
			if (playerID == 0) {
				return;
			}
			if (hasLifetime) {
				if (lifetime <= 0) {
					Destroy();
					return;
				}
				lifetime -= Time.deltaTime;
			}
			OnUpdate();
		}

		public void Destroy() {
			pool.Destroy(playerID, objectID);
			OnRelease();
		}

		public bool IsMine() {
			return playerID == TNet.TNManager.playerID;
		}

		public abstract void OnInstantiate(object[] data);

		public abstract object[] PrepareData();

		public abstract void OnReceiveData(object[] data);

		public abstract void OnRelease();
	}
}