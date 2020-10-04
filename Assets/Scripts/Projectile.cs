using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [System.NonSerialized] public Affector2D owner;
	[System.NonSerialized] public Vector2 direction;

	public float damage = 1;
	public float speed = 20;
	public GameObject sprites;

	bool destroyed;

	void Start() {
		Update();
	}

	void Update() {
		if (destroyed) {
			return;
		}
		var p = (Vector2) transform.position + direction * Time.deltaTime * speed;
		transform.position = new Vector3(p.x, p.y, owner.transform.position.z);
		transform.eulerAngles = new Vector3(0, 0, Utils.GetAngle2D(direction));
	}

	void OnTriggerEnter2D(Collider2D collision) {
		if (destroyed) {
			return;
		}
		if (collision.transform.IsChildOf(owner.transform)) {
			return;
		}
		if (collision.TryGetComponent<Projectile>(out _)) {
			return;
		}
		if (owner.TryGetComponent<IObjective>(out var ownerObjective)) {
			if (owner.CompareTag(collision.tag)) {
				return;
			}
			if (collision.TryGetComponent<IObjective>(out var collisionObjective)) {
				if (collisionObjective.GetRingIndex() == ownerObjective.GetRingIndex()) {
					collisionObjective.ReceiveDamage(damage);
				}
			}
		}
		CustomDestroy();
	}

	void CustomDestroy() {
		destroyed = true;
		Destroy(sprites);
		StartCoroutine(CustomDestroyRoutine());
	}

	IEnumerator CustomDestroyRoutine() {
		yield return new WaitForSeconds(5);
		Destroy(gameObject);
	}

	public static void Spawn(Affector2D sourceAffector, Vector2 position, Projectile projectile, Transform parent, Vector2 targetPosition) {
		var projectileInstance = Instantiate(projectile, parent);
		var a = projectileInstance.transform.position;
		var b = position;
		a.x = b.x;
		a.y = b.y;
		projectileInstance.transform.position = a;
		projectileInstance.transform.parent = null;
		projectileInstance.owner = sourceAffector;
		projectileInstance.direction = Utils.GetDirection2D(position, targetPosition);
	}
}