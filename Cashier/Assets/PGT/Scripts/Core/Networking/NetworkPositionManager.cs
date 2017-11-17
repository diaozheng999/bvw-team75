using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using PGT.Core.Func;
using PGT.Core.DataStructures;

// A primitive way of using typedef. Update this if you want a larger ID size.
// also, ParseObjectId function will need to be changed.
using object_id_t = System.Byte;

namespace PGT.Core.Networking {


    public class NetworkPositionManager : Singleton<NetworkPositionManager> {

            
        public const byte PGT_NPM_DELTA_POS = 0x10;
        public const byte PGT_NPM_DELTA_ROT = 0x11;
        public const byte PGT_NPM_UPDATE_POS = 0x12;
        public const byte PGT_NPM_UPDATE_ROT = 0x13;


        public const int OBJECT_ID_SIZE = sizeof(object_id_t);
        public const int POSITION_MSG_SIZE = OBJECT_ID_SIZE + 12; // byte + 3 * float
        public const int ROTATION_MSG_SIZE = OBJECT_ID_SIZE + 16; // byte + 4 * float

        public const int N_POS_PER_MESSAGE = Pipe.BUFFER_SIZE / POSITION_MSG_SIZE;

        public const int N_ROT_PER_MESSAGE = Pipe.BUFFER_SIZE / ROTATION_MSG_SIZE;


        public const int MAX_RETRY_COUNT = 5; // Number of retrys for a delta packet to update the concurrent dictionary
        public const float UPDATE_FREQ = 1f;

        HashSet<Pipe> pipes;
        Dictionary<object_id_t, Func.Tuple<Transform, Matrix4x4, Quaternion, Quaternion>> objects; 
        ConcurrentDictionary<object_id_t, Vector3> positionDelta;
        ConcurrentDictionary<object_id_t, Quaternion> rotationDelta;

        Dictionary<object_id_t, Transform> broadcastObjects;
        Dictionary<object_id_t, Vector3> broadcastPosition;
        Dictionary<object_id_t, Quaternion> broadcastRotation;

        protected override void Awake() {
            base.Awake();
            pipes = new HashSet<Pipe>();
            objects = new Dictionary<object_id_t, Func.Tuple<Transform, Matrix4x4, Quaternion, Quaternion>>();
            broadcastPosition = new Dictionary<object_id_t, Vector3>();
            broadcastObjects = new Dictionary<object_id_t, Transform>();
            broadcastRotation = new Dictionary<object_id_t, Quaternion>();
            positionDelta = new ConcurrentDictionary<object_id_t, Vector3>();
            rotationDelta = new ConcurrentDictionary<object_id_t, Quaternion>();
            AddDisposable(RemoveAllPipes);
            StartCoroutine(SendUpdatePackets());
        }

        void RemoveAllPipes() {
            foreach(var pipe in pipes) {
                pipe.Dispose();
            }
        }

        public void AddTrackableTransform(Transform transform, object_id_t id) {
            AddTrackableTransform(transform, id, Matrix4x4.identity);
        }

        public void AddBroadcastTransform(Transform transform, object_id_t id) {
            broadcastObjects.Add(id, transform);
            broadcastPosition.Add(id, Vector3.zero);
            broadcastRotation.Add(id, Quaternion.identity);
        }

        public void AddTrackableTransform(Transform transform, object_id_t id, Matrix4x4 post_transform){
            if(objects.ContainsKey(id)) 
                Debug.LogWarningFormat("NetworkPositionManager: Reregistering object {0} with {1}, (Previously {2}).", id, transform.name, objects[id].car.name);

            var rot = GetRotation(post_transform);
            var inv_rot = Quaternion.Inverse(rot);
            objects[id] = Func.Tuple._(transform, post_transform, rot, inv_rot);
            positionDelta[id] = Vector3.zero;
            rotationDelta[id] = Quaternion.identity;
        }

        public void RemoveTrackableTransform(object_id_t id) {
            if(!objects.ContainsKey(id))
                Debug.LogWarningFormat("NetworkPositionManager: Object {0} doesn't exist.", id);
            
            objects.Remove(id);
            Vector3 _v;
            Quaternion _v2;
            positionDelta.TryRemove(id, out _v);
            rotationDelta.TryRemove(id, out _v2);
        }

        public void RemoveBroadcastTransform(object_id_t id) {
            if(!broadcastObjects.ContainsKey(id))
                Debug.LogWarningFormat("NetworkPositionManager: Object {0} doesn't exist.", id);
            
            broadcastObjects.Remove(id);
            broadcastPosition.Remove(id);
            broadcastRotation.Remove(id);
        }
        
        public void AddEndpoint(IPEndPoint ep, int localPort) {
            var pipe = new UdpPipe(ep, localPort);
            pipe.AddParser(PGT_NPM_DELTA_POS, UpdateDeltaPosition, "PGT_NPM_DELTA_POS");
            pipe.AddParser(PGT_NPM_DELTA_ROT, UpdateDeltaRotation, "PGT_NPM_DELTA_ROT");
            pipe.AddParser(PGT_NPM_UPDATE_POS, UpdatePosition, "PGT_NPM_UPDATE_POS");
            pipe.AddParser(PGT_NPM_UPDATE_ROT, UpdateRotation, "PGT_NPM_UPDATE_ROT");
            pipes.Add(pipe);
        }

        Vector3 ParseVector3(byte[] buffer, int start) {
            var x = BitConverter.ToSingle(buffer, start);
            var y = BitConverter.ToSingle(buffer, start+4);
            var z = BitConverter.ToSingle(buffer, start+8);
            return new Vector3(x, y, z);
        }

        Quaternion ParseQuaternion(byte[] buffer, int start) {
            var x = BitConverter.ToSingle(buffer, start);
            var y = BitConverter.ToSingle(buffer, start+4);
            var z = BitConverter.ToSingle(buffer, start+8);
            var w = BitConverter.ToSingle(buffer, start+12);
            return new Quaternion(x,y,z,w);
        }

        object_id_t ParseObjectId(byte[] buffer, int start) {
            return buffer[start];
        }

        Quaternion GetRotation(Matrix4x4 mat) {
            return Quaternion.LookRotation(mat.GetColumn(2), mat.GetColumn(1));
        }

        void UpdateDeltaPosition(byte[] buffer, ushort length) {
            for (int i=0; i<length; i+=POSITION_MSG_SIZE) {
                var oid = ParseObjectId(buffer, i);
                if (!positionDelta.ContainsKey(oid)) continue;

                var d_pos = ParseVector3(buffer, i+OBJECT_ID_SIZE);

                // doesn't matter if we miss an update, we have update packets
                // waiting.
                for (int j=0; j<MAX_RETRY_COUNT; ++j) {
                    var c_pos = positionDelta[oid];
                    var n_pos = d_pos + c_pos;
                    if (positionDelta.TryUpdate(oid, n_pos, c_pos)) break;
                }
            }
        }

        void UpdateDeltaRotation(byte[] buffer, ushort length) {
            for (int i=0; i<length; i+=ROTATION_MSG_SIZE) {
                var oid = ParseObjectId(buffer, i);
                if (!rotationDelta.ContainsKey(oid)) continue;

                var d_rot = ParseQuaternion(buffer, i+OBJECT_ID_SIZE);

                // doesn't matter if we miss an update, we have update packets
                // waiting.
                for (int j=0; j<MAX_RETRY_COUNT; ++j) {
                    var c_rot = rotationDelta[oid];
                    var n_rot = c_rot * d_rot;
                    if (rotationDelta.TryUpdate(oid, n_rot, c_rot)) break;
                }
            }
        }

        void UpdatePosition(byte[] buffer, ushort length) {
            int n_obj = length / POSITION_MSG_SIZE;
            var oids = new object_id_t[n_obj];
            var pos = new Vector3[n_obj];

            for(int i=0, j=0; i<length; i+=POSITION_MSG_SIZE, ++j) {
                oids[j] = ParseObjectId(buffer, i);
                pos[j] = ParseVector3(buffer, i+OBJECT_ID_SIZE);
            }

            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                for(int i=0; i<n_obj; ++i) {
                    var oid = oids[i];
                    if (!objects.ContainsKey(oid)) continue;
                    var tf = objects[oid].car;
                    var tmat = objects[oid].cdr;
                    tf.position = tmat.MultiplyVector(pos[i]);
                }
            });
        }

        void UpdateRotation(byte[] buffer, ushort length) {
            int n_obj = length / ROTATION_MSG_SIZE;
            var oids = new object_id_t[n_obj];
            var rot = new Quaternion[n_obj];

            for(int i=0, j=0; i<length; i+=ROTATION_MSG_SIZE, ++j) {
                oids[j] = ParseObjectId(buffer, i);
                rot[j] = ParseQuaternion(buffer, i+OBJECT_ID_SIZE);
            }

            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                for(int i=0; i<n_obj; ++i) {
                    var oid = oids[i];
                    if (!objects.ContainsKey(oid)) continue;
                    var tf = objects[oid].car;
                    var trot = objects[oid].cpr;
                    tf.rotation = rot[i] * trot;
                }
            });
        }

        
        void PackObjectId(object_id_t item, byte[] buffer, int start) {
            buffer[start] = item;
        }

        byte[] PackPositionMessage(Sequence<Func.Tuple<object_id_t, Vector3>> item){
            var _item = item.Memoize();
            var _len = _item.Count * POSITION_MSG_SIZE;
            var buffer = new byte[_len];
            int ptr = 0;

            foreach(var _i in _item) {
                PackObjectId(_i.car, buffer, ptr);
                ptr += OBJECT_ID_SIZE;
                Array.Copy(BitConverter.GetBytes(_i.cdr.x), 0, buffer, ptr, 4);
                Array.Copy(BitConverter.GetBytes(_i.cdr.y), 0, buffer, ptr+4, 4);
                Array.Copy(BitConverter.GetBytes(_i.cdr.z), 0, buffer, ptr+8, 4);
                ptr += 12;
            }

            return buffer;
        }

        byte[] PackRotationMessage(Sequence<Func.Tuple<object_id_t, Quaternion>> item){
            var _item = item.Memoize();
            var _len = _item.Count * ROTATION_MSG_SIZE;
            var buffer = new byte[_len];
            int ptr = 0;
            
            foreach(var _i in _item) {
                PackObjectId(_i.car, buffer, ptr);
                ptr += OBJECT_ID_SIZE;
                Array.Copy(BitConverter.GetBytes(_i.cdr.x), 0, buffer, ptr, 4);
                Array.Copy(BitConverter.GetBytes(_i.cdr.y), 0, buffer, ptr+4, 4);
                Array.Copy(BitConverter.GetBytes(_i.cdr.z), 0, buffer, ptr+8, 4);
                Array.Copy(BitConverter.GetBytes(_i.cdr.w), 0, buffer, ptr+12, 4);
                ptr += 16;
            }

            return buffer;
        }
        

        void Update() {
            // update delta packets
            foreach(var o in objects) {
                var oid = o.Key;
                var tf = o.Value.car;
                var tmat = o.Value.cdr;
                var trot = o.Value.cpr;
                var tirot = o.Value.ctr;

                var d_pos = positionDelta[oid];
                if(d_pos != Vector3.zero) {
                    tf.position += tmat.MultiplyVector(d_pos);
                    positionDelta[oid] = Vector3.zero;
                }

                var d_rot = rotationDelta[oid];
                if(d_rot != Quaternion.identity) {
                    tf.rotation *= tirot * d_rot * trot;
                    rotationDelta[oid] = Quaternion.identity;
                }
            }
        }

        void FixedUpdate () {
            if(pipes.Count == 0) return;


            var n_broadcast = broadcastObjects.Count;
            var items = Sequence.UnrollN(n_broadcast, broadcastObjects);

            var positions = items.Map((KeyValuePair<object_id_t, Transform> item) => Func.Tuple._(item.Key, item.Value.position));
            var pos_to_update = 
                positions.Filter((Func.Tuple<object_id_t, Vector3> item) => item.cdr != broadcastPosition[item.car])
                    .Map((Func.Tuple<object_id_t, Vector3> item) => Func.Tuple.map_cdr(
                        (Vector3 p) => p - broadcastPosition[item.car],
                        item
                    ))
                    .Implode(N_POS_PER_MESSAGE)
                    .MapEagerly(PackPositionMessage);

            var rotations = items.Map((KeyValuePair<object_id_t, Transform> item) => Func.Tuple._(item.Key, item.Value.rotation));

            var rot_to_update = 
                rotations.Filter((Func.Tuple<object_id_t, Quaternion> item) => item.cdr != broadcastRotation[item.car])
                    .Map((Func.Tuple<object_id_t, Quaternion> item) => Func.Tuple.map_cdr(
                        (Quaternion p) => p * Quaternion.Inverse(broadcastRotation[item.car]),
                        item
                    ))
                    .Implode(N_ROT_PER_MESSAGE)
                    .MapEagerly(PackRotationMessage);

            foreach (var pipe in pipes) {
                foreach(var pos_msg in pos_to_update) {
                    pipe.SendMessageInBackground(PGT_NPM_DELTA_POS, pos_msg);
                }
                foreach(var rot_msg in rot_to_update) {
                    pipe.SendMessageInBackground(PGT_NPM_DELTA_ROT, rot_msg);
                }
            }

            foreach(var item in broadcastObjects.Keys) {
                broadcastPosition[item] = broadcastObjects[item].position;
                broadcastRotation[item] = broadcastObjects[item].rotation;
            }
        }


        IEnumerator SendUpdatePackets() {
            var wfs = new WaitForSeconds(UPDATE_FREQ);
            while (true) {
                var n_broadcast = broadcastObjects.Count;
                var items = Sequence.UnrollN(n_broadcast, broadcastObjects);

                var positions = items.Map((KeyValuePair<object_id_t, Transform> item) => Func.Tuple._(item.Key, item.Value.position));
                var pos_to_update = 
                    positions.Implode(N_POS_PER_MESSAGE)
                        .MapEagerly(PackPositionMessage);

                var rotations = items.Map((KeyValuePair<object_id_t, Transform> item) => Func.Tuple._(item.Key, item.Value.rotation));

                var rot_to_update = 
                    rotations
                        .Implode(N_ROT_PER_MESSAGE)
                        .MapEagerly(PackRotationMessage);

                foreach (var pipe in pipes) {
                    foreach(var pos_msg in pos_to_update) {
                        pipe.SendMessageInBackground(PGT_NPM_UPDATE_POS, pos_msg);
                    }
                    foreach(var rot_msg in rot_to_update) {
                        pipe.SendMessageInBackground(PGT_NPM_UPDATE_ROT, rot_msg);
                    }
                }

                
                yield return wfs;
            }
        }
        

    }

}