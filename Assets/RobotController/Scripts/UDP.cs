using System.Net;
using System.Net.Sockets;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RobotController
{
    public class UDP
    {
        private readonly UdpClient _udpClient;

        public UDP(int port)
        {
            _udpClient = new UdpClient(port);
        }

        public async void Broadcast(byte[] data, CancellationToken token = default)
        {
            await _udpClient.SendAsync(data, data.Length, new IPEndPoint(IPAddress.Broadcast, 10000));
            Debug.Log("Broadcast:\"" + data + "\"");
        }

        public async UniTask Send(byte[] data, IPEndPoint endPoint, CancellationToken token = default)
        {
            await _udpClient.SendAsync(data, data.Length, endPoint);
            Debug.Log("Send:\"" + data + "\" To:" + endPoint.Address + "," + endPoint.Port);
        }

        public async UniTask<UdpReceiveResult> Receive(CancellationToken token = default)
        {
            var result = await _udpClient.ReceiveAsync();
            var endPoint = result.RemoteEndPoint;
            Debug.Log("Receive:" + endPoint.Address + "," + endPoint.Port);
            return result;
        }

        public void Dispose()
        {
            _udpClient.Dispose();
        }
    }
}