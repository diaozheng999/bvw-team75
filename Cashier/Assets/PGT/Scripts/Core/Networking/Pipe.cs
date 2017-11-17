using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace PGT.Core.Networking {

    public abstract class Pipe : Base, IDisposable {

		public static byte GAME_CODE_HEADER = 0x40;
        public const int BUFFER_SIZE = 1024;

        string identifier;
        volatile bool disposed = false;

        protected bool verbose = false;

        byte[] buffer;

        ConcurrentDictionary<byte, Action<byte[], ushort>> parser;
        ConcurrentDictionary<byte, string> message_names;
        
        Thread listenerThread;

        volatile bool terminated;

        protected abstract int Read(byte[] buffer, ushort size, uint offset);
        protected abstract void Write(byte[] payload, ushort size, uint offset);
        protected abstract void Flush();

        object sending;

        public Pipe(int buffer_size = BUFFER_SIZE) {
            buffer = new byte[buffer_size];
            listenerThread = new Thread(Listen);
            parser = new ConcurrentDictionary<byte, Action<byte[], ushort>>();
            message_names = new ConcurrentDictionary<byte, string>();
            sending = new object();
        }

        protected void BeginListening() {
            listenerThread.Start();
            UnityExecutionThread.instance.ExecuteInMainThread(() => Debug.Log("hi!"));
        }

        protected void Listen() {
            while (!terminated) {
                
                // read the message header, which is always 4 bytes:
                /*
                    | GAME_CODE_HEADER | MSG_ID | LENGTH (ushort, little_endian) |
                    -------------------------------------------------------------- 
                    |      0x00        |  0x01  |     0x02     |      0x03       |
                 */
                for(ushort i=0; i < 4; i+=(ushort)(uint)Read(buffer, (ushort)(uint)(4 - i), i)) {
                    if (terminated || disposed) return;
                }

                if (buffer[0] != GAME_CODE_HEADER) {
                    Debug.LogErrorFormat("{0}: Message out of alignment.", this);
                    return;
                }

                var msg_type = buffer[1];
                var msg_len = BitConverter.ToUInt16(buffer, 2);
                

                // read and parse the message
                if (msg_len>0) Read(buffer, msg_len, 0);

                if (parser.ContainsKey(msg_type)) {
                    if (verbose) {
                        Debug.LogFormat("{0}: Read message with length {1} of type {2}. Contents: {3}", this, msg_len, message_names[msg_type], BufferToString(buffer, 0, (int)msg_len));
                    }
                    parser[msg_type].Invoke(buffer, msg_len);
                } else {
                    Debug.LogWarningFormat("{0}: No handler of message type 0x{1} was found.", this, msg_type.ToString("X2"));
                    
                    if (verbose) {
                        Debug.LogFormat("{0}: Read message with length {1} of type 0x{2}. Contents: {3}", this, msg_len, msg_type, BufferToString(buffer, 0, (int)msg_len));
                    }
                }
            }
        }

        /// <summary>
        /// Adds a parser for a message. Note that parser_fn will NOT be executed
        /// in the Unity thread, and will not hold on to the buffer array after
        /// execution finishes. It is recommended that should parser_fn need to 
        /// do any asynchronous processing or executing in the Unity thread,
        /// it should do the unpacking of the binary buffer in the function
        /// and allocate its own resources before passing that on to the 
        /// asynchronous processes.
        /// </summary>
        public bool AddParser(byte msg_id, Action<byte[], ushort> parser_fn, string msg_name) {
            if(parser.ContainsKey(msg_id)) {
                Debug.LogWarningFormat("{0}: Already a handler for message type {1}", this, message_names[msg_id]);
                return false;
            }
            parser[msg_id] = parser_fn;
            message_names[msg_id] = msg_name;
            return true;
        }

        /// <summary>
        /// Remove a parser for a message.
        /// </summary>
        public bool RemoveParser(byte msg_id){
            Action<byte[], ushort> fn;
            return parser.TryRemove(msg_id, out fn);
        }

        public void SendMessageInBackground(byte msg_id, byte[] message) {
            Task.Run(() => SendMessageSync(msg_id, message, (ushort)(uint)message.Length, 0));
        }

        
        public void SendMessageInBackground(byte msg_id, byte[] message, ushort size, uint offset) {
            Task.Run(() => SendMessageSync(msg_id, message, size, offset));
        }

        /*
        public async Task<bool> SendMessage(byte msg_id, byte[] message){
            return await SendMessage(msg_id, message, (ushort)(uint)message.Length, 0);
        }

        public async Task<bool> SendMessage(byte msg_id, byte[] message, ushort size){
            return await SendMessage(msg_id, message, size, 0);
        }

        public async Task<bool> SendMessage(byte msg_id, byte[] message, ushort size, uint offset) {
            return await new Task<bool>(() => SendMessageSync(msg_id, message, size, offset));
        }
        */

        private bool SendMessageSync(byte msg_id, byte[] message, ushort size, uint offset) {
            if (size > BUFFER_SIZE) return false;
            var header = new byte[4];
            header[0] = GAME_CODE_HEADER;
            header[1] = msg_id;
            Array.Copy(BitConverter.GetBytes(size), 0, header, 2, 2);
            try {
                lock(sending){
                    Write(header, 4, 0);
                    Write(message, size, offset);
                    Flush();

                    if(verbose) {
                        
                        string msg;
                        if (message_names.ContainsKey(msg_id)) {
                            msg = message_names[msg_id];
                        }else {
                            msg = "0x"+msg_id.ToString("X2");
                        }
                        
                        Debug.LogFormat("{0}: Sent message with length {1} of type {2}. Contents: {3}", this, size, msg, BufferToString(message, (int)offset, size));
                    }
                }
                return true;
            } catch {
                return false;
            }
        }

        public override string ToString() =>
            string.Format("Pipe ({0})", identifier);

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return; 

            if (disposing) {
                terminated = true;
                listenerThread.Abort();
                listenerThread = null;
            }

            disposed = true;
        }

        
        public void Dispose()
        {
            if (verbose) Debug.LogFormat("{0}: Disposed.", this);
            Dispose(true);
            GC.SuppressFinalize(this);           
        }

        public static string BufferToString(byte[] buffer, int offset, int size) {
            string _str = "";
            var end = offset+size;
            for(int i=offset; i < end; ++i) {
                _str += " "+buffer[i].ToString("X2");
            }

            return _str;
        }

    }

}