using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class StagePosition : MonoBehaviour
{
    [SerializeField] private int playerType;
    public int positionOnStage;
    private string cardId;
    private GameManager.CardState cardState;
    private int damageTaken;
    private List<string> cheerList;
    private List<string> fanList;
    private List<string> mascotList;
    private List<string> toolList;

    private void Start() {
        cardId = "";
        cardState = GameManager.CardState.FaceDown;

        fanList = new();
        mascotList = new();
        toolList = new();
    }

    public void GetStagePositionData(
        out int playerType,
        out int positionOnStage,
        out string cardId,
        out GameManager.CardState cardState,
        out List<string> cheerList,
        out List<string> fanList,
        out List<string> mascotList,
        out List<string> toolList)
    {
        playerType = this.playerType;
        positionOnStage = this.positionOnStage;
        cardId = this.cardId;
        cardState = this.cardState;

        cheerList = this.cheerList;
        fanList = this.fanList;
        mascotList = this.mascotList;
        toolList = this.toolList;
    }

    public void SetStagePositionData(int playerType, string cardId){
        this.playerType = playerType;
        this.cardId = cardId;
    }

    /// <summary>
    /// cardType: 0->Fan, 1->Mascot, 2->Tool
    /// </summary>
    /// <returns></returns>
    public void AddSupportCardToMemberCard(int cardType, string cardId){
        if(cardType == 0) this.fanList.Add(cardId);
        else if(cardType == 1) this.mascotList.Add(cardId);
        else if(cardType == 2) this.toolList.Add(cardId);
    }

    public bool PositionIsEmpty() {
        return cardId == "";
    }

    /// <summary>
    /// [0] - Fan, [1] - Mascot, [2] - Tool
    /// </summary>
    /// <returns></returns>
    public int[] GetPositionSupportCardNum(){
        return new int[]{fanList.Count(), mascotList.Count(), toolList.Count()};
    }
}
