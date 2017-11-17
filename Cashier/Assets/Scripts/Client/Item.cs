using System.Collections;
using System.Text;
using UnityEngine;
using PGT.Core;
using PGT.Core.Networking;

namespace Team75.Client {
    public class Item : MonoBehaviour {
        volatile byte TrackingId;
        short type;
        volatile bool tracking = false;
        
        [SerializeField]
        bool scanned = false;
        
        ItemPlacer placer;

        int itemId;

        bool held = false;

        bool destroyed = false;
        //----INSERT---------
        private Vector3 speed;

        private Vector3 position_last;

        [SerializeField] private int exp=1;

        private void Start()
        {
            if (tag == "Egg")
            {
                exp = 2;
            }
            else
            {
                exp = 1;
            }
            for (int i = 0; i < exp; i++)
            {
                Debug.LogError("Client Set");
                GetComponentsInChildren<Rigidbody>()[i].isKinematic = false;
            }
        }

        private void Update()
        {
            speed = (transform.position - position_last) / Time.deltaTime;
            position_last = transform.position;
        }
        //--------END-----------


        public void RequestTrackingId (short _type, ItemPlacer _placer, int _id) {
            type = _type;
            placer = _placer;
            itemId = _id;
            /*GetComponent<Rigidbody>().isKinematic = false;
            foreach(var rb in GetComponentsInChildren<Rigidbody>()){
                rb.isKinematic = false;
            }
            */
            NetworkManager.instance.RequestTrackingId(OnGetTrackingId);
            //this.StartCoroutine1(DeleteEventually());
        }

        void OnGetTrackingId(byte id) {
            TrackingId  = id;
            tracking = true;
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                NetworkManager.instance.AddTrackableItem(id, type, transform.position, transform.rotation);
                NetworkPositionManager.instance.AddBroadcastTransform(transform, TrackingId);
            });
        }

        void OnDestroy() {
            Scanner.instance.RemoveItem(this);
            destroyed = true;
            if(tracking){
                NetworkPositionManager.instance.RemoveBroadcastTransform(TrackingId);
                NetworkManager.instance.ReleaseTrackingId(TrackingId);
            }
        }

        public void SetScanned(bool _scanned, bool updatePlacement = true) {
            scanned = _scanned;
            if(_scanned && updatePlacement) {
                placer.PlaceItem(itemId);
            }
            if(!held) Destroy(gameObject);
        }

        public void DestroyIfScanned() {
            Debug.Log("Item released. Delete if appropriate.");
            if(scanned) Destroy(gameObject);
        }

        IEnumerator DeleteEventually() {
            yield return new WaitForSeconds(10f);
            Destroy(gameObject);
        }

        public void OnGrab(Hand.HandEnum hand) {
            held = true;
        }

        public void OnRelease(Hand.HandEnum hand) {
            if(destroyed) return;
            held = false;
            var _rb = GetComponent<Rigidbody>();
            if(_rb!=null) _rb.velocity = speed;
            Debug.Log("OnReleaseCalled!!!!");
            DestroyIfScanned();
        }
    }
}