using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFB;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public partial class GameManager : NetworkBehaviour
{
    public enum PlayerType
    {
        None, // 無
        PlayerA, // 玩家A
        PlayerB, // 玩家B
        Spectator, // 觀戰
    }

    public enum CardState
    {
        Activate, // 活動
        Resting, // 休息
        FaceDown, // 覆蓋
    }

    public enum PositionOnStage
    {
        Deck, // 牌組
        CheerDeck, // 應援牌組
        Archive, // 棄牌
        Power, // 能量
        HP, // 生命值
        Oshi, // 主推
        Center, // 中心
        Collabo, // 合作
        BackPosA, // 後臺A
        BackPosB, // 後臺B
        BackPosC, // 後臺C
        BackPosD, // 後臺D
        BackPosE, // 後臺E
        HandCard, // 手牌
    }
    
    public event EventHandler<OnGameSetEventArgs> OnGameSet; // 遊戲前置
    public class OnGameSetEventArgs : EventArgs
    {
        public int senderPlayer;
        public string[] playersName;
        public string[] playersOshi;
    }
    public event EventHandler OnMulliganAllCard; // 重新洗牌
    public class OnMulliganAllCardEventArgs : EventArgs
    {
        public int senderPlayer;
    }
    public event EventHandler OnGameStarted; // 遊戲正式開始
    public event EventHandler<OnDetermineCurrentPlayablePlayerTypeEventArgs> OnDetermineCurrentPlayablePlayerType; // 爭奪先手決定權
    public class OnDetermineCurrentPlayablePlayerTypeEventArgs : EventArgs
    {
        public bool isTie; // 平手重來
        public int winner; // 贏方(PlayerType to int)
    }
    public event EventHandler<OnPlayerMulliganAllCardEventArgs> OnPlayerMulliganAllCard; // 爭奪先手決定權
    public class OnPlayerMulliganAllCardEventArgs : EventArgs
    {
        public int playerType;
        public int numberOfCardDrawn;
    }
    public event EventHandler OnCurrentPlayablePlayerTypeChange; // 回合交換
    public event EventHandler OnHPChanged; // 生命值改變
    public event EventHandler OnPowerChanged; // Holo能量改變
    public event EventHandler<OnHoverStagePositionEventArgs> OnHoverStagePosition; // 滑鼠經過物件上方(非拖動物件狀態)
    public event EventHandler UnHoverStagePosition; // 滑鼠離開物件上方(非拖動物件狀態)
    public event EventHandler OnGameDisconnected; // 遊戲斷線
    public event EventHandler OnGameWin; // 遊戲勝利

    public class OnHoverStagePositionEventArgs : EventArgs
    {
        public int position;
        public int playerType;
        public string cardId;
        public CardState cardState;
        public string[] cheerList;
        public string[] fanList;
        public string[] mascotList;
        public string[] toolList;
    }

    public event EventHandler<OnPlaceCardOnStagePositionEventArgs> OnPlaceCardOnStagePosition;

    public class OnPlaceCardOnStagePositionEventArgs : EventArgs
    {
        public int stagePosition;
        public int playerType;
        public string cardId;
        public int cardIndexInDeck;
    }
}
