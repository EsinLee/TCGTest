using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SFB;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public partial class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Main camera")]
    [SerializeField]
    private CameraRayCast cameraRayCast;
    private SoundManager soundManager;
    public bool is3D;
    private string[] playersName;
    private string[] playersOshi;
    private int[] playerADrawsIndexList;
    private int[] playerBDrawsIndexList;
    private int[][] playersDrawsIndexList;
    private string playerNameTemp;
    int[] playersRPSResult; // initialize array data without 0 to 2.
    private CardWrapper currentPickedAllyCard; // Temporary card data (for place on the stage or attack)
    private CardWrapper currentPickedEnemyCard; // Temporary card data (for attack)
    #region Card
    private string playerAOshiCard;
    private List<CardBase> playerAHandCard;
    private CardBase[] playerADeckTemp;
    private CardBase[] playerACheerDeckTemp;
    private string[] playerADeck;
    private string[] playerACheerDeck;

    private string playerBOshiCard;
    private List<CardBase> playerBHandCard;
    private CardBase[] playerBDeckTemp;
    private CardBase[] playerBCheerDeckTemp;
    private string[] playerBDeck;
    private string[] playerBCheerDeck;
    #endregion
    #region Game play record paramter
    private int[] playerTurnRecord;
    private bool playerHasUsedALimitedtedCard;
    #endregion
    #region Serializer
    // Serialize CardBase[]
    private static void SerializeCardBase<T>(ref CardBase cardBase, BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        string id = string.Empty;
        int indexInDeck = 0;
        int location = 0;

        if (serializer.IsReader)
        {
            serializer.SerializeValue(ref id);
            serializer.SerializeValue(ref indexInDeck);
            serializer.SerializeValue(ref location);
            cardBase = new CardBase(id, indexInDeck, location);
        }
        else
        {
            id = cardBase.id;
            indexInDeck = cardBase.indexInDeck;
            location = cardBase.location;
            serializer.SerializeValue(ref id);
            serializer.SerializeValue(ref indexInDeck);
            serializer.SerializeValue(ref location);
        }
    }

    // Serialize string[] to NetworkString
    public struct playerDataToNetworkStringArrayOnNetworkSpawn : INetworkSerializable
    {
        public string[] playerDeck;
        public CardBase[] playerDeckTemp;
        public string[] playerCheerDeck;
        public CardBase[] playerCheerDeckTemp;
        public string[] playersName;
        public string[] playersOshi;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer)
            where T : IReaderWriter
        {
            // Serialize playerDeck
            var playerDeckLength = 0;
            if (!serializer.IsReader)
                playerDeckLength = playerDeck.Length;
            serializer.SerializeValue(ref playerDeckLength);
            if (serializer.IsReader)
                playerDeck = new string[playerDeckLength];
            for (var n = 0; n < playerDeckLength; ++n)
            {
                serializer.SerializeValue(ref playerDeck[n]);
            }

            // Serialize playerDeck
            var playerDeckTempLength = 0;
            if (!serializer.IsReader)
                playerDeckTempLength = playerDeckTemp.Length;
            serializer.SerializeValue(ref playerDeckTempLength);
            if (serializer.IsReader)
                playerDeckTemp = new CardBase[playerDeckTempLength];
            for (var n = 0; n < playerDeckTempLength; ++n)
            {
                SerializeCardBase(ref playerDeckTemp[n], serializer);
            }

            // Serialize playerCheerDeck
            var playerCheerDeckLength = 0;
            if (!serializer.IsReader)
                playerCheerDeckLength = playerCheerDeck.Length;
            serializer.SerializeValue(ref playerCheerDeckLength);
            if (serializer.IsReader)
                playerCheerDeck = new string[playerCheerDeckLength];
            for (var n = 0; n < playerCheerDeckLength; ++n)
            {
                serializer.SerializeValue(ref playerCheerDeck[n]);
            }

            // Serialize playerCheerDeckTemp
            var playerCheerDeckTempLength = 0;
            if (!serializer.IsReader)
                playerCheerDeckTempLength = playerCheerDeckTemp.Length;
            serializer.SerializeValue(ref playerCheerDeckTempLength);
            if (serializer.IsReader)
                playerCheerDeckTemp = new CardBase[playerCheerDeckTempLength];
            for (var n = 0; n < playerCheerDeckTempLength; ++n)
            {
                SerializeCardBase(ref playerCheerDeckTemp[n], serializer);
            }

            // Serialize playersName
            var playersNameLength = 0;
            if (!serializer.IsReader)
                playersNameLength = playersName.Length;
            serializer.SerializeValue(ref playersNameLength);
            if (serializer.IsReader)
                playersName = new string[playersNameLength];
            for (var n = 0; n < playersNameLength; ++n)
            {
                serializer.SerializeValue(ref playersName[n]);
            }

            // Serialize playersOshi
            var playersOshiLength = 0;
            if (!serializer.IsReader)
                playersOshiLength = playersOshi.Length;
            serializer.SerializeValue(ref playersOshiLength);
            if (serializer.IsReader)
                playersOshi = new string[playersOshiLength];
            for (var n = 0; n < playersOshiLength; ++n)
            {
                serializer.SerializeValue(ref playersOshi[n]);
            }
        }
    }

    public struct deckDataToNetworkStringArrayOnMulliganAllCard : INetworkSerializable
    {
        public CardBase[] playerDeckTemp;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer)
            where T : IReaderWriter
        {
            // Serialize playerDeck
            var playerDeckTempLength = 0;
            if (!serializer.IsReader)
                playerDeckTempLength = playerDeckTemp.Length;
            serializer.SerializeValue(ref playerDeckTempLength);
            if (serializer.IsReader)
                playerDeckTemp = new CardBase[playerDeckTempLength];
            for (var n = 0; n < playerDeckTempLength; ++n)
            {
                SerializeCardBase(ref playerDeckTemp[n], serializer);
            }
        }
    }
    #endregion

    private PlayerType localPlayerType;
    private NetworkVariable<PlayerType> currentPlayablePlayerType = new NetworkVariable<PlayerType>();

    private void InitializeGame(){
        playersRPSResult = new int[2]{9, 9};
    }

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Debug.Log("More than 1 GridGM instance!");
            DestroyImmediate(gameObject);
        }

        playersName = new string[] { "", "" };
        playersOshi = new string[] { "", "" };
        playersDrawsIndexList = new int[2][] {new int[]{99}, new int[]{99}};
        playerADrawsIndexList = new int[] {};
        playerBDrawsIndexList = new int[] {};
        playerNameTemp = "";
        playersRPSResult = new int[2]{9, 9};
        playerTurnRecord = new int[2]{-1, -1};
        playerHasUsedALimitedtedCard = false;

        playerAHandCard = new List<CardBase>();
        playerADeckTemp = new CardBase[50];
        playerACheerDeckTemp = new CardBase[20];
        playerADeck = new string[50];
        playerACheerDeck = new string[20];

        playerBHandCard = new List<CardBase>();
        playerBDeckTemp = new CardBase[50];
        playerBCheerDeckTemp = new CardBase[20];
        playerBDeck = new string[50];
        playerBCheerDeck = new string[20];
    }

    private void Start(){
        cameraRayCast = GameObject.Find("Main Camera").GetComponent<CameraRayCast>();
    }

    private void Update(){
        TriggerOnHoverStagePosition();
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            localPlayerType = PlayerType.PlayerA;
        }
        else if (NetworkManager.Singleton.LocalClientId == 1)
        {
            localPlayerType = PlayerType.PlayerB;
        }
        else
        {
            localPlayerType = PlayerType.Spectator;
        }

        Debug.Log(
            $"LocalClient ID is : {NetworkManager.Singleton.LocalClientId}, you are {localPlayerType}"
        );
        /*DebugConsoleForBuildWindow.Instance.Log(
            $"LocalClient ID is : {NetworkManager.Singleton.LocalClientId}, you are {localPlayerType}"
        );*/

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        currentPlayablePlayerType.OnValueChanged += (PlayerType oldPlayerType, PlayerType newPlayerType) => {
            OnCurrentPlayablePlayerTypeChange?.Invoke(this, EventArgs.Empty);
        };

        PreloadDeckAndFirstDrawRpc(
            (int)localPlayerType,
            GameResourcesManager.Instance.GetPlayerSelectedDeck(),
            playerNameTemp
        );

        /*
        // Update score data
        playerCrossScore.OnValueChanged += (int prevScore, int newScore) => {
            OnScoreChanged?.Invoke(this, EventArgs.Empty);
        };
        playerCircleScore.OnValueChanged += (int prevScore, int newScore) => {
            OnScoreChanged?.Invoke(this, EventArgs.Empty);
        };*/
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count >= 2)
        {
            // Game initialize (set first playable player as None.)
            currentPlayablePlayerType.Value = PlayerType.None;
        }
    }

    private void SerializeJsonContent(
        string path,
        out string oshi,
        out string[] mainDeck,
        out string[] cheerDeck
    )
    {
        // 讀取JSON文件內容
        string jsonContent = File.ReadAllText(path);
        // 解析JSON並轉換為陣列
        var jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<CustomDeckJsonModel>(jsonContent);

        List<string> mainDeckList = new();
        List<string> cheerDeckList = new();

        foreach(KeyValuePair<string, int> data in jsonData.MainDeck){
            for(int i = 0; i < data.Value; i++){
                mainDeckList.Add(data.Key);
            }
        }
        foreach(KeyValuePair<string, int> data in jsonData.CheerDeck){
            for(int i = 0; i < data.Value; i++){
                cheerDeckList.Add(data.Key);
            }
        }

        oshi = jsonData.Oshi;
        mainDeck = mainDeckList.ToArray();
        cheerDeck = cheerDeckList.ToArray();
    }

    private void LoadPlayerDeck(
        string playerDeckJsonPath,
        int playerAHandCardCount,
        ref string playerOshiCard,
        ref CardBase[] playerDeckTemp,
        ref CardBase[] playerCheerDeckTemp,
        ref string[] playerDeck,
        ref string[] playerCheerDeck
    )
    {
        //Debug.Log($"{localPlayerType} : Loading custom deck.");
        //DebugConsoleForBuildWindow.Instance.Log($"{localPlayerType} : Loading custom deck.");
        SerializeJsonContent(
            playerDeckJsonPath,
            out string oshi,
            out string[] mainDeck,
            out string[] cheerDeck
        );
        playerOshiCard = oshi;
        playerDeckTemp = ConvertIdsToCardListAndShuffle(mainDeck);
        playerCheerDeckTemp = ConvertIdsToCardListAndShuffle(cheerDeck);
        playerDeck = mainDeck;
        playerCheerDeck = cheerDeck;
        //Debug.Log($"Player:{localPlayerType} - oshi: {playerOshiCard} - MainDeck[{playerDeck.Length}] - CheerDeck[{playerCheerDeck.Length}]");
        //DebugConsoleForBuildWindow.Instance.Log($"Player:{localPlayerType} - oshi: {playerOshiCard} - MainDeck[{playerDeck.Length}] - CheerDeck[{playerCheerDeck.Length}]");
    }

    [Rpc(SendTo.Server)]
    private void PreloadDeckAndFirstDrawRpc(
        int senderPlayer,
        string jsonPath,
        string playerNameTemp
    )
    {
        // Save players deck on server.
        if (senderPlayer == (int)PlayerType.PlayerA)
        {
            LoadPlayerDeck(
                jsonPath,
                playerAHandCard.Count,
                ref playerAOshiCard,
                ref playerADeckTemp,
                ref playerACheerDeckTemp,
                ref playerADeck,
                ref playerACheerDeck
            );
        }
        else if (senderPlayer == (int)PlayerType.PlayerB)
        {
            LoadPlayerDeck(
                jsonPath,
                playerBHandCard.Count,
                ref playerBOshiCard,
                ref playerBDeckTemp,
                ref playerBCheerDeckTemp,
                ref playerBDeck,
                ref playerBCheerDeck
            );
        }

        // Generate first card draw index. (It will be executed twice.)
        List<int> randomNumbers = GetRandomNumbers(0, 49, 7);
        string randNum = "First draw (" + senderPlayer + ") - " + string.Join(" ", randomNumbers);
        //Debug.Log(randNum);

        // Modify data in deck array. ( Deck -> Hand )
        Debug.Log($"senderPlayer: {(PlayerType)senderPlayer} - A_Length: {playerADrawsIndexList.Length} - B_Length: {playerBDrawsIndexList.Length}");
        if (senderPlayer == (int)PlayerType.PlayerA && playerADrawsIndexList.Length != 7)
        {
            Debug.Log("Modify player deck temp A");
            playerADrawsIndexList = randomNumbers.ToArray();
            string ccc = DebutCardInHandCards(playerADeckTemp, playerADrawsIndexList) ? "Debut card in hand." : "No Debut card in hand.";
            Debug.Log($"Hand result >>> {ccc}");
            playersDrawsIndexList[0] = playerADrawsIndexList;
            foreach (int index in randomNumbers)
            {
                playerADeckTemp[index].location = (int)PositionOnStage.HandCard;
            }
        }
        else if (senderPlayer == (int)PlayerType.PlayerB && playerBDrawsIndexList.Length != 7)
        {
            Debug.Log("Modify player deck temp B");
            playerBDrawsIndexList = randomNumbers.ToArray();
            playersDrawsIndexList[1] = playerBDrawsIndexList;
            foreach (int index in randomNumbers)
            {
                playerBDeckTemp[index].location = (int)PositionOnStage.HandCard;
            }
        }

        // Save players name on server.
        if (senderPlayer == (int)PlayerType.PlayerA)
        {
            playersName[0] = playerNameTemp == "" ? "Player A" : playerNameTemp;
        }
        else if (senderPlayer == (int)PlayerType.PlayerB)
        {
            playersName[1] = playerNameTemp == "" ? "Player B" : playerNameTemp;
        }

        // Save players oshi on server.
        if (senderPlayer == (int)PlayerType.PlayerA)
        {
            playersOshi[0] = playerAOshiCard == "" ? "hBP01-007_OUR" : playerAOshiCard;
        }
        else if (senderPlayer == (int)PlayerType.PlayerB)
        {
            playersOshi[1] = playerBOshiCard == "" ? "hBP01-007_OUR" : playerBOshiCard;
        }

        int[] randomNumbersToArray_SendToClient = randomNumbers.ToArray();
        TriggerOnGameSetRpc(
            senderPlayer,
            new playerDataToNetworkStringArrayOnNetworkSpawn()
            {
                playerDeck = senderPlayer == (int)PlayerType.PlayerA ? playerADeck : playerBDeck,
                playerDeckTemp =
                    senderPlayer == (int)PlayerType.PlayerA ? playerADeckTemp : playerBDeckTemp,
                playerCheerDeck =
                    senderPlayer == (int)PlayerType.PlayerA ? playerACheerDeck : playerBCheerDeck,
                playerCheerDeckTemp =
                    senderPlayer == (int)PlayerType.PlayerA
                        ? playerACheerDeckTemp
                        : playerBCheerDeckTemp,
                playersName = playersName,
                playersOshi = playersOshi
            },
            playerADrawsIndexList,
            playerBDrawsIndexList
        );
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameSetRpc(
        int senderPlayer,
        playerDataToNetworkStringArrayOnNetworkSpawn deckStringArrayToNetworkStringArray,
        int[] _playerADrawsIndexList,
        int[] _playerBDrawsIndexList
    )
    {
        // Save player name into local array
        playersName = deckStringArrayToNetworkStringArray.playersName;
        playersOshi = deckStringArrayToNetworkStringArray.playersOshi;
        /*DebugConsoleForBuildWindow.Instance.Log(
            $"Local: {GetLocalPlayerType()} - Sender: {senderPlayer} - Origin[1]({playersName[1]}) to New[1]({deckStringArrayToNetworkStringArray.playersName[1]}) \n Origin[{playersName[0]},{playersName[1]}], New[{deckStringArrayToNetworkStringArray.playersName[0]},{deckStringArrayToNetworkStringArray.playersName[1]}]"
        );*/
        Debug.Log(
            $"Local: {GetLocalPlayerType()} - Sender: {senderPlayer} - Origin[1]({playersName[1]}) to New[1]({deckStringArrayToNetworkStringArray.playersName[1]}) \n Origin[{playersName[0]},{playersName[1]}], New[{deckStringArrayToNetworkStringArray.playersName[0]},{deckStringArrayToNetworkStringArray.playersName[1]}]"
        );
        
        //playersDrawsIndexList = _playersDrawsIndexList;
        //Debug.Log("OnGameSetRpc _playersDrawsIndexList[0] -" + _playerADrawsIndexList.Length);
        //Debug.Log("OnGameSetRpc _playersDrawsIndexList[1] -" + _playerBDrawsIndexList.Length);
        playerADrawsIndexList = _playerADrawsIndexList;
        playerBDrawsIndexList = _playerBDrawsIndexList;
        playersDrawsIndexList[0] = _playerADrawsIndexList;
        playersDrawsIndexList[1] = _playerBDrawsIndexList;

        // Save deck and cheerdeck data to client and Modify data in deck array.
        if (CanClientModifyData(senderPlayer, (int)PlayerType.PlayerA))
        {
            playerADeck = deckStringArrayToNetworkStringArray.playerDeck;
            playerADeckTemp = deckStringArrayToNetworkStringArray.playerDeckTemp;
            playerACheerDeck = deckStringArrayToNetworkStringArray.playerCheerDeck;
            playerACheerDeckTemp = deckStringArrayToNetworkStringArray.playerCheerDeckTemp;
        }
        else if (CanClientModifyData(senderPlayer, (int)PlayerType.PlayerB))
        {
            playerBDeck = deckStringArrayToNetworkStringArray.playerDeck;
            playerBDeckTemp = deckStringArrayToNetworkStringArray.playerDeckTemp;
            playerBCheerDeck = deckStringArrayToNetworkStringArray.playerCheerDeck;
            playerBCheerDeckTemp = deckStringArrayToNetworkStringArray.playerCheerDeckTemp;
        }
        else
        {
            if (senderPlayer == (int)PlayerType.PlayerA)
            {
                //playersName = deckStringArrayToNetworkStringArray.playersName;
                playerADeck = deckStringArrayToNetworkStringArray.playerDeck;
                playerADeckTemp = deckStringArrayToNetworkStringArray.playerDeckTemp;
                playerACheerDeck = deckStringArrayToNetworkStringArray.playerCheerDeck;
                playerACheerDeckTemp = deckStringArrayToNetworkStringArray.playerCheerDeckTemp;
            }
            else if (senderPlayer == (int)PlayerType.PlayerB)
            {
                //playersName = deckStringArrayToNetworkStringArray.playersName;
                playerBDeck = deckStringArrayToNetworkStringArray.playerDeck;
                playerBDeckTemp = deckStringArrayToNetworkStringArray.playerDeckTemp;
                playerBCheerDeck = deckStringArrayToNetworkStringArray.playerCheerDeck;
                playerBCheerDeckTemp = deckStringArrayToNetworkStringArray.playerCheerDeckTemp;
            }
        }

        OnGameSet?.Invoke(
            this,
            new OnGameSetEventArgs
            {
                senderPlayer = senderPlayer,
                playersName = deckStringArrayToNetworkStringArray.playersName,
                playersOshi = deckStringArrayToNetworkStringArray.playersOshi,
            }
        );
    }

    bool DebutCardInHandCards(CardBase[] mainDeckCardArray, int[] drawCardIdIndexList){
        bool result = false;
        foreach(int drawnCardId in drawCardIdIndexList){
            if(GameResourcesManager.Instance.cardDataDictionary[mainDeckCardArray[drawnCardId].id].BloomLevel == "Debut"){
                result = true;
                break;
            }
        }
        return result;
    }

    List<int> GetRandomNumbers(int minValue, int maxValue, int count)
    {
        List<int> numbers = new List<int>();
        for (int i = minValue; i <= maxValue; i++)
        {
            numbers.Add(i);
        }
        System.Random rand = new System.Random();
        int n = numbers.Count;
        while (n > 1)
        {
            n--;
            int k = rand.Next(n + 1);
            int value = numbers[k];
            numbers[k] = numbers[n];
            numbers[n] = value;
        }
        return numbers.GetRange(0, count);
    }

    /// <summary>
    /// Initialize deck and shuffle.(reset card id, index of deck, position of stage)
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    CardBase[] ConvertIdsToCardListAndShuffle(string[] ids)
    {
        CardBase[] originData = new CardBase[ids.Length];
        for (int i = 0; i < ids.Length; i++)
        {
            originData[i] = new CardBase(ids[i], i, (int)PositionOnStage.Deck);
        }
        // Shuffle
        System.Random random = new System.Random();
        CardBase[] result = originData.OrderBy(x => random.Next()).ToArray();
        return result;
    }

    [Rpc(SendTo.Everyone)]
    public void TriggerDetermineCurrentPlayablePlayerTypeRpc() {
        OnDetermineCurrentPlayablePlayerType?.Invoke(this, new OnDetermineCurrentPlayablePlayerTypeEventArgs(){
            isTie = true
        });
    }
    /// <summary>
    /// 0->Rock, 1->Paper, 2->Scissors
    /// </summary>
    /// <param name="senderPlayerType"></param>
    /// <param name="RPSResult"></param>
    [Rpc(SendTo.Server)]
    public void DoRPSRpc(int senderPlayerType, int RPSResult){
        if(senderPlayerType == (int)PlayerType.PlayerA){
            playersRPSResult[0] = RPSResult;
        } else if(senderPlayerType == (int)PlayerType.PlayerB){
            playersRPSResult[1] = RPSResult;
        }

        int initialValue = 5; // Can not be 0-3
        if(playersRPSResult[0] < initialValue && playersRPSResult[1] < initialValue){
            int a = playersRPSResult[0];
            int b = playersRPSResult[1];
            if(a == b){
                // Set array playersRPSResult to 1 plus the initial value.
                // to avoid executing "GotRPSRpcResultRpc(true, (int)PlayerType.None);" -> (Beginning state)
                // and not goes into "if(playersRPSResult[0] < initialValue && playersRPSResult[1] < initialValue)" again.
                playersRPSResult[0] = initialValue + 1;
                playersRPSResult[1] = initialValue + 1;
                GotRPSRpcResultRpc(true, (int)PlayerType.Spectator);
            } else if((a > b && (a - b) < 2) 
                || (a < b && (b - a) == 2)){
                GotRPSRpcResultRpc(false, (int)PlayerType.PlayerA);
            } else {
                GotRPSRpcResultRpc(false, (int)PlayerType.PlayerB);
            }
        } else if(playersRPSResult[0] == initialValue && playersRPSResult[1] == initialValue){
            GotRPSRpcResultRpc(true, (int)PlayerType.None);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void GotRPSRpcResultRpc(bool isTie, int winner){
        OnDetermineCurrentPlayablePlayerType?.Invoke(this, new OnDetermineCurrentPlayablePlayerTypeEventArgs(){
            isTie = isTie,
            winner = winner
        });
    }

    [Rpc(SendTo.Server)]
    public void DetermineCurrentPlayablePlayerTypeRpc(int senderPlayerType, bool winnerWantsFirstMove){
        if(winnerWantsFirstMove){
            currentPlayablePlayerType.Value = (PlayerType)senderPlayerType;
            TriggerOnGameStartedRpc();
        } else {
            currentPlayablePlayerType.Value = (PlayerType)senderPlayerType == PlayerType.PlayerA ? PlayerType.PlayerB : PlayerType.PlayerA;
            TriggerOnGameStartedRpc();
        }
    }

    [Rpc(SendTo.Server)]
    public void MulliganAllCardRpc(int senderPlayerType){
        if(senderPlayerType == (int)PlayerType.PlayerA){
            // Get number of currenr card drawn.(if equals 1, then lose. if greater than 1, decrease by 1.)
            int cardDrawOffset = playerTurnRecord[0] == -1 ? 0 : 1;
            int cardDrawn = playerADrawsIndexList.Length + cardDrawOffset;
            if(cardDrawn <= 1){
                Debug.Log($"Game over - {(PlayerType)senderPlayerType} lose!!");
            } else {
                cardDrawn -= cardDrawOffset;
                playerTurnRecord[0] = 0;
            }

            PlayerCardReDrawn(
                senderPlayerType,
                ref senderPlayerType == (int)PlayerType.PlayerA ? ref playerADeckTemp : ref playerBDeckTemp,
                ref senderPlayerType == (int)PlayerType.PlayerA ? ref playerADeck : ref playerBDeck,
                ref senderPlayerType == (int)PlayerType.PlayerA ? ref playerADrawsIndexList : ref playerBDrawsIndexList,
                ref senderPlayerType == (int)PlayerType.PlayerA ? ref playersDrawsIndexList[0] : ref playersDrawsIndexList[1],
                cardDrawn
            );
        } else if(senderPlayerType == (int)PlayerType.PlayerB){
            // Get number of currenr card drawn.(if equals 1, then lose. if greater than 1, decrease by 1.)
            int cardDrawOffset = playerTurnRecord[1] == -1 ? 0 : 1;
            int cardDrawn = playerBDrawsIndexList.Length + cardDrawOffset;
            if(cardDrawn <= 1){
                Debug.Log($"Game over - {(PlayerType)senderPlayerType} lose!!");
            } else {
                cardDrawn -= cardDrawOffset;
                playerTurnRecord[1] = 0;
            }

            PlayerCardReDrawn(
                senderPlayerType,
                ref senderPlayerType == (int)PlayerType.PlayerA ? ref playerADeckTemp : ref playerBDeckTemp,
                ref senderPlayerType == (int)PlayerType.PlayerA ? ref playerADeck : ref playerBDeck,
                ref senderPlayerType == (int)PlayerType.PlayerA ? ref playerADrawsIndexList : ref playerBDrawsIndexList,
                ref senderPlayerType == (int)PlayerType.PlayerA ? ref playersDrawsIndexList[0] : ref playersDrawsIndexList[1],
                cardDrawn
            );
        }

        TriggerOnMulliganAllCardRpc(
            senderPlayerType,
            new deckDataToNetworkStringArrayOnMulliganAllCard()
            {
                playerDeckTemp =
                    senderPlayerType == (int)PlayerType.PlayerA ? playerADeckTemp : playerBDeckTemp
            },
            senderPlayerType == (int)PlayerType.PlayerA ? playerADrawsIndexList : playerBDrawsIndexList
        );
    }

    private void PlayerCardReDrawn(
        int senderPlayerType,
        ref CardBase[] playerDeckTemp,
        ref string[] playerDeck,
        ref int[] playerDrawsIndexList,
        ref int[] playersDrawsIndexList,
        int numberOfReDrawn
    ){
        // Initialize data in deck array. ( AnyType -> Deck ). a
        // Shuffle the main deck and cheer deck
        playerDeckTemp = ConvertIdsToCardListAndShuffle(playerDeck);
        //playerBCheerDeckTemp = ConvertIdsToCardListAndShuffle(playerBCheerDeck);
        // Generate card drawn index. (It will be executed twice.)
        List<int> randomNumbers = GetRandomNumbers(0, 49, numberOfReDrawn);
        string randNum = "Redraw (" + senderPlayerType + ") - " + string.Join(" ", randomNumbers);

        // Modify data in deck array. ( Deck -> Hand )
        Debug.Log($"Modify player deck temp {senderPlayerType}");
        playerDrawsIndexList = randomNumbers.ToArray();
        playersDrawsIndexList = playerDrawsIndexList;
        foreach (int index in randomNumbers)
        {
            playerDeckTemp[index].location = (int)PositionOnStage.HandCard;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnMulliganAllCardRpc(
        int senderPlayer,
        deckDataToNetworkStringArrayOnMulliganAllCard deckDataArrayToNetworkStringArray,
        int[] _playerDrawsIndexList
    ){
        /*DebugConsoleForBuildWindow.Instance.Log(
            $"Mulligan - Local: {GetLocalPlayerType()} - Sender: {senderPlayer} - Origin[1]({playersName[1]}) to New[1]({deckDataArrayToNetworkStringArray.playerDeckTemp[1]})"
        );*/

        // Save deck data to client and Modify data in deck array.
        if (CanClientModifyData(senderPlayer, (int)PlayerType.PlayerA))
        {
            playerADrawsIndexList = _playerDrawsIndexList;
            playersDrawsIndexList[0] = _playerDrawsIndexList;
            playerADeckTemp = deckDataArrayToNetworkStringArray.playerDeckTemp;
            Debug.Log($"{(PlayerType)senderPlayer} - {string.Join(" ", _playerDrawsIndexList)}");
        }
        else if (CanClientModifyData(senderPlayer, (int)PlayerType.PlayerB))
        {
            playerBDrawsIndexList = _playerDrawsIndexList;
            playersDrawsIndexList[1] = _playerDrawsIndexList;
            playerBDeckTemp = deckDataArrayToNetworkStringArray.playerDeckTemp;
            Debug.Log($"{(PlayerType)senderPlayer} - {string.Join(" ", _playerDrawsIndexList)}");
        }
        else
        {
            if (senderPlayer == (int)PlayerType.PlayerA)
            {
                playerADrawsIndexList = _playerDrawsIndexList;
                playersDrawsIndexList[0] = _playerDrawsIndexList;
                playerADeckTemp = deckDataArrayToNetworkStringArray.playerDeckTemp;
            }
            else if (senderPlayer == (int)PlayerType.PlayerB)
            {
                playerBDrawsIndexList = _playerDrawsIndexList;
                playersDrawsIndexList[1] = _playerDrawsIndexList;
                playerBDeckTemp = deckDataArrayToNetworkStringArray.playerDeckTemp;
            }
        }
/*
        OnMulliganAllCard?.Invoke(
            this,
            new OnMulliganAllCardEventArgs
            {
                senderPlayer = senderPlayer,
            }
        );*/
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void TriggerMolliganAllCardRpc(
        playerDataToNetworkStringArrayOnNetworkSpawn deckStringArrayToNetworkStringArray,
        int[] _playerADrawsIndexList,
        int[] _playerBDrawsIndexList){

    }

    [Rpc(SendTo.Everyone)]
    public void TriggerOnGameStartedRpc()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    #region Place card
    [Rpc(SendTo.Server)]
    public void TriggerOnHandCardDragEndRpc(
        int senderPlayerType,
        string cardId,
        int cardIndexInDeck,
        bool isLegalPosition,
        int positionOnStage
    )
    {
        if((PlayerType)senderPlayerType != currentPlayablePlayerType.Value) {
            // Not player's turn.
            return;
        }

        // Check the card place in legal position.
        if (isLegalPosition)
        {
            TriggerOnPlaceCardOnStagePositionRpc(
                senderPlayerType,
                cardId,
                cardIndexInDeck,
                positionOnStage
            );
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void TriggerOnPlaceCardOnStagePositionRpc(
        int senderPlayerType,
        string cardId,
        int cardIndexInDeck,
        int stagePosition
    )
    {
        // 1. Delete the placed card from hand cards. (VisualManager)
        // 2. Modify stage position of deck array. (GameManager)
        // 3. Change target texture of stage position. (VisualManager)
        if (CanClientModifyData(senderPlayerType, (int)PlayerType.PlayerA))
        {
            playerADeckTemp[cardIndexInDeck].location = stagePosition; // Modify stage position of deck array.
        }
        else if (CanClientModifyData(senderPlayerType, (int)PlayerType.PlayerB))
        {
            playerBDeckTemp[cardIndexInDeck].location = stagePosition; // Modify stage position of deck array.
        }
        OnPlaceCardOnStagePosition?.Invoke(
            this,
            new OnPlaceCardOnStagePositionEventArgs
            {
                stagePosition = stagePosition,
                playerType = (int)senderPlayerType,
                cardId = cardId,
                cardIndexInDeck = cardIndexInDeck
            }
        );
    }
    #endregion

    public void TriggerOnHoverStagePosition()
    {
        if (
            cameraRayCast != null
            && cameraRayCast.hittedGameObject.transform != null
            && (
                cameraRayCast.hittedGameObject.transform.tag == "SelfStage_Placeable"
                || cameraRayCast.hittedGameObject.transform.tag == "SelfStage_Oshi"
                || cameraRayCast.hittedGameObject.transform.tag == "EnemyStage_Placeable"
                || cameraRayCast.hittedGameObject.transform.tag == "EnemyStage_Oshi"
            )
            && cameraRayCast.cursorOnLegalObject
        )
        {
            cameraRayCast.hittedGameObject.transform
                .GetComponent<StagePosition>()
                .GetStagePositionData(
                    out int playerType,
                    out int positionOnStage,
                    out string cardId,
                    out CardState cardState,
                    out List<string> cheerList,
                    out List<string> fanList,
                    out List<string> mascotList,
                    out List<string> toolList);
                //Debug.Log($"{(PlayerType)playerType} - {(PositionOnStage)positionOnStage} - Card Id: {cardId}");
            if(cardId != "" && cardState != CardState.FaceDown){
                OnHoverStagePosition?.Invoke(
                    this,
                    new OnHoverStagePositionEventArgs
                    {
                        position = positionOnStage,
                        playerType = playerType,
                        cardId = cardId,
                        cardState = cardState,
                        cheerList = cheerList.ToArray(),
                        fanList = fanList.ToArray(),
                        mascotList = mascotList.ToArray(),
                        toolList = toolList.ToArray(),
                    }
                );
            }
        }
        else
        {
            UnHoverStagePosition?.Invoke(this, EventArgs.Empty);
        }
    }

    public int GetHoverStagePosition()
    {
        return cameraRayCast.hittedGameObject.transform
            .GetComponent<StagePosition>()
            .positionOnStage;
    }

    public bool CheckTargetPlaceStagePositionLegal(bool targetPosMustBeAlly, string currentDraggedCardID)
    {
        bool result = false;
        if (targetPosMustBeAlly)
        {
            if (
                cameraRayCast != null
                && cameraRayCast.hittedGameObject.transform != null
                && cameraRayCast.hittedGameObject.transform.tag == "SelfStage_Placeable"
                && cameraRayCast.cursorOnLegalObject
            )
            {
                string currentDraggedCardType = GameResourcesManager.Instance.cardDataDictionary[currentDraggedCardID].cardType;
                bool isNoCardAtTheTargetPosition = cameraRayCast.hittedGameObject.transform.GetComponent<StagePosition>().PositionIsEmpty();
                if(currentDraggedCardType.Contains("ホロメン") 
                    && isNoCardAtTheTargetPosition){
                    result = true;
                } else if(currentDraggedCardType.Contains("サポート・ファン") 
                    && !isNoCardAtTheTargetPosition) {
                    result = true;
                } else if(currentDraggedCardType.Contains("サポート・マスコット") 
                    && cameraRayCast.hittedGameObject.transform.GetComponent<StagePosition>().GetPositionSupportCardNum()[1] < 1 
                    && !isNoCardAtTheTargetPosition) {
                    result = true;
                } else if(currentDraggedCardType.Contains("サポート・ツール") 
                    && cameraRayCast.hittedGameObject.transform.GetComponent<StagePosition>().GetPositionSupportCardNum()[2] < 1 
                    && !isNoCardAtTheTargetPosition) {
                    result = true;
                } else if(currentDraggedCardType.Contains("LIMITED") 
                    && !playerHasUsedALimitedtedCard 
                    && !isNoCardAtTheTargetPosition) {
                    result = true;
                } else if((currentDraggedCardType.Contains("サポート・スタッフ")
                        || currentDraggedCardType.Contains("サポート・イベント") 
                        || currentDraggedCardType.Contains("サポート・アイテム"))
                    && !currentDraggedCardType.Contains("LIMITED")  
                    && !isNoCardAtTheTargetPosition) {
                    result = true;
                }
            }
        }
        else
        {
            if (
                cameraRayCast != null
                && cameraRayCast.hittedGameObject.transform != null
                && cameraRayCast.hittedGameObject.transform.tag.Contains("Enemy")
                && cameraRayCast.cursorOnLegalObject
            )
            {
                /*Debug.Log(
                    $"hit: {cameraRayCast.hittedGameObject.transform.name}, {cameraRayCast.cursorOnLegalObject}"
                );*/
                result = true;
            }
        }
        return result;
    }

    private bool CanClientModifyData(int senderPlayer, int targetPlayer)
    {
        if (senderPlayer == targetPlayer && (int)localPlayerType == targetPlayer)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SetPlayerPorfile(string playerName)
    {
        //Debug.Log($"Set player name temp to {playerName}");
        playerNameTemp = playerName;
    }

    public string[] GetPlayersName()
    {
        return playersName;
    }

    public string[] GetPlayerOshiCard()
    {
        return playersOshi;
    }
    public void SetCurrentPickedCard(bool isAllyCard, CardWrapper cardEntity)
    {
        if (isAllyCard)
        {
            currentPickedAllyCard = cardEntity;
        }
        else
        {
            currentPickedEnemyCard = cardEntity;
        }
    }

    public CardWrapper GetCurrentPickedCard(bool isAllyCard)
    {
        if (isAllyCard)
        {
            return currentPickedAllyCard;
        }
        else
        {
            return currentPickedEnemyCard;
        }
    }

    public bool GameDataPrepareFinished()
    {
        return NetworkManager.Singleton.ConnectedClientsList.Count >= 2;
    }

    public PlayerType GetLocalPlayerType()
    {
        return localPlayerType;
    }

    public PlayerType GetCurrentPlayedPlayerType()
    {
        return currentPlayablePlayerType.Value;
    }

    public string[] GetLocalPlayerDeck()
    {
        if (localPlayerType == PlayerType.PlayerA)
        {
            return playerADeck;
        }
        else if (localPlayerType == PlayerType.PlayerB)
        {
            return playerBDeck;
        }
        else
        {
            return null;
        }
    }

    public int[][] GetPlayersDraw() {
        return playersDrawsIndexList;
        //return new int[2][] {playerADrawsIndexList, playerBDrawsIndexList};
    }

}
