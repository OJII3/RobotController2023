using System;
using System.Collections.Generic;
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
        private readonly List<byte[]> _sendBuffers = new();

        public UDPBroadcaster()
        {
            _udpClient = new UdpClient();
            _udpClient.EnableBroadcast = true;
        }

        public void updateSendBuffer(byte[] buffer)
        {
            _sendBuffers.Add(buffer);
        }

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
                foreach (var sendBuffer in _sendBuffers)
                    await _udpClient.SendAsync(sendBuffer, sendBuffer.Length, remoteEndPoint);
                _sendBuffers.Clear();
                await UniTask.Delay(interval);
            }
        }

        public void StopLoop()
        {
            _isRunning = false;
        }
    }
}