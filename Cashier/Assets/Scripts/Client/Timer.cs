﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PGT.Core;

namespace Team75.Client
{




	public class Timer : Singleton<Timer>
	{
		private Text _text;
		private Image _image;
		
		private const float width_max=1500f;
		private bool started = false;

		float maxTime = 120f;
		

		public void StartGame(int playerID)
		{
			started = true;
			_text = GetComponentInChildren<Text>();
			_image = GetComponentInChildren<Image>();
			_text.color = Color.green;
			_image.color = Color.green;
			if (playerID != 0)
			{
				GetComponent<RectTransform>().anchoredPosition=new Vector2(0.51f,0f);
				GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, -90, 0);
			}
			
		}

		public void SetMaxTime(float _maxTime) {
			maxTime = _maxTime;
		}

		// Update is called once per frame
		void Update()
		{
			if (started)
			{
				var currTime = ScoreManager.instance.GetTime() / maxTime;

				_text.text = ScoreManager.instance.ParseTime();
				_image.GetComponent<RectTransform>().sizeDelta =
					new Vector2(width_max * currTime, 200);

				if (currTime < 0.2f)
				{
					_text.color = Color.red;
					_image.color = Color.red;
				}
				else
				{
					_text.color = Color.green;
					_image.color = Color.green;
				}
			}
			else if(GameStateManager.instance.gameStarted)
			{
				started = true;
				StartGame(GameStateManager.instance.GetPlayerId());
			}
			
			
			
		}
	}
}