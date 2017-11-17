using UnityEngine;

namespace Team75.Client {
    
    public class Barcode : MonoBehaviour {
        
        Material mat;

        void Start() {
            mat = GetComponent<Renderer>().material;
        }

        void Update() {
            mat.SetVector("_Normal", -transform.forward);
        }

    }
}