using UnityEngine;

namespace MT.Packages.DPAE
{
	public static class EditorExtensionMethods
	{
		public static Texture2D Clone(this Texture2D @this) {
			var clone = new Texture2D(@this.width, @this.height, @this.format, false) {
				alphaIsTransparency = true,
				filterMode = FilterMode.Point
			};
			Graphics.CopyTexture(@this, clone);
			return clone;
		}
	}
}