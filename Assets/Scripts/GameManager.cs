using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    public int localIdx;//己方玩家idx
    public int otherIdx;//敌方玩家idx
    public enum SkillState
    {
        TrunStart,
    };
    public SkillState skillState;

    public int selectNum;//加数
    public int nowSum;//累计值
    public int goalNum;//阈值
    public Vector2 attackPos;//攻击位置

    public List<PlayerManager> players;
    public BoardManager boardManager;
    public Button startBtn;
    public List<SpriteRenderer> playerSprites;
    public List<Text> playerTexts;
    public Text logText;
    public List<TileScript> tiles;
    public Text turnText;
    public GameObject heroCardPanel;
    public List<GameObject> heroIcons;

    public int currentPlayerIdx;
    public Dictionary<string, object> tmpData;//若属性改变快于代码执行，则将数据缓存至此

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

    private void Start()
    {
        Instance = this;
        StartCoroutine(MainProcess());
    }
    IEnumerator MainProcess()
    {
        InitScene();//场景初始化
        yield return new WaitUntil(SyncReady);//等待双方玩家准备
        SelectHero();//玩家选择英雄
        yield return new WaitUntil(SyncHeroCardSelected);//等待双方选完英雄
        ShowBoard();//显示棋盘
        PlaceStars();//玩家放置星星
        yield return new WaitUntil(SyncPlaceComplete);//等待双方放置星星完毕
        //<回合>开始
        int round = 0;//清空回合数
        int roundWinnerIdx = -1;//回合获胜者idx
        while (true)
        {
            round++;//回合数+1
            RoundStart(round);//【回合开始】
            DecideFirst();//决定先手玩家
            yield return new WaitUntil(() => currentPlayerIdx >= 0);//等待先手玩家被分配
            //<轮次>开始
            int turn = 0;//清空轮数
            int turnWinnerIdx = -1;//轮次获胜者idx
            int ammos = -1;//弹药数
            nowSum = 0;//清空累计值
            while (true)
            {
                turn++;
                TurnStart(round, turn);//【轮次开始】
                ChooseNumbers();//玩家选择加数
                yield return new WaitUntil(() => selectNum >= 0);//等待玩家选择数字
                AddToNowSum();//累计值更新
                if (IsTurnContinue())//若轮次还将继续
                {
                    TurnOver();//【轮次结束】
                    NextTurn();//一轮结束，移交轮次控制权
                }
                else//若轮次停止循环
                {
                    turnWinnerIdx = SetTurnWinner();//决出轮次赢家
                    ammos = SetAmmos();//计算弹药
                    break;//结束<轮次>循环
                }
            }
            //<攻击>开始
            int restAmmos = ammos;//剩余弹药设置为弹药数
            while (true)
            {
                attackPos = new Vector2(-1, -1);//玩家还未攻击
                WinnerAttackOnce(turnWinnerIdx, restAmmos, ammos);//赢家进行一次攻击
                yield return new WaitUntil(() => attackPos != new Vector2(-1, -1));//等待赢家选择攻击坐标
                AttackBoard(turnWinnerIdx);//发起攻击
                restAmmos--;//剩余弹药-1
                if (restAmmos <= 0 || IsGameOver())//如果游戏结束或子弹用完
                {
                    AttackOver();//则停止循环，攻击结束
                    break;
                }
            }
            if (IsGameOver())//如果游戏结束
            {
                roundWinnerIdx = SetRoundWinner();//决出回合赢家
                break;
            }
            else { }//不然继续循环<回合>
        }
        InitGameOver(roundWinnerIdx);//显示游戏结束画面
        yield return new WaitForSeconds(5f);//等待5s
        RestartGame();//重新开始游戏
        yield break;
    }



    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        object tempObj;
        //[轮次控制者]
        if (propertiesThatChanged.TryGetValue("currentPlayer", out tempObj))
        {
            if (currentPlayerIdx == -1)
                currentPlayerIdx = (int)tempObj;
            else
                tmpData["currentPlayer"] = tempObj;

        }

    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        object tempObj;
        //[所选数字]
        if (changedProps.TryGetValue("selectNum", out tempObj))
        {
            if (selectNum == -1)
                selectNum = (int)tempObj;
            else
                tmpData["selectNum"] = tempObj;
        }
        //[攻击坐标]
        if (changedProps.TryGetValue("attackPos", out tempObj))
        {
            if (attackPos == new Vector2(-1, -1))
                attackPos = (Vector2)tempObj;
            else
                tmpData["attackPos"] = tempObj;
        }
    }

    private bool SyncPlayerState(PlayerManager.PlayerState state)
    {
        return players[0].myState == state && players[1].myState == state;
    }

    #region InitScene
    private void InitScene()
    {
        InitialPlayerInfo();
        InitializeUi();
        InitialData();
    }
    private void InitialPlayerInfo()
    {
        //根据服务器玩家的信息更新Player信息
        players[0].InitInfo();
        players[1].InitInfo();
        //判断本地玩家
        localIdx = PhotonNetwork.IsMasterClient ? 0 : 1;
        otherIdx = 1 - localIdx;
    }
    private void InitializeUi()
    {
        //根据玩家信息更新玩家UI
        for (int i = 0; i < players.Count; i++)
        {
            //玩家昵称
            playerTexts[i].text = players[i].myNickName;
            //玩家头像
            playerSprites[i].sprite = players[i].mySprite;
        }
        //更新场景文本
        logText.text = "请按下Start准备";
        //开放Start按钮
        startBtn.interactable = true;
    }
    private void InitialData()
    {
        tmpData = new Dictionary<string, object>();
        tmpData["currentPlayer"] = -1;
        tmpData["selectNum"] = -1;
        tmpData["attackPos"] = new Vector2(-1, -1);

        currentPlayerIdx = 0;
        selectNum = 0;
        attackPos = Vector2.zero;
    }
    #endregion

    #region SyncReady
    private bool SyncReady()
    {
        if (players[localIdx].myState == PlayerManager.PlayerState.Ready)
        {
            logText.text = "等待" + players[otherIdx].myNickName + "准备";
        }
        return SyncPlayerState(PlayerManager.PlayerState.Ready);
    }
    public void OnStartBtnClick()
    {
        //禁闭Start按钮
        startBtn.interactable = false;
        //按下准备按钮，则使[本地端.本地方]和[远程端.本地方]准备。
        CustomProperties.SetLocalState(PlayerManager.PlayerState.Ready);
        players[localIdx].myState = PlayerManager.PlayerState.Ready;

    }
    #endregion

    #region HeroCardPanelInit
    private void SelectHero()
    {
        HeroCardPanelInit();//打开英雄面板
    }
    private void HeroCardPanelInit()
    {
        //生成英雄卡片
        heroCardPanel.SetActive(true);//显示Panel
    }
    #endregion

    #region SyncHeroCardSelected
    private bool SyncHeroCardSelected()
    {
        if (players[localIdx].myHeroId >= 0)
        {
            logText.text = "等待" + players[otherIdx].myNickName + "选择英雄";
        }
        return players[0].myHeroId >= 0 && players[1].myHeroId >= 0;
    }
    public void OnHeroCardClick(int val)
    {
        heroCardPanel.SetActive(false);//关闭Panel
        players[localIdx].SelectHero(val);
    }
    #endregion

    #region ShowBoard
    private void ShowBoard()
    {
        //显示棋盘
        boardManager.playerOneCornerPoint.gameObject.SetActive(true);
        boardManager.playerTwoCornerPoint.gameObject.SetActive(true);
        //显示头像
        for (int i = 0; i < heroIcons.Count; i++)
        {
            var g = HeroManager.Instance.heroIconPrefabs[players[i].myHeroId];
            g.GetComponent<HeroIconScript>().playerNumber = players[i].myActorNumber;
            g = Instantiate(g);
            g.transform.SetParent(heroIcons[i].transform);
            g.transform.localPosition = Vector3.zero;
            
        }
    }
    #endregion

    #region PlaceStars
    private void PlaceStars()
    {
        logText.text = "请放置星星";
        players[0].PlaceStar();
        players[1].PlaceStar();
    }
    #endregion

    #region SyncPlaceComplete
    private bool SyncPlaceComplete()
    {
        if (players[localIdx].myState == PlayerManager.PlayerState.PlaceComplete)
        {
            logText.text = "等待" + players[otherIdx].myNickName + "放置";
        }
        return SyncPlayerState(PlayerManager.PlayerState.PlaceComplete);
    }
    #endregion

    #region RoundStart
    private void RoundStart(int round)
    {
        turnText.text = "第 " + round + " 回合   第 1 轮";
        currentPlayerIdx = -1;//还未决定先手玩家
        //【每回合开始】技能恢复
        heroIcons[localIdx].GetComponentInChildren<Hero>().OnRoundStart();
    }
    #endregion

    #region DecideFirst
    private void DecideFirst()
    {
        logText.text = "正在决定先手玩家";
        //1.只有[主机端]才有决定房间属性的权利
        if (PhotonNetwork.IsMasterClient)
        {
            int res = Random.Range(0, 2);
            CustomProperties.SetRoomProp("currentPlayer", res);
        }
        //如果有缓存数据
        if ((int)tmpData["currentPlayer"] >= 0)
        {
            //那么使用缓存
            currentPlayerIdx = (int)tmpData["currentPlayer"];
            //随后清空以备下一次接收
            tmpData["currentPlayer"] = -1;
        }
    }
    #endregion

    #region TurnStart
    private void TurnStart(int round, int turn)
    {
        turnText.text = "第 " + round + " 回合   第 " + turn + " 轮";//轮数+1
        selectNum = -1;//玩家还未选择加数
        if (currentPlayerIdx == localIdx)
            MyTurnStart();
    }
    private void MyTurnStart()
    {
        //【我的每轮开始】技能触发
        bool canUse = heroIcons[localIdx].GetComponentInChildren<Hero>().OnMyTurnStart();
        if (canUse)
            heroIcons[localIdx].GetComponentInChildren<HeroIconScript>().ChangeSprite(HeroIconScript.IconState.Enable);
        else
            heroIcons[localIdx].GetComponentInChildren<HeroIconScript>().ChangeSprite(HeroIconScript.IconState.Disable);
    }
    #endregion

    #region ChooseNumbers
    private void ChooseNumbers()
    {

        //无论是[本地端]还是[远程端]都会调用
        players[currentPlayerIdx].ChooseNum();
        //[currentPlayerIdx]为<轮次>控制方
        if (currentPlayerIdx == localIdx)//如果是[本地端.本地方]
        {
            logText.text = "请选择加数";
            //启用加数按钮
            foreach (var tile in tiles)
            {
                tile.ChangeSprite(TileScript.TileState.Enable);
            }
        }
        else//如果是[本地端.对方]
        {
            logText.text = "等待" + players[otherIdx].myNickName + "选择加数";
        }
        //如果有缓存数据
        if ((int)tmpData["selectNum"] >= 0)
        {
            //那么使用缓存
            selectNum = (int)tmpData["selectNum"];
            //随后清空以备下一次接收
            tmpData["selectNum"] = -1;
        }
    }
    #endregion

    #region AddToNowSum
    private void AddToNowSum()
    {
        Debug.Log("[累计值]" + nowSum + "+" + players[currentPlayerIdx].myNickName + "." + selectNum + "=" + (nowSum + selectNum));
        nowSum += selectNum;

    }
    #endregion

    #region IsTurnContinue
    private bool IsTurnContinue()
    {
        return nowSum <= goalNum;
    }
    #endregion


    #region TurnOver
    private void TurnOver()
    {
        if (currentPlayerIdx == localIdx)
            MyTurnOver();
    }
    private void MyTurnOver()
    {
        //【我的每轮结束】技能触发
        bool canUse = heroIcons[localIdx].GetComponentInChildren<Hero>().OnMyTurnOver();
        if (canUse)
            heroIcons[localIdx].GetComponentInChildren<HeroIconScript>().ChangeSprite(HeroIconScript.IconState.Enable);
        else
            heroIcons[localIdx].GetComponentInChildren<HeroIconScript>().ChangeSprite(HeroIconScript.IconState.Disable);
    }

    #endregion

    #region NextTurn
    private void NextTurn()
    {
        currentPlayerIdx = 1 - currentPlayerIdx;
    }
    #endregion


    #region SetTurnWinner
    private int SetTurnWinner()
    {
        return 1 - currentPlayerIdx;
    }
    #endregion

    #region SetAmmos
    private int SetAmmos()
    {
        return 6 - (nowSum - goalNum);
    }
    #endregion

    #region WinnerAttackOnce
    private void WinnerAttackOnce(int winnerIdx, int restAmmos, int ammos)
    {
        //无论是[本地端]还是[远程端]都会调用
        players[winnerIdx].Attack();
        //[winnerIdx]为攻击方
        if (winnerIdx == localIdx)//如果是[本地端.本地方]
        {
            logText.text = "当前累计值：" + nowSum + "/" + goalNum + "\n" +
                "你共有" + ammos + "发弹药，还剩" + restAmmos + "发";
        }
        else//如果是[本地端.对方]
        {
            logText.text = "当前累计值：" + nowSum + "/" + goalNum + "\n" +
                "等待" + players[otherIdx].myNickName + "攻击，还剩" + restAmmos + "发";
        }
        //如果有缓存数据
        if ((Vector2)tmpData["attackPos"] != new Vector2(-1, -1))
        {
            //那么使用缓存
            attackPos = (Vector2)tmpData["attackPos"];
            //随后清空以备下一次接收
            tmpData["attackPos"] = new Vector2(-1, -1);
        }
    }
    #endregion

    #region AttackBoard
    private void AttackBoard(int attackerIdx)
    {
        //获取[被攻击者]idx
        int ruinedIdx = 1 - attackerIdx;
        //获取[被攻击者]面板的攻击坐标的方块
        BoardScript board = boardManager.GetPosBoard(ruinedIdx, attackPos);
        //如果攻击到了星星
        if (board.gridPos.pos == players[ruinedIdx].myStarPos)
        {
            //回调，将被点击方块改成受损星星
            board.ChangeSprite(BoardScript.BoardState.DamagedStar);
            //[被攻击者].HP-1
            players[ruinedIdx].myHp--;
            //取消鼠标悬浮图标
            Hover.Instance.Deactivate();
            return;
        }
        //回调，将被点击方块改成受损
        board.ChangeSprite(BoardScript.BoardState.Damaged);
    }
    #endregion

    #region IsGameOver
    private bool IsGameOver()
    {
        return players[0].myHp == 0 || players[1].myHp == 0;
    }
    #endregion

    #region AttackOver
    private void AttackOver()
    {
        //取消鼠标悬浮图标
        Hover.Instance.Deactivate();
        //清除本地的攻击坐标缓存，以防影响下次攻击
        tmpData["attackPos"] = new Vector2(-1, -1);
    }
    #endregion

    #region SetRoundWinner
    private int SetRoundWinner()
    {
        if (players[0].myHp > 0)
            return 0;
        if (players[1].myHp > 0)
            return 1;
        return -1;
    }
    #endregion

    #region InitGameOver
    private void InitGameOver(int winnerIdx)
    {
        //公布赢家
        logText.text = "赢家是：" + players[winnerIdx].myNickName;
        //公开星星位置
        for (int i = 0; i < 2; i++)
        {
            Vector2 starPos = players[i].myStarPos;
            BoardScript board = boardManager.GetPosBoard(i, starPos);
            if (board.boardState == BoardScript.BoardState.Nothing)
            {
                board.ChangeSprite(BoardScript.BoardState.Star);
            }
        }
    }
    #endregion

    #region RestartGame
    private void RestartGame()
    {
        LoadArena();
    }
    #endregion

}