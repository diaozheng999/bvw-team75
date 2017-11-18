using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace PGT.Core.Networking {
    
    public class NetworkStreamPipe : Pipe {
        NetworkStream stream;

        bool connected = false;

        public NetworkStreamPipe() : base() {
            stream = null;
            verbose = true;
        }

        public NetworkStreamPipe(NetworkStream _stream) {
            stream = _stream;
            connected = true;
            BeginListening();
        }

        public void SetNetworkStream(NetworkStream _stream) {
            stream = _stream;
            connected = true;
            BeginListening(); 
        }


        protected override int Read(byte[] buffer, ushort size, uint offset){
            if(verbose)Debug.LogFormat("{0}: Attempting to read {1} byte(s)...", this, size);
            try {
                int i = stream.Read(buffer, (int)offset, size);
                if(verbose)Debug.LogFormat("{0}: Read {1} byte(s). Buffer is {2}", this, i, BufferToString(buffer, 0, (int)offset+size));
                return i;
            }catch (IOException e) {
                return 0;
            }catch (ObjectDisposedException e) {
                return 0;
            }
        }

        protected override void Write(byte[] payload, ushort size, uint offset){
            if (!connected) {
                if(verbose) Debug.LogWarningFormat("{0}: Sending to unconnected destination.", this);
                return;
            }
            stream.Write(payload, (int)offset, size);
        }

        protected override void Flush() {
            if (!connected) {
                if(verbose) Debug.LogWarningFormat("{0}: Sending to unconnected destination.", this);
                return;
            }
            if(verbose)Debug.LogFormat("{0}: Flush called.", this);
            stream.Flush();
        }


    }

}