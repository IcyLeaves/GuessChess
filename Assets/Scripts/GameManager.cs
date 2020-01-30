using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    public int goalNum = 20;
    public List<PlayerManager> players;

    public int currentPlayerIdx;

    public Text logText;

    private int nowNum;
    private GameState gameState;
    private enum GameState
    {
        Idle,Gaming,GameOver
    }
    private void Start()
    {
        Instance = this;
        gameState = GameState.Idle;
    }

    #region UI
    public void OnClickStartBtn()
    {
        if (gameState == GameState.GameOver)
            Restart();
        else
            StartNewRound();
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

    private void StartNewTurn()
    {
        //唤醒指定Id的玩家，新的回合开始
        players[currentPlayerIdx].StartTurn();
        logText.text = "现在进行玩家 " + currentPlayerIdx + "的回合";
    }//*
    public void OnCompleteTurn()
    {
        //当一个玩家主动结束他的回合后调用此方法
        if (gameState != GameState.Idle)
            //将回合交给下一位
            ChangeTurnToNext();
    }
    private void ChangeTurnTo(int playerId)
    {
        //直接令当前玩家为Id
        currentPlayerIdx = playerId;
        //开始一轮新的回合
        StartNewTurn();
    }
    public void AddNowNum(int val)
    {
        nowNum += val;
        Debug.Log("nowNum is " + nowNum);
        //判断当前数字有没有爆掉
        if (IsNowNumExceed())
        {
            //如果爆掉了，就游戏结束
            TurnFinish();
        }
        else
        {
            //如果没爆掉，游戏继续
        }

    }//!
    private void ChangeTurnToNext()
    {
        //将当前玩家序号递增一位
        currentPlayerIdx++;
        if (currentPlayerIdx >= players.Count)
        {
            currentPlayerIdx = 0;
        }
        StartNewTurn();
    }
    private bool IsNowNumExceed()
    {
        //如果当前数字大于目标数，那就爆掉了
        if (nowNum > goalNum)
        {
            return true;
        }
        return false;
    }

    #endregion

    //海战棋
    #region Round
    private void StartNewRound()
    {
        if (gameState != GameState.Idle) return;
        //开始一个新的游戏，首先分配星星
        AllocateStar();
    }
    private void AllocateStar()
    {
        logText.text = "请双方放置星星";
        starPlacedCount = 0;
        for (int i = 0; i < players.Count; i++)
        {
            players[i].PlaceStar();
        }
    }//*
    private int starPlacedCount;
    public void StarPlaced()
    {
        //每个玩家放置完星星都要调用这个方法，调用次数满后开始第一个Turn
        starPlacedCount++;
        if (starPlacedCount == 2)
            TurnBegin();
    }

    public void TurnBegin()
    {
        //一回合开始，清零当前数字，状态为游戏中
        nowNum = 0;
        gameState = GameState.Gaming;
        //决定先手
        int firstTurn = Random.Range(0, 2);//随机决定
        logText.text = "先手是玩家 " + firstTurn;
        //切换到先手回合
        ChangeTurnTo(firstTurn);
    }//*
    private void TurnFinish()
    {
        //判断赢家及伤害值
        int winner = 1 - currentPlayerIdx;
        int dmg = nowNum - goalNum;
        logText.text = "获胜的是：玩家 " + winner.ToString();
        //回合结束
        gameState = GameState.Idle;
        DealDamageFrom(winner, dmg);
    }//*

    private void DealDamageFrom(int playerIdx,int dmg)
    {
        logText.text = "玩家 " + playerIdx + "正在攻击";
        players[playerIdx].Attack(dmg);
    }//*
    #endregion

    #region Game
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void Over(int winner)
    {
        gameState = GameState.GameOver;
        logText.text = "赢家是：玩家" + winner;
    }//*
    #endregion
}
