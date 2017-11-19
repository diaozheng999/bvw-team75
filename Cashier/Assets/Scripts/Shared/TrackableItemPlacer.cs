using System.Collections.Generic;
using PGT.Core;
using PGT.Core.Networking;
using UnityEngine;

namespace Team75.Shared {

    public class TrackableItemPlacer : Singleton<TrackableItemPlacer> {

        Dictionary<ushort, Transform> trackableItems;

        void Start() {
            trackableItems = new Dictionary<ushort, Transform>();
        }

        public void AddTrackable (ushort id, short type, Vector3 pos, Quaternion rot) {
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                var _go = Instantiate(ItemDictionary.instance.GetItem(type), pos, rot);
                var tf = _go.transform;
                trackableItems[id] = tf;
                NetworkPositionManager.instance.AddTrackableTransform(tf, id);
            });
        }

        public void RemoveTrackable (ushort id, System.Action cont = null) {
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                NetworkPositionManager.instance.RemoveTrackableTransform(id);
                Destroy(trackableItems[id].gameObject);
                trackableItems.Remove(id);
                if(cont != null) cont();
            });
        }

    }

}