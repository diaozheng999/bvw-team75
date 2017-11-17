using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Test_Lotus : MonoBehaviour
{

	public float speed = 5;
	public float rotatespeed = 50;
	public Transform supermarket;
	private int takebody = 0;
	private Transform bodytook;
	public GameObject obj;

	//------TEST------------

	// Use this for initialization
	void Start()
	{
		StartCoroutine(DelayDestroy(obj));
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			//print(0);
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 10))
			{
				if (hit.transform.tag == "Player")
				{
					if (takebody != 0)
					{
						bodytook.parent = supermarket;

					}
					takebody = 1;
					transform.position = hit.transform.position;
					transform.rotation = hit.transform.rotation;
					hit.transform.parent = transform;
					bodytook = hit.transform;

				}
				if (hit.transform.tag == "ScannerBox")
				{
					hit.transform.GetComponent<Team75.Client.ScannerBox_Client>().pickput(1);
				}
				if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Item"))
				{
					Debug.Log("test pick up item");
				}
				if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Barcode"))
				{
					Debug.Log("test scan barcode");
				}
			}

		}
		if (Input.GetMouseButtonDown(1))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 10))
			{
				if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Item"))
				{
					Debug.Log("test release item");
				}
			}
		}
		if (Input.GetMouseButtonDown(2))
		{

		}
		if (Input.GetKey(KeyCode.UpArrow))
		{
			transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
		}
		if (Input.GetKey(KeyCode.DownArrow))
		{
			transform.Translate(-transform.forward * speed * Time.deltaTime, Space.World);
		}
		if (Input.GetKey(KeyCode.RightArrow))
		{
			transform.Rotate(transform.up, rotatespeed * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			transform.Rotate(transform.up, -rotatespeed * Time.deltaTime);
		}

	}

	IEnumerator DelayDestroy(GameObject obj)
	{
		yield return new WaitForSeconds(4);
		Destroy(obj);
	}


}
