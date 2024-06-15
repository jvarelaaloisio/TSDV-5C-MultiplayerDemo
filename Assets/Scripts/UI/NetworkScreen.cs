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

        [Header("Default values")]
        [SerializeField] private string ConnectBtnTxt = "Connect";
        [SerializeField] private string StartSvrBtnTxt = "Start Server";
        [SerializeField] private string StartHostBtnTxt = "Start Host";
        [SerializeField] private string DefaultIp = "127.0.0.1";
        [SerializeField] private int DefaultPort = 12345;
        [SerializeField] private string DefaultServerName = "server";
        [SerializeField] private string addressPlaceHolderTxt = "Enter IP Address...";
        [SerializeField] private string portPlaceHolderTxt = "Enter port...";
        [SerializeField] private string nicknamePlaceHolderTxt = "Enter nickname...";
    
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

        private void OnEnable()
        {
            NetworkManager.Instance.onConnectionSuccessful += HandleConnectionSuccessful;
            NetworkManager.Instance.onConnectionError += HandleConnectionError;
        }
        private void OnDisable()
        {
            if (NetworkManager.Instance == null)
                return;
            NetworkManager.Instance.onConnectionSuccessful -= HandleConnectionSuccessful;
            NetworkManager.Instance.onConnectionError -= HandleConnectionError;
        }

        protected override void Initialize()
        {
            if (buttonPrefab)
            {
                _connectBtn = Instantiate(buttonPrefab, buttonsParent);
                _connectBtn.name = "btn_Connect";
                _connectBtn.GetComponentInChildren<Text>().text = ConnectBtnTxt;
                _connectBtn.onClick.AddListener(HandleConnectBtnClick);
                
                _startServerBtn = Instantiate(buttonPrefab, buttonsParent);
                _startServerBtn.name = "btn_StartServer";
                _startServerBtn.GetComponentInChildren<Text>().text = StartSvrBtnTxt;
                _startServerBtn.onClick.AddListener(HandleStartServerBtnClick);
                
                _startHostBtn = Instantiate(buttonPrefab, buttonsParent);
                _startHostBtn.GetComponentInChildren<Text>().text = StartHostBtnTxt;
                _startHostBtn.name = "btn_StartHost";
                _startHostBtn.onClick.AddListener(HandleStartHostBtnClick);
            }

            if (inputFieldPrefab)
            {
                _addressInputField = Instantiate(inputFieldPrefab, inputFieldsParent);
                if (_addressInputField.placeholder is Text addressPlaceHolder)
                    addressPlaceHolder.text = addressPlaceHolderTxt;
                if (EventSystem.current)
                {
                    var currentEventSystem = EventSystem.current;
                    currentEventSystem.SetSelectedGameObject(_addressInputField.gameObject);
                }
                
                _portInputField = Instantiate(inputFieldPrefab, inputFieldsParent);
                if (_portInputField.placeholder is Text portPlaceHolder)
                    portPlaceHolder.text = portPlaceHolderTxt;
                
                _nickNameInputField = Instantiate(inputFieldPrefab, inputFieldsParent);
                if (_nickNameInputField.placeholder is Text nicknamePlaceHolder)
                    nicknamePlaceHolder.text = nicknamePlaceHolderTxt;
            }
        }

        private void HandleConnectBtnClick()
        {
            IPAddress ipAddress = string.IsNullOrWhiteSpace(_addressInputField.text)
                                    ? IPAddress.Parse(DefaultIp)
                                    : IPAddress.Parse(_addressInputField.text);
            GetPort(out var port);
            string nickName = _nickNameInputField.text;

            NetworkManager.Instance.StartClient(ipAddress, port, nickName);
        }

        private void HandleConnectionSuccessful(int clientId)
        {
            Debug.Log($"{name}: Connected as client. ID: {clientId}");
            SwitchToChatScreen();
        }

        private void HandleConnectionError(HandshakeResponseCodes result)
        {
            Debug.LogError($"{name}: Connection failed. Reason: {result}");
        }

        private void HandleStartServerBtnClick()
        {
            GetPort(out var port);
            NetworkManager.Instance.StartServer(port);
            SwitchToChatScreen();
        }
        
        private void HandleStartHostBtnClick()
        {
            GetPort(out var port);
            string nickName = _nickNameInputField.text;
            NetworkManager.Instance.StartHost(port, nickName);
            SwitchToChatScreen();
        }

        private void SwitchToChatScreen()
        {
            ChatScreen.Instance.gameObject.SetActive(true);
            this.gameObject.SetActive(false);
        }

        private void GetPort(out int port)
        {
            if (!int.TryParse(_portInputField.text, out port))
                port = DefaultPort;
        }
    }
}
