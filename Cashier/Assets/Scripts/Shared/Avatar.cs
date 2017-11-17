using System;
using System.Collections.Generic;
using UnityEngine;
using PGT.Core;
using PGT.Core.Networking;

namespace Team75.Shared {

    public class Avatar : MonoBehaviour {

        [SerializeField] int PlayerId;
        [SerializeField] Transform leftHand;
        [SerializeField] Transform rightHand;
        [SerializeField] Transform head;
        [SerializeField] Transform neck;
        [SerializeField] Transform body;
        [SerializeField] Transform itemPlacementPosition;
        [SerializeField] TextMesh nameTag;
        [SerializeField] string avatarMame = "";
        [SerializeField] float itemPlacementDelay = 2f;
        [SerializeField] float walkSpeed;
        [SerializeField] float turnSpeed;

        bool isTracking = false;
        Coroutine_ walkingCoroutine = null;

        LinkedList<Action<Action>> BeforeEnqueue = new LinkedList<Action<Action>>();
        LinkedList<Action<Action>> AfterEnqueue = new LinkedList<Action<Action>>();

        LinkedList<Action<Action>> BeforeQueueMove = new LinkedList<Action<Action>>();
        LinkedList<Action<Action>> AfterQueueMove = new LinkedList<Action<Action>>();

        LinkedList<Action<Action>> BeforeDequeue = new LinkedList<Action<Action>>();
        LinkedList<Action<Action>> AfterDequeue = new LinkedList<Action<Action>>();

        LinkedList<Action<Action>> BeforeLeave = new LinkedList<Action<Action>>();
        LinkedList<Action<Action>> AfterLeave = new LinkedList<Action<Action>>();


        public void StartTracking() {
            if (isTracking) {
                Debug.LogWarning("Avatar: Already started tracking.");    
                return;
            }

            Debug.LogFormat("Avatar {0}: Tracking started.", name);

            var _npm = NetworkPositionManager.instance;
            
            if(walkingCoroutine != null) walkingCoroutine.Interrupt();

            isTracking = true;
            body.transform.parent = neck;

            if(_npm == null) return;
 
            switch (PlayerId) {
                case 0:
                    _npm.AddTrackableTransform(head, Connection.PLAYER_ONE_CENTER_EYE);
                    _npm.AddTrackableTransform(leftHand, Connection.PLAYER_ONE_LEFT_HAND);
                    _npm.AddTrackableTransform(rightHand, Connection.PLAYER_ONE_RIGHT_HAND);
                    break;
                case 1:
                    _npm.AddTrackableTransform(head, Connection.PLAYER_TWO_CENTER_EYE);
                    _npm.AddTrackableTransform(leftHand, Connection.PLAYER_TWO_LEFT_HAND);
                    _npm.AddTrackableTransform(rightHand, Connection.PLAYER_TWO_RIGHT_HAND);
                    break;
            }
        }

        public void StopTracking() {
            if (!isTracking) {
                return;
            }

            var _npm = NetworkPositionManager.instance;

            isTracking = false;
            body.transform.parent = transform;

            if(_npm == null) return;
 
            switch (PlayerId) {
                case 0:
                    _npm.RemoveTrackableTransform(Connection.PLAYER_ONE_CENTER_EYE);
                    _npm.RemoveTrackableTransform(Connection.PLAYER_ONE_LEFT_HAND);
                    _npm.RemoveTrackableTransform(Connection.PLAYER_ONE_RIGHT_HAND);
                    break;
                case 1:
                    _npm.RemoveTrackableTransform(Connection.PLAYER_TWO_CENTER_EYE);
                    _npm.RemoveTrackableTransform(Connection.PLAYER_TWO_LEFT_HAND);
                    _npm.RemoveTrackableTransform(Connection.PLAYER_TWO_RIGHT_HAND);
                    break;
            }
        }


        void InvokeActionChain(LinkedListNode<Action<Action>> chain, Action cont) {
            if(chain == null) cont?.Invoke();
            else chain.Value.Invoke(() => InvokeActionChain(chain.Next, cont));
        }

        public void EnqueueTo (Vector3 position, Quaternion rotation, Action cont = null) {
            InvokeActionChain(BeforeEnqueue.First, () => {
                WalkTo(position, rotation, walkSpeed, turnSpeed, () => {
                    InvokeActionChain(AfterEnqueue.First, cont);
                });
            });
        }

        public void QueueMoveTo (Vector3 position, Quaternion rotation, Action cont = null) {
            InvokeActionChain(BeforeQueueMove.First, () => {
                WalkTo(position, rotation, walkSpeed, turnSpeed, () => {
                    InvokeActionChain(AfterQueueMove.First, cont);
                });
            });
        }

        public void DequeueTo (Vector3 position, Quaternion rotation, Action cont = null) {
            InvokeActionChain(BeforeDequeue.First, () => {
                WalkTo(position, rotation, walkSpeed, turnSpeed, () => {
                    InvokeActionChain(AfterDequeue.First, cont);
                });
            });
        }

        public void LeaveTo(Vector3 position, Quaternion rotation) {
            InvokeActionChain(BeforeLeave.First, () => {
                WalkTo(position, rotation, walkSpeed, turnSpeed, () => {
                    InvokeActionChain(AfterLeave.First, () => {
                        Destroy(gameObject);
                    });
                });
            });
        }

        public void WalkTo(Vector3 position, Quaternion rotation, float speed, float turnSpeed, Action cont = null) {
            if (walkingCoroutine != null) walkingCoroutine.Interrupt();
            walkingCoroutine = this.StartCoroutine1(WalkToTarget(position, rotation, speed, turnSpeed, cont));
        }

        public void SetName(string name) {
            avatarMame = name;
            if(nameTag != null) nameTag.text = name;
        }

        public void DeleteAfter(float seconds) {
            this.StartCoroutine1(DeleteAfterCoroutine(seconds));   
        }

        public float GetItemPlacementDelay() {
            return itemPlacementDelay;
        }

        IEnumerator<object> DeleteAfterCoroutine(float seconds){
            yield return new WaitForSeconds(seconds);
            Destroy(gameObject);
        }

        public Transform GetItemPlacementTransform(){
            return itemPlacementPosition;
        }

        IEnumerator<object> WalkToTarget(Vector3 position, Quaternion rotation, float speed, float turnSpeed, Action cont = null) {
            try {
                //Debug.LogFormat("Walking to {0}, rotation {1}...", position, rotation.eulerAngles);
                var path_fwd = position - transform.position;
                //Debug.Log(path_fwd);
                var rot = Quaternion.LookRotation(path_fwd, Vector3.up);

                //Debug.Log(rot.eulerAngles);

                if((Vector3.SqrMagnitude(transform.position - position) < 0.0001f) && Quaternion.Angle(rot, transform.rotation) < 5) yield break;

                var initial_rotation = transform.rotation;
                
                // rotate towards path_fwd
                var t = 0f;
                while(Quaternion.Angle(rot, transform.rotation) > 5) {
                    t += turnSpeed * Time.deltaTime;
                    transform.rotation = Quaternion.Slerp(initial_rotation, rot, t);
                    yield return null;
                }
                transform.rotation = rot;

                var velocity = Vector3.zero;
                
                // walk towards destination
                while(Vector3.SqrMagnitude(transform.position - position) > 0.0001f) {
                    transform.position = Vector3.SmoothDamp(transform.position, position, ref velocity, 1f, speed, Time.deltaTime);
                    yield return null;
                }

                transform.position = position;


                t = 0f;
                // rotate towards rotation
                while (Quaternion.Angle(rotation, transform.rotation) > 5) {
                    t += turnSpeed * Time.deltaTime;
                    transform.rotation = Quaternion.Slerp(rot, rotation, t);
                    yield return null;
                }

                transform.rotation = rotation;
                yield return null;
            } finally {
                cont?.Invoke();
            }
        }

        public object GetAwaiter(){
            if (walkingCoroutine == null || walkingCoroutine.Completed) return null;
            return walkingCoroutine.GetAwaiter();
        }

        void OnDisable() {
            StopTracking();
        }

        void OnDestroy() {
            StopTracking();
        }

        void Update() {
            if (!isTracking) return;

            var _el = body.transform.eulerAngles;
            body.transform.eulerAngles = new Vector3(0, _el.y, 0);
        }

    }

}