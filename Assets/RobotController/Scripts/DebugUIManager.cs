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

        private void Awake()
        {
            ResetUI();
            networkManager.OnConnected += ep => remoteEndPointText.text = $"R: {ep}";
            networkManager.OnDisconnected += () => remoteEndPointText.text = "R: NULL";
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
                _ => Color.black
            };

            if (networkManager.RemoteEndPoint != null)
                logText.text = $"\nTarget EP: {networkManager.RemoteEndPoint}";
        }

        private void ResetUI()
        {
            localEndPointText.text = "L: NULL";
            remoteEndPointText.text = "R: NULL";
            connectionStateImage.color = Color.black;
            logText.text = "";
        }
    }
}