using System;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using TcpClient_ = System.Net.Sockets.TcpClient;

namespace PGT.Core.Networking {

    public class TcpClient : NetworkStreamPipe {
        TcpClient_ _client;
        IPAddress address;
        int[] ports;

        int connectedPort = 0;

        public TcpClient(IPAddress addr, int _port) {
            _client = new TcpClient_();
            address = addr;
            connectedPort = _port;
            // verbose = true;
            Task.Run(() => Connect());
        }

        public async void Connect() {
            await _client.ConnectAsync(address, connectedPort);
            if(verbose) {
                Debug.LogFormat("{0}: Connection established.", this);
            }
            SetNetworkStream(_client.GetStream());
        }

        public override string ToString() {
            return string.Format("TCP Pipe to {0}:{1}:", address, connectedPort);
        }
    }

}