using UnityEngine;

namespace PGT.Core.Behaviours {

    public class LookAtCamera : MonoBehaviour {

        [SerializeField] Transform cam;
        [SerializeField] bool resetY;
        void Start() {
            if (cam == null) cam = Camera.main.transform;
        }

        void Update () {
            var worldpos = cam.position;
            if(resetY) worldpos.y = transform.position.y;

            transform.LookAt(worldpos);
        }

    }

}