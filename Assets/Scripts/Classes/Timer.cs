using UnityEngine;

namespace MT.Packages.LD47
{
	public class Timer
	{
		public System.Func<float> getDeltaTime = () => Time.deltaTime;

		public Vector2 interval { get; private set; } = Vector2.zero;

		float time;

		public static implicit operator Timer(float interval) {
			return new Timer(interval);
		}

		public static implicit operator Timer((float minInterval, float maxInterval) value) {
			return new Timer(value.minInterval, value.maxInterval);
		}

		public Timer(float interval) : this(interval, interval) { }

		public Timer(float minInterval, float maxInterval) {
			interval = new Vector2(minInterval, maxInterval);
			Reset();
		}

		public static implicit operator bool(Timer timer) {
			return timer != null;
		}

		public void Reset() {
			time = 0;
		}

		public bool Update() {
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
				return UnityEngine.Random.Range(interval.x, interval.y);
			}
			return interval.x;
		}
	}
}