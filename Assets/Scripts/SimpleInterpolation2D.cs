using UnityEngine;

public class SimpleInterpolation2D : MonoBehaviour
{
	public Transform source;
	public Transform target;
	public Vector2 interpolationSpeed = new Vector2(30, 30);
	public bool includeRotation;
	public bool fixedUpdate;
	public bool lateUpdate;

	Vector2 localPosition = Vector3.zero;

	public void SetPositionImmediatelly() {
		source.position = target.position;
	}

	void Update() {
		if (fixedUpdate == lateUpdate) {
			OnUpdate();
		}
	}

	void FixedUpdate() {
		if (fixedUpdate && !lateUpdate) {
			OnUpdate();
		}
	}

	void LateUpdate() {
		if (!fixedUpdate && lateUpdate) {
			OnUpdate();
		}
	}

	void OnUpdate() {
		if (!source || !target) {
			return;
		}
		if (interpolationSpeed != Vector2.zero) {
			UpdatePosition();
		} else {
			source.position = new Vector3(
				target.position.x,
				target.position.y,
				source.position.z
			);
		}
		if (includeRotation) {
			source.rotation = target.rotation;
		}
	}

	void UpdatePosition() {
		Vector3 p;
		
		p = source.position;
		var sourcePosition = new Vector2(p.x, p.y);
		
		p = target.position;
		var targetPosition = new Vector2(p.x, p.y);

		localPosition = targetPosition - sourcePosition;

		float x = interpolationSpeed.x;
		float y = interpolationSpeed.y;

		SetValue(ref localPosition.x, x);
		SetValue(ref localPosition.y, y);

		var newPosition = targetPosition - localPosition;

		UpdateValue(ref newPosition.x, targetPosition.x, x);
		UpdateValue(ref newPosition.y, targetPosition.y, y);

		source.position = new Vector3(
			newPosition.x,
			newPosition.y,
			source.position.z
		);
	}

	void SetValue(ref float localValue, float interpolationValue) {
		if (interpolationValue > 0) {
			float deltaTime;
			if (fixedUpdate && !lateUpdate) {
				deltaTime = Time.fixedDeltaTime;
			} else {
				deltaTime = Time.deltaTime;
			}
			localValue -= localValue * Mathf.Min(interpolationValue * deltaTime, 1);
		}
	}

	void UpdateValue(ref float newValue, float targetValue, float interpolationValue) {
		if (interpolationValue < 0) {
			newValue = targetValue;
		}
	}
}