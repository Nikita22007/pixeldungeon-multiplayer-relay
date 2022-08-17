using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace PDMPRelay
{
    class MasterServerServer
    {
        public class ServerAction
        {
            public string? action { get; set; }
            public string? name { get; set; }
        }

        public static void ServerThreadFunc(object info_obj)
        {
            ServerInfo info = (ServerInfo)info_obj;
            TcpClient socket = info.socket;
            var networkStream = socket.GetStream();
            TextReader reader = new StreamReader(networkStream);
            TextWriter writer = new StreamWriter(networkStream);
            try
            {
                System.Threading.Tasks.Task<string> read_task = null;
                while (socket.IsConnected())
                {
                    if (read_task == null)
                    {
                        read_task = reader.ReadLineAsync();
                    }
                    if (read_task.IsCompleted)
                    {
                        String json = read_task.Result;
                        read_task = null;
                        ServerAction? action;
                        try
                        {
                            action = JsonSerializer.Deserialize<ServerAction>(json);

                            if (action == null)
                            {
                                continue;
                            }
                            if (action.action == null)
                            {
                                continue;
                            }
                            switch (action.action)
                            {
                                case "name":
                                    {
                                        if (action.name == null)
                                        {
                                            continue;
                                        }
                                        if (action.name == "")
                                        {
                                            continue;
                                        }
                                        int add_id = 1;
                                        var has_id = false;
                                        String new_name = action.name;
                                        foreach (ServerInfo s_info in MasterServer.servers.Values)
                                        {
                                            if (s_info == info)
                                            {
                                                continue;
                                            }
                                            if (s_info.name == new_name)
                                            {
                                                has_id = true;
                                                break;
                                            }
                                        }
                                        if (has_id)
                                        {
                                            while (has_id)
                                            {
                                                has_id = false;
                                                new_name = action.name + '_' + add_id.ToString();
                                                foreach (ServerInfo s_info in MasterServer.servers.Values)
                                                {
                                                    if (s_info == info)
                                                    {
                                                        continue;
                                                    }
                                                    if (s_info.name == new_name)
                                                    {
                                                        has_id = true;
                                                        break;
                                                    }
                                                }
                                                add_id += 1;
                                            }
                                            break;
                                        }
                                        info.name = new_name;
                                        break;
                                    }
                            }
                        }
                        catch (JsonException)
                        {
                            continue;
                        }
                    }
                    int port;
                    if (info.clients.TryDequeue(out port))
                    {
                        writer.Write("{\"port\":" + port + "}" + "\n");
                        writer.Flush();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}", e.ToString());
            }
            finally
            {
                _ = MasterServer.servers.TryRemove(info.id, out _);
                socket.Close();
            }
            Console.WriteLine("server socket closed");

        }
        public static void ServerListener(object port_obj)
        {
            int port = (int)port_obj;
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);
            TcpListener tcp_listener = new TcpListener(localEndPoint);
            tcp_listener.Start();
            Console.WriteLine("started server listener on adress {0}", tcp_listener.LocalEndpoint.ToString());
            try
            {
                int new_server_id = 1;
                while (true)
                {
                    TcpClient server_socket = tcp_listener.AcceptTcpClient();
                    server_socket.SetKeepAlive();
                    Thread thread = new Thread(new ParameterizedThreadStart(ServerThreadFunc));
                    var info = new ServerInfo
                    {
                        id = new_server_id,
                        socket = server_socket,
                        name = "Server " + new_server_id
                    };

                    MasterServer.servers.TryAdd(new_server_id, info);
                    thread.Start(info);
                    Console.WriteLine("connected server: {0}. id: {1}", server_socket.Client.RemoteEndPoint.ToString(), new_server_id);
                    new_server_id += 1;
                }
            }
            catch (Exception e) {
                Console.WriteLine("server listener exception: {0}", e.ToString());
            }
            finally
            {
                tcp_listener.Stop();
                Console.WriteLine("stopped server listener");
                Environment.Exit(1);
            }
        }
    }
}
