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
        public int remotePort = 10000;
        public string pingMessage = "ping-robot";

        [FormerlySerializedAs("pingResultMessage")]
        public string pongMessage = "pong-robot";

        public ConnectionState connectionState = ConnectionState.Disconnected;
        private readonly int _connectionFailLimit = 10;
        private readonly int _pingInterval = 2;
        private readonly int _receiveTimeout = 1;
        private readonly int _sendTimeout = 2;
        private int _connectionFailCount;
        private byte[] _pingMessageBytes;
        private IPEndPoint _remoteEndPoint;
        private byte[] _sendMessageData;
        private UdpClient _udpClient;
        public IPEndPoint LocalEndPoint;
        public Action<IPEndPoint> OnConnected;
        public Action OnDisconnected;
        public Action<UdpReceiveResult> OnReceived;
        public byte[] SendMessageData { set; get; }

        private void Awake()
        {
            Configure();
        }

        private void Start()
        {
        }

        private void Update()
        {
            var token = this.GetCancellationTokenOnDestroy();
            switch (connectionState)
            {
                case ConnectionState.Disconnected:
                {
                    ConnectAsync(token).Forget();
                    break;
                }
                case ConnectionState.Connecting:
                {
                    break;
                }
                case ConnectionState.Connected:
                {
                    ReceiveAsync(token).Forget();
                    _sendMessageData ??= _pingMessageBytes;
                    Send(ref _sendMessageData);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnApplicationQuit()
        {
            _udpClient.Close();
            _udpClient.Dispose();
        }

        public void Restart()
        {
            _udpClient.Close();
            _udpClient.Dispose();
            Configure();
            connectionState = ConnectionState.Disconnected;
            OnDisconnected?.Invoke();
            _remoteEndPoint = null;
        }

        public void Send(ref byte[] buffer)
        {
            if (connectionState != ConnectionState.Connected) return;
            _udpClient.Send(buffer, buffer.Length, _remoteEndPoint);
            // Debug.Log("Sent: " + Encoding.ASCII.GetString(data));
        }

        private void Configure()
        {
            _pingMessageBytes = Encoding.ASCII.GetBytes(pingMessage);
            GetLocalEndPoint();
            _udpClient = new UdpClient(receivePort);
            _udpClient.Client.ReceiveTimeout = _receiveTimeout;
            _udpClient.Client.SendTimeout = _sendTimeout;
        }

        private async UniTask ConnectAsync(CancellationToken token)
        {
            connectionState = ConnectionState.Connecting;
            byte[] data = default;
            UdpReceiveResult result = default;
            while (!token.IsCancellationRequested && connectionState == ConnectionState.Connecting)
            {
                BroadCast(ref _pingMessageBytes);
                UniTask.Create(() =>
                {
                    try
                    {
                        data = _udpClient.Receive(ref _remoteEndPoint);
                        result = new UdpReceiveResult(data, _remoteEndPoint);
                        if (Encoding.ASCII.GetString(result.Buffer) == pongMessage)
                        {
                            connectionState = ConnectionState.Connected;
                            _remoteEndPoint = result.RemoteEndPoint;
                            OnConnected?.Invoke(_remoteEndPoint);
                        }

                        // Debug.Log($"Received: {Encoding.ASCII.GetString(result.Buffer)}");
                    }
                    catch (SocketException e)
                    {
                        // Debug.Log(e.Message);
                    }

                    return default;
                }).Forget();
                await UniTask.Delay(TimeSpan.FromSeconds(_pingInterval), cancellationToken: token);
            }
        }

        private async UniTaskVoid ReceiveAsync(CancellationToken token)
        {
            if (token.IsCancellationRequested) return;
            if (connectionState != ConnectionState.Connected) return;
            try
            {
                var data = _udpClient.Receive(ref _remoteEndPoint);
                var result = new UdpReceiveResult(data, _remoteEndPoint);
                OnReceived?.Invoke(result);

                if (Encoding.ASCII.GetString(result.Buffer) == pingMessage)
                {
                    var pongMessageBytes = Encoding.ASCII.GetBytes(pongMessage);
                    await _udpClient.SendAsync(pongMessageBytes, pongMessageBytes.Length, _remoteEndPoint);
                }
            }
            catch (SocketException _)
            {
                _connectionFailCount++;
                if (_connectionFailCount >= _connectionFailLimit)
                {
                    _connectionFailCount = 0;
                    connectionState = ConnectionState.Disconnected;
                    OnDisconnected?.Invoke();
                }
            }

            await UniTask.Delay(TimeSpan.Zero, cancellationToken: token);
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

        private void BroadCast(ref byte[] buffer)
        {
            _udpClient.Send(buffer, buffer.Length, new IPEndPoint(IPAddress.Broadcast, remotePort));
        }
    }
}