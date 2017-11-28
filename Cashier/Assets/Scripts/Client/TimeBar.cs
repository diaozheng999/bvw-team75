using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PGT.Core;

namespace Team75.Client
{

	


	public class TimeBar : Singleton<TimeBar>

	{
		
		
		private Text _text;
		private Image _image;
		private Image _frame;
		private Image _credits;
		
		private const float width_max=1500f;
		private bool started = false;

		float maxTime = 150f;
		

		public void StartGame(int playerID)
		{
			started = true;
			_text = GetComponentInChildren<Text>();
			Image[] tmp = GetComponentsInChildren<Image>();
			_image = tmp[0];
			_frame = tmp[1];
			_credits = tmp[2];
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

		public void ResetMaxTime() {
			maxTime = ScoreManager.instance.GetTime();
		}

		// Update is called once per frame
		void Update()
		{
			if (started)
			{
				var currTime = Mathf.Clamp01(ScoreManager.instance.GetTime() / maxTime);

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

		public void ShowCredits()
		{
			_image.gameObject.active = false;
			_frame.gameObject.active = false;
			_text.gameObject.active = false;
			_credits.gameObject.active = true;
		}
	}
}