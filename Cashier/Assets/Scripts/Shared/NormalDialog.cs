using System;
using System.Collections;
using Team75.Client;
using UnityEngine;


namespace Team75.Shared.SpecialEffects {

	public class NormalDialog : SpecialEffect
	{

		
		
		public AudioClip AudioClip1;
		public AudioClip AudioClip2;
		public AudioClip AudioClip3;

		private AudioSource _audio;
		private AudioSource audio
		{
			get
			{
				if (_audio == null)
				{
					_audio = GetComponent<AudioSource>();
					
				}
				return _audio;
			}
		}

		private bool end = false;

		
		

		public NormalDialog() : base() {
			executionFlag = ExecutionFlag.AFTER_DEQUEUE | ExecutionFlag.BEFORE_LEAVE;
		}

		protected override void AfterDequeue(Action cont) => StartCoroutine(AfterDequeueCoroutine(cont));
//			
//
		protected override void BeforeLeave(Action cont)
		{
			if (AudioClip3 != null)
			{
				audio.clip = AudioClip3;
				audio.Play();
			}
			end = true;
			cont();
		}

//		IEnumerator BeforeLeaveCoroutine(Action cont) {
//			
//			cont();
//		}
//
		IEnumerator AfterDequeueCoroutine(Action cont)
		{
			cont();
			if (AudioClip1 != null)
			{
				audio.clip = AudioClip1;
				audio.Play();
			}
			yield return new WaitForSecondsRealtime(3.5f);
			if (!end && AudioClip2!=null)
			{
				audio.clip = AudioClip2;
				audio.Play();
			}

		}

		

	}

}