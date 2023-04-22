using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Net
{
    public static class NetworkManager
    {
        private static IManager<object> Instance;
        
        private interface IManager<out T>
        {

        }
        
        private class Manager<T> : IManager<T>
        {

        }
        
        static NetworkManager()
        {
            Instance = null;
        }

        

        public static bool IsActive => User is not null;
        public static bool IsHost => User is NetworkServer;
        public static bool IsClient => User is NetworkClient;
        private static NetworkEntity User { get; set; }

        public static void Host(int port, int maxClients)
        {
            if (IsActive) return;
            User = new NetworkServer(port, maxClients);
        }

        public static void Connect(string ip, int port)
        {
            if (IsActive) return;
            User = new NetworkClient(port, ip);
        }

        public static void Disconnect()
        {
            if (!IsActive) return;
            User.Close();
            User = null;
        }
    }
}
