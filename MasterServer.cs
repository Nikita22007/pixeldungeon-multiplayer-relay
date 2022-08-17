using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;

namespace PDMPRelay
{
    class MasterServer
    {
        public static ConcurrentDictionary<int, ServerInfo> servers = new ConcurrentDictionary<int, ServerInfo>();
    }
}
