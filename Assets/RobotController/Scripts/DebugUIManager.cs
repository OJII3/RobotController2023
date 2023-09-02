using System.Collections.Generic;
using System.Net;
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
        [SerializeField] private Button clearButton;
        [SerializeField] private Button startStopButton;
        [SerializeField] private Toggle logModeToggle;
        private readonly List<byte[]> _messagesQueue = new();
        private bool _isLogModeString = true;
        private TMP_Text _startStopButtonText;

        private void Awake()
        {
            logText.text = "";
            _startStopButtonText = startStopButton.GetComponentInChildren<TMP_Text>();

            networkManager.OnConnectionChanged += OnConnectionChanged;
            networkManager.OnReceived += LogReceivedMessage;
            clearButton.onClick.AddListener(OnClear);
            startStopButton.onClick.AddListener(OnStartStop);
            logModeToggle.onValueChanged.AddListener(b => _isLogModeString = b);
        }

        private void Start()
        {
            localEndPointText.text = $"L: {networkManager.LocalEndPoint}";
            _startStopButtonText.text = "Start";
        }

        private void Update()
        {
            _messagesQueue?.ForEach(bytes =>
            {
                logText.text =
                    $"{(_isLogModeString ? Encoding.ASCII.GetString(bytes) : string.Join(",", bytes))}\n{logText.text}";
            });
            _messagesQueue?.Clear();
        }

        private void OnConnectionChanged(Connection connection, IPEndPoint ipEndPoint)
        {
            switch (connection)
            {
                case Connection.Connected:
                    connectionStateImage.color = Color.green;
                    remoteEndPointText.text = $"R: {ipEndPoint}";
                    break;
                case Connection.Disconnected:
                    connectionStateImage.color = Color.red;
                    remoteEndPointText.text = "R: NULL";
                    break;
                default:
                    connectionStateImage.color = Color.white;
                    break;
            }
        }

        private void LogReceivedMessage(UdpReceiveResult result)
        {
            _messagesQueue?.Add(result.Buffer);
        }

        private void OnClear()
        {
            logText.text = "";
            _messagesQueue?.Clear();
        }

        private void OnStartStop()
        {
            if (_startStopButtonText.text == "Start")
            {
                networkManager.StartNetwork();
                _startStopButtonText.text = "Stop";
            }
            else if (_startStopButtonText.text == "Stop")
            {
                networkManager.Stop();
                _startStopButtonText.text = "Start";
            }
        }
    }
}