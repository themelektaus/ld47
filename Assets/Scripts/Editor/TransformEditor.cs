using UnityEditor;
using UnityEngine;

namespace MT.Packages.LD47
{
	[CustomEditor(typeof(Transform))]
	public class TransformEditor : UnityEditor.Editor
	{
		Transform _this;

		public override void OnInspectorGUI() {
			_this = (Transform) target;
			DrawDefaultTransformInspector();
			DrawCustom();
		}

		void DrawCustom() {

		}

		void ResetPosition() {
			if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.Height(16))) {
				Undo.RecordObject(_this, "Reset Position");
				_this.localPosition = Vector3.zero;
			}
		}

		void RoundPosition() {
			if (GUILayout.Button("R", GUILayout.Width(18), GUILayout.Height(16))) {
				Undo.RecordObject(_this, "Round Position");
				Vector3 position = _this.localPosition;
				position.x = Mathf.Round(position.x * 2) / 2;
				position.y = Mathf.Round(position.y * 2) / 2;
				position.z = Mathf.Round(position.z * 2) / 2;
				_this.localPosition = position;
			}
		}

		void ResetRotation() {
			if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.Height(16))) {
				Undo.RecordObject(_this, "Reset Rotation");
				_this.localRotation = Quaternion.identity;
			}
		}

		void RoundRotation() {
			if (GUILayout.Button("R", GUILayout.Width(18), GUILayout.Height(16))) {
				Undo.RecordObject(_this, "Round Rotation");
				Vector3 rotation = _this.localEulerAngles;
				rotation.x = Mathf.Round(rotation.x);
				rotation.y = Mathf.Round(rotation.y);
				rotation.z = Mathf.Round(rotation.z);
				_this.localEulerAngles = rotation;
			}
		}

		void ResetScale() {
			if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.Height(16))) {
				Undo.RecordObject(_this, "Reset Scale");
				_this.localScale = Vector3.one;
			}
		}

		void RoundScale() {
			if (GUILayout.Button("R", GUILayout.Width(18), GUILayout.Height(16))) {
				Undo.RecordObject(_this, "Round Scale");
				Vector3 scale = _this.localScale;
				scale.x = Mathf.Round(scale.x * 20) / 20;
				scale.y = Mathf.Round(scale.y * 20) / 20;
				scale.z = Mathf.Round(scale.z * 20) / 20;
				_this.localScale = scale;
			}
		}

		void DrawDefaultTransformInspector() {
			var positionChanged = false;
			var rotationChanged = false;
			var scaleChanged = false;

			Vector3 _localPosition = _this.localPosition;
			Vector3 _localEulerAngles = _this.localEulerAngles;
			Vector3 _localScale = _this.localScale;

			EditorGUIUtility.labelWidth = 15;
			EditorGUIUtility.fieldWidth = 10;

			GUILayout.BeginHorizontal();
			ResetPosition();
			EditorGUILayout.LabelField("Position", GUILayout.Width(80));
			EditorGUI.BeginChangeCheck();
			Vector3 localPosition = _this.localPosition;
			localPosition.x = EditorGUILayout.FloatField("X", localPosition.x);
			localPosition.y = EditorGUILayout.FloatField("Y", localPosition.y);
			localPosition.z = EditorGUILayout.FloatField("Z", localPosition.z);
			if (EditorGUI.EndChangeCheck()) {
				positionChanged = true;
			}
			RoundPosition();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			ResetRotation();
			EditorGUILayout.LabelField("Rotation", GUILayout.Width(80));
			EditorGUI.BeginChangeCheck();
			Vector3 localEulerAngles = _this.localEulerAngles;
			localEulerAngles.x = EditorGUILayout.FloatField("X", localEulerAngles.x);
			localEulerAngles.y = EditorGUILayout.FloatField("Y", localEulerAngles.y);
			localEulerAngles.z = EditorGUILayout.FloatField("Z", localEulerAngles.z);
			if (EditorGUI.EndChangeCheck()) {
				rotationChanged = true;
			}
			RoundRotation();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			ResetScale();
			EditorGUILayout.LabelField("Scale", GUILayout.Width(80));
			EditorGUI.BeginChangeCheck();
			Vector3 localScale = _this.localScale;
			localScale.x = EditorGUILayout.FloatField("X", localScale.x);
			localScale.y = EditorGUILayout.FloatField("Y", localScale.y);
			localScale.z = EditorGUILayout.FloatField("Z", localScale.z);
			if (EditorGUI.EndChangeCheck()) {
				scaleChanged = true;
			}
			RoundScale();
			GUILayout.EndHorizontal();

			EditorGUIUtility.labelWidth = 0;
			EditorGUIUtility.fieldWidth = 0;

			if (positionChanged || rotationChanged || scaleChanged) {
				Undo.RecordObject(_this, _this.name);
				if (positionChanged) {
					_this.localPosition = localPosition;
				}
				if (rotationChanged) {
					_this.localEulerAngles = localEulerAngles;
				}
				if (scaleChanged) {
					_this.localScale = localScale;
				}
			}

			Transform[] selectedTransforms = Selection.transforms;
			if (selectedTransforms.Length > 1) {
				foreach (var transform in selectedTransforms) {
					if (positionChanged || rotationChanged || scaleChanged) {
						Undo.RecordObject(transform, transform.name);
					}
					if (positionChanged) {
						transform.localPosition = ApplyChangesOnly(transform.localPosition, _localPosition, _this.localPosition);
					}
					if (rotationChanged) {
						transform.localEulerAngles = ApplyChangesOnly(transform.localEulerAngles, _localEulerAngles, _this.localEulerAngles);
					}
					if (scaleChanged) {
						transform.localScale = ApplyChangesOnly(transform.localScale, _localScale, _this.localScale);
					}
				}
			}
		}

		Vector3 ApplyChangesOnly(Vector3 toApply, Vector3 initial, Vector3 changed) {
			if (!Mathf.Approximately(initial.x, changed.x)) {
				toApply.x = _this.localPosition.x;
			}
			if (!Mathf.Approximately(initial.y, changed.y)) {
				toApply.y = _this.localPosition.y;
			}
			if (!Mathf.Approximately(initial.z, changed.z)) {
				toApply.z = _this.localPosition.z;
			}
			return toApply;
		}
	}
}