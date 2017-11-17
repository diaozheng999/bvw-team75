using PGT.Core;
using UnityEngine;
using Team75.Shared;

namespace Team75.Server {

    public class SpecialCustomer : MonoBehaviour {
        [SerializeField] int customerId;
        [SerializeField] short[] items;
        [SerializeField] bool generateRandom;
        [SerializeField] int groceryBudget;
        [SerializeField] ulong generateAfter;

        void Start () {

            var cust = new Customer();
            cust.Name = " ";
            cust.CustomerId = customerId;
            if (generateRandom) cust.Items = ItemDictionary.instance.GetItemsWithBudget(groceryBudget);
            else cust.Items = items;

            CustomerQueue.instance.AddSpecialCustomer(generateAfter, cust);
        }
    }

}