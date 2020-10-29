using System.Linq;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class Attractor : MonoBehaviour
	{
		public enum Mode
		{
			Normal,
			Kinematic,
			Frozen
		}
		public Mode mode = Mode.Normal;

		public float localVelocityX;
		public Vector2 kinematicMovement;

		[SerializeField] Vector2 velocitySmoothTimes = new Vector2(.1f, .5f);
		[SerializeField] float downMultiplier = 1;
		[SerializeField] Animator animator = null;

		public Rigidbody2D body { get; private set; }
		public SmoothFloat rotation;

		Grounder[] grounder;
		Vector2 velocity;

		public Gravitation gravitation { get; private set; }

		public bool grounded {
			get { return grounder.Any(x => x.grounded); }
		}

		public float velocityY {
			set {
				if (mode == Mode.Normal) {
					velocity.y = value;
				}
			}
		}

		void Awake() {
			body = GetComponentInChildren<Rigidbody2D>();
			grounder = GetComponentsInChildren<Grounder>();
			gravitation = FindObjectOfType<Gravitation>();
			rotation = new SmoothFloat(() => body.rotation, x => body.SetRotation(x), .1f);
		}

		void Update() {
			if (!animator) {
				return;
			}
			animator.SetFloat("Move", Mathf.Abs(localVelocityX));
		}

		void FixedUpdate() {
			rotation.target = GetAngle();
			rotation.Update(true);
			switch (mode) {
				case Mode.Normal:
					Vector2 localVelocity = body.transform.InverseTransformDirection(body.velocity);
					localVelocity.y += velocity.y;
					float factor = gravitation ? -Mathf.Abs(gravitation.gravityFactor) : -2;
					if (localVelocity.y < 0) {
						factor *= downMultiplier;
					}
					localVelocity -= Physics2D.gravity * Time.fixedDeltaTime * factor;
					float velocitySmoothTime = grounded ? velocitySmoothTimes.x : velocitySmoothTimes.y;
					localVelocity.x = Mathf.SmoothDamp(localVelocity.x, localVelocityX, ref velocity.x, velocitySmoothTime);
					body.velocity = body.transform.TransformDirection(localVelocity);
					velocity.y = 0;
					break;

				case Mode.Kinematic:
					body.velocity = Vector2.SmoothDamp(body.velocity, kinematicMovement, ref velocity, velocitySmoothTimes.x);
					break;

				case Mode.Frozen:
					break;
			}
		}

		public float GetAngle() {
			if (!gravitation) {
				return 0;
			}
			var angle = Utils.GetAngle2D(transform.position, gravitation.position);
			if (gravitation.gravityFactor > 0) {
				angle += 180;
			}
			return angle;
		}

		public void ResetVelocityY() {
			if (body.bodyType != RigidbodyType2D.Static) {
				Vector2 localVelocity = body.transform.InverseTransformDirection(body.velocity);
				localVelocity.y = 0;
				body.velocity = body.transform.TransformDirection(localVelocity);
			}
		}
	}
}