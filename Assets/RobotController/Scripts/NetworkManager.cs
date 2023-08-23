using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace RobotController
{
    public class NetworkManager : MonoBehaviour
    {
        public int localPort = 11000;
        public int remotePort = 10000;
        public string pingMessage = "ping-robot";
        public string pongMessage = "pong-robot";

        private UdpClient _listenerClient;
        private UdpClient _senderClient;

        public IPEndPoint LocalEndPoint { get; private set; }
        public string TargetEndPoint { get; private set; }

        private void Awake()
        {
            GetCurrentIp();
            GetTargetIP();
        }

        private void GetCurrentIp()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    LocalEndPoint = new IPEndPoint(ip, localPort);
                    Debug.Log($"IP Address: {ip}");
                    return;
                }

            Debug.Log("No network adapters with an IPv4 address in the system!");
        }

        // get all devices connected to the same wireless local network by broadcasting to the subnet
        private void GetTargetIP()
        {
            _senderClient = new UdpClient(); // port in useless for sender
            _senderClient.EnableBroadcast = true;
            var request = Encoding.ASCII.GetBytes(pingMessage);
            var ipEndPoint = new IPEndPoint(IPAddress.Broadcast, remotePort);
            _senderClient.Send(request, request.Length, ipEndPoint);

            _listenerClient = new UdpClient(new IPEndPoint(IPAddress.Any, localPort));
            var response = _listenerClient.Receive(ref ipEndPoint); // blocking call
            if (Encoding.ASCII.GetString(response) == pongMessage)
            {
                Debug.Log($"IP Address: {ipEndPoint.Address}");
                TargetEndPoint = ipEndPoint.Address.ToString();
            }
            else
            {
                Debug.Log("Target device not found!");
            }
        }

        private void SendData(byte[] data)
        {
            _senderClient.Send(data, data.Length, TargetEndPoint, remotePort);
        }

        private IEnumerator<byte[]> ReceiveData()
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Any, localPort);
            var data = _listenerClient.Receive(ref ipEndPoint);
            yield return data;
        }
    }
}