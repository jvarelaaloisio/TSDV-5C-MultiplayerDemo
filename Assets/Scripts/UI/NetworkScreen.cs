using System.Net;
using Network;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class NetworkScreen : MonoBehaviourSingleton<NetworkScreen>
    {
        [Header("Buttons")]
        [SerializeField] private Button buttonPrefab;
        [SerializeField] private Transform buttonsParent;
        [Header("Input fields")]
        [SerializeField] private InputField inputFieldPrefab;
        [SerializeField] private Transform inputFieldsParent;
    
        private Button _connectBtn;
        private Button _startServerBtn;
        private Button _startHostBtn;
        private InputField _portInputField;
        private InputField _addressInputField;
        private InputField _nickNameInputField;

        protected override void Awake()
        {
            base.Awake();
            if (!buttonPrefab)
                Debug.LogError($"{name}: {nameof(buttonPrefab)} is null!");
            if (!inputFieldPrefab)
                Debug.LogError($"{name}: {nameof(inputFieldPrefab)} is null!");
        }

        protected override void Initialize()
        {
            if (buttonPrefab)
            {
                _connectBtn = Instantiate(buttonPrefab, buttonsParent);
                _connectBtn.name = "btn_Connect";
                _connectBtn.GetComponentInChildren<Text>().text = "Connect";
                _connectBtn.onClick.AddListener(HandleConnectBtnClick);
                
                _startServerBtn = Instantiate(buttonPrefab, buttonsParent);
                _startServerBtn.name = "btn_StartServer";
                _startServerBtn.GetComponentInChildren<Text>().text = "Start Server";
                _startServerBtn.onClick.AddListener(HandleStartServerBtnClick);
                
                _startHostBtn = Instantiate(buttonPrefab, buttonsParent);
                _startHostBtn.GetComponentInChildren<Text>().text = "Start Host";
                _startHostBtn.name = "btn_StartHost";
                _startHostBtn.onClick.AddListener(HandleStartHostBtnClick);
            }

            if (inputFieldPrefab)
            {
                _addressInputField = Instantiate(inputFieldPrefab, inputFieldsParent);
                if (_addressInputField.placeholder is Text addressPlaceHolder)
                    addressPlaceHolder.text = "Enter IP Address...";
                if (EventSystem.current)
                {
                    var currentEventSystem = EventSystem.current;
                    currentEventSystem.SetSelectedGameObject(_addressInputField.gameObject);
                }
                
                _portInputField = Instantiate(inputFieldPrefab, inputFieldsParent);
                if (_portInputField.placeholder is Text portPlaceHolder)
                    portPlaceHolder.text = "Enter port...";
                
                _nickNameInputField = Instantiate(inputFieldPrefab, inputFieldsParent);
                if (_nickNameInputField.placeholder is Text nicknamePlaceHolder)
                    nicknamePlaceHolder.text = "Enter nickname...";
            }
        }

        private void HandleConnectBtnClick()
        {
            IPAddress ipAddress =
                string.IsNullOrWhiteSpace(_addressInputField.text)? IPAddress.Parse("127.0.0.1") : IPAddress.Parse(_addressInputField.text);
            int port = System.Convert.ToInt32(_portInputField.text);
            string nickName = _nickNameInputField.text;

            NetworkManager.Instance.onConnectionSuccessful += HandleConnectionSuccessful;
            NetworkManager.Instance.onConnectionError += HandleConnectionError;
            NetworkManager.Instance.StartClient(ipAddress, port, nickName);
        }

        private void HandleConnectionSuccessful(int obj)
        {
            SwitchToChatScreen();
        }

        private void HandleConnectionError(HandshakeResponseCodes result)
        {
            Debug.LogError($"{name}: Connection failed. Reason: {result}");
        }

        private void HandleStartServerBtnClick()
        {
            int port = System.Convert.ToInt32(_portInputField.text);
            NetworkManager.Instance.StartServer(port);
            SwitchToChatScreen();
        }
        
        private void HandleStartHostBtnClick()
        {
            int port = System.Convert.ToInt32(_portInputField.text);
            string nickName = _nickNameInputField.text;
            NetworkManager.Instance.StartHost(port, nickName);
            SwitchToChatScreen();
        }

        private void SwitchToChatScreen()
        {
            ChatScreen.Instance.gameObject.SetActive(true);
            this.gameObject.SetActive(false);
        }
    }
}
