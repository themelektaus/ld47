
namespace MT.Packages.LD47.Audio
{
	public class MinMaxAttribute : UnityEngine.PropertyAttribute
	{
		public float Min { get; private set; } = 0;
		public float Max { get; private set; } = 0;

		public MinMaxAttribute(float min, float max) {
			Min = min;
			Max = max;
		}
	}
}