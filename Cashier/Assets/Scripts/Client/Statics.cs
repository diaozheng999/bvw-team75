using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PGT.Core;
using PGT.Core.DataStructures;
using UnityEngine;
using System;
using Team75.Shared;

namespace Team75.Client
{
	public class Statics : Singleton<Statics>
	{


		[SerializeField] private int kinds_merchandise;

		[SerializeField] private int kinds_customer;

		public int Item_Exist = 0;

		public int Item_Scanned = 0;

		public float Time_Scanning = 0;

		public int Customer_Exist = 0;

		public int Customer_Completed = 0;

		private float Percentage_Complete_Customer;

		public int[] Distribution_Merchandise_Exist;
		public int[] Distribution_Merchandise_Scanned;

		public int[] Distribution_Customer_Exist;


		private bool gamestarted=false;
		private float time_triggerdown;
		private bool triggerdown;

		bool packaged = false;
		private GameStat _package;
		
		
		void Start() {
			DontDestroyOnLoad(this);
		}

		// Use this for initialization
		public void StartGame(int playerID)
		{
			gamestarted = true;
			Distribution_Customer_Exist = new int[kinds_customer];
			Distribution_Merchandise_Exist = new int[kinds_merchandise];
			Distribution_Merchandise_Scanned = new int[kinds_merchandise];
		}

		public void GenerateItem(int index) 
		{
			Item_Exist++;
			Distribution_Merchandise_Exist[index]++;
		}

		public void GenerateCustomer(int index)  
		{
			Customer_Exist++;
			Distribution_Customer_Exist[index]++;
		}

		public void ScanItem(int index) // called
		{
			Item_Scanned++;
			Distribution_Merchandise_Scanned[index]++;
		}

		public void CustomerComplete(int index) 
		{
			Customer_Completed++;
		}

		public void TriggerDown() 
		{
			if (!triggerdown)
			{
				time_triggerdown = Time.time;
				triggerdown = true;
			}
		}

		public void TriggerUp()
		{
			if (triggerdown)
			{
				triggerdown = false;
				Time_Scanning += Time.time - time_triggerdown;
			}
		}

		public void ShowResult()
		{
			Percentage_Complete_Customer = (float) Customer_Completed / Customer_Exist * 100;
		}

		public Sequence<Tuple<ushort, int>> SortItems() {
			ushort len = (ushort)Distribution_Merchandise_Scanned.Length;
			var heap = new Heap<int, ushort>(len);

			for(ushort i=0; i<len; ++i) {
				heap.Insert(i, -Distribution_Merchandise_Scanned[i]);
			}

			return Sequence.Unroll(heap).Map((KeyValuePair<int, ushort> v) => 
				new Tuple<ushort, int>(v.Value, -v.Key));
		}


		
        public GameStat Pack() {
			if(packaged) return _package;
            _package = new GameStat();
			_package.customerServed = (uint)Customer_Exist;
			_package.customerCompleted = (uint)Customer_Completed;
			_package.itemGiven = (uint)Item_Exist;
			_package.itemScanned = (uint)Item_Scanned;
			_package.revenue = ScoreManager.instance.GetScore();

			var _Sorted = SortItems();
			if (Distribution_Merchandise_Scanned.Length > 5) {
				_Sorted = _Sorted.Take(5);
			}

			var _Sorted_Memoized = _Sorted.Memoize();

			_package.itemIds = _Sorted_Memoized.Map((Tuple<ushort, int> v) => v.Item1).ToArray();
			_package.itemCounts = _Sorted_Memoized.Map((Tuple<ushort, int> v) => (uint)v.Item2).ToArray();

			packaged = true;

			return _package;
        }

		public byte[] PackBinary(){
			return Connection.PackStats(Pack());
		}

	}
}
