using System.Linq;
using UnityEngine;
using FieldName = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace MT.Packages.LD47
{
	public class Attractor : MonoBehaviour
	{
		public enum State
		{
			Normal,
			Kinematic,
			Frozen
		}

		public State state = State.Normal;
		public float localVelocityX;
		public Vector2 kinematicMovement;

		[SerializeField, FieldName("xPrecition")] Vector2 velocitySmoothTimes = new Vector2(.1f, .5f);
		[SerializeField] float downMultiplier = 1;
		[SerializeField] Animator animator = null;

		public Rigidbody2D body { get; private set; }

		Grounder[] grounder;
		Gravitation gravitation;
		Vector2 velocity;

		public bool grounded {
			get { return grounder.Any(x => x.grounded); }
		}

		public float velocityY {
			set {
				if (state == State.Normal) {
					velocity.y = value;
				}
			}
		}

		void Awake() {
			body = GetComponentInChildren<Rigidbody2D>();
			grounder = GetComponentsInChildren<Grounder>();
			gravitation = FindObjectOfType<Gravitation>();
		}

		void Update() {
			if (!animator) {
				return;
			}
			animator.SetFloat("Move", Mathf.Abs(localVelocityX));
		}

		void FixedUpdate() {
			if (gravitation) {
				var angle = Utils.GetAngle2D(transform.position, gravitation.position);
				if (gravitation.gravityFactor > 0) {
					angle += 180;
				}
				body.SetRotation(angle);
			} else {
				body.SetRotation(0);
			}

			switch (state) {
				case State.Normal:
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

				case State.Kinematic:
					body.velocity = Vector2.SmoothDamp(body.velocity, kinematicMovement, ref velocity, velocitySmoothTimes.x);
					break;
			}
		}

		public void ResetVelocityY() {
			Vector2 localVelocity = body.transform.InverseTransformDirection(body.velocity);
			localVelocity.y = 0;
			body.velocity = body.transform.TransformDirection(localVelocity);
		}
	}
}