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
        Idle,Gaming
    }
    private void Start()
    {
        Instance = this;
        gameState = GameState.Idle;
    }

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

    #region Turn

    //当一个玩家主动结束他的回合后调用此方法
    public void OnCompleteTurn()
    {
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
            GameOver();
        }
        else
        {
            //如果没爆掉，游戏继续
        }

    }
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
    private void StartNewTurn()
    {
        //唤醒指定Id的玩家，新的回合开始
        players[currentPlayerIdx].StartTurn();
        logText.text = "现在进行玩家 " + currentPlayerIdx + "的回合";
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

    #region Round
    public void GameStart()
    {
        if (gameState == GameState.Gaming) return;
        //一回合开始，清零当前数字，状态为游戏中
        nowNum = 0;
        gameState = GameState.Gaming;
        //决定先手
        int firstTurn = Random.Range(0, 2);//随机决定
        logText.text = "先手是玩家 " + firstTurn;
        //切换到先手回合
        ChangeTurnTo(firstTurn);
    }
    private void GameOver()
    {
        //判断输家
        logText.text = "获胜的是：玩家 " + (1 - currentPlayerIdx).ToString();
        //回合结束
        gameState = GameState.Idle;
    }

    #endregion
}
