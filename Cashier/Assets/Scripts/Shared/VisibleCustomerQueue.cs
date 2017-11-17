using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using PGT.Core;


namespace Team75.Shared {
    public class VisibleCustomerQueue : Singleton<VisibleCustomerQueue> {


        [SerializeField] float queueGap;
        [SerializeField] Vector3 enqueueSpawn;
        [SerializeField] float movementSpeed;
        [SerializeField] float turnSpeed;

        [SerializeField] Transform[] customerPositions;
        [SerializeField] Transform[] customerLeavePositions;

        public const int RENDER_SIZE = 12;
        
        Avatar[] activeAvatars;

        LinkedList<Customer> customers;
        LinkedList<Avatar> avatars;
        Vector3[] targets;
        Quaternion queueRotation;

        Queue<Action> OnShuffleFinish;
        bool shuffling = false;

        int Count = 0;

        void Start () {
            avatars = new LinkedList<Avatar>();
            customers = new LinkedList<Customer>();
            activeAvatars = new Avatar[2];
            OnShuffleFinish = new Queue<Action>();

            // set destination values

            targets = new Vector3[RENDER_SIZE];
            for(int i=0; i<RENDER_SIZE; ++i) {
                targets[i] = transform.position + i * queueGap * transform.forward;
            }

            queueRotation = Quaternion.LookRotation(-transform.forward, Vector3.up);

        }

        public void Enqueue(Customer cust) {
            lock (avatars) {
                Count++;
                var avatar_go = Instantiate(ItemDictionary.instance.GetCustomer(cust.CustomerId), enqueueSpawn + targets[customers.Count], queueRotation);
                var avatar = avatar_go.GetComponent<Avatar>();
                avatar.SetName(cust.Name);
                avatar.EnqueueTo(targets[customers.Count], queueRotation);

                avatars.AddLast(avatar);
                customers.AddLast(cust);

                // really make sure all the customers are going to their correct positions
                var currentAvatar = avatars.First;
                if(currentAvatar != null) {
                    for (var i=0; currentAvatar.Next != null; ++i, currentAvatar = currentAvatar.Next) {
                        if(Vector3.SqrMagnitude(currentAvatar.Value.transform.position - targets[i]) > 0.0001f)
                            currentAvatar.Value.QueueMoveTo(targets[i], queueRotation);
                    }
                }

            }
        }

        void Shuffle(Action cont) {
            if (shuffling) {
                OnShuffleFinish.Enqueue(() => Shuffle(cont));
                return;
            }
            shuffling = true;
            _shuffle(avatars.First, 0, () => {
                shuffling = false;
                cont.Invoke();
                if (shuffling == false && OnShuffleFinish.Count > 0) {
                    OnShuffleFinish.Dequeue().Invoke();
                }
            });
        }

        void _shuffle(LinkedListNode<Avatar> avatar, int i, Action cont) {
            if (avatar == null) cont?.Invoke();
            else avatar.Value.QueueMoveTo(targets[i], queueRotation, () => {
                _shuffle(avatar.Next, i+1, cont);
            });
        }


        public bool IsEmpty() {
            lock (avatars) {
                return Count == 0;
            }
        }

        public bool IsFull() {
            lock (avatars) {
                return Count == RENDER_SIZE;
            }
        }


        public Customer Dequeue(int playerId, ref Avatar avatar, bool returnAvatar = true) {
            lock (avatars) {
                var _avatar = avatars.First.Value;
                var customer = customers.First.Value;
                avatars.RemoveFirst();
                customers.RemoveFirst();

                var currentAvatar = avatars.First;
                if(currentAvatar != null) {
                    for (var i=0; currentAvatar.Next != null; ++i, currentAvatar = currentAvatar.Next) {
                        currentAvatar.Value.WalkTo(targets[i], queueRotation, movementSpeed, turnSpeed);
                    }
                }

                _avatar.DequeueTo(customerPositions[playerId].position, customerPositions[playerId].rotation);
                Shuffle(PGT.Core.Func.Function.noop);

                /// TODO: remove timeout
                //_avatar.DeleteAfter(20);

                if(returnAvatar) avatar = _avatar;
                activeAvatars[playerId] = _avatar;

                return customer;
            }
        }

        public Customer Dequeue(int playerId) {
            Avatar flag = null;
            return Dequeue(playerId, ref flag, false);
        }

        public void CustomerLeave(int playerId) {
            activeAvatars[playerId].LeaveTo(customerLeavePositions[playerId].position, customerLeavePositions[playerId].rotation);
            activeAvatars[playerId] = null;
        }

        public bool HasActiveCustomer(int playerId) {
            return activeAvatars[playerId] != null;
        }

        public Avatar GetActiveCustomer(int playerId) {
            return activeAvatars[playerId];
        }

    }
}