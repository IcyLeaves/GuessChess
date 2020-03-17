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
using TMPro;
using System.IO;



public class GameManager : MonoBehaviourPunCallbacks
{
    const int GOAL_NUM = 20;

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
    public int ammos;
    public bool isResetAmmo;//是否重载弹药
    public List<Hero> heroScripts;

    public List<PlayerManager> players;
    public BoardManager boardManager;
    public Button startBtn;
    public List<SpriteRenderer> playerSprites;
    public List<TMP_Text> playerTexts;
    public TMP_Text logText;
    public List<TileScript> tiles;
    public TMP_Text turnText;
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
        MyAnimation.Instance.ShowOnlyContent(MyAnimation.Contents.Star);
        yield return new WaitUntil(AnimationUnlocked);//等待动画完毕
        PlaceStars();//玩家放置星星
        yield return new WaitUntil(SyncPlaceComplete);//等待双方放置星星完毕
        yield return StartCoroutine(PlaceTrap());//【放置额外陷阱】
        yield return new WaitUntil(SyncTrapPlaced);//等待双方放置额外陷阱
        //<回合>开始
        int round = 0;//清空回合数
        int roundWinnerIdx = -1;//回合获胜者idx
        while (true)
        {
            round++;//回合数+1
            MyAnimation.Instance.Round(round);
            yield return new WaitUntil(AnimationUnlocked);//等待动画完毕
            RoundStart(round);//【回合开始】
            DecideFirst();//决定先手玩家
            yield return new WaitUntil(() => currentPlayerIdx >= 0);//等待先手玩家被分配
            //<轮次>开始
            int turn = 0;//清空轮数
            int turnWinnerIdx = -1;//轮次获胜者idx
            ammos = -1;//弹药数
            nowSum = 0;//清空累计值
            while (true)
            {
                turn++;
                TurnStart(round, turn);//【轮次开始】
                ChooseNumbers();//玩家选择加数
                yield return new WaitUntil(() => selectNum >= 0);//等待玩家选择数字
                MyAnimation.Instance.LoadSum(0, -1);//隐藏累计值，显示阈值
                NumSelected();//【玩家选择数字后】
                AddToNowSum();//累计值更新
                AfterNumAdded();//【累计值更新后】
                yield return new WaitUntil(AnimationUnlocked);//等待动画完毕
                if (IsTurnContinue())//若轮次还将继续
                {
                    TurnOver();//【轮次结束】
                    NextTurn();//一轮结束，移交轮次控制权
                }
                else//若轮次停止循环
                {
                    turnWinnerIdx = SetTurnWinner();//决出轮次赢家
                    TurnOver();//【轮次结束】
                    MyAnimation.Contents tmpState =
                        turnWinnerIdx == localIdx ? MyAnimation.Contents.Win : MyAnimation.Contents.Lose;
                    MyAnimation.Instance.ShowOnlyContent(tmpState);//展示"获胜"或"失败"动画
                    MyAnimation.Instance.LoadSum(1, 1);
                    yield return new WaitUntil(AnimationUnlocked);//等待动画完毕
                    break;//结束<轮次>循环
                }
            }
            //<攻击>开始
            ammos = SetAmmos(turnWinnerIdx);//计算弹药
            isResetAmmo = false;//目前弹药还没有重载
            AttackStart(turnWinnerIdx, ammos);//【攻击开始】
            int restAmmos = ammos;//剩余弹药设置为弹药数
            yield return new WaitUntil(SyncAttackStart);//等待玩家都进入攻击阶段
            while (true)
            {
                if (restAmmos <= 0)//如果子弹用完
                {
                    AttackOver();//则停止循环，攻击结束
                    break;
                }
                attackPos = new Vector2(-1, -1);//玩家还未攻击
                WinnerAttackOnce(turnWinnerIdx, restAmmos, ammos);//赢家进行一次攻击
                yield return new WaitUntil(() => (
                attackPos != new Vector2(-1, -1)) || isResetAmmo
                );//等待赢家选择攻击坐标 或 弹药已重载
                if (isResetAmmo)//如果重载了弹药
                {
                    isResetAmmo = false;
                    restAmmos = ammos;//更新剩余弹药
                    continue;//用新的弹药值重新攻击
                }
                AttackBoard(turnWinnerIdx);//发起攻击
                yield return new WaitUntil(AnimationUnlocked);//等待动画完毕
                restAmmos--;//剩余弹药-1
                OnceAttackOver(turnWinnerIdx);//【一次攻击完成】
                if (IsGameOver())//如果游戏结束
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
            //只接受远程坐标
            if (!targetPlayer.IsLocal)
                if (attackPos == new Vector2(-1, -1))
                {
                    attackPos = (Vector2)tempObj;
                }
                else
                {
                    if (tmpData["attackPos"] == null) tmpData["attackPos"] = new Queue<Vector2>();
                    (tmpData["attackPos"] as Queue<Vector2>).Enqueue((Vector2)tempObj);
                }

        }
    }

    private bool SyncPlayerState(PlayerManager.PlayerState state)
    {
        return players[0].myState == state && players[1].myState == state;
    }
    public PlayerManager GetPlayerByActorNumber(int playerNum)
    {
        return players[PhotonNetwork.CurrentRoom.Players[playerNum].IsLocal ? localIdx : otherIdx];
    }
    private bool AnimationUnlocked()
    {
        return MyAnimation.locked == false;
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
        logText.text = "";
        //开放Start按钮
        startBtn.interactable = true;
    }
    private void InitialData()
    {
        tmpData = new Dictionary<string, object>();
        tmpData["currentPlayer"] = -1;
        tmpData["selectNum"] = -1;
        tmpData["attackPos"] = new Queue<Vector2>();

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
        GameStartUIInit();//游戏开始后的UI初始化
    }
    private void GameStartUIInit()
    {
        Destroy(startBtn.gameObject);//摧毁开始按钮
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
        Destroy(heroCardPanel);//摧毁Panel
        players[localIdx].SelectHero(val);
    }
    #endregion

    #region ShowBoard
    private void ShowBoard()
    {
        heroScripts = new List<Hero>();
        //显示棋盘
        boardManager.playerOneCornerPoint.gameObject.SetActive(true);
        boardManager.playerTwoCornerPoint.gameObject.SetActive(true);
        //显示头像
        for (int i = 0; i < heroIcons.Count; i++)
        {
            var g = HeroManager.Instance.heroIconPrefabs[players[i].myHeroId];
            g = Instantiate(g);
            var gIcon = g.GetComponent<HeroIconScript>();
            gIcon.playerNumber = players[i].myActorNumber;
            g.transform.SetParent(heroIcons[i].transform);
            g.transform.localPosition = Vector3.zero;
            if (i == otherIdx)
            {
                gIcon.isPassive = true;
            }
            //本地化一下脚本组件
            heroScripts.Add(g.GetComponentInChildren<Hero>());
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
            logText.text = "等待" + players[otherIdx].myNickName + "放置星星";
        }
        return SyncPlayerState(PlayerManager.PlayerState.PlaceComplete);
    }
    #endregion

    #region PlaceTrap
    private IEnumerator PlaceTrap()
    {
        players[0].myState = heroScripts[0].OnStarPlaced(localIdx == 0) ?
            PlayerManager.PlayerState.PlaceTrap : PlayerManager.PlayerState.PlaceComplete;
        yield return new WaitUntil(AnimationUnlocked);//等待动画完毕
        players[1].myState = heroScripts[1].OnStarPlaced(localIdx == 1) ?
            PlayerManager.PlayerState.PlaceTrap : PlayerManager.PlayerState.PlaceComplete;
        yield return new WaitUntil(AnimationUnlocked);//等待动画完毕
    }
    #endregion

    #region SyncTrapPlaced
    private bool SyncTrapPlaced()
    {
        //if (players[localIdx].myState == PlayerManager.PlayerState.PlaceComplete)
        //{
        //    logText.text = "等待" + players[otherIdx].myNickName + "放置"
        //        + heroScripts[otherIdx].GetTrapName();
        //}
        return SyncPlayerState(PlayerManager.PlayerState.PlaceComplete);
    }
    #endregion

    #region RoundStart
    private void RoundStart(int round)
    {
        turnText.text = "第 " + round + " 回合   第 1 轮";
        currentPlayerIdx = -1;//还未决定先手玩家
        goalNum = GOAL_NUM;//阈值默认20
        MyAnimation.Instance.LoadSum(0, 1);//隐藏累计值，显示阈值
        //清空历史记录
        HistoryScript.Instance.EmptyHistory();
        //【每回合开始】技能恢复
        heroScripts[localIdx].OnRoundStart();
        heroScripts[otherIdx].OnRoundStart();
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
        bool canUse = heroScripts[localIdx].OnMyTurnStart();
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

    #region NumSelected
    private void NumSelected()
    {
        if (currentPlayerIdx != localIdx)
            EnemyNumSelected();
    }
    private void EnemyNumSelected()
    {
        //【敌方选择数字后】技能触发
        heroScripts[localIdx].OnEnemyNumberSelected(selectNum);
    }
    #endregion

    #region AddToNowSum
    private void AddToNowSum()
    {
        Debug.Log("[累计值]" + nowSum + "+" + players[currentPlayerIdx].myNickName + "." + selectNum + "=" + (nowSum + selectNum));
        nowSum += selectNum;
        LogInHistory(selectNum);
    }
    private void LogInHistory(int num)
    {
        //产生历史数块
        if (currentPlayerIdx == localIdx)//是本地玩家吗
        {
            HistoryScript.Instance.CreateHistory(true, num);
        }
        else
        {
            HistoryScript.Instance.CreateHistory(false, 0);//创建一个问号
        }
    }
    #endregion

    #region AfterNumAdded
    private void AfterNumAdded()
    {
        heroScripts[localIdx].OnNowSumChanged(true);
        heroScripts[otherIdx].OnNowSumChanged(false);
    }

    #endregion

    #region IsTurnContinue
    private bool IsTurnContinue()
    {
        //【失败时触发】
        if (nowSum > goalNum)
        {
            if (heroScripts[localIdx].OnFailure(true) ||
                heroScripts[otherIdx].OnFailure(false))//防止失败的技能
            {
                return true;
            }
        }
        return nowSum <= goalNum;
    }
    #endregion


    #region TurnOver
    private void TurnOver()
    {
        if (currentPlayerIdx == localIdx)
            MyTurnOver();
        //【每轮结束】技能触发
        heroScripts[localIdx].OnEveryTurnOver();
        heroScripts[otherIdx].OnEveryTurnOver();
    }
    private void MyTurnOver()
    {
        //【我的每轮结束】技能触发
        bool canUse = heroScripts[localIdx].OnMyTurnOver();
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
    private int SetAmmos(int winnerIdx)
    {
        return 6 - (nowSum - goalNum) + heroScripts[winnerIdx].GetExtraAmmos();
    }
    #endregion

    #region AttackStart
    private void AttackStart(int winnerIdx, int ammos)
    {
        //将[本地]和[远程]的“自己”进入攻击状态
        players[localIdx].myState = PlayerManager.PlayerState.Attack;
        CustomProperties.SetLocalPlayerProp("state", PlayerManager.PlayerState.Attack);
        //清除本地的攻击坐标缓存，以防影响第一次攻击
        attackPos = new Vector2(-1, -1);
        (tmpData["attackPos"] as Queue<Vector2>).Clear();
        if (winnerIdx == localIdx)
        {
            MyAttackStart();
        }
    }
    private void MyAttackStart()
    {
        //【我的攻击开始】技能触发
        bool canUse = heroScripts[localIdx].OnMyAttackStart();
        if (canUse)
            heroIcons[localIdx].GetComponentInChildren<HeroIconScript>().ChangeSprite(HeroIconScript.IconState.Enable);
        else
            heroIcons[localIdx].GetComponentInChildren<HeroIconScript>().ChangeSprite(HeroIconScript.IconState.Disable);
    }
    #endregion

    #region SyncAttackStart
    private bool SyncAttackStart()
    {
        return SyncPlayerState(PlayerManager.PlayerState.Attack);
    }
    #endregion
    #region WinnerAttackOnce
    private void WinnerAttackOnce(int winnerIdx, int restAmmos, int ammos)
    {
        //如果有缓存数据
        if ((tmpData["attackPos"] as Queue<Vector2>).Count != 0)
        {
            //那么使用缓存
            attackPos = (tmpData["attackPos"] as Queue<Vector2>).Peek();
            //随后清空以备下一次接收
            (tmpData["attackPos"] as Queue<Vector2>).Dequeue();
        }
        //无论是[本地端]还是[远程端]都会调用
        players[winnerIdx].Attack();
        MyAnimation.Instance.LoadAmmo(winnerIdx, restAmmos, ammos);
        ////[winnerIdx]为攻击方
        //if (winnerIdx == localIdx)//如果是[本地端.本地方]
        //{

        //    logText.text = "当前累计值：" + nowSum + "/" + goalNum + "\n" +
        //        "你共有" + ammos + "发弹药，还剩" + restAmmos + "发";
        //}
        //else//如果是[本地端.对方]
        //{
        //    logText.text = "当前累计值：" + nowSum + "/" + goalNum + "\n" +
        //        "等待" + players[otherIdx].myNickName + "攻击，还剩" + restAmmos + "发";
        //}

    }
    #endregion

    #region AttackBoard
    public void AttackBoard(int attackerIdx)
    {
        //获取[被攻击者]idx
        int ruinedIdx = 1 - attackerIdx;
        //获取[被攻击者]面板的攻击坐标的方块
        BoardScript board = boardManager.GetPosBoard(ruinedIdx, attackPos);
        //【如果攻击到了陷阱】
        if (heroScripts[ruinedIdx].IsInTrap(board.gridPos.pos))
        {
            heroScripts[ruinedIdx].Ability(board);
            return;
        }
        //如果攻击到了星星
        if (board.gridPos.pos == players[ruinedIdx].myStarPos)
        {
            //【我的星星消亡】
            if (heroScripts[ruinedIdx].OnMyStarRuined())
            {
                StartCoroutine(MyAnimation.Instance.DelayToInvokeDo(delegate ()
                {
                    heroScripts[ruinedIdx].Ability(board);
                }, MyAnimation.myAnimationTime));
            }
            else
            {
                //回调，将被点击方块改成受损星星
                board.ChangeSprite(BoardScript.BoardState.DamagedStar);
                //[被攻击者].HP-1
                players[ruinedIdx].myHp--;
                //取消鼠标悬浮图标
                Hover.Instance.Deactivate();
            }
            return;
        }
        //回调，将被点击方块改成受损
        board.ChangeSprite(BoardScript.BoardState.Damaged);
    }
    #endregion

    #region OnceAttackOver
    private void OnceAttackOver(int winnerIdx)
    {
        if (winnerIdx == localIdx)
            MyOnceAttackOver();
    }
    private void MyOnceAttackOver()
    {
        //【我的一次攻击结束】技能触发
        bool canUse = heroScripts[localIdx].OnMyOnceAttackOver();
        if (canUse)
            heroIcons[localIdx].GetComponentInChildren<HeroIconScript>().ChangeSprite(HeroIconScript.IconState.Enable);
        else
            heroIcons[localIdx].GetComponentInChildren<HeroIconScript>().ChangeSprite(HeroIconScript.IconState.Disable);
    }
    #endregion

    #region IsGameOver
    public bool IsGameOver()
    {
        return players[0].myHp == 0 || players[1].myHp == 0;
    }
    #endregion

    #region AttackOver
    private void AttackOver()
    {
        //【一回合攻击结束】技能触发
        heroScripts[localIdx].OnAttackOver();
        heroScripts[otherIdx].OnAttackOver();
        //取消鼠标悬浮图标
        Hover.Instance.Deactivate();
        //隐藏子弹面板
        MyAnimation.Instance.StopAmmo();
    }
    #endregion

    #region SetRoundWinner
    public int SetRoundWinner()
    {
        if (players[0].myHp > 0)
            return 0;
        if (players[1].myHp > 0)
            return 1;
        return -1;
    }
    #endregion

    #region InitGameOver
    public void InitGameOver(int winnerIdx)
    {
        //公布赢家
        logText.gameObject.SetActive(true);
        logText.text = "赢家是：" + players[winnerIdx].myNickName;
        //【赢家公布后】
        heroScripts[localIdx].OnGameOver();
        heroScripts[otherIdx].OnGameOver();
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
        WriteWins(heroScripts[winnerIdx].heroId, heroScripts[1 - winnerIdx].heroId);
    }
    private void WriteWins(int winnerHeroId, int loserHeroId)
    {
        string filename = Application.dataPath + @"\Statistics\wins.txt";
        List<string> res = new List<string>();
        string[] strs = File.ReadAllLines(filename);
        foreach (var str in strs)
        {
            var arr = str.Split(' ');
            var heroId = int.Parse(arr[0]);
            var wins = int.Parse(arr[1]);
            var alls = int.Parse(arr[2]);
            if (heroId == winnerHeroId)
            {
                wins++;
                alls++;
            }
            else if (heroId == loserHeroId)
            {
                alls++;
            }
            res.Add(arr[0] + " " + wins.ToString() + " " + alls.ToString());
        }
        File.WriteAllLines(filename, res.ToArray());
        Debug.Log(res);
    }
    #endregion

    #region RestartGame
    public void RestartGame()
    {
        LoadArena();
    }
    #endregion

}