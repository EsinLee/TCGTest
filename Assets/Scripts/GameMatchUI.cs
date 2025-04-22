using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameMatchUI : MonoBehaviour
{
    [SerializeField]
    private Button BackButton;

    [SerializeField]
    private GameObject networkManagerUI;

    [SerializeField]
    private Button StartGameButton;

    [SerializeField]
    private Animator LoadingAnime;

    [Header("Player A")]
    [SerializeField]
    private Image PlayerAIcon;

    [SerializeField]
    private TextMeshProUGUI PlayerAName;
    private Text PlayerAState;

    [Header("Player B")]
    [SerializeField]
    private Image PlayerBIcon;

    [SerializeField]
    private TextMeshProUGUI PlayerBName;
    private Text PlayerBState;
    private bool gameReady;

    private void Awake() { }

    private void Start()
    {
        gameReady = false;
        LoadingAnime.SetBool("IsReady", false);
        StartGameButton.GetComponentInChildren<TextMeshProUGUI>().text = "Waiting...";
        StartGameButton.GetComponent<Image>().color = new Color(
            200f / 255f,
            200f / 255f,
            200f / 255f,
            1
        );
        BackButton.onClick.AddListener(() =>
        {
            if (networkManagerUI != null)
            {
                NetworkManager.Singleton.Shutdown();
                gameObject.SetActive(false);
                networkManagerUI.SetActive(true);
                UpdateConnectionStatusUI();
            }
        });
        StartGameButton.onClick.AddListener(() =>
        {
            //if (gameReady) GameManager.Instance.TriggerOnGameStartedRpc();
            if (gameReady) GameManager.Instance.TriggerDetermineCurrentPlayablePlayerTypeRpc();
        });

        GameManager.Instance.OnGameSet += GameManager_OnGameSet;
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnected; // 本機為Host或Server, 當Client斷線時觸發
        NetworkManager.Singleton.OnServerStopped += NetworkManager_OnServerStopped; // 本機為Client，當Host或Server斷線時觸發
        GameManager.Instance.OnDetermineCurrentPlayablePlayerType += GameManager_OnDetermineCurrentPlayablePlayerType;
    }

    private void GameManager_OnDetermineCurrentPlayablePlayerType(object sender, GameManager.OnDetermineCurrentPlayablePlayerTypeEventArgs e){
        Hide();
    }

    private void GameManager_OnGameSet(object sender, GameManager.OnGameSetEventArgs e)
    {
        //Debug.Log($"Change UI player name, 0:{e.playersName[0]} - 1:{e.playersName[1]}");
        PlayerAName.text = e.playersName[0] == "" ? "Waiting Player ..." : e.playersName[0];
        PlayerBName.text = e.playersName[1] == "" ? "Waiting Player ..." : e.playersName[1];
        Debug.Log($"PlayerA Name: {PlayerAName.text} - PlayerB Name: {PlayerBName.text}");
        if (
            NetworkManager.Singleton.ConnectedClientsList.Count >= 2
            && e.playersName[0] != ""
            && e.playersName[1] != ""
        )
        {
            UpdateConnectionStatusUI();
        }
    }

    private void GameManager_OnGameStarted(object sender, EventArgs e)
    {
        Debug.Log("Game Start.");
    }

    private void NetworkManager_OnClientDisconnected(ulong clientId)
    {
        UpdateConnectionStatusUI();
    }
    private void NetworkManager_OnServerStopped(bool isHost) {
        Debug.Log("Server or host disconnected.");
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void UpdateConnectionStatusUI()
    {
        int connectedClients = NetworkManager.Singleton.ConnectedClientsList.Count;
        string connectedClientsListString = "";
        foreach(NetworkClient c in NetworkManager.Singleton.ConnectedClientsList) {
            connectedClientsListString += (c.ClientId + " - ");
        }
        Debug.Log($"Connected devices count:{connectedClients}");
        Debug.Log($"Connected device{connectedClientsListString}");

        if(connectedClients <= 1){
            if(GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.PlayerA){
                PlayerBName.text = "";
            }
            else if(GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.PlayerB){
                PlayerAName.text = "";
            }

            LoadingAnime.SetBool("IsReady", false);
            StartGameButton.GetComponentInChildren<TextMeshProUGUI>().text = "Waiting...";
            StartGameButton.GetComponent<Image>().color = new Color(
                200f / 255f,
                200f / 255f,
                200f / 255f,
                1
            );
            gameReady = false;
        } else {
            LoadingAnime.SetBool("IsReady", true);
            StartGameButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start";
            StartGameButton.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            gameReady = true;
        }
    }

    private void OnDestroy()
    {
        //NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnected;
        //NetworkManager.Singleton.OnServerStopped -= NetworkManager_OnServerStopped;
    }
}
