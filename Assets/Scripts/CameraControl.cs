﻿using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{
	public static CameraControl instance;

	public Vector4 playerReceiveDamageShake = new Vector4(5, 7, .1f, .1f);
	public Vector4 enemyReceiveDamageShake = new Vector4(3, 4, .1f, .1f);

	[Range(3, 100)] public float targetOrthographicSize = 8;
	[Range(0.01f, 2)] public float zoomDuration = .1f;
	
	public string wheelAxisName = "Mouse ScrollWheel";
	public float wheelSensitivity = 10;

	Camera _camera;
	float _velocity;
	
	void Awake() {
		instance = this;
		Application.targetFrameRate = 60;
		_camera = GetComponent<Camera>();
	}

	void Update() {
		float wheel = Input.GetAxis(wheelAxisName);
		targetOrthographicSize += (-wheel * wheelSensitivity) * (targetOrthographicSize / wheelSensitivity);
		targetOrthographicSize += -wheel * wheelSensitivity;
		targetOrthographicSize = Mathf.Clamp(targetOrthographicSize, 3, 100);
		_camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, targetOrthographicSize, ref _velocity, zoomDuration);
	}

	public static EZCameraShake.CameraShakeInstance ShakeOnce(Vector4 arguments) {
		return EZCameraShake.CameraShaker.Instance.ShakeOnce(arguments.x, arguments.y, arguments.z, arguments.w);
	}
}