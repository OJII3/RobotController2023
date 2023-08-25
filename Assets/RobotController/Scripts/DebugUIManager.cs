using TMPro;
using UnityEngine;

namespace RobotController
{
    public class DebugUIManager : MonoBehaviour
    {
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private TMP_Text localEpText;
        [SerializeField] private TMP_Text logText;

        private void Start()
        {
            localEpText.text = $"Listener EP: {networkManager.LocalEndPoint}";
        }

        private void Update()
        {
            if (networkManager.RemoteEndPoint != null)
                logText.text = $"\nTarget EP: {networkManager.RemoteEndPoint}";
        }
    }
}