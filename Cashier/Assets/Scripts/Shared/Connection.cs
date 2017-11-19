using System;
using System.Text;
using System.Net;
using UnityEngine;
using PGT.Core;
using PGT.Core.DataStructures;
using PGT.Core.Networking;


namespace Team75.Shared {
    public static class Connection {
        public const int UDP_PEER_PORT_1 = 47500;
        public const int UDP_PEER_PORT_2 = 47504;
        public const int UDP_SERVER_PORT = 47501;
        public const int TCP_SERVER_PORT_1 = 47502;
        public const int TCP_SERVER_PORT_2 = 47503;
        public const int WEB_SERVER_PORT = 47505;

        // trackable object ids
        public const byte PLAYER_ONE_CENTER_EYE = 0;
        public const byte PLAYER_ONE_LEFT_HAND = 1;
        public const byte PLAYER_ONE_RIGHT_HAND = 2;
        public const byte PLAYER_ONE_SCANNER = 6;
        public const byte PLAYER_TWO_CENTER_EYE = 3;
        public const byte PLAYER_TWO_LEFT_HAND = 4;
        public const byte PLAYER_TWO_RIGHT_HAND = 5;
        public const byte PLAYER_TWO_SCANNER = 7;

        // messages
        public const byte SET_PLAYER_ID = 0xA0;
        public const byte SET_OPPONENT = 0xA1;
        public const byte TRACKING_ID_REQUEST = 0x20;
        public const byte TRACKING_ID_RESPONSE = 0x21;
        public const byte TRACKING_ID_DESTROY = 0xB0;
        public const byte TRACKING_ID_PURGE = 0xB1;
        public const byte SPAWN_ITEM = 0xB2;
        public const byte TRACKING_ID_PURGED = 0xB3;
        public const byte CUSTOMER_REQUEST = 0x22;
        public const byte CUSTOMER_RESPONSE = 0x23;
        public const byte CUSTOMER_QUEUE_EMPTY = 0x24;
        public const byte ENQUEUE_CUSTOMER = 0x25;
        public const byte CUSTOMER_LEAVE = 0x26;
        public const byte REMOTE_CUSTOMER = 0x27;
        public const byte CUSTOMER_FINISH_ITEMS = 0x28;
        public const byte START_GAME = 0xFF;
        public const byte END_GAME = 0xFE;
        public const byte TIMER_SYNC = 0xFD;
        public const byte FRENZY_START = 0xFC;

        public const byte SCORE_ADD_ITEM = 0x30;
        public const byte SCORE_SET = 0x31;
        
        //-------------INSERT-----------
        public const byte UPDATE_BUTTON = 0x41;
        public const byte SYNC_BUTTON = 0x42;
        //-----------END_-----------------

        static UTF8Encoding textEncoder = new UTF8Encoding();

        public static byte[] PackOpponent(byte opponent, IPAddress addr) {
            var b_addr = addr.GetAddressBytes();
            var buffer = new byte[b_addr.Length + 1];
            buffer[0] = opponent;
            Array.Copy(b_addr, 0, buffer, 1, b_addr.Length);
            return buffer;
        }

        public static Tuple<byte, IPAddress> UnpackOpponent(byte[] buffer, int length) {
            var opponent = buffer[0];
            var ipaddr_len = length - 1;
            var ipaddr = new byte[ipaddr_len];
            Array.Copy(buffer, 1, ipaddr, 0, ipaddr_len);
            return new Tuple<byte, IPAddress>(opponent, new IPAddress(ipaddr));
        }

        public static byte[] PackCustomerDequeueInfo(byte playerId, Customer cust) {
            var _b = PackCustomer(cust);
            var buffer = new byte[_b.Length + 1];
            buffer[0] = playerId;
            Array.Copy(_b, 0, buffer, 1, _b.Length);
            return buffer;
        }

        public static Tuple<byte, Customer> UnpackCustomerDequeueInfo(byte[] buffer, int offset) {
            return new Tuple<byte, Customer>(buffer[offset], UnpackCustomer(buffer, offset+1));
        }

        public static byte[] PackTrackingId(ushort trackingId) {
            return BitConverter.GetBytes(trackingId);
        }

        public static ushort UnpackTrackingId(byte[] buffer, int offset) {
            return BitConverter.ToUInt16(buffer, offset);
        }

        public static byte[] PackCustomer(Customer cust) {
            // packing this way:
            // cust_id (4)
            // cust_type (2)
            // item_len (2)
            // items (item_len * 2)
            // name_len (1)
            // name (name_len)
            var encoder = textEncoder;
            var encoded_name = encoder.GetBytes(cust.Name);

            int length = encoded_name.Length + cust.Items.Length * 2 + 9;
            byte[] buffer = new byte[length];
            Array.Copy(BitConverter.GetBytes(cust.CustomerId), 0, buffer, 0, 4);
            Array.Copy(BitConverter.GetBytes(cust.CustomerId), 0, buffer, 4, 2);
            Array.Copy(BitConverter.GetBytes((short)cust.Items.Length), 0, buffer, 6, 2);
            for(int i=0; i<cust.Items.Length; ++i) {
                Array.Copy(BitConverter.GetBytes(cust.Items[i]), 0, buffer, i*2+8, 2);
            }
            int curr_len = cust.Items.Length * 2 + 8;


            buffer[curr_len] = (byte)encoded_name.Length;
            ++curr_len;
            Array.Copy(encoded_name, 0, buffer, curr_len, encoded_name.Length);

            return buffer;
        }

        public static Customer UnpackCustomer(byte[] buffer, int offset) {
            var cust = new Customer();
            cust.CustomerId = BitConverter.ToInt32(buffer, offset);
            cust.CustomerType = BitConverter.ToInt16(buffer, offset+4);
            var item_len = (int)BitConverter.ToInt16(buffer, offset+6);
            cust.Items = new short[item_len];
            for(int i=0; i<item_len; ++i) {
                cust.Items[i] = BitConverter.ToInt16(buffer, offset+8+i*2);
            }

            var decoder = textEncoder;
            int len = buffer[offset + 8 + item_len * 2];

            cust.Name = decoder.GetString(buffer, offset + 9 + item_len*2, len);

            return cust;
            
        }

        public static byte[] PackTrackableItem(ushort id, short type, Vector3 pos, Quaternion rot) {
            var buffer = new byte[32];
            //buffer[0] = id;
            Array.Copy(BitConverter.GetBytes(id), 0, buffer, 0, 2);
            Array.Copy(BitConverter.GetBytes(type), 0, buffer, 2, 2);
            Array.Copy(BitConverter.GetBytes(pos.x), 0, buffer, 4, 4);
            Array.Copy(BitConverter.GetBytes(pos.y), 0, buffer, 8, 4);
            Array.Copy(BitConverter.GetBytes(pos.z), 0, buffer, 12, 4);
            Array.Copy(BitConverter.GetBytes(rot.x), 0, buffer, 16, 4);
            Array.Copy(BitConverter.GetBytes(rot.y), 0, buffer, 20, 4);
            Array.Copy(BitConverter.GetBytes(rot.z), 0, buffer, 24, 4);
            Array.Copy(BitConverter.GetBytes(rot.w), 0, buffer, 28, 4);
            return buffer;
        }

        public static Tuple<ushort, short, Vector3, Quaternion> UnpackTrackableItem(byte[] buffer, int offset) {
            //var id = buffer[offset];
            var id = BitConverter.ToUInt16(buffer, offset);
            var type = BitConverter.ToInt16(buffer, offset+2);
            var pos_x = BitConverter.ToSingle(buffer, offset+4);
            var pos_y = BitConverter.ToSingle(buffer, offset+8);
            var pos_z = BitConverter.ToSingle(buffer, offset+12);
            var pos = new Vector3(pos_x, pos_y, pos_z);
            var rot_x = BitConverter.ToSingle(buffer, offset+16);
            var rot_y = BitConverter.ToSingle(buffer, offset+20);
            var rot_z = BitConverter.ToSingle(buffer, offset+24);
            var rot_w = BitConverter.ToSingle(buffer, offset+28);
            var rot = new Quaternion(rot_x, rot_y, rot_z, rot_w);
            return new Tuple<ushort, short, Vector3, Quaternion>(id, type, pos, rot);
        }

        public static byte[] PackScore(byte playerId, uint score) {
            var buffer = new byte[5];
            buffer[0] = playerId;
            Array.Copy(BitConverter.GetBytes(score), 0, buffer, 1, 4);
            return buffer;
        }

        public static Tuple<byte, uint> UnpackScore(byte[] buffer, int offset) {
            var playerId = buffer[offset];
            var score = BitConverter.ToUInt32(buffer, offset+1);
            return new Tuple<byte, uint>(playerId, score);
        }
        
        //-----------INSERT-------------
        public static byte[] PackButton(byte playerID, int state)
        {
            var buffer = new byte[5];
            buffer[0] = playerID;
            Array.Copy(BitConverter.GetBytes(state),0,buffer,1,4);
            return buffer;
        }

        public static Tuple<byte, int> UnpackButton(byte[] buffer, int offset)
        {
            var playerID = buffer[offset];
            var state = BitConverter.ToInt32(buffer, offset + 1);
            return new Tuple<byte, int>(playerID, state);
        }
        //-----------END---------------


    }
}