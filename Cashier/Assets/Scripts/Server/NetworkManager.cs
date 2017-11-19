using System;
using System.Net;
using PGT.Core;
using PGT.Core.Networking;
using UnityEngine;
using UnityEngine.UI;
using Team75.Shared;

namespace Team75.Server {

    public class NetworkManager : Singleton<NetworkManager> {
        public const int NUM_PLAYERS = 2;

        [SerializeField] GameObject AwaitingCanvas;
        [SerializeField] Text ListeningIps;
        [SerializeField] Text ConnectedClients;

        [SerializeField] InputField webSocketAddress;
        [SerializeField] Button startGame;


        TcpServer[] servers;
        TcpClient remoteClient;

        int connected_players = 0;

        IPAddress[] remoteAddress;

        void Start() {
            servers = new TcpServer[NUM_PLAYERS];
            servers[0] = new TcpServer(Connection.TCP_SERVER_PORT_1, OnConnect(0));
            servers[1] = new TcpServer(Connection.TCP_SERVER_PORT_2, OnConnect(1));

            remoteAddress = new IPAddress[NUM_PLAYERS];

            for(int i=0; i<NUM_PLAYERS; ++i) {
                remoteAddress[i] = IPAddress.Parse("127.0.0.1");
            }

            
            foreach(var ip in servers[0].GetLocalAddresses()) {
                ListeningIps.text += string.Format("\n{0}", ip);
            }
            ConnectedClients.text = "";
            NetworkPositionManager.instance.AddEndpoint(new IPEndPoint(IPAddress.Any, Connection.UDP_SERVER_PORT), Connection.UDP_SERVER_PORT);

            for(int i=0; i<NUM_PLAYERS; ++i) {
                servers[i].AddParser(Connection.CUSTOMER_REQUEST, RequestCustomer(i), "CUSTOMER_REQUEST");
                servers[i].AddParser(Connection.TRACKING_ID_REQUEST, RequestTrackingId(i), "TRACKING_ID_REQUEST");
                servers[i].AddParser(Connection.SPAWN_ITEM, SpawnObject(i), "SPAWN_ITEM");
                servers[i].AddParser(Connection.TRACKING_ID_DESTROY, ReleaseTrackingId(i), "TRACKING_ID_DESTROY");
                servers[i].AddParser(Connection.TRACKING_ID_PURGED, PurgeTrackingId, "TRACKING_ID_PURGED");
                servers[i].AddParser(Connection.CUSTOMER_LEAVE, CustomerLeave(i), "CUSTOMER_LEAVE");
                servers[i].AddParser(Connection.CUSTOMER_FINISH_ITEMS, OnCustomerFinishItems(i), "CUSTOMER_FINISH_ITEMS");
                servers[i].AddParser(Connection.SCORE_ADD_ITEM, AddLineItem(i), "SCORE_ADD_ITEM");
                servers[i].AddParser(Connection.UPDATE_BUTTON, UpdateButton(i), "UPDATE_BUTTON");
                AddDisposable(servers[i]);
            }

            startGame.onClick.AddListener(GameStateManager.instance.StartGame);
        }

        public void StartGame() {
            
            AwaitingCanvas.SetActive(false);
            remoteClient = new TcpClient(IPAddress.Parse(webSocketAddress.text), Connection.WEB_SERVER_PORT);
            remoteClient.AddParser(Connection.REMOTE_CUSTOMER, OnRemoteCustomer, "REMOTE_CUSTOMER");
            OnComplete();
            AddDisposable(remoteClient);
        }


        /// [Not guaranteed to run on main thread]
        void OnComplete() {

            servers[0].SendMessageInBackground(Connection.SET_OPPONENT, Connection.PackOpponent((byte)1,remoteAddress[1]));
            servers[1].SendMessageInBackground(Connection.SET_OPPONENT, Connection.PackOpponent((byte)0,remoteAddress[0]));

        }

        /// [Not guaranteed to run on main thread]
        Action<IPAddress> OnConnect(int playerId) => (IPAddress radd) => {
            servers[playerId].SendMessageInBackground(Connection.SET_PLAYER_ID, new byte[1]{(byte)playerId});
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                remoteAddress[playerId] = radd;
                connected_players++;
                ConnectedClients.text += string.Format("\nClient connected, Remote IP: {0}", radd);
                GameStateManager.instance.StartTracking(playerId);
                ScoreManager.instance.Reset();
                if(connected_players >= 2) {
                    //OnComplete();
                }
            });
        };



        public void SendMessageToBoth(byte message, byte[] payload) {
            if(UnityEngine.Random.value < 0.5f){
                servers[0].SendMessageInBackground(message, payload);
                servers[1].SendMessageInBackground(message, payload);
            } else {
                servers[1].SendMessageInBackground(message, payload);
                servers[0].SendMessageInBackground(message, payload);
            }
        }

        void OnRemoteCustomer(byte[] buffer, ushort len) {
            var customer = Connection.UnpackCustomer(buffer, 0);
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                CustomerQueue.instance.Enqueue(customer);
            });
        }

        public void SendStartGameMessage() {
            SendMessageToBoth(Connection.START_GAME, new byte[0]{});
            ScoreManager.instance.Reset();
        }

        Action<byte[], ushort> RequestCustomer(int playerId) => (byte[] buffer, ushort len) => {
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                CustomerQueue.instance.Dequeue(playerId);
            });
        };

        Action<byte[], ushort> RequestTrackingId(int playerId) => (byte[] buffer, ushort len) => {
            var id = TrackingIdManager.instance.GetUnusedId();
            servers[playerId].SendMessageInBackground(Connection.TRACKING_ID_RESPONSE, new byte[1]{id});
        };

        Action<byte[], ushort> SpawnObject(int playerId) => (byte[] buffer, ushort len) => {
            var nb = new byte[len];
            Array.Copy(buffer, nb, len);
            servers[(playerId+1)%2].SendMessageInBackground(Connection.SPAWN_ITEM, nb);
            var result = Connection.UnpackTrackableItem(buffer, 0);
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                TrackableItemPlacer.instance.AddTrackable(
                    result.Item1, result.Item2, result.Item3, result.Item4
                );
            });
        };

        Action<byte[], ushort> CustomerLeave(int playerId) => (byte[] buffer, ushort len) => {
            var targetPlayerId = buffer[0];
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                VisibleCustomerQueue.instance.CustomerLeave(targetPlayerId, () => {});
                servers[(playerId+1)%2].SendMessageInBackground(Connection.CUSTOMER_LEAVE, new byte[1]{targetPlayerId});
            });
        };
        
        Action<byte[], ushort> ReleaseTrackingId(int playerId) => (byte[] buffer, ushort len) => {
            var id = buffer[0];
            TrackableItemPlacer.instance.RemoveTrackable(id, () => TrackingIdManager.instance.FreeId(id));
            servers[(playerId+1)%2].SendMessageInBackground(Connection.TRACKING_ID_PURGE, new byte[1]{id});
        };

        Action<byte[], ushort> AddLineItem(int playerId) => (byte[] buffer, ushort len) => {
            var scoreDelta = Connection.UnpackScore(buffer, 0);
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                GameStateManager.instance.PlayBeep(playerId);
                var n_score = ScoreManager.instance.AddValue(scoreDelta.Item1, scoreDelta.Item2);
                servers[(playerId+1)%2].SendMessageInBackground(Connection.SCORE_SET, Connection.PackScore(scoreDelta.Item1, n_score));
            });
        };

        Action<byte[], ushort> OnCustomerFinishItems(int playerId) => (byte[] buffer, ushort len) => {
            servers[1-playerId].SendMessageInBackground(Connection.CUSTOMER_FINISH_ITEMS, new byte[0]{});
            UnityExecutionThread.instance.ExecuteInMainThread(() => {
                VisibleCustomerQueue.instance.GetActiveCustomer(playerId).OnAfterItems();
            });
        };

        
        //----------INSERT-23:58,11-16-------------------
        Action<byte[], ushort> UpdateButton(int playerID) => (byte[] buffer, ushort len) =>
        {


            var buttontuple = Connection.UnpackButton(buffer, 0);
            UnityExecutionThread.instance.ExecuteInMainThread(() =>
            {
                GameStateManager.instance.SetButtonState( playerID,buttontuple.Item2);
                SyncButton(1-playerID,buttontuple.Item2);
            });

        };

        void SyncButton(int playerID,int buttonstate)
        {
            var thing = Connection.PackButton((byte) playerID, buttonstate);
            servers[playerID].SendMessageInBackground(Connection.SYNC_BUTTON,thing);
        }
        //----------------END-----------------

        void PurgeTrackingId(byte[] buffer, ushort len) {
            var id = buffer[0];
            TrackingIdManager.instance.FreeId(id);
        }
        
        


    }

}