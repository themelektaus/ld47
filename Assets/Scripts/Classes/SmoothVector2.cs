using System;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class SmoothVector2 : SmoothValue<Vector2>
	{
		public float smoothTime;

		Vector2 velocity;

		public SmoothVector2(Vector2 value, float smoothTime) : base(value) {
			this.smoothTime = smoothTime;
		}

		public SmoothVector2(Func<Vector2> getCurrent, Action<Vector2> setCurrent, float smoothTime) : base(getCurrent, setCurrent) {
			this.smoothTime = smoothTime;
		}

		protected override Vector2 Update(Vector2 current, Vector2 target) {
			if (smoothTime == 0) {
				return target;
			}
			return Vector2.SmoothDamp(current, target, ref velocity, smoothTime);
		}
	}
}