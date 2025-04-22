using System;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameVisualManager : NetworkBehaviour
{
    #region Parameter
    [Header("Prefabs")]
    [SerializeField]
    private GameObject previewStageCard;

    [SerializeField]
    private GameObject card2DPrefab;

    [SerializeField]
    private GameObject card3DPrefab;

    [Header("3D card draw animator obj")]
    [SerializeField]
    private GameObject cardDeckAnimationObj;

    [SerializeField]
    private GameObject cardDeckAnimationObj_Enemy;

    [Header("UI Panel animator obj")]
    [SerializeField]
    private RockPaperScissorsUI RockPaperScissorsUI;

    [Header("2D UI")]
    public RectTransform uiRootObj;

    [SerializeField]
    private Transform cardSpawnPosition;

    [SerializeField]
    private GameObject handCardAlly;

    [SerializeField]
    private GameObject handCardEnemy;

    [SerializeField]
    private TextMeshProUGUI playerNameAlly;

    [SerializeField]
    private TextMeshProUGUI playerNameEnemy;

    [SerializeField]
    private TextMeshProUGUI playerHPAlly;

    [SerializeField]
    private TextMeshProUGUI playerHPEnemy;

    [SerializeField]
    private Image playerIconAlly;

    [SerializeField]
    private Image playerIconEnemy;

    [SerializeField]
    private Button TurnEndButton;

    [SerializeField]
    private Button ReDrawButton;

    [Header("Sprites")]
    [SerializeField]
    private Sprite cardBackImgCheer;
    [SerializeField]
    private Sprite cardBackImgMain;
    [SerializeField]
    private Sprite cardBackImgMainFlip;

    [Header("Ally stage position Obj")]
    [SerializeField]
    private GameObject[] stagePositionAlly;

    [Header("Enemy stage position Obj")]
    [SerializeField]
    private GameObject[] stagePositionEnemy;

    [Header("Number display on stage (Ally)")]
    [SerializeField]
    private TextMeshProUGUI mainDeckNumTextAlly;
    [SerializeField]
    private TextMeshProUGUI cheerDeckNumTextAlly;
    [SerializeField]
    private TextMeshProUGUI holoPowerNumTextAlly;

    [Header("Number display on stage (Enemy)")]
     [SerializeField]
    private TextMeshProUGUI mainDeckNumTextEnemy;
    [SerializeField]
    private TextMeshProUGUI cheerDeckNumTextEnemy;
    [SerializeField]
    private TextMeshProUGUI holoPowerNumTextEnemy;
    #endregion

    private void Start()
    {
        handCardAlly.SetActive(false);
        handCardEnemy.SetActive(false);
        previewStageCard.SetActive(false);

        // The line must to execute in Start(), because it will not throw exception while excute in Awale()
        GameManager.Instance.OnHoverStagePosition += GameManager_OnHoverStagePosition;
        GameManager.Instance.UnHoverStagePosition += GameManager_UnHoverStagePosition;
        GameManager.Instance.OnGameSet += GameManager_OnGameSet;
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnPlaceCardOnStagePosition += GameManager_OnPlaceCardOnStagePosition;
        GameManager.Instance.OnCurrentPlayablePlayerTypeChange += GameManager_OnCurrentPlayablePlayerTypeChange;
        GameManager.Instance.OnDetermineCurrentPlayablePlayerType += GameManager_OnDetermineCurrentPlayablePlayerType;

        ReDrawButton.onClick.AddListener(() => {
            GameManager.Instance.MulliganAllCardRpc((int)GameManager.Instance.GetLocalPlayerType());
        });
    }

    private void GameManager_OnDetermineCurrentPlayablePlayerType(object sender, GameManager.OnDetermineCurrentPlayablePlayerTypeEventArgs e){
        int allyCode = (int)GameManager.Instance.GetLocalPlayerType() - 1;
        int enemyCode = allyCode == 0 ? 1 : 0;
        //Debug.Log($"allyCode-{allyCode} enemyCode-{enemyCode}");
        // Name
        playerNameAlly.text = GameManager.Instance.GetPlayersName()[allyCode];
        playerNameEnemy.text = GameManager.Instance.GetPlayersName()[enemyCode];
        // Icon
        playerIconAlly.sprite = TextureToSprite.ConvertTextureToSprite(
            GameResourcesManager.Instance.imageDictionary[GameManager.Instance.GetPlayerOshiCard()[allyCode]]);
        playerIconEnemy.sprite = TextureToSprite.ConvertTextureToSprite(
            GameResourcesManager.Instance.imageDictionary[GameManager.Instance.GetPlayerOshiCard()[enemyCode]]);

        string hintString = "";
        if(e.isTie && e.winner == (int)GameManager.PlayerType.None) {
            hintString = "Choose one";
            RockPaperScissorsUI.DisplayPanel(hintString, e.isTie, true);
            //Debug.Log($"Tie: {e.isTie}, win: true");
        } else if(e.isTie && e.winner == (int)GameManager.PlayerType.Spectator) {
            hintString = "Tie";
            RockPaperScissorsUI.DisplayPanel(hintString, e.isTie, false);
            //Debug.Log($"Tie: {e.isTie}, win: true");
        } else if (!e.isTie && e.winner == (int)GameManager.Instance.GetLocalPlayerType()) {
            hintString = "You win!\nChoose to go first or second.";
            RockPaperScissorsUI.DisplayPanel(hintString, e.isTie, true);
            //Debug.Log($"Tie: {e.isTie}, win: true");
        } else if (!e.isTie && e.winner != (int)GameManager.Instance.GetLocalPlayerType()) {
            hintString = "You lose!\nWait for the opponent to choose.";
            RockPaperScissorsUI.DisplayPanel(hintString, e.isTie, false);
            //Debug.Log($"Tie: {e.isTie}, win: false");
        }
        
    }

    private void GameManager_OnCurrentPlayablePlayerTypeChange(object sender, EventArgs e){
        //Update UI;
    }

    private void GameManager_OnPlaceCardOnStagePosition(
        object sender,
        GameManager.OnPlaceCardOnStagePositionEventArgs e
    )
    {
        if (e.playerType == (int)GameManager.Instance.GetLocalPlayerType())
        {
            //Debug.Log($"{e.playerType} Change visual.");
            Renderer renderer = stagePositionAlly[e.stagePosition].GetComponent<Renderer>();
            StagePosition stagePosition = stagePositionAlly[e.stagePosition].GetComponent<StagePosition>();
                string cardType = GameResourcesManager.Instance.cardDataDictionary[e.cardId].cardType;
            if (stagePosition != null)
            {
                if(cardType.Contains("ホロメン")){
                    stagePosition.SetStagePositionData(
                        (int)GameManager.Instance.GetLocalPlayerType(),
                        e.cardId
                    );
                } else if(cardType.Contains("ファン")){
                    stagePosition.AddSupportCardToMemberCard(0, e.cardId);
                } else if(cardType.Contains("マスコット")){
                    stagePosition.AddSupportCardToMemberCard(1, e.cardId);
                } else if(cardType.Contains("ツール")){
                    stagePosition.AddSupportCardToMemberCard(2, e.cardId);
                } else {
                    // Staff, Event, Item
                }
            }
            if (renderer != null)
            {
                //Debug.Log("Change image 3D ");
                // Change the main texture of the material
                if(cardType.Contains("ホロメン")){
                    renderer.material.mainTexture = GameResourcesManager.Instance.imageDictionary[
                        e.cardId
                    ];
                }
                handCardAlly
                    .GetComponent<CardContainer>()
                    .DestroyCard(GameManager.Instance.GetCurrentPickedCard(true));
            }
        }
        else
        {
            Renderer renderer = stagePositionEnemy[e.stagePosition].GetComponent<Renderer>();
            StagePosition stagePosition = stagePositionEnemy[
                e.stagePosition
            ].GetComponent<StagePosition>();
            if (stagePosition != null)
            {
                stagePosition.SetStagePositionData(
                    (int)GameManager.Instance.GetLocalPlayerType(),
                    e.cardId
                );
            }
            if (renderer != null)
            {
                renderer.material.mainTexture = GameResourcesManager.Instance.imageDictionary[
                    e.cardId
                ];
                //handCardAlly.GetComponent<CardContainer>().DestroyCard(GameManager.Instance.GetCurrentPickedCard(true));
            }
        }
        //GameManager.Instance.GetCurrentPickedCard(true).gameObject.SetActive(false);
        //Destroy(GameManager.Instance.GetCurrentPickedCard(true));
    }

    private void GameManager_OnHoverStagePosition(
        object sender,
        GameManager.OnHoverStagePositionEventArgs e
    )
    {
        //Debug.Log($"Hover: {e.cardId}");
        Sprite sprite = Sprite.Create(
            GameResourcesManager.Instance.imageDictionary[e.cardId],
            new Rect(
                0,
                0,
                GameResourcesManager.Instance.imageDictionary[e.cardId].width,
                GameResourcesManager.Instance.imageDictionary[e.cardId].height
            ),
            new Vector2(0.5f, 0.5f)
        );
        previewStageCard.GetComponent<Image>().sprite = sprite;
        previewStageCard.GetComponent<Image>().color = new Color(255, 255, 255, 255);
        // Reset sprite in previewStageCard
    }

    private void GameManager_UnHoverStagePosition(object sender, EventArgs e)
    {
        previewStageCard.GetComponent<Image>().color = new Color(255, 255, 255, 0);
        // Reset sprite in previewStageCard
    }

    private void GameManager_OnGameStarted(object sender, EventArgs e)
    {
        //Debug.Log($"Do card first draw : {GameManager.Instance.GetLocalPlayerType()}");
        RockPaperScissorsUI.Hide();
        handCardAlly.SetActive(true);
        handCardEnemy.SetActive(true);
        previewStageCard.SetActive(true);
        //if(GameManager.Instance.GetLocalPlayerType() != e.senderPlayer || !GameManager.Instance.GameDataPrepareFinished()) return;
        var abDrawData = GameManager.Instance.GetPlayersDraw();
        int[] playerFirstDrawAlly = GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.PlayerA ? abDrawData[0] : abDrawData[1];
        int[] playerFirstDrawEnemy = GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.PlayerA ? abDrawData[1] : abDrawData[0];
        /*if(abDrawData[1].Length > 0) {Debug.Log($"PlayerB draw: {string.Join(" ", abDrawData[1])}");}
        else {Debug.Log($"PlayerB draw: null");}
        
        if(abDrawData[0].Length > 0) {Debug.Log($"PlayerA draw: {string.Join(" ", abDrawData[0])}");}
        else {Debug.Log($"PlayerA draw: null");}*/

        // Generate ally hand card
        Sprite newSprite = null;
        foreach (int index in playerFirstDrawAlly)
        {
            // Convert position of card deck to world position.
            Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(
                cardDeckAnimationObj.transform.position
            ); 
            // Convert to position of canvas.
            Vector2 WorldObject_ScreenPosition =
                ViewportPosition * uiRootObj.GetComponent<CanvasScaler>().referenceResolution; 
            //Debug.Log($"Stop draw animation.");
            //cardDrawAnimationObjPrefab.transform.GetComponent<Animator>().SetBool(GameManager.Instance.is3D ? "Draw3D" : "Draw2D", false);

            string cardId = GameManager.Instance.GetLocalPlayerDeck()[index];

            // Create a sprite from the loaded Texture2D
            //Debug.Log($"Card[{index}] {cardId}");
            /*Texture2D texture = GameResourcesManager.Instance.imageDictionary[cardId];
            newSprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );*/

            // Create a new card.
            GameObject g = Instantiate(
                card2DPrefab,
                WorldObject_ScreenPosition,
                transform.rotation
            );
            g.GetComponent<CardBase>().id = cardId;
            g.GetComponent<CardBase>().indexInDeck = index;
            g.GetComponent<CardBase>().location = (int)GameManager.PositionOnStage.HandCard;
            g.transform.GetComponent<Image>().sprite = TextureToSprite.ConvertTextureToSprite(GameResourcesManager.Instance.imageDictionary[cardId]);
            /*if (newSprite != null)
            {
                g.transform.GetComponent<Image>().sprite = newSprite;
            }*/

            // Set parent of new card
            g.transform.SetParent(handCardAlly.transform);
        }

        // Generate enemy hand card
        foreach (int index in playerFirstDrawEnemy)
        {
            // Convert position of card deck to world position.
            Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(
                cardDeckAnimationObj_Enemy.transform.position
            ); 
            // Convert to position of canvas.
            Vector2 WorldObject_ScreenPosition =
                ViewportPosition * uiRootObj.GetComponent<CanvasScaler>().referenceResolution;
            // Create a new card.
            GameObject g = Instantiate(
                card2DPrefab,
                WorldObject_ScreenPosition,
                transform.rotation
            );
            g.GetComponent<CardBase>().id = "";
            g.GetComponent<CardBase>().indexInDeck = index;
            g.GetComponent<CardBase>().location = (int)GameManager.PositionOnStage.HandCard;
            if (cardBackImgMain != null)
            {
                g.transform.GetComponent<Image>().sprite = cardBackImgMain;
            }

            // Set parent of new card
            g.transform.SetParent(handCardEnemy.transform);
        }

        int allyCode = (int)GameManager.Instance.GetLocalPlayerType() - 1;
        int enemyCode = allyCode == 0 ? 1 : 0;
        //Debug.Log($"allyCode-{allyCode} enemyCode-{enemyCode}");
        // Name
        /*playerNameAlly.text = GameManager.Instance.GetPlayersName()[allyCode];
        playerNameEnemy.text = GameManager.Instance.GetPlayersName()[enemyCode];
        // Icon
        playerIconAlly.sprite = TextureToSprite.ConvertTextureToSprite(
            GameResourcesManager.Instance.imageDictionary[GameManager.Instance.GetPlayerOshiCard()[allyCode]]);
        playerIconEnemy.sprite = TextureToSprite.ConvertTextureToSprite(
            GameResourcesManager.Instance.imageDictionary[GameManager.Instance.GetPlayerOshiCard()[enemyCode]]);*/
        // Life
        playerHPAlly.text = GameResourcesManager.Instance.cardDataDictionary[GameManager.Instance.GetPlayerOshiCard()[allyCode]].HP;
        playerHPEnemy.text = GameResourcesManager.Instance.cardDataDictionary[GameManager.Instance.GetPlayerOshiCard()[enemyCode]].HP;
        // Oshi card    
        GameObject allyOshiGameObj = stagePositionAlly[(int)GameManager.PositionOnStage.Oshi];
        if (allyOshiGameObj != null)
        {
            // Change the main texture of the material
            allyOshiGameObj.GetComponent<Renderer>().material.mainTexture = GameResourcesManager.Instance.imageDictionary[
                GameManager.Instance.GetPlayerOshiCard()[allyCode]
            ];
            allyOshiGameObj.GetComponent<StagePosition>().SetStagePositionData(
                (int)GameManager.Instance.GetLocalPlayerType(),
                GameManager.Instance.GetPlayerOshiCard()[allyCode]
            );
        }
        GameObject enemyOshiGameObj = stagePositionEnemy[(int)GameManager.PositionOnStage.Oshi];
        if (enemyOshiGameObj != null)
        {
            // Change the main texture of the material
            enemyOshiGameObj.GetComponent<Renderer>().material.mainTexture = TextureToSprite.ConvertSpriteToTexture2D(cardBackImgMainFlip);
            /*enemyOshiGameObj.GetComponent<Renderer>().material.mainTexture = GameResourcesManager.Instance.imageDictionary[
                GameManager.Instance.GetPlayerOshiCard()[enemyCode]
            ];*/
            enemyOshiGameObj.GetComponent<StagePosition>().SetStagePositionData(
                (int)GameManager.Instance.GetLocalPlayerType(),
                GameManager.Instance.GetPlayerOshiCard()[enemyCode]
            );
        }
        
    }

    private void GameManager_OnGameSet(object sender, GameManager.OnGameSetEventArgs e)
    {
        if ((int)GameManager.Instance.GetLocalPlayerType() != e.senderPlayer)
            return;

        //Debug.Log($"{GameManager.Instance.GetLocalPlayerType()}: Get first draw data.");
    }
}
