using System;
using System.Net.Sockets;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace RobotController
{
    public class UDPListener
    {
        private readonly UdpClient _udpClient;
        private bool _isRunning;
        private UdpReceiveResult _result;

        public UDPListener(int localListenPort)
        {
            _udpClient = new UdpClient(localListenPort);
        }

        public async UniTask StartListenLoop(Action<UdpReceiveResult> onReceivedCallback,
            CancellationToken token = default)
        {
            _isRunning = true;
            while (!token.IsCancellationRequested && _isRunning)
            {
                _result = await _udpClient.ReceiveAsync();
                onReceivedCallback?.Invoke(_result);
            }
        }

        ~UDPListener()
        {
            _udpClient.Close();
            _udpClient.Dispose();
        }

        public void StopLoop()
        {
            _isRunning = false;
        }
    }
}