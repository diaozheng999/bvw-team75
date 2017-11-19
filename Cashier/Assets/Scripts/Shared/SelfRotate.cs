using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfRotate : MonoBehaviour
{

	[SerializeField] private float speed_rotate;

	[SerializeField] private bool clockwise;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(transform.up,speed_rotate*Time.deltaTime*(clockwise?1:-1));
	}
}
