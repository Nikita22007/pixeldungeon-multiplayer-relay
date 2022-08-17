using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PDMPRelay
{
    class RelayThread
    {
        public static int delay_time = 5000; // ms
        public static int BUFF_SIZE = 1024 * 8; //  bytes

        public static Socket StartListener()
        {
            // Create a TCP/IP socket.  
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 0);

            Socket listener = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(localEndPoint);
            listener.Listen(2);
            return listener;
        }
        public static int GetPort(Socket listener)
        {
            return int.Parse(listener.LocalEndPoint.ToString().Split(":")[1]);
        }
        public static void RelayFunc(object socket_obj)
        {
            String  endpoint = "NAN";
            try
            {
                using (Socket listener = (Socket)socket_obj)
                {
                    var accept = listener.BeginAccept(null, null);
                    endpoint = listener.LocalEndPoint.ToString();
                    Thread.Sleep(delay_time);
                    if (!accept.IsCompleted)
                    {
                        Console.WriteLine("closed relay {0}. Nobody connected", endpoint);
                        return;
                    }
                    using Socket handler1 = listener.EndAccept(accept);
                    accept = listener.BeginAccept(null, null);
                    Thread.Sleep(delay_time);
                    if (!accept.IsCompleted)
                    {
                        handler1.Close();
                        Console.WriteLine("closed relay {0}. Only one connected", endpoint);
                        return;
                    }
                    Console.WriteLine("Both clients connected to relay {0}. Starting relaying", endpoint);
                    using Socket handler2 = listener.EndAccept(accept);
                    byte[] buff = new byte[BUFF_SIZE];
                    int count = 0;
                    try
                    {
                        while (handler1.IsConnected() && handler2.IsConnected())
                        {
                            count = System.Math.Min(handler1.Available, BUFF_SIZE);
                            if (count > 0)
                            {
                                handler1.Receive(buff, 0, count, SocketFlags.None);
                                handler2.Send(buff, 0, count, SocketFlags.None);
                            }
                            count = System.Math.Min(handler2.Available, BUFF_SIZE);
                            if (count > 0)
                            {
                                handler2.Receive(buff, 0, count, SocketFlags.None);
                                handler1.Send(buff, 0, count, SocketFlags.None);
                            }
                        }
                        Thread.Sleep(10);
                    }
                    catch (SocketException)
                    {
                        handler1.Close();
                        handler2.Close();
                    }
                    Console.WriteLine("closed relay {0}", endpoint);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("closed relay {0}. Exception: {1}", endpoint, e.ToString());
            }
        }
    }
}

