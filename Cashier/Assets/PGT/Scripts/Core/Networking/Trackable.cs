using UnityEngine;

namespace PGT.Core.Networking {

    public class Trackable : Disposable {
        [SerializeField] byte id;
        [SerializeField] Transform transformProvider;
        void Start () {
            NetworkPositionManager.instance.AddTrackableTransform(transform, id, transform.worldToLocalMatrix);
            AddDisposable(() => NetworkPositionManager.instance?.RemoveTrackableTransform(id));
        }
    }

}