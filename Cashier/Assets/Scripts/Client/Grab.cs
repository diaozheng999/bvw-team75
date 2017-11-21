using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Team75.Client{
	public class Grab : MonoBehaviour
	{

		[SerializeField] Hand.HandEnum hand;

		//Dictionary<int, Item> intersect;
		
		//Dictionary<int, Tuple<Item, FixedJoint>> held;

		int intersectedInstanceId;
        [SerializeField]
		Item intersectedItem;

		int heldInstanceId;
		[SerializeField] private float threshold;
		Item heldItem;
		FixedJoint heldJoint;


		OVRInput.Button trigger;
		OVRInput.Axis1D indexTrigger;
		OVRInput.Axis1D handTrigger;

		OVRInput.Axis1D grabbedAxis;
		
		private bool grabbed = false;
		
        Rigidbody rb;

		void Start(){
			//intersect = new Dictionary<int, Item>();
			//held = new Dictionary<int, Tuple<Item, FixedJoint>>();

			switch (hand) {
				case Hand.HandEnum.LEFT:
					indexTrigger = OVRInput.Axis1D.PrimaryIndexTrigger;
					handTrigger = OVRInput.Axis1D.PrimaryHandTrigger;
					break;
				case Hand.HandEnum.RIGHT:
					indexTrigger = OVRInput.Axis1D.SecondaryIndexTrigger;
					handTrigger = OVRInput.Axis1D.SecondaryHandTrigger;
					break;
			}


            rb = GetComponent<Rigidbody>();
		}

		void OnTriggerEnter(Collider other) {
			var item = other.GetComponent<Item>();
			if((item != null) && heldItem == null) {
				intersectedInstanceId = item.GetInstanceID();
				intersectedItem = item;
				//intersect[item.GetInstanceID()] = item;
			}
			var ring = other.GetComponent<Ring>();
			if (ring != null)
			{
				ring.GetHit();
			}
		}

		void OnTriggerExit(Collider other) {
			var item = other.GetComponent<Item>();
			if(item != null) {
			    var itemid = item.GetInstanceID();
				// only interact if nothing is held.
				if (intersectedInstanceId == itemid && heldItem == null) {
					intersectedInstanceId = 0;
					intersectedItem = null;
				}
				/*
				intersect.Remove(item.GetInstanceID());
				if(held.ContainsKey(itemid)){
					var joint = held[item.GetInstanceID()].Item2;
					held.Remove(item.GetInstanceID());
					item.OnRelease(hand);
                    Destroy(joint);
				}
				*/
			}
		}


		void OnRelease() {
			if(heldItem != null) {
				Debug.Log("Released object "+heldItem.gameObject.name);
				heldItem.OnRelease(hand);
				heldItem = null;
				heldInstanceId = 0;

				if(heldJoint != null) {
					Destroy(heldJoint);
				}
			}
		}

		void OnGrab() {
            
			if(heldItem != null) {
				Debug.LogError("Object released by physics.");
			}

			if (intersectedItem != null) {
				heldItem = intersectedItem;
				heldInstanceId = intersectedInstanceId;
				Debug.Log("Held object "+heldItem.gameObject.name);

				GetComponent<AudioSource>().Play();
				var itemsound = heldItem.gameObject.GetComponent<ItemSound>();
				if (itemsound != null) itemsound.PlaySound(1);
					

				heldJoint = heldItem.gameObject.AddComponent<FixedJoint>();
				heldJoint.breakForce = Mathf.Infinity;
				heldJoint.breakTorque = Mathf.Infinity;
				heldJoint.connectedBody = rb;

                if (heldItem.tag == "Soap")
                {
                    heldItem.GetComponent<Soap>().Set(grabbedAxis, this);
                }
			}

		}

		
		void Update() {
			if(Input.GetKeyUp(KeyCode.Space)) {
				grabbed = false;
				OnRelease();
			} else if(Input.GetKeyDown(KeyCode.Space)) {
				grabbed = true;
				OnGrab();
			}


			if (grabbed) {
				if (OVRInput.Get(grabbedAxis) < threshold) {
					grabbed = false;
					if (heldItem.tag == "Soap")
					{
						heldItem.GetComponent<Soap>().Release();
					}
					OnRelease();
				}
			} else {
				if (OVRInput.Get(indexTrigger) >= threshold) {
					grabbed = true;
					grabbedAxis = indexTrigger;
					OnGrab();

				} else if(OVRInput.Get(handTrigger) >= threshold) {
					grabbed = true;
					grabbedAxis = handTrigger;;
					OnGrab();
				}
			}
		}

		public void ClearSoap()
		{
			if (heldItem.tag == "Soap")
			{
                Soap sp = heldItem.gameObject.GetComponent<Soap>();
				OnRelease();
                sp.Jump();
            }
		}

		

            
	/*

	// Update is called once per frame
	void Update()
	{
		if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger) || Input.GetKeyDown(KeyCode.L))
		{
                print("grab");
			lefthand.grab();
			if (righthand == lefthand)
			{
				righthand.release();
			}
		}
		else if (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger) || Input.GetKeyDown(KeyCode.Semicolon))
		{
			lefthand.release();
			lefthand.draw();
		}
		if (OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger) || Input.GetKeyDown(KeyCode.R))
		{
			righthand.grab();
			if (righthand == lefthand)
			{
				lefthand.release();
			}
		}
		else if (OVRInput.GetUp(OVRInput.Button.SecondaryHandTrigger) || Input.GetKeyDown(KeyCode.T))
		{
			righthand.release();
			righthand.draw();
		}
	}
		*/
		
	}

}
