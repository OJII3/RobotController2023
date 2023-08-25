using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace RobotController
{
    public class NetworkManager : MonoBehaviour
    {
        public int receivePort = 10001;
        public int sendPort = 9999;
        [FormerlySerializedAs("targetPort")] public int remotePort = 10000;
        public string pingMessage = "ping-robot";
        public string pongMessage = "pong-robot";
        public bool isConnected;
        private Socket _socket;

        private UdpClient _udpClient;
        private UDPLib _udpLib;

        public IPEndPoint LocalEndPoint;
        public IPEndPoint RemoteEndPoint;

        private void Awake()
        {
            GetCurrentEndPoint();
        }

        private void Start()
        {
            _udpClient = new UdpClient(receivePort);

            var token = this.GetCancellationTokenOnDestroy();
            Connect(token).Forget();
        }

        private void OnApplicationQuit()
        {
            _udpClient.Dispose();
        }

        private async UniTaskVoid Connect(CancellationToken token)
        {
            Debug.Log("Connecting...");
            var pingBytes = Encoding.ASCII.GetBytes(pingMessage);
            var broadCastEndPoint = new IPEndPoint(IPAddress.Broadcast, remotePort);
            UdpReceiveResult receiveResult = default;

            while (receiveResult.RemoteEndPoint == null)
            {
                await _udpClient.SendAsync(pingBytes, pingBytes.Length, broadCastEndPoint);
                receiveResult = await _udpClient.ReceiveAsync();
                await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);
                Debug.Log("Failed to Connect. Retrying...");
            }

            Debug.Log("Received Response");

            var endPoint = receiveResult.RemoteEndPoint;
            var pong = Encoding.ASCII.GetString(receiveResult.Buffer);
            if (pong == pongMessage)
            {
                RemoteEndPoint = endPoint;
                Debug.Log($"Connected to {endPoint.Address}:{endPoint.Port}");
            }

            Debug.Log($"Received {pong}");
        }

        private void Send(byte[] data)
        {
            _udpLib.Send(data, RemoteEndPoint).Forget();
        }

        private async UniTask<UdpReceiveResult> Receive(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var data = await _udpClient.ReceiveAsync();
                return data;
            }

            return default;
        }


        private void GetCurrentEndPoint()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily != AddressFamily.InterNetwork) continue;
                LocalEndPoint = new IPEndPoint(ip, receivePort);
                break;
            }
        }

        private async void BroadCast(byte[] buffer, CancellationToken token = default)
        {
            await _udpClient.SendAsync(buffer, buffer.Length, new IPEndPoint(IPAddress.Broadcast, remotePort));
        }
    }
}