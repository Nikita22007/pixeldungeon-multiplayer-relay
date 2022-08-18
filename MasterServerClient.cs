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
    class MasterServerClient
    {
        public class ClientAction
        {
            public string? action { get; set; }
            public int? server { get; set; }
        }
        public static void ClientThreadFunc(object soket_obj)
        {
            TcpClient socket = (TcpClient)soket_obj;
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
                        ClientAction? action;
                        try
                        {
                            action = JsonSerializer.Deserialize<ClientAction>(json);
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
                                case "get":
                                    {
                                        SendServers(writer);
                                        break;
                                    }
                                case "connect":
                                    {
                                        if (action.server == null)
                                        {
                                            WriteConnectError(writer);
                                            continue;
                                        }
                                        ServerInfo serverInfo;
                                        bool getted = MasterServer.servers.TryGetValue((int)action.server, out serverInfo);
                                        if (!getted)
                                        {
                                            WriteConnectError(writer);
                                            continue;
                                        }
                                        if (serverInfo == null)
                                        {
                                            WriteConnectError(writer);
                                            continue;
                                        }
                                        if (!serverInfo.socket.IsConnected())
                                        {
                                            WriteConnectError(writer);
                                            continue;
                                        }
                                        StartConnect(writer, serverInfo);
                                        break;
                                    }
                            }
                        }
                        catch (JsonException e)
                        {
                            Program.WriteLine("{0}", e.ToString());
                            continue;
                        }
                    }
                    Thread.Sleep(200);
                }
            }
            catch (Exception e)
            {
                Program.WriteLine("{0}", e.ToString());
            }
            finally
            {
                socket.Close();
            }
            Program.WriteLine("client socket closed");
        }

        private static void SendServers(TextWriter writer)
        {
            String result = "{\"servers\":[";
            foreach (int serverId in MasterServer.servers.Keys)
            {
                ServerInfo serverInfo;
                bool getted = MasterServer.servers.TryGetValue(serverId, out serverInfo);
                if (!getted)
                {
                    continue;
                }
                result += serverInfo.ToString();
                result += ",";
            }
            result += "]}";
            writer.WriteLine(result);
            writer.Flush();
        }

        private static void StartConnect(TextWriter writer, ServerInfo serverInfo)
        {
            Socket listener = RelayThread.StartListener();
            int port = RelayThread.GetPort(listener);
            String result = "{port:" + port + "}";
            writer.WriteLine(result);
            writer.Flush();
            serverInfo.clients.Enqueue(port);
            Program.WriteLine("opened relay on port {0} for server {1}", port, serverInfo.id);
            Thread thread = new Thread(new ParameterizedThreadStart(RelayThread.RelayFunc));
            thread.Start(listener); 
        }

        private static void WriteConnectError(TextWriter writer)
        {
            String result = "{port:0}";
            writer.WriteLine(result);
            writer.Flush();
        }

        public static void ClientListener(object port_obj)
        {
            int port = (int)port_obj;
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);
            TcpListener tcp_listener = new TcpListener(localEndPoint);
            tcp_listener.Start();
            Program.WriteLine("started client listener on adress {0}", tcp_listener.LocalEndpoint.ToString());
            try
            {
                while (true)
                {
                    TcpClient client_socket = tcp_listener.AcceptTcpClient();
                    client_socket.SetKeepAlive();
                    Thread thread = new Thread(new ParameterizedThreadStart(ClientThreadFunc));
                    Program.WriteLine("connected client: {0}.", client_socket.Client.RemoteEndPoint.ToString());
                    thread.Start(client_socket);
                }
            }
            catch { }
            finally
            {
                tcp_listener.Stop();
                Program.WriteLine("stopped client listener");
                Environment.Exit(1);
            }
        }
    }
}
