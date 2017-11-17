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
            lock (avatars) {
                var avatar_go = Instantiate(ItemDictionary.instance.GetCustomer(cust.CustomerId), enqueueSpawn + targets[customers.Count], queueRotation);
                var avatar = avatar_go.GetComponent<Avatar>();
                avatar.SetName(cust.Name);
                avatar.WalkTo(targets[customers.Count], queueRotation, movementSpeed, turnSpeed);

                avatars.AddLast(avatar);
                customers.AddLast(cust);

                // really make sure all the customers are going to their correct positions
                var currentAvatar = avatars.First;
                if(currentAvatar != null) {
                    for (var i=0; currentAvatar.Next != null; ++i, currentAvatar = currentAvatar.Next) {
                        if(Vector3.SqrMagnitude(currentAvatar.Value.transform.position - targets[i]) > 0.0001f)
                            currentAvatar.Value.WalkTo(targets[i], queueRotation, movementSpeed, turnSpeed);
                    }
                }

            }
        }

        public bool IsEmpty() {
            lock (avatars) {
                return avatars.Count == 0;
            }
        }

        public bool IsFull() {
            lock (avatars) {
                return avatars.Count == RENDER_SIZE;
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

                _avatar.WalkTo(customerPositions[playerId].position, customerPositions[playerId].rotation, movementSpeed, turnSpeed);

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
            activeAvatars[playerId].LeaveTo(customerLeavePositions[playerId].position, customerLeavePositions[playerId].rotation, movementSpeed, turnSpeed);
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