using UnityEngine;

namespace MT.Packages.LD47
{
	public class EnemySpawner : NetworkSpawner
	{
		protected override void OnInstantiate(GameObject gameObject) {
			base.OnInstantiate(gameObject);
			if (gameObject.TryGetComponent<Enemy>(out var enemy)) {
				enemy.SetRingIndex(this.GetRingIndex());
			}
		}
	}
}