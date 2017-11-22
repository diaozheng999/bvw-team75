using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace Team75.Shared {
    public class Flicker : MonoBehaviour {

        [SerializeField] float onTime;
        [SerializeField] float offTime;

        [SerializeField] GameObject go;

        Image img;

        void Start() {
            img = GetComponent<Image>();
        }

        void OnEnable() {
            StartCoroutine(SlideShows());
        }

        IEnumerator SlideShows() {
            var w_on = new WaitForSeconds(onTime);
            var w_off = new WaitForSeconds(offTime);
            while(true) {
                go.SetActive(true);
                yield return w_on;
                go.SetActive(false);
                yield return w_off;
            }
        }
    }
}