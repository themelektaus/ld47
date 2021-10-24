using UnityEngine;

namespace MT.Packages.LD47
{
    [ExecuteInEditMode]
    public class CharacterSkin : MonoBehaviour
    {
        [System.Serializable]
        public class Data
		{
            public Color headColor;
            public Color bodyColor;
            public Color handColor;
            public Color footColor;
            public string hatURL;
        }
        public Data data = new Data();
        
        public SpriteRenderer[] headSprites = new SpriteRenderer[0];
        public SpriteRenderer[] bodySprites = new SpriteRenderer[0];
        public SpriteRenderer[] handSprites = new SpriteRenderer[0];
        public SpriteRenderer[] footSprites = new SpriteRenderer[0];
        public WebSprite hatSprite;

		void Update() {
            foreach (var headSprite in headSprites) {
                headSprite.color = data.headColor;
            }
            foreach (var bodySprite in bodySprites) {
                bodySprite.color = data.bodyColor;
            }
            foreach (var handSprite in handSprites) {
                handSprite.color = data.handColor;
            }
            foreach (var footSprite in footSprites) {
                footSprite.color = data.footColor;
            }
            hatSprite.url = data.hatURL;
        }
	}
}