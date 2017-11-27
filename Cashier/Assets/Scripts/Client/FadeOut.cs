using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Team75.Client {

    public class FadeOut : MonoBehaviour {

        [SerializeField] float maxVelocity;
        [SerializeField] float smoothTime;


        public void Execute() {
            StartCoroutine(FadeOutCoroutine());
        }

        IEnumerator FadeOutCoroutine() {
            var img = GetComponent<Image>();

            var absAlpha = 1f;
            var velocity = 0f;
            yield return null;
            while(absAlpha > 0.0001f) {
                absAlpha = Mathf.SmoothDamp(absAlpha, 0f, ref velocity, smoothTime, maxVelocity);
                img.color = new Color(1,1,1,absAlpha);
                yield return null;
            }
            gameObject.SetActive(false);
        
        }

    }

}