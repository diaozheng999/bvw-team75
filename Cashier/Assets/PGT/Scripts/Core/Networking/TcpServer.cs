using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using UnityEngine;

namespace PGT.Core.Networking {

    public class TcpServer : NetworkStreamPipe {
        List<IPAddress> ip;

        TcpListener[] listeners;

        bool disposed = false;
        
        int port;

        Action<IPAddress> onConnect;

        public TcpServer(int port, Action<IPAddress> OnConnect) {
            ip = new List<IPAddress>();
            onConnect = OnConnect;
            //verbose = true;
            this.port = port;
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach(var entry in host.AddressList)
            {
                if(entry.AddressFamily == AddressFamily.InterNetwork)
                {
                    ip.Add(entry);
                }
            }

            listeners = new TcpListener[ip.Count];

            for(int i=0; i<ip.Count; i++)
            {
                try
                {
                    var ep = new IPEndPoint(ip[i], port);
                    listeners[i] = new TcpListener(ep);
                    listeners[i].Start();
                    listeners[i].BeginAcceptTcpClient(new AsyncCallback(Transition), listeners[i]);
                } catch {
                    listeners[i] = null;
                }
            }

            if (verbose) {
                Debug.LogFormat("{0}: Beginning to listen on {1} interface(s).", this, listeners.Length);
            }
        }


        public IEnumerable<IPAddress> GetLocalAddresses() {
            return ip;
        }

        protected void Transition(IAsyncResult res)
        {
            //stop all other listeners
            var listener = (TcpListener)res.AsyncState;
            var client = listener.EndAcceptTcpClient(res);

            var stream = client.GetStream();
            var ipep = (IPEndPoint)client.Client.RemoteEndPoint;
            

            SetNetworkStream(stream);


            
            if (verbose) {
                Debug.LogFormat("{0}: Remote endpoint {1} connected.", this, ipep);
            }

            onConnect(ipep.Address);

            StopListeners();

        }

        
        protected void StopListeners()
        {
            if (listeners == null) return;
            for (int i=0; i<ip.Count; i++)
            {
                if(listeners[i]!=null && !listeners[i].Server.Connected)
                {
                    listeners[i].Stop();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return; 

            if (disposing) {
                // dispose stuff;
                StopListeners();
            }

            base.Dispose(disposing);

            disposed = true;
        }

        public override string ToString() {
            return string.Format("TCP Server Pipe at port {0}", port);
        }
        
    }

}