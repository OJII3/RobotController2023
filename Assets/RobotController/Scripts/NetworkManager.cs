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
        public int localReceivePort = 10001;
        public int remoteReceivePort = 10000;
        public string pingMessage = "ping-robot";

        public ConnectionState connectionState = ConnectionState.Disconnected;
        private readonly TimeSpan _connectionTimeout = TimeSpan.FromSeconds(3);
        private readonly int _pingInterval = 2;
        private readonly int _receiveTimeout = 1;
        private readonly int _sendTimeout = 2;
        private DateTime _lastReceivedTime = DateTime.Now;
        private byte[] _pingMessageBytes;
        private UdpReceiveResult _receiveResult;
        private IPEndPoint _remoteEndPoint;
        private byte[] _sendBuffer;
        private UdpClient _udpClient;
        public IPEndPoint LocalEndPoint;
        public Action<IPEndPoint> OnConnected;
        public Action OnDisconnected;
        public Action<UdpReceiveResult> OnReceived;

        public byte[] SendBuffer
        {
            set => _sendBuffer = value;
        }

        private void Awake()
        {
            _pingMessageBytes = Encoding.ASCII.GetBytes(pingMessage);
            Configure();
        }

        private void Start()
        {
        }

        private void FixedUpdate()
        {
            var token = this.GetCancellationTokenOnDestroy();
            if (DateTime.Now - _lastReceivedTime > _connectionTimeout)
            {
                connectionState = ConnectionState.Disconnected;
                OnDisconnected?.Invoke();
            }

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
                    if (_sendBuffer.Length == 0) _sendBuffer = _pingMessageBytes;
                    Send(ref _sendBuffer);
                    ReceiveAsync(token).Forget();
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

        private void Send(ref byte[] buffer)
        {
            if (connectionState != ConnectionState.Connected) return;
            _udpClient.Send(buffer, buffer.Length, _remoteEndPoint);
            // Debug.Log("Sent: " + Encoding.ASCII.GetString(data));
        }

        private void Configure()
        {
            GetLocalEndPoint();
            _udpClient = new UdpClient(localReceivePort);
            _udpClient.Client.ReceiveTimeout = _receiveTimeout;
            _udpClient.Client.SendTimeout = _sendTimeout;
        }

        private async UniTask ConnectAsync(CancellationToken token)
        {
            if (token.IsCancellationRequested || connectionState == ConnectionState.Connected) return;
            connectionState = ConnectionState.Connecting;
            byte[] data = default;
            while (!token.IsCancellationRequested && connectionState == ConnectionState.Connecting)
            {
                BroadCast(ref _pingMessageBytes);
                UniTask.Create(() =>
                {
                    try
                    {
                        data = _udpClient.Receive(ref _remoteEndPoint);
                        if (Encoding.ASCII.GetString(data).Equals(pingMessage))
                        {
                            _lastReceivedTime = DateTime.Now;
                            connectionState = ConnectionState.Connected;
                            _remoteEndPoint.Port = remoteReceivePort;
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
                IPEndPoint tempEndPoint = default;
                var buffer = _udpClient.Receive(ref tempEndPoint);
                if (tempEndPoint.Address.Equals(_remoteEndPoint.Address))
                {
                    _lastReceivedTime = DateTime.Now;

                    if (Encoding.ASCII.GetString(buffer).Equals(pingMessage))
                        await _udpClient.SendAsync(_pingMessageBytes, _pingMessageBytes.Length, _remoteEndPoint);
                    else
                        OnReceived?.Invoke(new UdpReceiveResult(buffer, tempEndPoint));
                }
            }
            catch (SocketException _)
            {
            }

            await UniTask.Delay(TimeSpan.Zero, cancellationToken: token);
        }

        private void GetLocalEndPoint()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily != AddressFamily.InterNetwork) continue;
                LocalEndPoint = new IPEndPoint(ip, localReceivePort);
                break;
            }
        }

        private void BroadCast(ref byte[] buffer)
        {
            _udpClient.Send(buffer, buffer.Length, new IPEndPoint(IPAddress.Broadcast, remoteReceivePort));
        }
    }
}