using UnityEngine;

namespace MT.Packages.LD47
{
	public class Timer
	{
		public System.Func<float> getDeltaTime = () => Time.deltaTime;

		public Vector2 interval { get; private set; } = Vector2.zero;

		readonly bool updateOnStart;

		float? time;

		public Timer(float interval) : this(interval, interval) { }
		public Timer(float minInterval, float maxInterval) : this(minInterval, maxInterval, false) { }
		public Timer(float interval, bool updateOnStart) : this(interval, interval, updateOnStart) {		}
		public Timer(float minInterval, float maxInterval, bool updateOnStart) {
			interval = new Vector2(minInterval, maxInterval);
			this.updateOnStart = updateOnStart;
		}

		public static implicit operator bool(Timer timer) {
			return timer != null;
		}

		public bool Update() {
			if (!time.HasValue) {
				if (updateOnStart) {
					time = 0;
				} else {
					time = GetInterval();
				}
			}
			bool result = false;
			if (time <= 0) {
				time += GetInterval();
				result = true;
			}
			time -= getDeltaTime();
			return result;
		}

		float GetInterval() {
			if (interval.y > interval.x) {
				return Random.Range(interval.x, interval.y);
			}
			return interval.x;
		}
	}
}