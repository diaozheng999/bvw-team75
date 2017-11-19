using System.Collections;
using System.Collections.Generic;
using Team75.Shared;
using UnityEngine;

namespace Team75.Shared
{



	public class TrackablebyDoor : MonoBehaviour
	{

		public Door door;

		private float distance_approach=4;

		//private float distance_leave=2;

		private int inTracking = 0;

		// Use this for initialization
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{
			if (inTracking == 0 && Vector3.Distance(door.transform.position, transform.position) < distance_approach-0.1)
			{
				inTracking = 1;
				door.CustomerApproach();
			}
			else if (inTracking == 1 && Vector3.Distance(door.transform.position, transform.position) > distance_approach+0.1)
			{
				inTracking = 2;
				door.CustomerLeave();
				Destroy(this);
			}
		}
	}
}