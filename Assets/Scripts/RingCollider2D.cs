using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class RingCollider2D : MonoBehaviour
{
	public Transform colliderContainer;
    public float radius = 72;

	public int segments = 144;
    public float segmentHeight = 1;
	public float segmentLength = 3.1312f;
	public int[] holeSegments = new int[0];

	public bool showColliders;

	[SerializeField, HideInInspector] BoxCollider2D[] children;

	string hash = null;

	string GetHash() {
		string holdSegementsHash = string.Join(",", holeSegments);
		return $"{radius};{segments};{segmentHeight};{segmentLength};[{holdSegementsHash}]";
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
		children = new BoxCollider2D[segments];
		float angles = 360f / segments * Mathf.PI / 180;
		for (int i = 0; i < segments; i++) {
			if (!holeSegments.Contains(i)) {
				children[i] = CreateSegment(i, angles);
			}
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
		this.children = new BoxCollider2D[0];
		return true;
	}

	BoxCollider2D CreateSegment(int index, float angles) {
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

		return child.AddComponent<BoxCollider2D>();
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