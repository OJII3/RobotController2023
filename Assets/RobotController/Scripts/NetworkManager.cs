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
        public int sendPort = 9999;
        public int remotePort = 10000;
        public string pingMessage = "ping-robot";
        public string pongMessage = "pong-robot";

        private UDP _udp;

        public IPEndPoint LocalEndPoint;
        public IPEndPoint TargetEndPoint;

        private void Awake()
        {
            GetCurrentEndPoint();
        }

        private void Start()
        {
            _udp = new UDP(receivePort);

            var token = this.GetCancellationTokenOnDestroy();
            WaitConnect(token).Forget();
            WaitReceive(token).Forget();
        }

        private void OnApplicationQuit()
        {
            _udp.Dispose();
        }

        private async UniTask WaitConnect(CancellationToken token)
        {
            Debug.Log("Connecting...");
            while (!token.IsCancellationRequested)
            {
                _udp.Broadcast(Encoding.ASCII.GetBytes(pingMessage), token);
                var receiveResult = await _udp.Receive(token);
                var endPoint = receiveResult.RemoteEndPoint;
                var pong = Encoding.ASCII.GetString(receiveResult.Buffer);
                if (pong == pongMessage)
                {
                    TargetEndPoint = endPoint;
                    Debug.Log($"Connected to {endPoint.Address}:{endPoint.Port}");
                    return;
                }

                Debug.Log($"Received {pong}");
                Debug.Log("Trying to connect...");


                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
            }
        }

        private void Send(byte[] data)
        {
            _udp.Send(data, TargetEndPoint).Forget();
        }

        private async UniTaskVoid WaitReceive(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var data = await _udp.Receive(token);
                Debug.Log(data);
                //await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: token);
            }
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
    }
}