using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Team75.Shared
{



	public class Door : MonoBehaviour
	{

		[SerializeField] private Transform leftdoor;
		[SerializeField] private Transform rightdoor;
		[SerializeField] private float speed_max;
		[SerializeField] private float acceleration;
		[SerializeField] private float thresh_close;
		[SerializeField] private float thresh_open;
		private int state;
		private int num_tracking;

		private float speeds_goal;

		private float speeds_real; // positive: go left; negative: go right
		// 0 for closed; 1 for opening; 2 for opened; 3 closing

		// Use this for initialization
		void Start()
		{
			state = 0;
			num_tracking = 0;
			speeds_goal = 0;
			speeds_real = 0;
		}

		// Update is called once per frame
		void Update()
		{
			leftdoor.transform.localPosition += new Vector3(speeds_real, 0, 0);
			rightdoor.transform.localPosition = new Vector3(-leftdoor.transform.localPosition.x,
				leftdoor.transform.localPosition.y, leftdoor.transform.localPosition.z);

			speeds_real = accelerate(speeds_real, speeds_goal, Time.deltaTime);

			if (state == 1 && leftdoor.transform.localPosition.x > thresh_open)
			{
				setState(2);
			}
			else if (state == 3 && leftdoor.transform.localPosition.x < thresh_close)
			{
				setState(0);
			}
			
			//if(Input.GetKeyDown(KeyCode.LeftArrow))

		}

		public void CustomerApproach()
		{
			num_tracking++;
			if (state == 0 || state == 3)
			{
				setState(1);
			
			}
		}

		public void CustomerLeave()
		{
			num_tracking--;
			if (num_tracking <= 0)
			{
				Debug.LogError("num_tracking: " + num_tracking.ToString());
				if (state == 1 || state == 2)
				{
					setState(3);
				}
			}
		}

		private void setState(int i)
		{
			state = i;
			switch (state)
			{
				case 0:
				{
					speeds_goal= 0;
					break;
				}
				case 1:
				{
					speeds_goal= speed_max;
					break;
				}
				case 2:
				{
					speeds_goal= 0;
					break;
				}
				case 3:
				{
					speeds_goal= -speed_max;
					break;
				}
			}
		}

		private float accelerate(float real, float goal,float deltatime)
		{
			if (Mathf.Abs(real - goal) < 0.01) return goal;
			else
			{
				return real + Mathf.Sign(goal - real) * acceleration * deltatime;
			}
		}
	}


}