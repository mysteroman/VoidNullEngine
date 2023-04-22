using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Net
{
    internal class NetworkClient : NetworkEntity
    {
        private const string LOCALHOST = "127.0.0.1";
        
        private readonly TcpClient _client;
        private readonly Socket _socket;
        private readonly BackgroundWorker _worker;

        public NetworkClient(int port, string ip = LOCALHOST) : base()
        {
            try
            {
                _client = new TcpClient(ip, port);
            }
            catch (SocketException)
            {
                throw;
            }
            _socket = _client.Client;

            _worker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };
            _worker.DoWork += _worker_DoWork;
        }

        protected internal override void Close()
        {
            _worker.CancelAsync();
            _client.Close();
        }

        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {

        }
    }
}
