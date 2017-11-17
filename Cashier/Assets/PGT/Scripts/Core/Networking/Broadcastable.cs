using UnityEngine;

namespace PGT.Core.Networking {

    public class Broadcastable : Disposable {
        [SerializeField] byte id;
        void Start () {
            NetworkPositionManager.instance.AddBroadcastTransform(transform, id);
            AddDisposable(() => NetworkPositionManager.instance.RemoveBroadcastTransform(id));
        }
    }

}