using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace PGT.Core.Networking {
    
    public class NetworkStreamPipe : Pipe {
        NetworkStream stream;

        public NetworkStreamPipe() : base() {
            stream = null;
            verbose = true;
        }

        public NetworkStreamPipe(NetworkStream _stream) {
            stream = _stream;
            BeginListening();
        }

        public void SetNetworkStream(NetworkStream _stream) {
            stream = _stream;
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
            stream.Write(payload, (int)offset, size);
        }

        protected override void Flush() {
            if(verbose)Debug.LogFormat("{0}: Flush called.", this);
            stream.Flush();
        }


    }

}