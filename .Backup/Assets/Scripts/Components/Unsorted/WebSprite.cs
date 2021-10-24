using MT.Packages.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace MT.Packages.LD47
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class WebSprite : MonoBehaviour
	{
		public int textureSize = 256;
		public string url; // Example: https://pngimg.com/uploads/hat/hat_PNG5718.png

		string currentURL = null;

		UnityWebRequest request;
		Texture2D texture;

		void OnEnable() {
			currentURL = null;
			Clear();
		}

		void OnDisable() {
			currentURL = null;
			Clear();
		}

		void Clear() {
			if (request != null) {
				request.Abort();
				request = null;
			}
			if (texture) {
				Destroy(texture);
			}
			if (this.GetFromCache<SpriteRenderer>().sprite) {
				Destroy(this.GetFromCache<SpriteRenderer>().sprite);
			}
			this.GetFromCache<SpriteRenderer>().sprite = null;
			transform.localScale = Vector3.one;
		}

		void Update() {
			if (currentURL == url) {
				return;
			}
			currentURL = url;
			Clear();
			if (string.IsNullOrEmpty(currentURL)) {
				return;
			}
			request = UnityWebRequest.Get(currentURL);
			request.SendWebRequest().completed += x => {
				if (request == null || request.result != UnityWebRequest.Result.Success) {
					return;
				}
				texture = new Texture2D(1, 1, TextureFormat.ARGB32, true);
				texture.LoadImage(request.downloadHandler.data);
				texture.Apply();
				transform.localScale = Vector3.one * (textureSize / (float) texture.width);
				this.GetFromCache<SpriteRenderer>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2);
			};
		}
	}
}