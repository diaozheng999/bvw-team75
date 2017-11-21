using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSound : MonoBehaviour
{

	[SerializeField] private AudioClip[] audios;
	[SerializeField] private bool[] loop;

	private AudioSource audioSource;
	// Use this for initialization
	void Start ()
	{
		audioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// 0: generation; 
	// 1: grab;  done
	// 2: release; 
	// 3: scan
	public void PlaySound(int index)
	{
		audioSource.Stop();
		if (audios.Length>index && loop.Length>index &&  audios[index] != null)
		{
			audioSource.clip = audios[index];
			audioSource.loop = loop[index];
			audioSource.Play();
		}
	}
}
