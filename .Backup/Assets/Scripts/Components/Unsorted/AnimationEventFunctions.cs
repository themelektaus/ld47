using Mirror;
using MT.Packages.Core;
using System.Collections;
using UnityEngine;

namespace MT.Packages.LD47 {
	public class AnimationEventFunctions : MonoBehaviour
	{
		[ServerCallback]
		public void Server_SetEnemyReady() {
			this.GetParentFromCache<Enemy>().ready = true;
		}

		[ServerCallback]
		public void Server_DestroyEnemy(float delay) {
			StartCoroutine(Server_DestroyEnemyRoutine(delay));
		}

		IEnumerator Server_DestroyEnemyRoutine(float delay) {
			yield return new WaitForSeconds(delay);
			NetworkServer.Destroy(GetComponentInParent<Enemy>().gameObject);
		}

		[ServerCallback]
		public void Server_EyeEnemy_Shoot() {
			this.GetParentFromCache<EyeEnemy>().Shoot();
		}
	}
}