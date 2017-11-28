using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Team75.Client
{

	

	public class ShowResult : MonoBehaviour
	{

		private Transform win;
		private Transform lose;
		[SerializeField] private float speed_rise;
		[SerializeField] private float distance_flow;
		[SerializeField] private float height;
		[SerializeField] private float speed_flow;
		public void StartShow(bool winned)
		{
			if (winned)
			{
				lose.gameObject.active = false;
			}
			else
			{
				win.gameObject.active = false;
			}
			StartCoroutine(rise());
		}

		// Use this for initialization
		void Start()
		{
			Transform[] tmp = GetComponentsInChildren<Transform>();
			win = tmp[1];
			lose = tmp[2];
		}

		private IEnumerator rise()
		{
			var tmp = new WaitForSecondsRealtime(0.02f);
			while (transform.position.y < height)
			{
				yield return tmp;
				transform.Translate(0,0.02f*speed_rise,0);
			}
			yield return null;
			StartCoroutine(flow());
		}

		private IEnumerator flow()
		{
			var tmp = new WaitForSecondsRealtime(0.02f);
			bool rising = true;
			while (true)
			{
				yield return tmp;
				if (rising)
				{
					transform.Translate(0, 0.02f * speed_flow, 0);
					if (transform.position.y > height + distance_flow)
					{
						rising = false;
					}
				}
				else
				{
					transform.Translate(0, -0.02f * speed_flow, 0);
					if (transform.position.y < height - distance_flow)
					{
						rising = true;
					}
				}
			}
			yield return null;
		}
	}
}