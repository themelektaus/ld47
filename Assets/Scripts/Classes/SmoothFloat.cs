using System;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class SmoothFloat : SmoothValue<float>
	{
		public float smoothTime;

		float velocity;

		public SmoothFloat(float value, float smoothTime) : base(value) {
			this.smoothTime = smoothTime;
		}

		public SmoothFloat(Func<float> getCurrent, Action<float> setCurrent, float smoothTime) : base(getCurrent, setCurrent) {
			this.smoothTime = smoothTime;
		}

		protected override float Update(float current, float target) {
			if (smoothTime == 0) {
				return target;
			}
			return Mathf.SmoothDamp(current, target, ref velocity, smoothTime);
		}
	}
}