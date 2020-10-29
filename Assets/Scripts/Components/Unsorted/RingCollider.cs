using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MT.Packages.LD47
{
	[ExecuteInEditMode]
	public class RingCollider : MonoBehaviour
	{
		[SerializeField] Transform colliderContainer = null;
		[SerializeField] float radius = 72;

		[SerializeField] int segments = 144;
		[SerializeField] float segmentHeight = 1;
		[SerializeField] float segmentLength = 3.1312f;
		[SerializeField] int[] holeSegments = new int[0];

		[SerializeField] Sprite sprite = null;
		[SerializeField] Material spriteMaterial = null;
		[SerializeField] Color spriteColor = Color.white;
		[SerializeField] SpriteRenderer backgroundSpriteRenderer = null;
		[SerializeField] Color backgroundSpriteColor = Color.gray;

		[SerializeField] bool showColliders = false;

		[SerializeField, ReadOnly] GameObject[] children;

		string hash = null;

		static string ConvertToHash(Color color) {
			return color.r + "," + color.g + "," + color.b + "," + color.a;
		}

		string GetHash() {
			string result = $"{radius};{segments};{segmentHeight};{segmentLength};";
			result += $"[{string.Join(",", holeSegments)}];";
			result += (sprite ? sprite.name : "null") + ";";
			result += (spriteMaterial ? spriteMaterial.name : "null") + ";";
			result += "[" + ConvertToHash(spriteColor) + "];";
			result += (backgroundSpriteRenderer ? backgroundSpriteRenderer.name : "null") + ";";
			result += "[" + ConvertToHash(backgroundSpriteColor) + "];";
			return result;
		}

		void OnEnable() {
			Refresh();
		}

		void OnDisable() {
			Clear();
		}

		void Update() {
			var s = transform.localScale.x;
			transform.localScale = new Vector3(s, s, s);

			var hash = GetHash();
			if (this.hash == hash) {
				return;
			}
			this.hash = hash;
			Refresh();
		}

		void Refresh() {
			if (!Clear()) {
				return;
			}
			children = new GameObject[segments];
			float angles = 360f / segments * Mathf.PI / 180;
			for (int i = 0; i < segments; i++) {
				if (!holeSegments.Contains(i)) {
					children[i] = CreateSegment(i, angles);
				}
			}
			if (backgroundSpriteRenderer) {
				var s = (radius + segmentHeight) * 2;
				backgroundSpriteRenderer.transform.localScale = new Vector3(s, s, s);
				backgroundSpriteRenderer.color = backgroundSpriteColor;
			}
		}

		bool Clear() {
			if (Application.isPlaying) {
				return false;
			}
			if (!colliderContainer) {
				return false;
			}
			var children = new List<GameObject>();
			foreach (Transform t in colliderContainer.transform) {
				children.Add(t.gameObject);
			}
			foreach (var child in children) {
				DestroyImmediate(child);
			}
			this.children = new GameObject[0];
			return true;
		}

		GameObject CreateSegment(int index, float angles) {
			var child = new GameObject(index.ToString());
			var transform = child.transform;
			transform.SetParent(colliderContainer, false);

			float angle = angles * index;
			transform.localPosition = new Vector3(
				(radius + segmentHeight) * Mathf.Cos(angle),
				(radius + segmentHeight) * Mathf.Sin(angle),
				0
			);
			transform.localEulerAngles = new Vector3(0, 0, angle * Mathf.Rad2Deg);
			transform.localScale = new Vector3(segmentHeight, segmentLength, 1);

			child.AddComponent<BoxCollider2D>();

			if (sprite && spriteMaterial) {
				var renderer = child.AddComponent<SpriteRenderer>();
				renderer.sprite = sprite;
				renderer.material = spriteMaterial;
				renderer.color = spriteColor;
			}

			return child;
		}

		void OnDrawGizmos() {
			if (!showColliders) {
				return;
			}
			Gizmos.color = Color.gray;
			foreach (var child in children) {
				if (!child) {
					continue;
				}
				Gizmos.matrix = child.transform.localToWorldMatrix;
				Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
			}
		}
	}
}