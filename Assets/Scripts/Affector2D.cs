using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Affector2D : MonoBehaviour
{
	public bool isKinematic;
	public float localVelocityX;
	public Vector2 xPrecition = new Vector2(.1f, .5f);

	public Vector2 kinematicMovement;
	public Animator animator;
	
	Rigidbody2D body;
	Grounder2D[] grounder;

	[HideInInspector] public Vector2 velocity;

	readonly List<Gravitation2D> gravitations = new List<Gravitation2D>();

	Gravitation2D activeGravitation {
		get {
			if (gravitations.Count == 0) {
				return null;
			}
			return gravitations[0];
		}
	}

	public Vector2 gravityDirection {
		get {
			if (!activeGravitation) {
				return Vector2.down;
			}
			return Utils.GetDirection2D(activeGravitation.position, position);
		}
	}

	public Vector2 position {
		get { return body.position; }
		set { body.position = value; }
	}

	public bool grounded {
		get { return grounder.Any(x => x.grounded); }
	}

	void Awake() {
		body = GetComponentInChildren<Rigidbody2D>();
		grounder = GetComponentsInChildren<Grounder2D>();
	}

	public void Register(Gravitation2D gravitation) {
		gravitations.Add(gravitation);
	}

	void Update() {
		if (!animator) {
			return;
		}
		animator.SetFloat("Move", Mathf.Abs(localVelocityX));
	}

	void FixedUpdate() {
		body.SetRotation(Utils.GetAngle2D(position, activeGravitation.position));

		if (isKinematic) {
			body.velocity = Vector2.SmoothDamp(body.velocity, kinematicMovement, ref velocity, xPrecition.x);
		} else {
			Vector2 localVelocity = body.transform.InverseTransformDirection(body.velocity);
			localVelocity.y += velocity.y;
			localVelocity -= Physics2D.gravity * Time.fixedDeltaTime * activeGravitation.gravityFactor;
			localVelocity.x = Mathf.SmoothDamp(localVelocity.x, localVelocityX, ref velocity.x, grounded ? xPrecition.x : xPrecition.y);
			body.velocity = body.transform.TransformDirection(localVelocity);
			velocity.y = 0;
		}
	}

	public void SetYVelocity(float yVelocity) {
		if (isKinematic) {
			return;
		}
		velocity.y = yVelocity;
	}
}