const WebSocket = require('ws');
const http = require('http');
const express = require('express');
const bodyParser = require('body-parser');
const cors = require('cors');
const net = require('net');
const permutation = require('array-permutation');
const app = express();

app.use(bodyParser.json());
app.use(cors());
app.use(bodyParser.urlencoded({extended : true}));

const sock_port = 47505;
const http_port = 47506;

const GAME_CODE_HEADER = 0x40;
const REMOTE_CUSTOMER = 0x27;


let queue = [];

let ws_connected = false;
let _socket = null;


let server = net.createServer((sock) => {
  console.log("Game server connected from "+sock.remoteAddress)
  ws_connected = true
  _socket = sock
  if(queue.length > 0) {
    console.log("Dequeuing queued customers...")
    queue.forEach((item) => sock.write(item))
  }
  sock.on('error', () => {
    if(_socket == sock) {
      console.log("TCP connection error.")
      _socket = null
      ws_connected = false
    }
  })
})

app.post('/', (req, res) => {
  console.log("Customer avatar:"+req.body.customerType);
  if (req.body.customerName.length > 50) {
    console.log("Incoming order: customer name too long.")
    return
  }

  let encoded = Buffer.from(req.body.customerName, "utf-8")
  let payload_length = encoded.length + req.body.items.length * 2 + 9

  if (payload_length > 1024) {
    console.log("Incoming order: payload size too big.")
    return
  }

  let packet_length = payload_length + 4
  let out_buffer = Buffer.alloc(packet_length, 0x00)

  let items = permutation.shuffle(req.body.items)

  out_buffer.writeUInt8(GAME_CODE_HEADER, 0)
  out_buffer.writeUInt8(REMOTE_CUSTOMER, 1)
  out_buffer.writeUInt16LE(payload_length, 2)
  out_buffer.writeInt32LE(req.body.customerType, 4)
  out_buffer.writeInt16LE(0, 8) // customer type is not used
  out_buffer.writeInt16LE(req.body.items.length, 10)
  for (let i=0; i<items.length; i++) {
    out_buffer.writeInt16LE(items[i], i*2 + 12)
  }
  let curr_len = items.length * 2 + 12
  out_buffer.writeUInt8(encoded.length, curr_len)
  curr_len ++
  encoded.copy(out_buffer, curr_len)

  console.log("Incoming order: "+out_buffer.toString("hex"))
  

  
  if (!ws_connected) {
    queue.push(out_buffer)
    console.log("Queued incoming order.")
    res.send('Queued.')
    return
  }

  _socket.write(out_buffer)
  console.log("Wrote to out buffer.")
  
  res.send("Okay.");

})

server.listen(sock_port, () => console.log("Listening to TCP..."));
app.listen(http_port, () => console.log("Express running."));