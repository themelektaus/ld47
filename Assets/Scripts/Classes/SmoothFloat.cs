using System;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class SmoothFloat : SmoothValue<float>
	{
		public float smoothTime;

		float velocity;
		bool isAngle;

		public static implicit operator SmoothFloat((float value, float smoothTime) v) {
			return new SmoothFloat(v.value, v.smoothTime);
		}

		public SmoothFloat(float value, float smoothTime) : base(value) {
			this.smoothTime = smoothTime;
		}

		public SmoothFloat(Func<float> getCurrent, Action<float> setCurrent, float smoothTime) : base(getCurrent, setCurrent) {
			this.smoothTime = smoothTime;
		}

		public override void Update() {
			isAngle = false;
			base.Update();
		}

		public void Update(bool isAngle) {
			this.isAngle = isAngle;
			base.Update();
		}

		protected override float Update(float current, float target) {
			if (smoothTime == 0) {
				return target;
			}
			return isAngle ?
				Mathf.SmoothDampAngle(current, target, ref velocity, smoothTime) :
				Mathf.SmoothDamp(current, target, ref velocity, smoothTime);
		}
	}
}