using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MT.Packages.LD47
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(UnityEngine.UI.Text))]
    public class FPS : MonoBehaviour
    {
        const float MAX_DELTATIMES = 50;

        UnityEngine.UI.Text uiText;

		readonly List<float> deltaTimes = new List<float>();

		void Awake() {
            uiText = GetComponent<UnityEngine.UI.Text>();
        }

		void Update() {
            while (deltaTimes.Count > MAX_DELTATIMES) {
                deltaTimes.RemoveAt(0);
            }
            deltaTimes.Add(Time.deltaTime);
            uiText.text = Mathf.CeilToInt(1 / (deltaTimes.Sum() / deltaTimes.Count)).ToString();
        }
    }
}