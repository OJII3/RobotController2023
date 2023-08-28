using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RobotController
{
    public class DebugUIManager : MonoBehaviour
    {
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private TMP_Text localEndPointText;
        [SerializeField] private TMP_Text logText;
        [SerializeField] private TMP_Text remoteEndPointText;
        [SerializeField] private Image connectionStateImage;
        [SerializeField] private Button StopButton;

        private void Awake()
        {
            logText.text = "";
            networkManager.OnConnected += ep => remoteEndPointText.text = $"R: {ep}";
            networkManager.OnDisconnected += () => remoteEndPointText.text = "R: NULL";
            networkManager.OnReceived += LogReceivedMessage;
            StopButton.onClick.AddListener(OnRestart);
        }

        private void Start()
        {
            localEndPointText.text = $"L: {networkManager.LocalEndPoint}";
        }

        private void Update()
        {
            connectionStateImage.color = networkManager.connectionState switch
            {
                ConnectionState.Connected => Color.green,
                ConnectionState.Disconnected => Color.red,
                ConnectionState.Connecting => Color.yellow,
                _ => Color.white
            };
        }

        private void LogReceivedMessage(UdpReceiveResult result)
        {
            var message = Encoding.ASCII.GetString(result.Buffer);
            logText.text += $"{message}\n";
        }

        private void OnRestart()
        {
            networkManager.Restart();
            logText.text = "";
        }
    }
}