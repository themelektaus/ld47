
namespace MT.Packages.LD47
{
	public class ReadOnlyAttribute : UnityEngine.PropertyAttribute
	{
		public bool duringPlayMode;
	}

	public class ResourcePathAttribute : UnityEngine.PropertyAttribute
	{
		public string[] extensions { get; private set; }

		public ResourcePathAttribute(params string[] extensions) {
			this.extensions = extensions;
		}
	}
}