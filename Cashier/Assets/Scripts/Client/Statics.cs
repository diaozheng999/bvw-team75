using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PGT.Core;
using UnityEngine;

namespace Team75.Client
{
	public class Statics : Singleton<Statics>
	{


		[SerializeField] private int kinds_merchandise;

		[SerializeField] private int kinds_customer;

		private int Item_Exist = 0;

		private int Item_Scanned = 0;

		private float Time_Scanning = 0;

		private int Customer_Exist = 0;

		private int Customer_Completed = 0;

		private float Percentage_Complete_Customer;

		private int[] Distribution_Merchandise_Exist;
		private int[] Distribution_Merchandise_Scanned;

		private int[] Distribution_Customer_Exist;


		private bool gamestarted=false;
		private float time_triggerdown;
		private bool triggerdown;
		
		

		// Use this for initialization
		public void StartGame(int playerID)
		{
			gamestarted = true;
			Distribution_Customer_Exist = new int[kinds_merchandise];
			Distribution_Merchandise_Exist = new int[kinds_customer];
			Distribution_Merchandise_Scanned = new int[kinds_merchandise];
		}

		// Update is called once per frame
		void Update()
		{
			
		}

		public void GenerateItem(int index) //called
		{
			Item_Exist++;
			Distribution_Merchandise_Exist[index]++;

		}

		public void GenerateCustomer(int index)
		{
			Customer_Exist++;
			Distribution_Customer_Exist[index]++;
		}

		public void ScanItem(int index)
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


	}
}
