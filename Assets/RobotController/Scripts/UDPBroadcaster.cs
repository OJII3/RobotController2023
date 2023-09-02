using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace RobotController
{
    public class UDPBroadcaster
    {
        private readonly UdpClient _udpClient;
        private bool _isRunning;

        public UDPBroadcaster()
        {
            _udpClient = new UdpClient();
            _udpClient.EnableBroadcast = true;
        }

        public byte[] SendBuffer { private get; set; }

        ~UDPBroadcaster()
        {
            _udpClient.Close();
            _udpClient.Dispose();
        }

        public async UniTask StartBroadcastLoop(int remoteListenPort, TimeSpan interval,
            CancellationToken token = default)
        {
            _isRunning = true;
            var remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, remoteListenPort);
            while (!token.IsCancellationRequested && _isRunning)
            {
                await _udpClient.SendAsync(SendBuffer, SendBuffer.Length, remoteEndPoint);
                await UniTask.Delay(interval);
            }
        }

        public void StopLoop()
        {
            _isRunning = false;
        }
    }
}