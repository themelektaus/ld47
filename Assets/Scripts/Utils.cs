using UnityEngine;

public static class Utils
{
	public static Vector2 GetDirection2D(Vector2 a, Vector2 b) {
		return (b - a).normalized;
	}

	public static float GetAngle2D(Vector2 direction) {
		return GetAngle2D(Vector2.zero, direction);
	}

	public static float GetAngle2D(Vector2 a, Vector2 b) {
		var p = a - b;
		return Mathf.Atan2(p.y, p.x) * Mathf.Rad2Deg + 90;
	}
}