using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanLight : MonoBehaviour {

	// Use this for initialization
	void Awake()
	{
		GetComponent<Light>().shadowNearPlane = 0.000000001f;

	}
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
