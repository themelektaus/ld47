using System;

namespace MT.Packages.LD47
{
	public abstract class SmoothValue<T>
	{
		T _current;
		readonly Func<T> _getCurrent;
		readonly Action<T> _setCurrent;

		public T target { get; set; }

		public SmoothValue() : this(default) { }

		public SmoothValue(T value) {
			_getCurrent = () => _current;
			_setCurrent = x => _current = x;
			_current = value;
			target = value;
		}

		public SmoothValue(Func<T> getCurrent, Action<T> setCurrent) {
			_getCurrent = getCurrent;
			_setCurrent = setCurrent;
			_current = getCurrent();
			target = getCurrent();
		}

		public static implicit operator bool(SmoothValue<T> value) {
			return value != null;
		}

		public void Update() {
			_setCurrent(Update(_getCurrent(), target));
		}

		protected abstract T Update(T current, T target);

		public T current { get { return _getCurrent(); } }
		public T value { set { _setCurrent(value); target = value; } }
	}
}