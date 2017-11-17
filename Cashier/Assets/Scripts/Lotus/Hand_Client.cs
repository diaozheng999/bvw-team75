using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand_Client : MonoBehaviour {

	//---------INPUT-------------
	
	//--------OUTPUT--------------
	public bool scanner_in_hand=false;
	public bool item_in_hand=false;
	public Transform item;
	//----------PRIVATE------------
	
	//-------------TEST-------------
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public bool grabscanner()
	{
		if (scanner_in_hand || item_in_hand)
		{
			Debug.Log("multi grab");
			return false;
		}
		else
		{
			scanner_in_hand = true;
			return true;
		}
	}

	public bool releasescanner()
	{
		if (scanner_in_hand)
		{
			scanner_in_hand = false;
			return true;
		}
		else
		{
			return false;
		}
	}
}
