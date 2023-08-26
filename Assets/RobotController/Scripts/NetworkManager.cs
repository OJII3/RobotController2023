using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RobotController
{
    public class NetworkManager : MonoBehaviour
    {
        public int receivePort = 10001;
        public int remotePort = 10000;
        public string pingMessage = "ping-robot";
        public string pingResultMessage = "pong-robot";
        public ConnectionState connectionState = ConnectionState.Disconnected;
        private readonly float _pingInterval = 1f;
        private readonly float _pingTimeout = 3f;
        private readonly float _receiveInterval = 0.01f;
        private byte[] _pingMessageBytes;
        private UdpClient _udpClient;
        public IPEndPoint LocalEndPoint;
        public Action<IPEndPoint> OnConnected;
        public Action OnDisconnected;
        public Action<UdpReceiveResult> OnReceived;
        public IPEndPoint RemoteEndPoint;

        private void Awake()
        {
            _pingMessageBytes = Encoding.ASCII.GetBytes(pingMessage);
            GetLocalEndPoint();
            _udpClient = new UdpClient(receivePort);
        }

        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();
            Connect(token).Forget();
        }

        private void OnApplicationQuit()
        {
            _udpClient.Dispose();
        }

        private async UniTask Connect(CancellationToken token)
        {
            connectionState = ConnectionState.Connecting;
            UdpReceiveResult result = default;
            while (!token.IsCancellationRequested && connectionState == ConnectionState.Connecting)
            {
                BroadCast(_pingMessageBytes);
                // avoid blocking
                UniTask.Create(async () =>
                {
                    result = await _udpClient.ReceiveAsync();
                    if (Encoding.ASCII.GetString(result.Buffer) == pingResultMessage)
                    {
                        connectionState = ConnectionState.Connected;
                        RemoteEndPoint = result.RemoteEndPoint;
                        OnConnected?.Invoke(RemoteEndPoint);
                    }
                }).Forget();
                await UniTask.Delay(TimeSpan.FromSeconds(_pingTimeout), cancellationToken: token);
            }
        }

        public async void StartSend(byte[] data)
        {
            if (connectionState != ConnectionState.Connected) return;
            await _udpClient.SendAsync(data, data.Length, RemoteEndPoint);
        }

        private async UniTask StartReceive(CancellationToken token)
        {
            while (!token.IsCancellationRequested && connectionState == ConnectionState.Connected)
            {
                // avoid blocking
                UniTask.Create(async () =>
                {
                    var result = await _udpClient.ReceiveAsync();
                    OnReceived?.Invoke(result);
                }).Forget();
                await UniTask.Delay(TimeSpan.FromSeconds(_receiveInterval), cancellationToken: token);
            }
        }

        private void GetLocalEndPoint()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily != AddressFamily.InterNetwork) continue;
                LocalEndPoint = new IPEndPoint(ip, receivePort);
                break;
            }
        }

        private void BroadCast(byte[] buffer)
        {
            _udpClient.Send(buffer, buffer.Length, new IPEndPoint(IPAddress.Broadcast, remotePort));
        }
    }
}