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
        private readonly byte[] _defaultPingMessage;

        private readonly List<byte[]> _sendBuffers = new();
        private readonly UdpClient _udpClient;

        private bool _isRunning;

        public UDPBroadcaster(byte[] defaultPingMessage)
        {
            _udpClient = new UdpClient();
            _udpClient.EnableBroadcast = true;
            _defaultPingMessage = defaultPingMessage;
        }

        public void UpdateSendBuffer(byte[] buffer)
        {
            if (_sendBuffers.Count >= 2) _sendBuffers.Clear();
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
                if (_sendBuffers.Count == 0)
                    await _udpClient.SendAsync(_defaultPingMessage, _defaultPingMessage.Length, remoteEndPoint);
                for (var i = 0; i < _sendBuffers.Count; i++)
                    await _udpClient.SendAsync(_sendBuffers[i], _sendBuffers[i].Length, remoteEndPoint);
                _sendBuffers.Clear();
                await UniTask.Delay(interval, cancellationToken: token);
            }
        }

        public void StopLoop()
        {
            _isRunning = false;
        }
    }
}