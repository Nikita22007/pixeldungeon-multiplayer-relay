using System;
using System.IO;
using System.Net;
using System.Threading;

namespace PDMPRelay
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!File.Exists("delay_time.txt")) {
                File.WriteAllText("delay_time.txt", "5000");
            }
            RelayThread.delay_time = int.Parse(File.ReadAllLines("delay_time.txt")[0]);
            if (!File.Exists("ip.txt"))
            {
                File.WriteAllText("ip.txt", "0.0.0.0");
            }
            {
                if (!File.Exists("master-port.txt"))
                {
                    File.WriteAllText("master-port.txt", "25555");
                }
                String[] ports = File.ReadAllLines("master-port.txt");
                String port = ports[0];
                {
                    Thread z = new Thread(new ParameterizedThreadStart(MasterServerServer.ServerListener));
                    z.Start(int.Parse(port));
                }
            }
            {
                if (!File.Exists("client-port.txt"))
                {
                    {
                        File.WriteAllText("client-port.txt", "25556");
                    }
                }
                String[] ports = File.ReadAllLines("client-port.txt");
                String port = ports[0];
                {
                    Thread z = new Thread(new ParameterizedThreadStart(MasterServerClient.ClientListener));
                    z.Start(int.Parse(port));
                }
            }
            System.Console.WriteLine("Write any char to stop");
            System.Console.ReadLine();
            Environment.Exit(0);
        }

    }
}

