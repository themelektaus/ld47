using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float after;

	void Update() {
		after -= Time.deltaTime;
		if (after <= 0) {
			Destroy(gameObject);
		}
	}
}