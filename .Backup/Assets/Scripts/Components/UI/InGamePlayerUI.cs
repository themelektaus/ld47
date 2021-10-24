using MT.Packages.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MT.Packages.LD47.UI
{
    public class InGamePlayerUI : MonoBehaviour
    {
        [SerializeField] Image hpBar = null;
        [SerializeField] Image ammoBar = null;
        [SerializeField] Image castBar = null;

		readonly SmoothFloat hp = (0, .1f);
        readonly SmoothFloat ammo = (0, .1f);
        readonly SmoothFloat cast = (0, .05f);

		void Update() {
            var character = this.GetParentFromCache<Character>();
            
            hp.target = character.GetHealthPercentage();
            hp.Update();
            hpBar.fillAmount = hp.current;

            ammo.target = character.GetAmmoPercentage();
            ammo.Update();
            ammoBar.fillAmount = ammo.current;

            cast.target = character.GetCastedTimePercentage();
            cast.Update();
            castBar.fillAmount = cast.current;
        }
	}
}