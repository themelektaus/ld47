using System.Collections.Generic;
using UnityEngine;

namespace MT.Packages.LD47
{
	[ExecuteInEditMode]
	public class ObjectGroup : MonoBehaviour
	{
		public enum Mode
		{
			AllTheSame,
			SingleChoice
		}

		public Mode mode = Mode.AllTheSame;
		public float value = 0;
		public int choice = -1;

		[SerializeField] List<Core.Field> fields = new List<Core.Field>();

		void Update() {
			if (!Core.Hash.HasChanged(this, mode, value, choice, fields.Count)) {
				return;
			}
			for (int i = 0; i < fields.Count; i++) {
				var type = fields[i].GetFieldType();
				if (type != null) {
					fields[i].Set(GetValue(type, i));
				}
			}
		}

		object GetValue(System.Type type, int index) {
			float result = 0;
			if (mode == Mode.SingleChoice) {
				if (index == choice) {
					result = value;
				}
			} else {
				result = value;
			}
			if (type == typeof(bool)) {
				return result > 0;
			}
			return result;
		}
	}
}