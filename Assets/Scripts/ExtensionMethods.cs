using MT.Packages.Core;
using UnityEngine;

namespace MT.Packages.LD47
{
	public static class ExtensionMethods
	{
		public static Vector2 Rotate2D(this Vector2 @this, float angle) {
			angle *= Mathf.Deg2Rad;
			return new Vector2(
				@this.x * Mathf.Cos(angle) - @this.y * Mathf.Sin(angle),
				@this.x * Mathf.Sin(angle) + @this.y * Mathf.Cos(angle)
			);
		}
		
		public static byte GetRingIndex(this Object @this) {
			return @this.GetFromCache<IObjectInfo>().GetRingIndex();
		}

		public static void SetRingIndex(this Object @this, byte ringIndex) {
			@this.GetFromCache<IObjectInfo>().SetRingIndex(ringIndex);
		}
	}
}