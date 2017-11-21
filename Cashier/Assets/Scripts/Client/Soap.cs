using System.Collections;
using System.Collections.Generic;
using Team75.Client;
using UnityEngine;

public class Soap : MonoBehaviour
{

	public bool held=false;
	public OVRInput.Axis1D depth;
	public Grab grabscript;

	[SerializeField] private float speed_jump=1.2f;
	[SerializeField] private float force=0.9f;

	//[SerializeField] private float speed_slide;

	private Vector3 speed_move;
	private Vector3 position_last;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		speed_move = (transform.position - position_last) / Time.deltaTime;
		if (speed_move.magnitude < 1) speed_move *= (1 / speed_move.magnitude);
		
		
		position_last = transform.position;
		if (held || Input.GetKeyDown(KeyCode.R))
		{
			if (OVRInput.Get(depth) > force || Input.GetKeyDown(KeyCode.R)) 
			{
                //Debug.LogError("Too Tight");
				Release();
				//grabscript.cleansoap();
				 //Vector3.Normalize(transform.position - grabscript.transform.position) * 0.3f;
				grabscript.ClearSoap();
			}
		}
	}

	public void Set(OVRInput.Axis1D d, Grab g)
	{
        //Debug.LogError("Set Soap");
		held = true;
		depth = d;
		grabscript = g;
	}

	public void Release()
	{
		held = false;
	}

    public void Jump()
    {
	    var source = GetComponent<ItemSound>();
	    if (source != null)
	    {
		    source.PlaySound(2);
	    }
        GetComponent<Rigidbody>().velocity = speed_move +
                    new Vector3(0, speed_jump, 0);
    }
}
