using PGT.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Team75.Shared {
    public class ItemDictionary : Singleton<ItemDictionary> {

        //customers
        [SerializeField] GameObject[] customers;
        [SerializeField] GameObject[] items;
        [SerializeField] private float[] possibilities;
        private float[] insections;
        private float possi_total;

        [SerializeField] int[] itemValues;

        [SerializeField] int specialCustomers;

        protected override void Awake()
        {
            base.Awake();
            insections = new float[Mathf.Max(0,items.Length-1)];
            if (insections.Length > 0)
            {
                insections[0] = possibilities[0];
                for (int i = 1; i < insections.Length; i++)
                {
                    insections[i] = insections[i - 1] + possibilities[i];
                }
            }
            possi_total = 0;
            for (int i = 0; i < possibilities.Length; i++)
            {
                possi_total += possibilities[i];
            }
        }

        public GameObject GetItem(int id) {
            return items[id];
        }

        public int GetItemValue(int id) {
            return itemValues[id];
        }

        public int UniqueItems() {
            return items.Length;
        }

        public GameObject GetCustomer(int id) {
            return customers[id];
        }

        public int GetRandomNormalCustomer(){
            return Random.Range(0, specialCustomers);
        }
        
        public short[] GetItemsWithBudget(int budget) {
            var list = new List<short>();
            var _budget = budget;
            do {
                var item = generaterandomitem();
                var price = GetItemValue(item);
                if(price < _budget) {
                    _budget -= price;
                    list.Add(item);
                } else {
                    break;
                }
            } while (_budget > 0);
            return list.ToArray();
        }
        
        public short generaterandomitem()
        {
            float seed = Random.Range(0f, possi_total);
            short result = 0;
            foreach (float f in insections)
            {
                if (f < seed) result++;
                else break;
            }
            return result;
        }

    }

}