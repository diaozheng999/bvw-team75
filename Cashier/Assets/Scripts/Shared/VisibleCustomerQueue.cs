using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using PGT.Core;
using Fn = PGT.Core.Func.Function;


namespace Team75.Shared {
    public class VisibleCustomerQueue : Singleton<VisibleCustomerQueue> {


        [SerializeField] float queueGap;
        [SerializeField] Vector3 enqueueSpawn;
        [SerializeField] float movementSpeed;
        [SerializeField] float turnSpeed;
        
        
        [SerializeField] private Door door;

        [SerializeField] Transform[] customerPositions;
        [SerializeField] Transform[] customerLeavePositions;

        [SerializeField] GameObject santa;
        [SerializeField] Transform santaSpawnPosition;
        [SerializeField] Transform santaDestinationPosition;
        [SerializeField] Transform santaLeavePosition;
        [SerializeField] Transform[] frenzySpawnPositions;

        public const int RENDER_SIZE = 12;
        
        Avatar[] activeAvatars;
        Avatar santaAvatar;

        LinkedList<Customer> customers;
        LinkedList<Avatar> avatars;
        Vector3[] targets;
        Quaternion queueRotation;

        int Count = 0;

        int Shuffling = 0;

        void Start () {
            avatars = new LinkedList<Avatar>();
            customers = new LinkedList<Customer>();
            activeAvatars = new Avatar[2];

            // set destination values   

            targets = new Vector3[RENDER_SIZE];
            for(int i=0; i<RENDER_SIZE; ++i) {
                targets[i] = transform.position + i * queueGap * transform.forward;
            }

            queueRotation = Quaternion.LookRotation(-transform.forward, Vector3.up);

        }


        public void Enqueue(Customer cust) {
            Count++;

            var avatar_go = Instantiate(ItemDictionary.instance.GetCustomer(cust.CustomerId), enqueueSpawn + targets[customers.Count], queueRotation);
            var opendoor = avatar_go.AddComponent<TrackablebyDoor>();
            opendoor.door = door;
            
            var avatar = avatar_go.GetComponent<Avatar>();
            avatar.SetName(cust.Name);
            if (Shuffling>0) {
                avatar.EnqueueTo(enqueueSpawn + targets[customers.Count], queueRotation);
            } else {
                avatar.EnqueueTo(targets[customers.Count], queueRotation);
            }
            avatars.AddLast(avatar);
            customers.AddLast(cust);
            /*
            if (shuffling) {
                OnShuffleFinish.Enqueue(() => _enqueue(cust));
            }

            // really make sure all the customers are going to their correct positions
            var currentAvatar = avatars.First;
            if(currentAvatar != null) {
                for (var i=0; currentAvatar.Next != null; ++i, currentAvatar = currentAvatar.Next) {
                    if(Vector3.SqrMagnitude(currentAvatar.Value.transform.position - targets[i]) > 0.0001f)
                        currentAvatar.Value.QueueMoveTo(targets[i], queueRotation);
                }
            }*/
        }

        void Shuffle(Action cont) {
            Shuffling++;
            _shuffle(avatars.First, 0, () => {
                Shuffling--;
                cont?.Invoke();
            });
        }

        void _shuffle(LinkedListNode<Avatar> avatar, int i, Action cont) {
            if (avatar == null || i >= targets.Length) cont?.Invoke();
            else avatar.Value.QueueMoveTo(targets[i], queueRotation, () => {
                _shuffle(avatar.Next, i+1, cont);
            });
        }


        public bool IsEmpty() {
            lock (avatars) {
                return avatars.Count == 0;
            }
        }

        public bool IsFull() {
            lock (avatars) {
                return Count == RENDER_SIZE;
            }
        }

        public Avatar SpawnSanta() {
            foreach(var cust in avatars) {
                cust.LeaveTo(customerLeavePositions[2].position, customerLeavePositions[2].rotation, Fn.noop);
            }
            var _santa = Instantiate(santa, santaSpawnPosition.position, santaSpawnPosition.rotation);

            santaAvatar = _santa.GetComponent<Avatar>();
            Debug.Log("Hello?");
            santaAvatar.DequeueTo(santaDestinationPosition.position, santaDestinationPosition.rotation);
            Debug.Log("Hello!");
            return santaAvatar;
        }

        public void SantaLeave(Action onBeforeLeave) {
            if(santaAvatar == null) Debug.LogError("Santa avatar is inactive or Santa has left.");
            santaAvatar.LeaveTo(santaLeavePosition.position, santaLeavePosition.rotation, onBeforeLeave);
            santaAvatar = null;
        }


        public Customer Dequeue(int playerId, ref Avatar avatar, bool returnAvatar = true) {
            var _avatar = avatars.First.Value;
            var customer = customers.First.Value;
            avatars.RemoveFirst();
            customers.RemoveFirst();

            var currentAvatar = avatars.First;
            /*
            if(currentAvatar != null) {
                for (var i=0; currentAvatar.Next != null; ++i, currentAvatar = currentAvatar.Next) {
                    currentAvatar.Value.WalkTo(targets[i], queueRotation, movementSpeed, turnSpeed);
                }
            }*/

            _avatar.DequeueTo(customerPositions[playerId].position, customerPositions[playerId].rotation);
            Shuffle(Fn.noop);


            if(returnAvatar) avatar = _avatar;
            activeAvatars[playerId] = _avatar;

            Count--;
            return customer;
        }

        public Customer Dequeue(int playerId) {
            Avatar flag = null;
            return Dequeue(playerId, ref flag, false);
        }

        public void CustomerLeave(int playerId, Action onBeforeLeave) {
            activeAvatars[playerId].LeaveTo(customerLeavePositions[playerId].position, customerLeavePositions[playerId].rotation, onBeforeLeave);
            activeAvatars[playerId] = null;
        }

        public bool HasActiveCustomer(int playerId) {
            return activeAvatars[playerId] != null;
        }

        public Avatar GetActiveCustomer(int playerId) {
            return activeAvatars[playerId];
        }

        public Transform GetFrenzySpawnPosition(int playerId) {
            return frenzySpawnPositions[playerId];
        }

        public void CustomerLeaveIfActive(int playerId) {
            if(activeAvatars[playerId]!=null) CustomerLeave(playerId, Fn.noop);
        }

    }
}