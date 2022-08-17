using System.Net.Sockets;

namespace PDMPRelay
{
    public static class ExtendionMethod
    {

        public static bool IsConnected(this Socket s)
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
        public static bool IsConnected(this TcpClient s)
        {
            return IsConnected(s.Client);
        }
        public static void SetKeepAlive(this Socket s, int TcpKeepAliveTime = 10, int TcpKeepAliveRetryCount = 4, int TcpKeepAliveInterval = 3)
        {
            s.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, TcpKeepAliveTime);
            s.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, TcpKeepAliveRetryCount);
            s.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, TcpKeepAliveInterval);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        }
        public static void SetKeepAlive(this TcpClient s, int TcpKeepAliveTime = 10, int TcpKeepAliveRetryCount = 4, int TcpKeepAliveInterval = 3)
        {
            SetKeepAlive(s.Client, TcpKeepAliveTime, TcpKeepAliveRetryCount, TcpKeepAliveInterval);
        }
    }
}

