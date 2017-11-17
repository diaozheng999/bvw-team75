using System.Collections;
using System.Collections.Generic;
using System.Net.Configuration;
using UnityEngine;


namespace Team75.Client
{
	public class ScannerBox_Client : MonoBehaviour {
	
		//---------INPUT-------------
		[SerializeField] private Transform _left;

		public Transform Left
		{
			get
			{
				return _left;
			}
			set { _left = value; }
		}

		public Transform left;
		public Transform right;
		public Transform scanner;
		//--------OUTPUT--------------
		// void pickup(int handnum) // 1 for left, 2 for right
		// void putdown()

		enum State
		{
			IN_BOX, IN_LEFT_HAND, IN_RIGHT_HAND
		}

		private State _state = State.IN_BOX;
		
		//----------PRIVATE------------
		public int state = 0; // 0: in box; 1: in left hand; 2: in right hand
		public int enterbox = 0; // 0: no hand in box; 1=left; 2=right; set value when hand enter box; clean value when event triggered or hand leave box
		//-------------TEST-------------
		
		
		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
		void Update ()
		{
			int i=0;
			int[] buffer = new int[4];
			
			if ((i++ < 0) || (i++ > 1))
			
			enterbox = Input.GetKeyDown(KeyCode.L) ? 1 : enterbox;
			enterbox = Input.GetKeyDown(KeyCode.R) ? 2 : enterbox;
	
			if (state==0 && enterbox!=0) // scanner in box
			{
				pickup(enterbox);
			}
			else if(state!=0 && state==enterbox)// scanner in hand
			{
				
				putdown(enterbox);
			}
		}
	
		private void OnTriggerEnter(Collider other)
		{
			if (other.tag == "Left")
			{
				enterbox = 1;
			}
			if (other.tag == "Right")
			{
				enterbox = 2;
			}
		}
	
		private void OnTriggerExit(Collider other)
		{
			if (enterbox == 1 && other.tag == "Left")
			{
				enterbox = 0;
			}
			if (enterbox == 2 && other.tag == "Right")
			{
				enterbox = 0;
			}
		}
	
		public void pickput(int handnum)
		{
			enterbox = 0;
			if (state == 0)
			{
				pickup(handnum);
			}
			else
			{
				putdown(handnum);
			}
		}
	
		public void pickup(int handnum) // 1 for left, 2 for right
		{
			//print(handnum);
			switch (handnum)
			{
				case 1:
				{
					if (left.GetComponent<Hand_Client>().grabscanner())
					{
						state = handnum;
						enterbox = 0;
						scanner.parent = left.transform;
						scanner.localPosition=new Vector3(0,0,0);
						scanner.rotation = left.rotation;
					}
					
					break;
				}
				case 2:
				{
					if (right.GetComponent<Hand_Client>().grabscanner())
					{
						state = handnum;
						enterbox = 0;
						scanner.parent = right.transform;
						scanner.localPosition = new Vector3(0, 0, 0);
						scanner.rotation = right.rotation;
					}
					break;
				}
			}
		}
	
		public void putdown(int handnum)
		{
			switch (handnum)
			{
				case 1:
				{
					if (left.GetComponent<Hand_Client>().releasescanner())
					{
						state = 0;
						enterbox = 0;
						scanner.parent = transform;
						scanner.localPosition=new Vector3(0,0,0);
						scanner.rotation=Quaternion.Euler(0,0,0);
					}
					break;
				}
				case 2:
				{
					if (right.GetComponent<Hand_Client>().releasescanner())
					{
						state = 0;
						enterbox = 0;
						scanner.parent = transform;
						scanner.localPosition=new Vector3(0,0,0);
						scanner.rotation=Quaternion.Euler(0,0,0);
					}
					break;
				}
			}
			
		}
	}
	
}
