
using System;
using UnityEngine;

namespace Team75.Shared {

    public class Item :  MonoBehaviour {
        [SerializeField] TextMesh barcode;

        private Rigidbody[] rigs;
        [SerializeField] private int exp=1;

        private void Start()
        {
            Debug.LogError("Shared Set");
            rigs = GetComponentsInChildren<Rigidbody>();
            for (int i=exp;i<rigs.Length;i++)
            {
                rigs[i].isKinematic = false;
                rigs[i].transform.parent =  null;
            }
        }

        public void SetBarcode(object value) {
            barcode.text = string.Format("0x{0}", value.GetHashCode().ToString("X8"));
        }
        public Transform GetBarcodeLocation() {
            return barcode.transform;
        }

        public void release()
        {
            rigs = GetComponentsInChildren<Rigidbody>();
            
        }

        
        private void OnDestroy()
        {
            for (int i = exp; i < rigs.Length; i++)
            {
                Destroy(rigs[i].gameObject);
                
            }
            //Destroy(gameObject);
                
        }
    }

}