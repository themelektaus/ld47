using UnityEngine;

namespace MT.Packages.LD47
{
	[ExecuteInEditMode]
	public class Waypoint : MonoBehaviour
	{
		[SerializeField] bool drawAllRadiusGizmos = false;
		[SerializeField] bool drawAllConnectionGizmos = false;

		public byte ringIndex;
		public float radius = 3;
		public Vector2 offset;
		public float distanceDamping;
		public float jump = 10;
		public Waypoint[] nextWaypoints = new Waypoint[0];

		static bool _drawAllRadiusGizmos;
		static bool _drawAllConnectionGizmos;

		public Vector2 GetPosition() {
			var position = (Vector2) transform.position;
			position += offset.Rotate2D(transform.localEulerAngles.z);
			return position;
		}

		void Update() {
			if (_drawAllRadiusGizmos != drawAllRadiusGizmos) {
				_drawAllRadiusGizmos = drawAllRadiusGizmos;
				foreach (var waypoint in FindObjectsOfType<Waypoint>()) {
					waypoint.drawAllRadiusGizmos = _drawAllRadiusGizmos;
				}
			}
			if (_drawAllConnectionGizmos != drawAllConnectionGizmos) {
				_drawAllConnectionGizmos = drawAllConnectionGizmos;
				foreach (var waypoint in FindObjectsOfType<Waypoint>()) {
					waypoint.drawAllConnectionGizmos = _drawAllConnectionGizmos;
				}
			}
		}

		public Waypoint GetNextWaypoint() {
			if (nextWaypoints.Length == 0) {
				return null;
			}
			return nextWaypoints[0];
		}

#if UNITY_EDITOR
		void OnDrawGizmos() {
			var position = GetPosition();
			var activeGameObject = UnityEditor.Selection.activeGameObject;
			if (activeGameObject) {
				if (activeGameObject.TryGetComponent(out CharacterBot bot)) {
					if (bot.target.waypoint == this) {
						Gizmos.color = Color.red;
						Gizmos.DrawWireSphere(position, 1);
						Gizmos.DrawWireSphere(position, 2);
					}
				}
			}
			if (drawAllRadiusGizmos) {
				DrawRadiusGizmos(position);
			}
			if (drawAllConnectionGizmos) {
				DrawConnectionGizmos(position);
			}
		}

		void OnDrawGizmosSelected() {
			if (!drawAllRadiusGizmos) {
				DrawRadiusGizmos(GetPosition());
			}
			if (!drawAllConnectionGizmos) {
				DrawConnectionGizmos(GetPosition());
			}
		}

		void DrawRadiusGizmos(Vector2 position) {
			Gizmos.color = new Color(1, 1, 0, .2f);
			Gizmos.DrawWireSphere(position, radius);
		}

		void DrawConnectionGizmos(Vector2 position) {
			Gizmos.color = new Color(0, 1, 0, .6f);
			foreach (var waypoint in nextWaypoints) {
				if (!waypoint) {
					continue;
				}
				var waypointPosition = waypoint.GetPosition();
				Vector2 lineDirection = waypointPosition - position;
				Vector2 lineDirectionNormalized = lineDirection.normalized;
				float lineMagnitude = lineDirection.magnitude;

				float offset = 1 / lineMagnitude;

				Vector2 start = Vector2.Lerp(position, waypointPosition, offset / 2);
				Vector2 end = Vector2.Lerp(position, waypointPosition, 1 - offset / 2);

				// DrawArrow(start + lineDirectionNormalized * 2, lineDirectionNormalized);
				DrawArrow(end, lineDirectionNormalized);

				Gizmos.DrawLine(start, end);
			}
		}

		void DrawArrow(Vector2 position, Vector2 direction) {
			var a = position + direction.Rotate2D(160);
			var b = position + direction.Rotate2D(-160);
			Gizmos.DrawLine(position, a);
			Gizmos.DrawLine(position, b);
			Gizmos.DrawLine(a, b);
		}
#endif
	}
}