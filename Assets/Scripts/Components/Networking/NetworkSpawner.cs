using Mirror;
using UnityEngine;

public class NetworkSpawner : NetworkBehaviour
{
	public GameObject prefab;
	public int count = 3;
	public float radius = 3;

	GameObject[] instances;

	public override void OnStartServer() {
		instances = new GameObject[count];
		for (int i = 0; i < instances.Length; i++) {
			instances[i] = Instantiate(prefab);
			instances[i].transform.parent = transform;
			instances[i].transform.position = Random.insideUnitCircle * radius;
			NetworkServer.Spawn(instances[i]);
		}
	}
}