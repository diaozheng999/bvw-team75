using PGT.Core;
using Team75.Shared;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;


namespace Team75.Server {

    public class GameStateManager : Singleton<GameStateManager> {

        [SerializeField] Shared.Avatar[] avatars;

        [SerializeField] Scanner[] scanners;

        [SerializeField] Transform[] customerPositions;

        [SerializeField] float totalTime;
        [SerializeField] float frenzyTime;

        [SerializeField] Text countDown;

        [SerializeField] private Ring ring0;

        [SerializeField] private Ring ring1;

        bool started = false;
        bool frenzy = false;
        bool ended = false;

        GameStat[] stats = new GameStat[2];
        int statsSet = 0;

        public void StartTracking(int player) {
            avatars[player].StartTracking();
            scanners[player].StartTracking();
        }

        public void StartGame() {
            NetworkManager.instance.SendStartGameMessage();
            CustomerQueue.instance.StartAccepting();
            started = true;
            BackgroundMusic.instance.StartGame(() => countDown.gameObject.SetActive(true));
            StartCoroutine(SendSyncMessages());
            NetworkManager.instance.StartGame();
        }

        public void PlayBeep(int player) {
            scanners[player].StartCoroutine1(scanners[player].scan_success());
        }

        IEnumerator SendSyncMessages() {
            var wfs = new WaitForSeconds(1);
            while(!ended) {
                NetworkManager.instance.SendMessageToBoth(Connection.TIMER_SYNC, System.BitConverter.GetBytes(totalTime));
                yield return wfs;
            }
        }


        void Update() {
            if (ended) return;

            if(Input.GetKeyUp(KeyCode.Return)) {
                CustomerQueue.instance.Enqueue(CustomerQueue.GenerateRandom());
            }

            if(Input.GetKeyUp(KeyCode.J)) {
                CustomerQueue.instance.Enqueue(CustomerQueue.GetSpecific(7)); //joker
            }

            if(Input.GetKeyUp(KeyCode.H)) {
                CustomerQueue.instance.Enqueue(CustomerQueue.GetSpecific(6, 2000)); //hitman
            }

            if(Input.GetKeyUp(KeyCode.F)) {
                totalTime = 0;
            }

            if(Input.GetKeyUp(KeyCode.LeftArrow)) {
                CustomerQueue.instance.Dequeue(0);
            }

            if(Input.GetKeyUp(KeyCode.RightArrow)) {
                CustomerQueue.instance.Dequeue(1);
            }

            if(!started) return;

            totalTime -= Time.deltaTime;
            if(totalTime <= 0 && !ended) {

                if (!frenzy) {
                    totalTime = frenzyTime;
                    NetworkManager.instance.SendMessageToBoth(Connection.FRENZY_START, new byte[0]{});
                    CustomerQueue.instance.StopCustomerSpawns();
                    VisibleCustomerQueue.instance.CustomerLeaveIfActive(0);
                    VisibleCustomerQueue.instance.CustomerLeaveIfActive(1);
                    VisibleCustomerQueue.instance.SpawnSanta();
                    frenzy = true;
                    BackgroundMusic.instance.StartFrenzy();
                    
                } else {
                    totalTime = 0;
                    ended = true;
                    NetworkManager.instance.SendMessageToBoth(Connection.END_GAME, new byte[0]{});
                    VisibleCustomerQueue.instance.SantaLeave(() => {
                        BackgroundMusic.instance.StopGame();
                    });
                }
            }
            countDown.text = ParseTime();
        }

        string ParseTime() {
            var mins = Mathf.FloorToInt(totalTime / 60);
            var secs = Mathf.FloorToInt(totalTime % 60);
            return string.Format("{0}:{1}", mins.ToString("D2"), secs.ToString("D2"));
        }
        
        //------------INSERT--------------
        public void SetButtonState(int playerID,int buttonstate)
        {
            if(playerID==0) ring0.SetAble(buttonstate!=0);
            else ring1.SetAble(buttonstate!=0);
        }

        
        //--------END_--------------------

        
        public void SetGameStat(int playerId, GameStat stat) {
            stats[playerId] = stat;
            statsSet++;
            if(statsSet == 2) {
                Messenger.SendMessage<GameStat[]>("stats", stats);
                SceneManager.LoadScene("Tally");
            }
        }

    }

}