using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Team75.Client
{
	public class Line : MonoBehaviour
	{
		[SerializeField] private float speed;

		[SerializeField] private float position_max;

		//[SerializeField] private float position_min;
		[SerializeField] private float width;

		private int size;

		private Transform[] blocks;

		private int index;
		// Use this for initialization

		void Awake()
		{
			blocks = GetComponentsInChildren<Transform>();
			size = blocks.Length - 1;
			index = size;
		}
		
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{
			for (int i = 1; i <= size; i++)
			{
				blocks[i].localPosition+=new Vector3(speed*Time.deltaTime,0,0);
				
			}
			while (blocks[index].localPosition.x > position_max)
			{
				int tmp = index % size + 1;
				blocks[index].localPosition = blocks[tmp].localPosition - new Vector3(width, 0, 0);
				index = index == 1 ? size : index - 1;
			}
		}
	}
}
