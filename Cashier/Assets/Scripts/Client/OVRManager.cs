using PGT.Core;
using PGT.Core.Networking;
using UnityEngine;
using Team75.Shared;

namespace Team75.Client {

    public class OVRManager : Singleton<OVRManager> {
        [SerializeField] Transform centerEye;
        [SerializeField] Transform leftHand;
        [SerializeField] Transform rightHand;

        public float GetHeadDelta() {
            return centerEye.localEulerAngles.y;
        }

        public void StartBroadcasting(int playerId) {
            var _npm = NetworkPositionManager.instance;
            switch(playerId) {
                case 0:
                    _npm.AddBroadcastTransform(centerEye, Connection.PLAYER_ONE_CENTER_EYE);
                    _npm.AddBroadcastTransform(leftHand, Connection.PLAYER_ONE_LEFT_HAND);
                    _npm.AddBroadcastTransform(rightHand, Connection.PLAYER_ONE_RIGHT_HAND);
                    break;
                case 1:
                    _npm.AddBroadcastTransform(centerEye, Connection.PLAYER_TWO_CENTER_EYE);
                    _npm.AddBroadcastTransform(leftHand, Connection.PLAYER_TWO_LEFT_HAND);
                    _npm.AddBroadcastTransform(rightHand, Connection.PLAYER_TWO_RIGHT_HAND);
                    break;
            }
        }

    }


}