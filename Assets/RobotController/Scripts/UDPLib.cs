using System.Net;
using System.Net.Sockets;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace RobotController
{
    public abstract class UDPLib
    {
        private readonly UdpClient _udpClient;

        protected UDPLib(int port)
        {
            _udpClient = new UdpClient(port);
        }

        public async void Broadcast(byte[] data, CancellationToken token = default)
        {
            await _udpClient.SendAsync(data, data.Length, new IPEndPoint(IPAddress.Broadcast, 10000));
        }

        public async UniTask Send(byte[] data, IPEndPoint endPoint, CancellationToken token = default)
        {
            await _udpClient.SendAsync(data, data.Length, endPoint);
        }

        public async UniTask<UdpReceiveResult> Receive(CancellationToken token = default)
        {
            var result = await _udpClient.ReceiveAsync();
            return result;
        }

        public void Dispose()
        {
            _udpClient.Dispose();
        }
    }
}