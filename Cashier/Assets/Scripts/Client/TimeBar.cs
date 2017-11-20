using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Team75.Client
{




	public class TimeBar : MonoBehaviour
	{
		private Text _text;
		private Image _image;
		
		private const float width_max=1500f;
		private bool started = false;
		

		// Use this for initialization
		void Start()
		{
			
		}

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

		// Update is called once per frame
		void Update()
		{
			if (started)
			{
				_text.text = ScoreManager.instance.ParseTime();
				_image.GetComponent<RectTransform>().sizeDelta =
					new Vector2(width_max * ScoreManager.instance.GetTime() / 180, 200);

				if (ScoreManager.instance.GetTime() < 20)
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