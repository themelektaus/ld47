using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{
	[Range(3, 100)] public float targetOrthographicSize = 8;
	[Range(0.01f, 2)] public float zoomDuration = .1f;

	public string wheelAxisName = "Mouse ScrollWheel";
	public float wheelSensitivity = 10;

	Camera _camera;
	float _velocity;
	float _z;
	
	void Awake() {
		_camera = GetComponent<Camera>();
		_z = _camera.transform.localPosition.z;
	}

	void Update() {
		// float wheel = Input.GetAxis(wheelAxisName);
		// targetOrthographicSize += (-wheel * wheelSensitivity) * (targetOrthographicSize / wheelSensitivity);
		// targetOrthographicSize += -wheel * wheelSensitivity;
		// targetOrthographicSize = Mathf.Clamp(targetOrthographicSize, 3, 100);
		_camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, targetOrthographicSize, ref _velocity, zoomDuration);
		var p = _camera.transform.localPosition;
		_camera.transform.localPosition = new Vector3(p.x, p.y, _z);
	}
}