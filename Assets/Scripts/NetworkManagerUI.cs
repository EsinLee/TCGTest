using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button startServerButton;
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;
    [SerializeField] private InputField playerNameInputField;
    [SerializeField] private InputField serverAddressInputField;
    [SerializeField] private InputField serverPortInputField;
    private string ipAddress;
    private ushort ipAddressPort;
    private UnityTransport transport;
    [SerializeField] OpeningSceneManager openingSceneManager;

    private void Awake()
    {
        startServerButton.onClick.AddListener(() =>
        {
            ipAddress = serverAddressInputField.text;
            ConvertStringToUshort(serverPortInputField.text, out bool isConvertError, out ushort returnResult);
            if(isConvertError){
                ipAddressPort = 7777;
            }
            else {
                ipAddressPort = returnResult;
            }
            Debug.Log("Start server.");
            //GameManager.Instance.SetPlayerPorfile(playerNameInputField.text);
            SetIpAddress();
		    //GetLocalIPAddress();
            NetworkManager.Singleton.StartServer();
            
            //DebugConsoleForBuildWindow.Instance.Log($"===============================\nCurrent IP: {transport.ConnectionData.Address}: {transport.ConnectionData.Port}\n===============================");

            Hide();
            
            if(openingSceneManager != null){
                //openingSceneManager.SwitchToPlayScene(2);
                openingSceneManager.DisplayGameMatchPanel();
            }
        });

        startHostButton.onClick.AddListener(() =>
        {
            ipAddress = serverAddressInputField.text;
            ConvertStringToUshort(serverPortInputField.text, out bool isConvertError, out ushort returnResult);
            if(isConvertError){
                ipAddressPort = 7777;
            }
            else {
                ipAddressPort = returnResult;
            }
            Debug.Log("Start host.");
            //GameManager.Instance.SetPlayerPorfile(playerNameInputField.text);
            SetIpAddress();
		    //GetLocalIPAddress();
            NetworkManager.Singleton.StartHost();
            
            //DebugConsoleForBuildWindow.Instance.Log($"===============================\nCurrent IP: {transport.ConnectionData.Address}: {transport.ConnectionData.Port}\n===============================");

            Hide();

            if(openingSceneManager != null){
                //openingSceneManager.SwitchToPlayScene(2);
                openingSceneManager.DisplayGameMatchPanel();
            }
        });
        
        startClientButton.onClick.AddListener(() =>
        {
            ipAddress = serverAddressInputField.text;
            ConvertStringToUshort(serverPortInputField.text, out bool isConvertError, out ushort returnResult);
            if(isConvertError){
                ipAddressPort = 7777;
            }
            else {
                ipAddressPort = returnResult;
            }
            Debug.Log("Start client.");
            //GameManager.Instance.SetPlayerPorfile(playerNameInputField.text);
            SetIpAddress();
            NetworkManager.Singleton.StartClient();
            
            //DebugConsoleForBuildWindow.Instance.Log($"===============================\nCurrent IP: {transport.ConnectionData.Address}: {transport.ConnectionData.Port}\n===============================");

            Hide();

            if(openingSceneManager != null){
                //openingSceneManager.SwitchToPlayScene(2);
                openingSceneManager.DisplayGameMatchPanel();
            }
        });
    }

    void Start()
    {
        /*
        // Get the TelepathyTransport component
        var telepathyTransport = NetworkManager.Singleton.GetComponent<TelepathyTransport>();

        // Set the IP address and port
        telepathyTransport.port = 7777;

        // Enable the TelepathyTransport
        telepathyTransport.enabled = true;

        // Disable the default UnityTransport
        NetworkManager.Singleton.GetComponent<UnityTransport>().enabled = false;
*/
        serverAddressInputField.text = "127.0.0.1";
        serverPortInputField.text = "7777";
        ipAddress = serverAddressInputField.text;
        ConvertStringToUshort(serverPortInputField.text, out bool isConvertError, out ushort returnResult);
        if(isConvertError){
            ipAddressPort = 7777;
        }
        else {
            ipAddressPort = returnResult;
        }
		SetIpAddress(); // Set the Ip to the above address
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    /* Sets the Ip Address of the Connection Data in Unity Transport
	to the Ip Address which was input in the Input Field */
	// ONLY FOR CLIENT SIDE
	public void SetIpAddress() {
		transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
		transport.ConnectionData.Address = ipAddress;
		transport.ConnectionData.Port = ipAddressPort;
	}

    /* Gets the Ip Address of your connected network and
	shows on the screen in order to let other players join
	by inputing that Ip in the input field */
	// ONLY FOR HOST SIDE 
	public string GetLocalIPAddress() {
		var host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (var ip in host.AddressList) {
			if (ip.AddressFamily == AddressFamily.InterNetwork) {
                Debug.Log("===============================");
                Debug.Log($"Host IP: {ip.ToString()}");
                Debug.Log("===============================");
                //DebugConsoleForBuildWindow.Instance.Log("===============================");
                //DebugConsoleForBuildWindow.Instance.Log($"Current IP: {ipAddress}: {ipAddressPort}");
                //DebugConsoleForBuildWindow.Instance.Log("===============================");
				ipAddress = ip.ToString();
				return ip.ToString();
			}
		}
		throw new System.Exception("No network adapters with an IPv4 address in the system!");
	}

    private void ConvertStringToUshort(string str, out bool isError, out ushort returnResult){
        ushort result;
        if (ushort.TryParse(str, out result))
        {
            isError = false;
            returnResult = result;
        }
        else
        {
            isError = true;
            returnResult = result;
        }
    }
}
