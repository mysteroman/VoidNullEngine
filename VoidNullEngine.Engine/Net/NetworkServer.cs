using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Net
{
    internal sealed class NetworkServer : NetworkEntity
    {
        private readonly TcpListener _server;
        private readonly Thread[] _handlers;
        private readonly Client[] _clients;
        private readonly long _maxClients;
        private long _active = 0;

        public long ActiveConnections => Interlocked.Read(ref _active);

        public NetworkServer(int port, long maxClients) : base()
        {
            _server = new TcpListener(IPAddress.Any, port);
            _handlers = new Thread[_maxClients = maxClients];
            _server.Start();
            for (int i = 0; i < _maxClients; ++i)
            {
                _handlers[i] = new Thread(HandleClient);
            }
        }

        protected internal override void Close()
        {
            _server.Stop();
        }

        private void HandleClient()
        {
            Socket client = _server.AcceptSocket();
            Interlocked.Increment(ref _active);

            // Get number of expected bytes
            byte[] buffer = new byte[8];
            client.Receive(buffer);

        }

        private class Client
        {
            public readonly Guid ClientId;
            public readonly Task Handler;

            public Client(Task handler, Guid id)
            {
                ClientId = id;
            }
        }
    }
}
