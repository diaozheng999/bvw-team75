using System.Collections.Generic;
using PGT.Core;
using PGT.Core.DataStructures;
using UnityEngine;
using Team75.Shared;

namespace Team75.Server {

    public class CustomerQueue : Singleton<CustomerQueue> {

        [SerializeField] float EnqueueFrequency;
        Coroutine_ countdownTimer;
        
        Queue<Customer> visibleCustomerQueue;
        Queue<Customer> overflowCustomerQueue;

        ulong n_customers;

        Coroutine_ generationCoroutine = null;

        Heap<ulong, Customer> specialCustomers;

        void Start () {
            visibleCustomerQueue = new Queue<Customer>(VisibleCustomerQueue.RENDER_SIZE);
            overflowCustomerQueue = new Queue<Customer>();
        }

        public void StartAccepting () {
            generationCoroutine = this.StartCoroutine1(GenerationCoroutine());
        }

        IEnumerator<object> GenerationCoroutine() {
            Debug.Log("Generating in "+EnqueueFrequency+" second(s)...");
            yield return new WaitForSeconds(EnqueueFrequency);
            Debug.Log("Generating.");
            Enqueue(GenerateRandom());
        }

        public static Customer GenerateRandom(int budget = 400) {
            var cust = new Customer();
            //------MODIFY-----------------
            cust.CustomerId = ItemDictionary.instance.GetRandomNormalCustomer();
            //------------END-----------------
            cust.Name = " ";
            cust.Items = ItemDictionary.instance.GetItemsWithBudget(budget);

            return cust;
        }

        public static Customer GetSpecific(int customer, int budget = 400) {
            var cust = new Customer();
            cust.CustomerId = customer;
            cust.Name = " ";
            cust.Items = ItemDictionary.instance.GetItemsWithBudget(budget);

            return cust;
        }

        public void StopCustomerSpawns() {
            if(generationCoroutine != null && !generationCoroutine.Completed)
                generationCoroutine.Interrupt();
        }


        public void AddSpecialCustomer(ulong after, Customer cust) {
            if(specialCustomers == null) specialCustomers = new Heap<ulong, Customer>();
            specialCustomers.Insert(cust, after);
        }

        void ResetTimer(){
            n_customers++;

            if(generationCoroutine != null && !generationCoroutine.Completed)  generationCoroutine.Interrupt();

            Debug.Log(specialCustomers.Count);
            if(specialCustomers.Count > 0) {
                Debug.Log(specialCustomers.Peek().Key+", "+n_customers);
            }

            if(specialCustomers.Count > 0 && specialCustomers.Peek().Key == n_customers) {
                var cust = specialCustomers.DeleteMin().Value;
                this.StartCoroutine1(GenerateAfter(cust));
                generationCoroutine = null;
            } else {
                generationCoroutine = this.StartCoroutine1(GenerationCoroutine());
            }
        }

        public void Enqueue(Customer customer) {   
            lock(visibleCustomerQueue) {
                if(!VisibleCustomerQueue.instance.IsFull()) {
                    VisibleCustomerQueue.instance.Enqueue(customer);
                    NetworkManager.instance.SendMessageToBoth(Connection.ENQUEUE_CUSTOMER, Connection.PackCustomer(customer));
                    ResetTimer();
                    return;
                }
            }
            lock(overflowCustomerQueue) {
                overflowCustomerQueue.Enqueue(customer);
            }

            ResetTimer();

        }

        IEnumerator<object> GenerateAfter(Customer cust){
            yield return new WaitForSeconds(EnqueueFrequency / 5);
            Enqueue(cust);
        }
        

        public void Dequeue(int toPlayer) {
            lock (visibleCustomerQueue) {
                if (VisibleCustomerQueue.instance.IsEmpty()) {
                    NetworkManager.instance.SendMessageToBoth(Connection.CUSTOMER_QUEUE_EMPTY, new byte[0]{});
                    Debug.LogWarning("Visible Queue Empty!!!");
                    return;
                }
                Customer cust = VisibleCustomerQueue.instance.Dequeue(toPlayer);
                
                NetworkManager.instance.SendMessageToBoth(
                    Connection.CUSTOMER_RESPONSE,
                    Connection.PackCustomerDequeueInfo((byte)toPlayer, cust)
                );

                while (!VisibleCustomerQueue.instance.IsFull()) {
                    lock (overflowCustomerQueue) {
                        if (overflowCustomerQueue.Count == 0) return;
                        var transfer_cust = overflowCustomerQueue.Dequeue();
                        visibleCustomerQueue.Enqueue(transfer_cust);
                        VisibleCustomerQueue.instance.Enqueue(transfer_cust);
                        NetworkManager.instance.SendMessageToBoth(Connection.ENQUEUE_CUSTOMER, Connection.PackCustomer(transfer_cust));
                    }
                }
            }
        }

    }

}