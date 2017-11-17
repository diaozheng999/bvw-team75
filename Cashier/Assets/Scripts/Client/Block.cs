using System.Collections;
using System.Collections.Generic;
//using UnityEditor;
using UnityEngine;

public class Block : MonoBehaviour
{

	[SerializeField] private float speed;

	[SerializeField] private float position_max;

	//[SerializeField] private float position_min;

	[SerializeField] private Block last;

	[SerializeField] private float width;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.Translate(transform.right*speed*Time.deltaTime,Space.World);
		if (transform.localPosition.x > position_max && last.transform.localPosition.x<position_max)
		{
			transform.localPosition = last.transform.localPosition - new Vector3(width, 0, 0);
		}
	}
}
