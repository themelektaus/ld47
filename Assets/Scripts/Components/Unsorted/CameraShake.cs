using UnityEngine;

namespace MT.Packages.LD47
{
    public class CameraShake : Singleton<CameraShake>
    {
        [SerializeField, Range(0, 1)] float shake;
        [SerializeField] Vector2 noise = new Vector2(1, 10);
        [SerializeField] float strength = 10;
        [SerializeField] float magnitude = .6f;
        [SerializeField] float rotationMagnitude = 17;
        [SerializeField] float decay = 1.3f;
        [SerializeField, Range(.1f, .9f)] float power = .3f;
        [SerializeField] float backupTime = .5f;

        float z;
        float time;
        Vector3 velocity;

		protected override void Awake() {
            base.Awake();
            z = transform.localPosition.z;
        }

        public static void Add(float shake) {
            instance.shake = Mathf.Clamp01(shake);
        }

        void Update() {
            if (shake > 0) {
                float delta = Time.deltaTime;
                if (CameraControl.instance.targetFrameRate != -1) {
                    delta *= 60f / CameraControl.instance.targetFrameRate;
                }
                time += delta * Mathf.Pow(shake, power) * strength;
                transform.localPosition = new Vector3(
                    (Mathf.PerlinNoise(noise.x, time) - 0.5f) * 2,
                    (Mathf.PerlinNoise(noise.y, time) - 0.5f) * 2,
                    z
                ) * magnitude * shake;
                shake = Mathf.Max(0, shake - Time.deltaTime * decay * (shake + power));
            } else {
                transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref velocity, backupTime);
            }
            transform.localRotation = Quaternion.Euler(transform.localPosition * rotationMagnitude);
        }
    }
}