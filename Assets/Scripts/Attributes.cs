
namespace MT.Packages.LD47
{
	public class ReadOnlyAttribute : UnityEngine.PropertyAttribute
	{

	}

	public class ResourcePathAttribute : UnityEngine.PropertyAttribute
	{
		public string[] Extensions { get; private set; }

		public ResourcePathAttribute(params string[] extensions) {
			Extensions = extensions;
		}
	}
}