using System;
using PGT.Core;
using PGT.Core.Func;
using PGT.Core.DataStructures;
using System.Collections.Generic;
using UnityEngine;
using URandom = UnityEngine.Random;

namespace Team75.Shared {
    public class ItemDictionary : Singleton<ItemDictionary> {

        //customers
        [SerializeField] GameObject[] customers;
        [SerializeField] GameObject[] items;
        [SerializeField] private float[] possibilities;
        [SerializeField] float[] frenzyLikelihood;
        [SerializeField] private float[] insections;
        float[] frenzyCDF;
        private float possi_total;


        [SerializeField] int[] itemValues;

        [SerializeField] int specialCustomers;

        protected override void Awake()
        {
            base.Awake();
            insections = BuildCDF(possibilities);
            //frenzyCDF = BuildCDF(frenzyLikelihood);
            /*
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
            */
        }

        float[] BuildCDF(float[] likelihood) {
            var _likelihood = Sequence.Array(likelihood);
            var total = _likelihood.Reduce(Function.fadd, 0);
            var pdf = _likelihood.Map((float v)=> v/total);
            return pdf.ScanIncl(Function.fadd, 0).ToArray();
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
            return URandom.Range(0, specialCustomers);
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
        
        public short GenerateRandomItem(float[] CDF) {
            var result = Array.BinarySearch<float>(CDF, URandom.value);
            if(result < 0) {
                var result_ = ~result;
                if(result_ == CDF.Length) return (short)(result_ - 1);
                return (short)result_;
            }
            return (short)result;
        }

        public short generaterandomitem()
        {
            return GenerateRandomItem(insections);
        }

    }

}