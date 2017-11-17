using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace PGT.Core.Networking {

    public class UdpPipe : Pipe {

        volatile UdpClient _listener;
        volatile Socket _sender;
        IPEndPoint ipep;
        IPEndPoint ipep_recv;
        EndPoint remote_ep;

        byte[] outbuffer;
        byte[] inbuffer;
        uint outbuffer_ptr;
        uint inbuffer_ptr;
        uint inbuffer_size;

        bool disposed;

        public UdpPipe (IPEndPoint ep, int localPort) : base() {
            ipep = ep;
            ipep_recv = new IPEndPoint(IPAddress.Any, localPort);
            _listener = new UdpClient(localPort);
            _sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            outbuffer = new byte[Pipe.BUFFER_SIZE+4];
            outbuffer_ptr = 0;
            inbuffer_ptr = 0;
            inbuffer_size = 0;
            Debug.LogFormat("{0} Started.", this);
            BeginListening();
        }

        protected override int Read(byte[] buffer, ushort size, uint offset) {
            if (inbuffer_ptr == inbuffer_size) {
                try {
                    inbuffer = _listener.Receive(ref ipep_recv);
                } catch (ObjectDisposedException e) {
                    // pipe disposed
                    return 0;
                } catch (SocketException e) {
                    // socket error
                    return 0;
                }
                inbuffer_ptr = 0;
                inbuffer_size = (uint)inbuffer.Length;
            }

            uint _size = size;

            if(inbuffer_size - inbuffer_ptr < size) {
                _size = inbuffer_size - inbuffer_ptr;
            }

            Array.Copy(inbuffer, inbuffer_ptr, buffer, offset, _size);

            inbuffer_ptr += _size;
            return (int)_size;
        }

        protected override void Write (byte[] payload, ushort size, uint offset) {
            Array.Copy(payload, offset, outbuffer, outbuffer_ptr, size);
            outbuffer_ptr += size;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return; 

            if (disposing) {
                _sender.Close();
                _listener.Close();
                inbuffer = null;
                outbuffer = null;
            }

            base.Dispose(disposing);

            disposed = true;
        }


        protected override void Flush() {
            //Debug.Log("UdpPipe::Flush: Nothing to write.");
            if(outbuffer_ptr == 0) return; // nothing to write
            
            //Debug.LogFormat("UdpPipe::Flush: Sent data {0} to endpoint {1}.", Pipe.BufferToString(outbuffer, 0, (int)outbuffer_ptr), ipep);
            _sender.SendTo(outbuffer, 0,  (int)outbuffer_ptr, SocketFlags.None, ipep);
            outbuffer_ptr = 0;
        }

        public override string ToString() {
            return string.Format("UDP Pipe with {0} (listening on {1})", ipep, ipep_recv);
        }
    }

}