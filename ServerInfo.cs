using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.Json.Serialization;
using System.Threading;
using System.Collections.Concurrent;
using System.Text.Json;

namespace PDMPRelay
{
    class ServerInfo
    {
        public TcpClient socket = null;
        public string name { get; set; } = "some-server";
        public int id { get; set; }
        public ConcurrentQueue<int> clients = new ConcurrentQueue<int>();
        public override string ToString()
        {
            string jsonString = JsonSerializer.Serialize(this);
            return jsonString;
        }
    }
}
