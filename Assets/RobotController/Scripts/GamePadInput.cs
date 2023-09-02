using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RobotController
{
    [RequireComponent(typeof(PlayerInput))]
    public class GamePadInput : MonoBehaviour
    {
        [SerializeField] private NetworkManager networkManager;

        private readonly string _actionButtonEast = "ButtonEast";
        private readonly string _actionButtonNorth = "ButtonNorth";
        private readonly string _actionButtonSouth = "ButtonSouth";
        private readonly string _actionButtonWest = "ButtonWest";
        private readonly string _actionDPadDown = "DPadDown";
        private readonly string _actionDPadLeft = "DPadLeft";
        private readonly string _actionDPadRight = "DPadRight";
        private readonly string _actionDPadUp = "DPadUp";
        private readonly string _actionL1 = "L1";
        private readonly string _actionL2 = "L2";
        private readonly string _actionLeftStickPress = "LeftStickPress";
        private readonly string _actionLeftStickX = "LeftStickX";
        private readonly string _actionLeftStickY = "LeftStickY";
        private readonly string _actionR1 = "R1";
        private readonly string _actionR2 = "R2";
        private readonly string _actionRightStickPress = "RightStickPress";
        private readonly string _actionRightStickX = "RightStickX";
        private readonly string _actionRightStickY = "RightStickY";
        private readonly string _actionSelect = "Select";
        private readonly string _actionStart = "Start";

        private readonly byte _footerChar = Encoding.ASCII.GetBytes("E")[0];

        private readonly byte _headerChar = Encoding.ASCII.GetBytes("S")[0];

        // { header, header, buttons, buttons, buttons, buttons, left stick x,
        // left stick y, right stick x, right stick y, footer, footer }
        private readonly byte[]
            _messageData =
                new byte[13];

        private PlayerInput _playerInput;

        private void Awake()
        {
            TryGetComponent(out _playerInput);
        }

        private void Update()
        {
            _messageData[0] = 0;
            _messageData[1] = _headerChar;
            _messageData[2] = _headerChar;

            _messageData[3] = 0;
            _messageData[3] +=
                (byte)(_playerInput.actions[_actionRightStickPress].IsInProgress() ? 0b0000_0001 : 0b0000_0000);
            _messageData[3] +=
                (byte)(_playerInput.actions[_actionLeftStickPress].IsInProgress() ? 0b0000_0010 : 0b0000_0000);
            _messageData[3] +=
                (byte)(_playerInput.actions[_actionSelect].IsInProgress() ? 0b0000_0100 : 0b0000_0000);
            _messageData[3] +=
                (byte)(_playerInput.actions[_actionStart].IsInProgress() ? 0b0000_1000 : 0b0000_0000);

            _messageData[4] = 0;
            _messageData[4] +=
                (byte)(_playerInput.actions[_actionR2].IsInProgress() ? 0b0000_0001 : 0b0000_0000);
            _messageData[4] +=
                (byte)(_playerInput.actions[_actionL2].IsInProgress() ? 0b0000_0010 : 0b0000_0000);
            _messageData[4] +=
                (byte)(_playerInput.actions[_actionR1].IsInProgress() ? 0b0000_0100 : 0b0000_0000);
            _messageData[4] +=
                (byte)(_playerInput.actions[_actionL1].IsInProgress() ? 0b0000_1000 : 0b0000_0000);

            _messageData[5] = 0;
            _messageData[5] +=
                (byte)(_playerInput.actions[_actionButtonWest].IsInProgress() ? 0b0000_0001 : 0b0000_0000);
            _messageData[5] +=
                (byte)(_playerInput.actions[_actionButtonSouth].IsInProgress() ? 0b0000_0010 : 0b0000_0000);
            _messageData[5] +=
                (byte)(_playerInput.actions[_actionButtonEast].IsInProgress() ? 0b0000_0100 : 0b0000_0000);
            _messageData[5] +=
                (byte)(_playerInput.actions[_actionButtonNorth].IsInProgress() ? 0b0000_1000 : 0b0000_0000);

            _messageData[6] = 0;
            _messageData[6] +=
                (byte)(_playerInput.actions[_actionDPadLeft].IsInProgress() ? 0b0000_0001 : 0b0000_0000);
            _messageData[6] +=
                (byte)(_playerInput.actions[_actionDPadDown].IsInProgress() ? 0b0000_0010 : 0b0000_0000);
            _messageData[6] +=
                (byte)(_playerInput.actions[_actionDPadRight].IsInProgress() ? 0b0000_0100 : 0b0000_0000);
            _messageData[6] +=
                (byte)(_playerInput.actions[_actionDPadUp].IsInProgress() ? 0b0000_1000 : 0b0000_0000);

            _messageData[7] = (byte)(_playerInput.actions[_actionLeftStickX].ReadValue<float>() * 50);
            _messageData[8] = (byte)(_playerInput.actions[_actionLeftStickY].ReadValue<float>() * 50);
            _messageData[9] = (byte)(_playerInput.actions[_actionRightStickX].ReadValue<float>() * 50);
            _messageData[10] = (byte)(_playerInput.actions[_actionRightStickY].ReadValue<float>() * 50);

            _messageData[11] = _footerChar;
            _messageData[12] = _footerChar;

            networkManager.UpdateSendBuffer(_messageData);
        }
    }
}