using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public int myId = 1;
    public Vector2 myStarPos;
    public enum PlayerState
    {
        Idle, Ready, PlaceStar, PlaceComplete, ChooseNumbers, Attack, OthersTurn,
    }

    private int myAmmos;

    #region UI

    public void OnClickStartBtn()
    {
        //1.只有本地玩家触发
        //2.若玩家为<闲置>,切换为<已准备>
        if (PhotonNetwork.CurrentRoom.Players[myId].IsLocal &&
            CustomProperties.GetLocalState() == PlayerState.Idle)
        {
            CustomProperties.SetLocalState(PlayerState.Ready);
        }
    }

    #endregion

    #region Prop
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        object tempObj;
        //[房间当前玩家]
        if (propertiesThatChanged.TryGetValue("currentPlayer", out tempObj))
        {
            int currentPlayer = (int)tempObj;
            //1.此Player脚本是客户端方
            if (PhotonNetwork.LocalPlayer.ActorNumber == myId)
            {
                //2.此客户端方是轮方
                if (myId == currentPlayer)
                {
                    StartTurn();
                }
                //2.此客户端方不是轮方
                else
                {
                    CustomProperties.SetLocalPlayerProp("state", PlayerState.OthersTurn);
                }
            }
        }
        //[房间当前数字]
        if (propertiesThatChanged.TryGetValue("nowNum",out tempObj))
        {
            int nowNum = (int)tempObj;
        }
    }
    #endregion

    public virtual void StartTurn()
    {
        CustomProperties.SetLocalState(PlayerState.ChooseNumbers);
    }
    public virtual void AddNum(int value)
    {
        //传递玩家选择的数字
        CustomProperties.SetLocalPlayerProp("num", value);
        //Debug.Log("+" + value);
    }//!
    public virtual void PlaceStar()
    {
        CustomProperties.SetLocalState(PlayerState.PlaceStar);
        //启用鼠标图标渲染
        Hover.Instance.Activate(Hover.HoverState.Star);
        //等待BoardScript接收单击指令调用ClickGrid
    }
    protected virtual void ChooseStar(Vector2 pos)
    {
        CustomProperties.SetLocalPlayerProp("starPos", pos);
    }
    public virtual void Attack(int ammos)
    {
        //装填弹药
        myAmmos = ammos;
        //启用鼠标图标渲染
        Hover.Instance.Activate(Hover.HoverState.Attack);
        //等待BoardScript接收单击指令调用ClickGrid
    }
    public virtual void ClickGrid(BoardScript board)
    {
        switch (CustomProperties.GetLocalState())
        {
            case PlayerState.PlaceStar:
                //1.必须是自己的面板
                //2.方块上必须什么都没有
                if (CustomProperties.playersNumber[board.gridPos.playerIdx] == myId)
                {
                    //将此玩家的星星值设置成方块的pos
                    ChooseStar(board.gridPos.pos);
                    //回调，将被点击方块改成星星
                    board.ChangeSprite(BoardScript.BoardState.Star);
                    //取消鼠标悬浮图标
                    Hover.Instance.Deactivate();
                    //放置完成，通知GameManager
                    CustomProperties.SetLocalState(PlayerState.PlaceComplete);
                }
                break;
            case PlayerState.Attack:
                //1.必须是别人的面板
                //2.方块上不能是已经被攻击过的
                //3.游戏状态必须为<Round>
                if (CustomProperties.playersNumber[board.gridPos.playerIdx] != myId &&
                    board.boardState != BoardScript.BoardState.Damaged &&
                    GameManager.Instance.GetRoomState()==GameManager.GameState.Round)
                {
                    //剩余次数-1
                    myAmmos--;
                    //发送攻击坐标
                    CustomProperties.SetLocalPlayerProp("attackPos", board.gridPos.pos);
                    //如果攻击到了星星
                    if (board.gridPos.pos == (Vector2)CustomProperties.GetPlayerProp("starPos",false))
                    {
                        //回调，将被点击方块改成受损星星
                        board.ChangeSprite(BoardScript.BoardState.DamagedStar);
                        //取消鼠标悬浮图标
                        Hover.Instance.Deactivate();
                        //通知GameManager游戏结束，我是赢家
                        GameManager.Instance.Over(myId);
                        break;
                    }
                    //回调，将被点击方块改成受损
                    board.ChangeSprite(BoardScript.BoardState.Damaged);
                    //如果攻击完成
                    if (myAmmos <= 0)
                    {
                        //取消鼠标悬浮图标
                        Hover.Instance.Deactivate();
                        //通知GameManager，开始新的Turn
                        GameManager.Instance.TurnBegin();
                    }
                }
                break;
            default:
                break;
        }
    }
}
