using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{
	public static CameraControl instance;

	[Range(-1, 60)] public int targetFrameRate = 60;
	[Range(4, 9)] public float targetOrthographicSize = 9;
	[Range(0.01f, 2)] public float zoomDuration = .2f;
	
	public string wheelAxisName = "Mouse ScrollWheel";
	public float wheelSensitivity = 5;

	public float playerReceiveDamageShake = .6f;
	public float enemyReceiveDamageShake = .3f;

	Camera _camera;
	float _velocity;
	
	void Awake() {
		instance = this;
		Application.targetFrameRate = targetFrameRate;
		_camera = GetComponent<Camera>();
	}

	void Update() {
		float wheel = Input.GetAxis(wheelAxisName);
		targetOrthographicSize += (-wheel * wheelSensitivity) * (targetOrthographicSize / wheelSensitivity);
		targetOrthographicSize += -wheel * wheelSensitivity;
		targetOrthographicSize = Mathf.Clamp(targetOrthographicSize, 4, 9);
		_camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, targetOrthographicSize, ref _velocity, zoomDuration);
	}
}