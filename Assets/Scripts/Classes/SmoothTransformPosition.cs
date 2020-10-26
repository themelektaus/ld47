using UnityEngine;

namespace MT.Packages.LD47
{
	public class SmoothTransformPosition : SmoothVector3
	{
		public static implicit operator SmoothTransformPosition((Transform transform, float smoothTime) value) {
			return new SmoothTransformPosition(value.transform, value.smoothTime);
		}

		public SmoothTransformPosition(Transform transform, float smoothTime)
			: base(() => transform.position, x => transform.position = x, smoothTime) {
			
		}
	}
}