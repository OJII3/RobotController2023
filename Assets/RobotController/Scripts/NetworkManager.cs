using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RobotController
{
    public class NetworkManager : MonoBehaviour
    {
        public int localReceivePort = 10001;
        public int remoteReceivePort = 10000;
        public string pingMessage = "0SSping-robotEE";

        public byte[] pingMessageBytes = { 83, 83, 1, 112, 105, 110, 103, 45, 114, 111, 98, 111, 116, 69, 69 };

        public Connection connection = Connection.Disconnected;
        private readonly TimeSpan _broadcastInterval = TimeSpan.FromMilliseconds(5);
        private readonly TimeSpan _connectionTimeout = TimeSpan.FromSeconds(1);
        private DateTime _lastReceivedTime;
        private IPEndPoint _remoteEndPoint;
        private UDPBroadcaster _udpBroadcaster;
        private UDPListener _udpListener;

        public IPEndPoint LocalEndPoint;
        public Action<Connection, IPEndPoint> OnConnectionChanged;
        public Action<UdpReceiveResult> OnReceived;

        private void Awake()
        {
            GetLocalEndPoint();
            _udpListener = new UDPListener(localReceivePort);
            _udpBroadcaster = new UDPBroadcaster(pingMessageBytes);
        }

        private void FixedUpdate()
        {
            if (connection == Connection.Connected && DateTime.Now - _lastReceivedTime > _connectionTimeout)
            {
                connection = Connection.Disconnected;
                OnConnectionChanged?.Invoke(connection, _remoteEndPoint);
            }
        }

        public void Stop()
        {
            _udpBroadcaster.StopLoop();
            _udpListener.StopLoop();
            connection = Connection.Disconnected;
            OnConnectionChanged?.Invoke(connection, _remoteEndPoint);
        }

        public void StartNetwork()
        {
            _udpBroadcaster.StopLoop();
            _udpListener.StopLoop();
            var token = this.GetCancellationTokenOnDestroy();
            UniTask.Create(() => _udpBroadcaster.StartBroadcastLoop(remoteReceivePort, _broadcastInterval, token));
            Debug.Log("Start Broadcast Loop");
            UniTask.Create(() => _udpListener.StartListenLoop(OnReceivedCallback, token));
            Debug.Log("Start Listen Loop");
        }

        public void UpdateSendBuffer(byte[] buffer)
        {
            _udpBroadcaster.UpdateSendBuffer(buffer);
        }

        private void OnReceivedCallback(UdpReceiveResult result)
        {
            if (result.Buffer.Length > 4 && result.Buffer[0] == 'S' && result.Buffer[1] == 'S' &&
                result.Buffer[^1] == 'E' && result.Buffer[^2] == 'E')
            {
                Debug.Log("Received message: " + Encoding.ASCII.GetString(result.Buffer));
                _lastReceivedTime = DateTime.Now;
                switch (connection)
                {
                    case Connection.Disconnected:
                    {
                        connection = Connection.Connected;
                        _remoteEndPoint = result.RemoteEndPoint;
                        _remoteEndPoint.Port = remoteReceivePort;
                        OnConnectionChanged?.Invoke(connection, _remoteEndPoint);
                        break;
                    }
                    case Connection.Connected:
                    {
                        if (result.Buffer.Equals(pingMessageBytes))
                            OnReceived?.Invoke(result);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
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
    }
}