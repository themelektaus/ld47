using UnityEngine;

namespace MT.Packages.LD47
{
	[RequireComponent(typeof(Camera))]
	public class CameraControl : Singleton<CameraControl>
	{
		public float playerReceiveDamageShake = .6f;
		public float enemyReceiveDamageShake = .3f;

		[Range(-1, 60)] public int targetFrameRate = 60;

		[SerializeField, Range(4, 10)] float targetOrthographicSize = 10;
		[SerializeField, Range(0.01f, 2)] float zoomDuration = .2f;

		[SerializeField] string wheelAxisName = "Mouse ScrollWheel";
		[SerializeField] float wheelSensitivity = 5;

		Camera _camera;
		float _velocity;

		protected override void OnAwake() {
			base.OnAwake();
			if (targetFrameRate == 0) {
				Debug.LogWarning("Cannot setup a target framerate of 0. Fallback to -1");
				Application.targetFrameRate = -1;
			} else {
				Application.targetFrameRate = targetFrameRate;
			}
			_camera = GetComponent<Camera>();
		}

		void Update() {
			float wheel = Input.GetAxis(wheelAxisName);
			targetOrthographicSize += (-wheel * wheelSensitivity) * (targetOrthographicSize / wheelSensitivity);
			targetOrthographicSize += -wheel * wheelSensitivity;
			targetOrthographicSize = Mathf.Clamp(targetOrthographicSize, 4, 9);
			_camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, targetOrthographicSize, ref _velocity, zoomDuration);
		}

		void OnDestroy() {
			Application.targetFrameRate = -1;
		}
	}
}