using System.Collections.Concurrent;
using UnityEngine;
using PGT.Core;
using Team75.Shared;


namespace Team75.Server {
    
    public class TrackingIdManager : Singleton<TrackingIdManager> {

        public const int MAX_TRACKABLE_OBJECT = 1024;

        volatile byte[] ids;

        void Start () {
            ids = new byte[MAX_TRACKABLE_OBJECT];
            for(int i=0; i<MAX_TRACKABLE_OBJECT; ++i) {
                ids[i] = 0;
            }

            // disable base trackable ids
            ids[Connection.PLAYER_ONE_CENTER_EYE] = 2;
            ids[Connection.PLAYER_ONE_LEFT_HAND] = 2;
            ids[Connection.PLAYER_ONE_RIGHT_HAND] = 2;
            ids[Connection.PLAYER_ONE_SCANNER] = 2;
            ids[Connection.PLAYER_TWO_CENTER_EYE] = 2;
            ids[Connection.PLAYER_TWO_LEFT_HAND] = 2;
            ids[Connection.PLAYER_TWO_RIGHT_HAND] = 2;
            ids[Connection.PLAYER_TWO_SCANNER] = 2;
        }

        public ushort GetUnusedId() {
            lock(ids) {
                for(int i=0; i<MAX_TRACKABLE_OBJECT; ++i) {
                    if(ids[i]==0) {
                        ids[i] = 2;
                        return (ushort)i;
                    }
                }
            }
            return (ushort)65535;
        }

        public void FreeId(ushort id) {
            lock(ids) {
                if (ids[id] > 0)
                    ids[id]--;
            }
        }

    }

}