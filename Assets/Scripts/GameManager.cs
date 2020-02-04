using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks
{

    public static GameManager Instance;
    public int goalNum = 20;
    public List<PlayerManager> players;

    public Text logText;
    public Text PlayerOneText;
    public Text PlayerTwoText;

    private int nowNum;
    private enum GameState
    {
        Idle, Round, Turn, GameOver,Restart
    }


    private void Start()
    {
        Instance = this;
        Begin();
    }

    public int GetOtherPlayerId(int thisId)
    {
        int i;
        for (i = 0; i < players.Count; i++)
        {
            if (players[i].myId == thisId)
            {
                break;
            }
        }
        return players[(i + 1) % players.Count].myId;
    }

    #region UI

    public void OnClickStartBtn()
    {
        //1.若游戏<结束>，则游戏<开始>
        if((GameState)CustomProperties.GetRoomProp("state")==GameState.GameOver)
        {
            CustomProperties.SetRoomProp("state", GameState.Restart);
        }
    }

    #endregion

    #region Prop

    private PlayerManager.PlayerState GetPlayerState(int id)
    {
        return (PlayerManager.PlayerState)CustomProperties.GetPlayerProp("state", id);
    }
    private GameState GetRoomState()
    {
        return (GameState)CustomProperties.GetRoomProp("state");
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        Debug.Log("[玩家属性改变]" + targetPlayer.NickName + "的" + changedProps);
        object tempObj;
        //[玩家状态]
        if (changedProps.TryGetValue("state", out tempObj))
        {
            PlayerManager.PlayerState state = (PlayerManager.PlayerState)tempObj;
            Player other = PhotonNetwork.CurrentRoom.Players[CustomProperties.playersNumber[CustomProperties.playerOtherIdx]];
            switch (state)
            {
                case PlayerManager.PlayerState.Ready:
                    if (targetPlayer.IsLocal)
                    {
                        logText.text = "等待" + other.NickName + "准备";
                    }
                    SyncReady();
                    break;
                case PlayerManager.PlayerState.PlaceStar:
                    logText.text = "请放置星星";
                    break;
                case PlayerManager.PlayerState.PlaceComplete:
                    if (targetPlayer.IsLocal)
                    {
                        logText.text = "等待" + other.NickName + "放置星星";
                    }
                    SyncPlaceStar();
                    break;
                case PlayerManager.PlayerState.ChooseNumbers:
                    if (targetPlayer.IsLocal)
                    {
                        logText.text = "请选择数字";
                    }
                    break;
                case PlayerManager.PlayerState.Attack:
                    int nowNum = (int)CustomProperties.GetRoomProp("nowNum");
                    if (targetPlayer.IsLocal)
                    {
                        logText.text = "当前数字：" + nowNum + "/20" + "\n"
                        + "请攻击"+(nowNum-goalNum)+"次";
                        DealDamageFrom(targetPlayer.ActorNumber, nowNum - goalNum);
                    }
                    else
                    {
                        logText.text = "当前数字：" + nowNum + "/20" + "\n"
                        + targetPlayer.NickName + "正在攻击";
                    }
                    break;
                case PlayerManager.PlayerState.OthersTurn:
                    if (targetPlayer.IsLocal)
                    {
                        logText.text = "等待" + other.NickName + "操作";
                    }
                    break;
                default:
                    break;
            }
        }
        //[玩家星星位置]
        if (changedProps.TryGetValue("starPos", out tempObj))
        {
            Vector2 starPos = (Vector2)tempObj;
        }
        //[玩家所选数字]
        if (changedProps.TryGetValue("num", out tempObj))
        {
            int num = (int)tempObj;
            //裁判方累积数字
            if (PhotonNetwork.IsMasterClient)
            {
                //累积后判断是否爆掉
                if (AddNowNum(num, targetPlayer.ActorNumber))
                {
                    //爆掉设置房间属性[爆掉的玩家Number]
                    CustomProperties.SetRoomProp("exceedPlayer", targetPlayer.ActorNumber);
                }
                else
                {
                    //没爆掉就正常结束Turn
                    OnCompleteTurn();
                }

            }

        }
    }
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        object tempObj;
        Debug.Log("[房间属性改变]" + propertiesThatChanged);
        //[房间当前数字是否爆掉]
        if (propertiesThatChanged.TryGetValue("exceedPlayer", out tempObj))
        {
            int exceedPlayer = (int)tempObj;
            //如果爆掉了，就游戏结束
            TurnFinish(exceedPlayer);
        }
        //[房间状态]
        if(propertiesThatChanged.TryGetValue("state",out tempObj))
        {
            GameState state = (GameState)tempObj;
            switch(state)
            {
                case GameState.Restart:
                    Restart();
                    break;
                default:
                    break;
            }
        }
    }

    #endregion

    #region Sync

    public void SyncReady()
    {
        //1.两位玩家都是<已准备>状态
        //2.若游戏状态为<闲置>，切换为<回合内>
        if (GetPlayerState(0) == PlayerManager.PlayerState.Ready &&
            GetPlayerState(1) == PlayerManager.PlayerState.Ready &&
            GetRoomState() == GameState.Idle)
        {
            CustomProperties.SetRoomProp("state", GameState.Round);
            StartNewRound();
        }
    }
    public void SyncPlaceStar()
    {
        //1.两位玩家都是<已放置星星>状态
        //2.若游戏状态为<回合内>，切换为<轮内>
        if (GetPlayerState(0) == PlayerManager.PlayerState.PlaceComplete &&
            GetPlayerState(1) == PlayerManager.PlayerState.PlaceComplete &&
            GetRoomState() == GameState.Round)
        {
            CustomProperties.SetRoomProp("state", GameState.Turn);
            TurnBegin();
        }
    }

    #endregion

    #region Lobby

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        //Launcher场景
        SceneManager.LoadScene(0);
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        }
        Debug.Log("PhotonNetwork : Loading Level : " + PhotonNetwork.CurrentRoom.PlayerCount);
        PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("OnPlayerEnteredRoom() " + newPlayer.NickName);
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("OnPlayerEnteredRoom isMasterClient " + PhotonNetwork.IsMasterClient);
            LoadArena();
        }
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("OnPlayerLeftRoom() " + otherPlayer.NickName);
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("OnPlayerEnteredRoom isMasterClient " + PhotonNetwork.IsMasterClient);
            LoadArena();
        }
    }

    #endregion

    //心理战游戏
    #region Turn

    public void OnCompleteTurn()
    {
        //将回合交给下一位
        ChangeTurnToNext();
    }
    public bool AddNowNum(int val, int playerId)
    {
        int nowNum = (int)CustomProperties.GetRoomProp("nowNum");
        nowNum += val;
        CustomProperties.SetRoomProp("nowNum", nowNum);
        Debug.Log("nowNum is " + nowNum);
        //判断当前数字有没有爆掉
        return IsNowNumExceed();
    }//!
    private void ChangeTurnToNext()
    {
        //将当前玩家序号递增一位
        int currentIdx = (int)CustomProperties.GetRoomProp("currentPlayer");
        CustomProperties.SetRoomProp("currentPlayer", GetOtherPlayerId(currentIdx));
    }
    private void ChangeTurnTo(int playerNumber)
    {
        CustomProperties.SetRoomProp("currentPlayer", playerNumber);
    }
    private bool IsNowNumExceed()
    {
        //如果当前数字大于目标数，那就爆掉了
        if ((int)CustomProperties.GetRoomProp("nowNum") > goalNum)
        {
            return true;
        }
        return false;
    }
    private void DecideFirst()
    {
        CustomProperties.SetRoomProp("currentPlayer", CustomProperties.playersNumber[Random.Range(0, 2)]);
    }

    #endregion

    //海战棋
    #region Round

    private void StartNewRound()
    {
        //开始一个新的游戏，首先分配星星
        AllocateStar();
    }
    private void AllocateStar()
    {
       players[CustomProperties.playerLocalIdx].PlaceStar();
    }//*
    public void TurnBegin()
    {
        //只有一方（如主机方），代表系统做出先手判断
        if (PhotonNetwork.IsMasterClient)
        {
            //设置当前数字为0
            CustomProperties.SetRoomProp("nowNum", 0);
            //分配先手玩家
            DecideFirst();
        }
    }//*
    private void TurnFinish(int exceedPlayer)
    {
        //判断赢家及伤害值
        int winnerId = GetOtherPlayerId(exceedPlayer);
        int nowNum = (int)CustomProperties.GetRoomProp("nowNum");
        
        if(PhotonNetwork.IsMasterClient)
        {
            //将回合交给赢家
            CustomProperties.SetPlayerProp("state", PlayerManager.PlayerState.OthersTurn, exceedPlayer);
            CustomProperties.SetPlayerProp("state", PlayerManager.PlayerState.Attack, winnerId);
            //进入海战棋Round
            CustomProperties.SetRoomProp("state", GameState.Round);
        }
        
    }//*
    private void DealDamageFrom(int playerIdx, int dmg)
    {
        players[CustomProperties.playerLocalIdx].Attack(dmg);
    }//*

    #endregion

    #region Game

    public void Restart()
    {
        LoadArena();
    }
    public void Over(int winnerNumber)
    {
        CustomProperties.SetRoomProp("state", GameState.GameOver);
        logText.text = "赢家是：" + PhotonNetwork.CurrentRoom.Players[winnerNumber].NickName;
    }//*
    public void Begin()
    {
        //游戏初始态
        CustomProperties.SetRoomProp("state", GameState.Idle);
        //分配玩家idx
        CustomProperties.SetPlayers();
        players[0].myId = CustomProperties.playersNumber[0];
        players[1].myId = CustomProperties.playersNumber[1];
        //设置游戏场景
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            if (PlayerOneText == null) Debug.LogWarning("Missing PlayerOneText");
            PlayerOneText.text = CustomProperties.GetPlayerByIdx(0).NickName;
        }
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            if (PlayerOneText == null) Debug.LogWarning("Missing PlayerOneText");
            if (PlayerTwoText == null) Debug.LogWarning("Missing PlayerTwoText");
            PlayerOneText.text = CustomProperties.GetPlayerByIdx(0).NickName;
            PlayerTwoText.text = CustomProperties.GetPlayerByIdx(1).NickName;
            CustomProperties.SetLocalPlayerProp("state", PlayerManager.PlayerState.Idle);
            logText.text = "请按下Start Game准备";
        }
    }//!

    #endregion
}
