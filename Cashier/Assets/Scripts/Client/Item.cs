using System.Collections;
using System.Text;
using UnityEngine;
using PGT.Core;
using PGT.Core.Networking;

namespace Team75.Client {
    public class Item : Scannable {


        [SerializeField] bool scanned = false;
        [SerializeField] private int exp=1;



        volatile byte TrackingId;
        short type;
        volatile bool tracking = false;        
        ItemPlacer placer;
        int itemId;
        bool held = false;
        bool destroyed = false;
        private Vector3 speed;
        private Vector3 position_last;




        private void Update()
        {
            speed = (transform.position - position_last) / Time.deltaTime;
            position_last = transform.position;
        }
        

        public void RequestTrackingId (short _type, ItemPlacer _placer, int _id) {
            type = _type;
            placer = _placer;
            itemId = _id;

            NetworkManager.instance.RequestTrackingId(OnGetTrackingId);

            // Moved Start() initialisation code here, so it is explicitly called in the correct sequence
            // Also it doesn't need to be executed if this is added to an avatar

            if (tag == "Egg")
            {
                exp = 2;
            }
            else
            {
                exp = 1;
            }

            var components = GetComponentsInChildren<Rigidbody>();
            
            for (int i = 0; i < exp; i++)
            {
                components[i].isKinematic = false;
            }
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

        public override void SetScanned(bool _scanned) {
            SetScanned(_scanned, true);
        }

        public void SetScanned(bool _scanned, bool updatePlacement) {
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
            DestroyIfScanned();
        }

        public override int GetId() => GetComponent<Shared.Item>().GetID();
    }
}