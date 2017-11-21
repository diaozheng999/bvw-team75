using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace Team75.Shared {
    public class FrenzySlideshow : MonoBehaviour {

        [SerializeField] Sprite[] sprites;
        [SerializeField] float dTime;

        Image img;

        void Start() {
            img = GetComponent<Image>();
        }

        void OnEnable() {
            StartCoroutine(SlideShows());
        }

        IEnumerator SlideShows() {
            img = GetComponent<Image>();
            while(true) {
                foreach(var sprite in sprites) {
                    img.sprite = sprite;
                    yield return new WaitForSeconds(dTime);
                }
            }
        }
    }
}