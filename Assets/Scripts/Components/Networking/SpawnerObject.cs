using TNet;

namespace MT.Packages.LD47
{
	public abstract class SpawnerObject : TNBehaviour
	{
		[UnityEngine.SerializeField, ReadOnly] protected bool isReadyAndMine;

		protected override void Awake() {
			base.Awake();
			Spawner.spawnerObjects.Add(this);
			if (MultiplayerGame.isReadyAndHosting) {
				OnHostAwake();
			}
		}

		public void Update() {
			isReadyAndMine = MultiplayerGame.isReadyAndHosting;
			if (isReadyAndMine) {
				OnHostUpdate();
			} else {
				OnRemoteUpdate();
			}
		}

		public void FixedUpdate() {
			if (isReadyAndMine) {
				OnFixedHostUpdate();
			}
		}

		void OnDestroy() {
			Spawner.spawnerObjects.Remove(this);
		}

		protected abstract void OnHostAwake();

		protected abstract void OnHostUpdate();

		protected abstract void OnRemoteUpdate();

		protected virtual void OnFixedHostUpdate() { }
	}
}