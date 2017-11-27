using System.Net;
using PGT.Core;
using PGT.Core.Networking;
using UnityEngine;
using UnityEngine.UI;
using Team75.Shared;

namespace Team75.Client {

    public class GameStateManager : Singleton<GameStateManager> {

        [SerializeField]
        float OVRHeight = 1;

        [SerializeField]
        Shared.Avatar[] Players;

        [SerializeField]
        Shared.Scanner[] scanners;

        [SerializeField]
        Transform[] ScorePlacements;

        [SerializeField] private Ring ring0;
        [SerializeField] private Ring ring1;


        [SerializeField] float FrenzySkewTolerance;
        [SerializeField] float FrenzyFaceTolerance;

        [SerializeField] GameObject[] frenzyIndicators;
        [SerializeField] GameObject announcement;

        [SerializeField] FadeOut[] tutorials;
        

        private Ring ring;

        int revenue;
        int opponentRevenue;

        int myPlayerId;

        public bool gameStarted = false;

        [SerializeField] bool callable = true;

        FrenzyItemPlacer fip;

        public void StartGame(int playerId) {
            Debug.Log("GameStateManager: starting game as player "+playerId);

            Statics.instance.StartGame(playerId);
            
            
            myPlayerId = playerId;
            ring = playerId == 0 ? ring0 :  ring1;

            var _pos = Players[playerId].transform.position;
            var _rot = Players[playerId].transform.rotation;

            var headDelta = OVRManager.instance.GetHeadDelta() * Vector3.down;
            OVRManager.instance.transform.position = _pos + OVRHeight * Vector3.up;
            OVRManager.instance.transform.rotation = _rot * Quaternion.Euler(headDelta);

            Destroy(Players[playerId].gameObject);
            OVRManager.instance.StartBroadcasting(playerId);
            Hand.Right.SetAsScanner();

            scanners[playerId].gameObject.SetActive(false);
            Players[(playerId+1) % 2].StartTracking();
            scanners[(playerId+1) % 2].StartTracking();

            ScoreManager.instance.transform.position = ScorePlacements[playerId].transform.position;
            ScoreManager.instance.transform.rotation = ScorePlacements[playerId].transform.rotation;

            ScoreManager.instance.Reset();

            Scanner.instance.StartBroadcasting();
            


            ring.SetAble(callable);
        }

        public void FadeOutTutorial() {
            announcement.SetActive(false);
            foreach (var item in tutorials) {
                item.Execute();
            }
        }

        public void SetAnnouncement() {
            announcement.SetActive(true);
        }
        
        public void PlayBeep(int player) {
            scanners[player].StartCoroutine1(scanners[player].scan_success());
        }

        //--------INSERT-23:38,11-16--------------
        public void SetButtonState(int state)
        {
            var oppositering = myPlayerId == 0 ? ring1 : ring0;
            oppositering.SetAble(state!=0);
        }
        //-----------END_-------------------------

        public void SetCallable() {
            callable = true;
            ring.SetAble(callable);
            NetworkManager.instance.SetButton(1);
        }

        public void UnsetCallable() {
            callable = false;
            ring.SetAble(callable);
            NetworkManager.instance.SetButton(0);
        }

        public void RequestCustomer() {
            if(!gameStarted || !callable) return;
            UnsetCallable();
            if(VisibleCustomerQueue.instance.HasActiveCustomer(myPlayerId)) {
                NetworkManager.instance.SendCustomerLeave(myPlayerId);
                var _avatar = VisibleCustomerQueue.instance.GetActiveCustomer(myPlayerId);
                var _ip = _avatar.GetComponent<ItemPlacer>();
                _ip.Cleanup();
                VisibleCustomerQueue.instance.CustomerLeave(myPlayerId, NetworkManager.instance.RequestCustomer);
            } else {
                NetworkManager.instance.RequestCustomer();
            }
        }

        void Update() {
            
            if (Input.GetKeyDown(KeyCode.A)) {
                RequestCustomer();
            }
            
            if (OVRInput.GetDown(OVRInput.Button.One)){
                RequestCustomer();
            }
        }

        public int GetPlayerId(){
            return myPlayerId;
        }

        public int GetOpponentPlayerId() {
            return 1-myPlayerId;
        }

        public void StartFrenzy() {
            UnsetCallable(); // make sure it's never callable
            
            // cleanup customers
            VisibleCustomerQueue.instance.CustomerLeaveIfActive(GetOpponentPlayerId());
            if(VisibleCustomerQueue.instance.HasActiveCustomer(myPlayerId)) {
                var _avatar = VisibleCustomerQueue.instance.GetActiveCustomer(myPlayerId);
                var _ip = _avatar.GetComponent<ItemPlacer>();
                _ip.Cleanup();
                VisibleCustomerQueue.instance.CustomerLeave(myPlayerId, () => {});
            }

            // spawn santa
            var santa = VisibleCustomerQueue.instance.SpawnSanta();
            fip = santa.gameObject.AddComponent<FrenzyItemPlacer>();
            fip.SetAvatar(santa, myPlayerId);

            BackgroundMusic.instance.StartFrenzy();

            Scanner.instance.FrenzyTolerance(FrenzyFaceTolerance, FrenzySkewTolerance);
            // TODO: stop hardcoding frenzy times, but frankly I don't care :)
            ScoreManager.instance.SetTime(23f);
            TimeBar.instance.SetMaxTime(23f); 

            foreach(var frenzyInd in frenzyIndicators) {
                frenzyInd.SetActive(true);
            }
        }

        public void StopFrenzy() {
            fip.Cleanup();
            VisibleCustomerQueue.instance.SantaLeave(() => {
                BackgroundMusic.instance.StopGame();
            });
            foreach(var frenzyInd in frenzyIndicators) {
                frenzyInd.SetActive(false);
            }
        }


        public void SetStats(GameStat stat) {
            // pass
        }

    }
    
    
}