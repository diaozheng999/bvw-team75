using System.Collections;
using PGT.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Team75.Server {

    public class ScoreManager : Singleton<ScoreManager> {
        uint[] scores;

        [SerializeField] Text[] scoreDisplays;
        //--------INSERT----------
        [SerializeField] private Vector2[] positions;

        [SerializeField] private GameObject smallscore;
        [SerializeField] private float y_min;
        [SerializeField] private float speed;
        private uint[] scores_delayed;
        //---------END-------------------
        

        void Start(){
            scores = new uint[2];
            scores[0] = 0;
            scores[1] = 0;
            //--------------INSERT---------------
            scores_delayed = new uint[2];
            scores[0] = 0;
            scores[1] = 0;
            //------------END----------------
            Reset();
        }

        public void Reset(){
            scores[0] = 0;
            scores[1] = 0;
            scoreDisplays[0].text = "$ 0";
            scoreDisplays[1].text = "$ 0";
        }

        public uint AddValue(byte playerId, uint score)
        {
            Debug.LogError("Executing this.");

            scores[playerId] += score;
            Debug.LogError("Executing this too.");
            //scoreDisplays[playerId].text = "$ " + scores[playerId];
            StartCoroutine(falltoadd(null, score, playerId));
            Debug.LogError("Heya~~~");
            return scores[playerId];
        }

        private IEnumerator falltoadd(GameObject addscore, uint score, int id)
        {
            GameObject newscore = Instantiate(smallscore);
            newscore.transform.parent = transform;
            newscore.transform.localPosition=new Vector3(0,0,0);
            newscore.transform.localScale=new Vector3(1,1,1);

            newscore.GetComponent<Text>().text = "$ " + score.ToString();
            newscore.GetComponent<RectTransform>().anchoredPosition = positions[id];

            while (newscore.GetComponent<RectTransform>().anchoredPosition.y > y_min)
            {
                yield return new WaitForSecondsRealtime(0.05f);
                newscore.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0,0.05f * speed);
            }
            yield return null;
            scores_delayed[id] += score;
            scoreDisplays[id].text = "$ "+scores_delayed[id];
            Destroy(newscore);
        }

        void Update()
        {
            
        }

    }

}