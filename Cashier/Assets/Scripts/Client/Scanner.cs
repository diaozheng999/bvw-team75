using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PGT.Core;
using PGT.Core.Networking;
using Team75.Shared;

namespace Team75.Client
{



	public class Scanner: Singleton<Scanner>
	{

		Hand.HandEnum hand;
		Shared.Scanner _scanner;

		[SerializeField] int playerId;
		[SerializeField] bool startBroadcasting = false;

		[SerializeField] float minClip;
		[SerializeField] float maxClip;
		[SerializeField] float faceTolerance;
		[SerializeField] float skewTolerance;


		[SerializeField] Transform laser_origin;

		[SerializeField] Transform trigger;
		[SerializeField] float triggerMinRotationX;
		[SerializeField] float triggerMaxRotationX;
		[SerializeField] private GameObject light;
		
		//---------INSERT-------------
		[SerializeField] private AudioClip clip0;

		[SerializeField] private AudioClip clip1;
		//-------------END---------------


		Dictionary<int, Tuple<Scannable, Transform>> items;
		
		// Use this for initialization
		void Start()
		{
			enabled = false;
			items = new Dictionary<int, Tuple<Scannable, Transform>>();
		}

		public void StartBroadcasting() {
			startBroadcasting = true;
			NetworkPositionManager.instance.AddBroadcastTransform(transform, (playerId == 0) ? Connection.PLAYER_ONE_SCANNER : Connection.PLAYER_TWO_SCANNER);
		}

		public void AddItem(Scannable item) {
			var barcodeTransform = item.GetComponent<IBarcodeProvider>().GetBarcodeLocation();
			items[item.GetInstanceID()] = new Tuple<Scannable, Transform>(item, barcodeTransform);
		}

        public void RemoveItem(Scannable item)
        {
            var itemid = item.GetInstanceID();
            if (items.ContainsKey(itemid))
            {
                items.Remove(itemid);
            }
        }

		public void SetActiveHand(Hand.HandEnum _hand) {
			hand = _hand;
		}


		protected override void OnDestroy() {
			if(startBroadcasting)
				NetworkPositionManager.instance.RemoveBroadcastTransform((playerId == 0) ? Connection.PLAYER_ONE_SCANNER : Connection.PLAYER_TWO_SCANNER);
			base.OnDestroy();
		}

		public Scannable Scan()
		{
			light.SetActive(true);
			
			var toRemove = new LinkedList<int>();

			foreach (var item in items) {
				var client_item = item.Value.Item1;
				var barcode = item.Value.Item2;

				if(barcode==null) {
					toRemove.AddLast(item.Key);	
				}
				
				if (Vector3.Angle(laser_origin.forward, barcode.position - laser_origin.position) > Shared.Scanner.SPREAD) {
					continue;
				}

				var _tf = laser_origin.InverseTransformPoint(barcode.position);

				if(_tf.z < minClip || _tf.z > maxClip) continue;

				if (Vector3.Angle(laser_origin.forward, barcode.forward) > faceTolerance) continue;

				var _id = Vector3.Angle(laser_origin.right, barcode.right);

				if(_id > skewTolerance && _id < (180 - skewTolerance)) continue;

				// passed all checks.
				//---------------INSERT-----------------
				Statics.instance.ScanItem(client_item.GetId());
				GetComponent<AudioSource>().clip = (playerId == 0) ? clip0 : clip1;
				GetComponent<AudioSource>().Play();
				//-------------------END--------------
				items.Remove(client_item.GetInstanceID());
				return client_item;
			}

			foreach(var id in toRemove) {
				items.Remove(id);
			}

			return null;

		}

		public void NotScan()
		{
			light.SetActive(false);
		}

		public void SetTriggerState(float state) {
			trigger.localEulerAngles = new Vector3(Mathf.Lerp(triggerMinRotationX, triggerMaxRotationX, state), 0, 0);
		}


		private void OnTriggerEnter(Collider other)
		{
			var ring = other.GetComponent<Ring>();
			if (ring != null)
			{
				ring.GetHit();
			}
		}

		//---------INPUT-------------
		//public Transform position_raycast1;

		//public Transform position_raycast2;

		//public bool scanning = false;

		//public float distance_scan;

		//public GameObject light;
		//--------OUTPUT--------------
		/*
		//----------PRIVATE------------
		private Ray ray1, ray2;

		private RaycastHit hit1, hit2;
        //-------------TEST-------------
        public Transform t1;
        public Transform t2;
		*/
		/*
		// Update is called once per frame
		void Update()
		{
			//light.active = scanning;
			if (scanning)
			{
				ray1 = new Ray(position_raycast1.position, transform.forward);
				ray2 = new Ray(position_raycast2.position, transform.forward);
				Debug.DrawRay(ray1.origin, ray1.direction * distance_scan, Color.magenta, 1f);
				Debug.DrawRay(ray2.origin, ray2.direction * distance_scan, Color.red, 1f);
				if (Physics.Raycast(ray1, out hit1, distance_scan) && Physics.Raycast(ray2, out hit2, distance_scan))
				{
                    t1 = hit1.transform;
                    t2 = hit2.transform;
					if (hit1.transform.parent == hit2.transform.parent &&
					    (hit1.transform.tag == "Barcode1" && hit2.transform.tag == "Barcode2" ||
					     hit1.transform.tag == "Barcode2" && hit2.transform.tag == "Barcode1"))
					{
						scaned(hit1.transform.parent);
					}

				}
				
			}
		}

		*/


		/*
		public void scaned(Transform barcode)
		{
			var _barcode = barcode.GetComponent<Barcode>();
			/*
			OVRHapticsClip scan = new OVRHapticsClip(new byte[8]{255,255,255,255,128,255,128,255}, 8);

			_barcode.OnScan();

            
			if(leftHand)
				OVRHaptics.LeftChannel.Preempt(scan);
			else
				OVRHaptics.RightChannel.Preempt(scan);
                
			Debug.LogWarning("scaned");
		}
		*/
	}
}
