using UnityEngine;

namespace MT.Packages.LD47
{
	[RequireComponent(typeof(Camera))]
	public class CameraControl : Core.Singleton<CameraControl>
	{
		public float characterReceiveDamageShake = .6f;
		public float enemyReceiveDamageShake = .3f;

		[Range(-1, 60), Core.Attributes.ReadOnly(onlyDuringPlayMode = true)]
		public int targetFrameRate = 60;

		[Range(4, 20), SerializeField]
		float targetOrthographicSize = 10;

		[SerializeField, Range(0.01f, 2)] float zoomDuration = .2f;

		[SerializeField] string wheelAxisName = "Mouse ScrollWheel";
		[SerializeField] float wheelSensitivity = 5;

		float velocity;
		SimpleInterpolation2D interpolation;

		protected override void Awake() {
			base.Awake();
			if (targetFrameRate == 0) {
				Debug.LogWarning("Cannot setup a target framerate of 0. Fallback to -1");
				Application.targetFrameRate = -1;
			} else {
				Application.targetFrameRate = targetFrameRate;
			}
			interpolation = GetComponent<SimpleInterpolation2D>();
		}

		void Update() {
			float wheel = Input.GetAxis(wheelAxisName);
			targetOrthographicSize += (-wheel * wheelSensitivity) * (targetOrthographicSize / wheelSensitivity);
			targetOrthographicSize += -wheel * wheelSensitivity;
			targetOrthographicSize = Mathf.Clamp(targetOrthographicSize, 4, Spectator.instance.enabled ? 20 : 10);
			var camera = Camera.main;
			camera.orthographicSize = Mathf.SmoothDamp(camera.orthographicSize, targetOrthographicSize, ref velocity, zoomDuration);
		}

		protected override void OnDestroy() {
			Application.targetFrameRate = -1;
			base.OnDestroy();
		}

		public void Reset(float targetOrthographicSize) {
			this.targetOrthographicSize = targetOrthographicSize;
			var position = transform.localPosition;
			position.x = 0;
			position.y = 0;
			transform.localPosition = position;
			transform.localRotation = Quaternion.identity;
		}

		public void SetTarget(Transform target, float interpolationSpeed, float rotationSmoothness) {
			interpolation.target = target;
			interpolation.interpolationSpeed = Vector3.one * interpolationSpeed;
			interpolation.rotationSmoothness = rotationSmoothness;
		}
	}
}