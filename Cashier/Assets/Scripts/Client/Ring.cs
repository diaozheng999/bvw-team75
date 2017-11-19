using System.Collections;
using System.Collections.Generic;
using Team75.Client;
using UnityEngine;

public class Ring : MonoBehaviour
{

	[SerializeField] private float emis;
	private Renderer rend;

	private Color finalColor;

	private bool able=false;
	// Use this for initialization
	void Start ()
	{
		rend = GetComponent<Renderer>();
		finalColor = new Color(0.4f,0f,0f,0f) * emis;
		
	}
	
	// Update is called once per frame
	void Update () {
		
		//rend.material.SetColor("_EmissionColor",new Color(Time.time-Mathf.Floor(Time.time),0,0,1));
	}

	
	public void SetAble(bool a)
	{
		able = a;
		if (able)
		{
			rend.material.SetColor("_EmissionColor", finalColor);

		}
		else
		{
			rend.material.SetColor("_EmissionColor",Color.black);

		}
	}

	public void GetHit()
	{
		transform.parent.GetComponent<Animator>().SetTrigger("PLAY");
		GameStateManager.instance.RequestCustomer();
	}
}
