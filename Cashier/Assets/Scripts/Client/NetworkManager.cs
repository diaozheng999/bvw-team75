using System.Net;
using System.Collections;
using System.Collections.Concurrent;
using PGT.Core;
using PGT.Core.Networking;
using UnityEngine;
using UnityEngine.UI;
using Team75.Shared;

namespace Team75.Client {

    public class NetworkManager : Singleton<NetworkManager> {

        [SerializeField] InputField ipAddressString;
        [SerializeField] Button connectP1;
        [SerializeField] Button connectP2;
        TcpClient client;

        volatile bool gameStarted = false;
        volatile int playerId = -1;

        IPAddress server_address;

        ConcurrentQueue<System.Action<ushort>> trackingIdRequests;

        void Start() {
            connectP1.onClick.AddListener(() => {
                DisableInteractions();
                Connect(server_address, Connection.TCP_SERVER_PORT_1);
            });
            
            connectP2.onClick.AddListener(() => {
                DisableInteractions();
                Connect(server_address, Connection.TCP_SERVER_PORT_2);
            });
            StartCoroutine(StartGame());
            trackingIdRequests = new ConcurrentQueue<System.Action<ushort>>();
        }

        IEnumerator StartGame() {
            yield return new WaitUntil(() => gameStarted);
            GameStateManager.instance.StartGame(playerId);
        }



        void DisableInteractions() {
            connectP1.interactable = false;
            connectP2.interactable = false;
            ipAddressString.interactable = false;
            server_address = IPAddress.Parse(ipAddressString.text);
        }

        void Connect(IPAddress addr, int port) {
            client = new TcpClient(addr, port);
            client.AddParser(Connection.SET_PLAYER_ID, OnSetPlayerId, "SET_PLAYER_ID");
            client.AddParser(Connection.SET_OPPONENT, OnSetOpponent, "SET_OPPONENT");
            client.AddParser(Connection.ENQUEUE_CUSTOMER, OnEnqueueCustomer, "ENQUEUE_CUSTOMER");
            client.AddParser(Connection.CUSTOMER_RESPONSE, OnDequeueCustomer, "CUSTOMER_RESPONSE");
            client.AddParser(Connection.TRACKING_ID_RESPONSE, OnReceiveTrackingId, "TRACKING_ID_RESPONSE");
            client.AddParser(Connection.TRACKING_ID_PURGE, OnPurgeTrackingId, "TRACKING_ID_PURGE");
            client.AddParser(Connection.SPAWN_ITEM, OnAddTrackableItem, "SPAWN_ITEM");
            client.AddParser(Connection.CUSTOMER_LEAVE, OnCustomerLeave, "CUSTOMER_LEAVE");
            client.AddParser(Connection.CUSTOMER_FINISH_ITEMS, OnCustomerFinishItems, "CUSTOMER_FINISH_ITEMS");
            client.AddParser(Connection.SCORE_SET, OnSetScore, "SCORE_SET");
            client.AddParser(Connection.START_GAME, OnStartGame, "START_GAME");
            client.AddParser(Connection.END_GAME, OnStopGame, "END_GAME");
            client.AddParser(Connection.TIMER_SYNC, OnTimerSync, "TIMER_SYNC");
            client.AddParser(Connection.SYNC_BUTTON, OnSetButton, "SET_BUTTON");
            client.AddParser(Connection.FRENZY_START, OnFrenzyStart, "FRENZY_START");
            AddDisposable(client);
        }

        void OnSetPlayerId (byte[] buffer, ushort length) {
            // we don't care about the length since it's always one byte
            playerId = buffer[0];
            gameStarted = true;
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                NetworkPositionManager.instance.AddEndpoint(new IPEndPoint(server_address, Connection.UDP_SERVER_PORT), Connection.UDP_SERVER_PORT);
            });
        }

        void OnSetScore(byte[] buffer, ushort length) {
            var score = Connection.UnpackScore(buffer, 0);
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                if(score.Item1 != (byte)GameStateManager.instance.GetPlayerId()){
                    GameStateManager.instance.PlayBeep(score.Item1);
                    ScoreManager.instance.SetOpponentScore(score.Item2);
                }
            });
        }

        void OnSetOpponent(byte[] buffer, ushort length) {
            var opponent = Connection.UnpackOpponent(buffer, (int)length);
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                int remotePort;
                int localPort;
                switch (opponent.Item1) {
                    case 0:
                        localPort = Connection.UDP_PEER_PORT_1;
                        remotePort = Connection.UDP_PEER_PORT_2;
                        break;
                    default:
                        localPort = Connection.UDP_PEER_PORT_2;
                        remotePort = Connection.UDP_PEER_PORT_1;
                        break;
                    
                }
                NetworkPositionManager.instance.AddEndpoint(new IPEndPoint(opponent.Item2, remotePort), localPort);
            });
        }

        void OnCustomerFinishItems(byte[] buffer, ushort length) {
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                VisibleCustomerQueue.instance.GetActiveCustomer(GameStateManager.instance.GetOpponentPlayerId()).OnAfterItems();
            });
        }

        public void SendScoreUpdateMessage(uint delta) {
            var buffer = Connection.PackScore((byte)GameStateManager.instance.GetPlayerId(), delta);
            client.SendMessageInBackground(Connection.SCORE_ADD_ITEM, buffer);
        }

        public void RequestCustomer() {
            client.SendMessageInBackground(Connection.CUSTOMER_REQUEST, new byte[0]{});
        }

        public void SendCustomerFinishItems() {
            client.SendMessageInBackground(Connection.CUSTOMER_FINISH_ITEMS, new byte[0]{});
        }

        //---------INSERT---------------
        public void SetButton(int state)
        {
            var buffer = Connection.PackButton((byte)GameStateManager.instance.GetPlayerId(),state);
            client.SendMessageInBackground(Connection.UPDATE_BUTTON,buffer);
        }

        public void OnSetButton(byte[] buffer, ushort length)
        {
            var state = Connection.UnpackButton(buffer, 0);
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                if(state.Item1 != (byte)GameStateManager.instance.GetPlayerId()){
                    GameStateManager.instance.SetButtonState(state.Item2);
                }
            });
        }
        //---------END

        public void OnCustomerLeave(byte[] buffer, ushort length) {
            var playerId = (int) buffer[0];
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                VisibleCustomerQueue.instance.CustomerLeave(playerId, () => {});
            });
        }

        public void RequestTrackingId(System.Action<ushort> response) {
            trackingIdRequests.Enqueue(response);
            client.SendMessageInBackground(Connection.TRACKING_ID_REQUEST, new byte[0]{});
        }

        public void AddTrackableItem(ushort id, short type, Vector3 position, Quaternion rotation) {
            client.SendMessageInBackground(
                Connection.SPAWN_ITEM,
                Connection.PackTrackableItem(id, type, position, rotation)
            );
        }

        public void OnAddTrackableItem(byte[] buffer, ushort length) {
            var result = Connection.UnpackTrackableItem(buffer, 0);
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                TrackableItemPlacer.instance.AddTrackable(
                    result.Item1, result.Item2, result.Item3, result.Item4
                );
            });
        }

        public void ReleaseTrackingId(ushort id) {
            client.SendMessageInBackground(Connection.TRACKING_ID_DESTROY, Connection.PackTrackingId(id));
        }

        void OnReceiveTrackingId(byte[] buffer, ushort length) {
            var trackingId = Connection.UnpackTrackingId(buffer, 0);
            System.Action<ushort> cont;
            if (trackingIdRequests.TryDequeue(out cont)) {
                cont(trackingId);
            } else {
                Debug.LogError("NetworkManager: Receiving more tracking ID responses than requests. Error?");
            }
        }

        void SendTrackingIdPurged(byte id){
            client.SendMessageInBackground(Connection.TRACKING_ID_PURGED, new byte[1]{id});
        }
        public void SendCustomerLeave(int playerId) {
            client.SendMessageInBackground(Connection.CUSTOMER_LEAVE, new byte[1]{(byte)playerId});
        }

        void OnPurgeTrackingId(byte[] buffer, ushort length) {
            var id = buffer[0];
            TrackableItemPlacer.instance.RemoveTrackable(id, () => SendTrackingIdPurged(id));
        }

        void OnEnqueueCustomer(byte[] buffer, ushort length) {
            var cust = Connection.UnpackCustomer(buffer, 0);
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                VisibleCustomerQueue.instance.Enqueue(cust);
            });
        }

        void OnDequeueCustomer(byte[] buffer, ushort length) {
            var unpacked = Connection.UnpackCustomerDequeueInfo(buffer, 0);
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                Shared.Avatar avatar = new Shared.Avatar();
                VisibleCustomerQueue.instance.Dequeue(unpacked.Item1, ref avatar); 
                if(unpacked.Item1 == GameStateManager.instance.GetPlayerId()) {
                    GameStateManager.instance.UnsetCallable();
                    var itemPlacer = avatar.gameObject.AddComponent<ItemPlacer>();
                    itemPlacer.SetAvatar(avatar, unpacked.Item2);

                    Statics.instance.GenerateCustomer(avatar.GetID());


                    if(avatar.IsScannable()) {
                        var sa = avatar.gameObject.AddComponent<ScannableAvatar>();
                    }
                }
            });
        }

        void OnStartGame(byte[] buffer, ushort length) {
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                GameStateManager.instance.gameStarted = true;
                BackgroundMusic.instance.StartGame();
            });
        }

        void OnStopGame(byte[] buffer, ushort length) {
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                GameStateManager.instance.gameStarted = false;
                GameStateManager.instance.StopFrenzy();
            });
        }

        void OnTimerSync(byte[] buffer, ushort length) {
            var timer = System.BitConverter.ToSingle(buffer, 0);
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                ScoreManager.instance.SetTime(timer);
            });
        }

        void OnFrenzyStart(byte[] buffer, ushort length) {
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                GameStateManager.instance.StartFrenzy();
            });
        }

    }
}