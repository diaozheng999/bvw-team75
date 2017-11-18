using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Team75.Client{
	public class Hand : MonoBehaviour
	{
		public static Hand Left;
		public static Hand Right;

		[SerializeField] HandEnum hand;
		[SerializeField] GameObject sprite;
		[SerializeField] GameObject scanner;

		private bool triggerdown_scanner=false;
		
		Scanner mbScanner;

		public enum HandEnum { LEFT=0, RIGHT=1 }

		//public GameObject item;
		//public bool haveitem = false;

		bool isScanner;

        private Vector3 speed;
        private Vector3 position_last;

		OVRHapticsClip hapticsClip;


		void Awake() {
			switch (hand) {
				case HandEnum.LEFT:
					if(Left != null) Debug.LogError("There can only be one left hand.");
					Left = this;
					break;
				case HandEnum.RIGHT:
					if(Right != null) Debug.LogError("There can only be one right hand.");
					Right = this;
					break;
			}
		}

		public static Hand GetHand(HandEnum hand) {
			switch (hand) {
				case HandEnum.LEFT: return Left;
				case HandEnum.RIGHT: return Right;
				default: return null;
			}
		}

		public static Hand Other(HandEnum hand) {
			switch (hand) {
				case HandEnum.LEFT: return Right;
				case HandEnum.RIGHT: return Left;
				default: return null;
			}
		}

		public void SetAsScanner() {
			isScanner = true;

			//scanning
			scanner.transform.SetParent(transform);
			scanner.transform.localPosition = sprite.transform.localPosition;
			scanner.transform.localRotation = sprite.transform.localRotation;
			sprite.SetActive(false);
		}

		public void RemoveScanner() {
			isScanner = false;
			sprite.SetActive(true);
		}


		void Update()
		{
			var curr_pos = transform.position;
            speed = (curr_pos - position_last) / Time.deltaTime;
            position_last = curr_pos;

			if (isScanner) {
				var t = OVRInput.Get(GetIndexTrigger());
				Scanner.instance.SetTriggerState(t);
				if (t > 0.7f)
				{
					if (!triggerdown_scanner)
					{
						triggerdown_scanner = true;
						Statics.instance.TriggerDown();
					}
					var item = Scanner.instance.Scan();
					if (item != null)
					{
						item.SetScanned(true);

						if (hapticsClip == null)
							hapticsClip = new OVRHapticsClip(new byte[8] {255, 255, 255, 255, 128, 255, 128, 255}, 8);

						switch (hand)
						{
							case HandEnum.LEFT:
								OVRHaptics.LeftChannel.Preempt(hapticsClip);
								break;
							case HandEnum.RIGHT:
								OVRHaptics.RightChannel.Preempt(hapticsClip);
								break;
						}
					}
				}
				else
				{
					if (triggerdown_scanner)
					{
						triggerdown_scanner = false;
						Statics.instance.TriggerUp();
					}
					Scanner.instance.NotScan();
				}
			}

			/*
			if (haveitem && item.tag == "Scanner")
			{
				var _scanner = item.GetComponent<Scanner>();
				if (tag == "Left")
				{
					_scanner.scanning = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger);
					_scanner.SetHand(true);
				}
				if (tag == "Right")
				{
					_scanner.scanning = OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger);
					_scanner.SetHand(false);
				}
			}
			if (tag == "Left")
			{
				if (haveitem && item.tag == "Scanner")
				{
					item.GetComponent<Scanner>().scanning =
						OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) || Input.GetKey(KeyCode.Quote);
				}
			}
			if (tag == "Right")
			{
				if (haveitem && item.tag == "Scanner")
				{
					item.GetComponent<Scanner>().scanning =
						OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) || Input.GetKey(KeyCode.Y);
				}
			}*/
		}

		public Vector3 GetVelocity() => speed;

		OVRInput.Axis1D GetIndexTrigger(){
			switch (hand) {
				case HandEnum.LEFT: return OVRInput.Axis1D.PrimaryIndexTrigger;
				case HandEnum.RIGHT: return OVRInput.Axis1D.SecondaryIndexTrigger;
				default: return 0;
			}
		}
		/*
		private void OnTriggerEnter(Collider other)
		{
			//print("enter");
			if (!haveitem && other.gameObject.layer == LayerMask.NameToLayer("Grabable"))
			{
				item = other.gameObject;
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (!haveitem && other.gameObject==item)
			{
				item = null;
			}
		}

		private void OnTriggerStay(Collider other)
		{
			if (!haveitem && other.gameObject.layer==LayerMask.NameToLayer("Grabable") && item==null)			
			{
				item = other.gameObject;
			}
		}

		public void grab()
		{
			if (!haveitem && item!=null)
			{
				haveitem = true;
				if (item.tag == "Scanner")
				{
					item.transform.position = transform.position;
					item.transform.rotation = transform.rotation * Quaternion.Euler(90, 90, 0);
				}
				FixedJoint joint = gameObject.AddComponent<FixedJoint>();
				joint.breakForce = 10000;
				joint.breakTorque = 10000;
				joint.connectedBody = item.GetComponent<Rigidbody>();
				GetComponent<Animator>().ResetTrigger("ReleaseSmall");
				GetComponent<Animator>().SetTrigger("GrabSmall");
			}
		}

		public void release()
		{
			if (haveitem)
			{
				GetComponent<Item>()?.DestroyIfScanned();
				haveitem = false;
				Destroy(gameObject.GetComponent<FixedJoint>());
				GetComponent<Animator>().ResetTrigger("GrabSmall");
				GetComponent<Animator>().SetTrigger("ReleaseSmall");
			}
		}
		
		public static bool operator ==(Hand c1, Hand c2)
		{
			return c1.haveitem && c2.haveitem && c1.item == c2.item;
		}
		
		public static bool operator !=(Hand c1, Hand c2)
		{
			return !c1.haveitem || !c2.haveitem || c1.item != c2.item;

		}

		public void draw()
		{
			if (item != null)
			{
				item.GetComponent<Rigidbody>().velocity = speed;
			}
		}*/
	}
	
	
	

}
