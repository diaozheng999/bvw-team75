using System.Collections;
using UnityEngine;
using PGT.Core.Networking;


namespace Team75.Shared {
    public class Scanner : MonoBehaviour {
        public const float SPREAD = 20f;
        [SerializeField] int playerId;
        [SerializeField] private GameObject light;

        bool startTracking = false;

        byte GetTrackingId() => (playerId == 0) ? Connection.PLAYER_ONE_SCANNER : Connection.PLAYER_TWO_SCANNER;

        public void StartTracking() {
            startTracking = true;
            NetworkPositionManager.instance.AddTrackableTransform(transform, GetTrackingId());
		}

        void OnDestroy() {
            if(startTracking) {
                NetworkPositionManager.instance.RemoveTrackableTransform(GetTrackingId());
            }
        }

        public void scanning()
        {
            light.SetActive(true);
        }

        public void notscanning()
        {
            light.SetActive(false);
        }

        public IEnumerator scan_success()
        {
            scanning();
            GetComponent<AudioSource>().Play();
            yield return new WaitForSecondsRealtime(1f);
            notscanning();
        }
    }
}