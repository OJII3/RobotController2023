using System.Collections.Generic;
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
        [SerializeField] private Button stopButton;
        [SerializeField] private Toggle logModeToggle;
        private readonly List<byte[]> _messagesQueue = new();
        private bool _isLogModeString = true;

        private void Awake()
        {
            logText.text = "";
            networkManager.OnConnected += ep => remoteEndPointText.text = $"R: {ep}";
            networkManager.OnDisconnected += () => remoteEndPointText.text = "R: NULL";
            networkManager.OnReceived += LogReceivedMessage;
            stopButton.onClick.AddListener(OnRestart);
            logModeToggle.onValueChanged.AddListener(b => _isLogModeString = b);
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
            _messagesQueue?.ForEach(bytes =>
            {
                var text = _isLogModeString ? Encoding.ASCII.GetString(bytes) : string.Join(",", bytes);
                logText.text =
                    $"{text}\n{logText.text}";
            });
            _messagesQueue?.Clear();
        }

        private void LogReceivedMessage(UdpReceiveResult result)
        {
            _messagesQueue?.Add(result.Buffer);
        }

        private void OnRestart()
        {
            networkManager.Restart();
            logText.text = "";
        }
    }
}