using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public PlayerState myState;
    public int myId = 0;
    public Vector2 myStarPos;
    public enum PlayerState
    {
        PlaceStar, PlaceComplete, ChooseNumbers, Attack, OthersTurn,
    }

    private int myAmmos;

    private void Update()
    {

    }

    public virtual void StartTurn()
    {
        myState = PlayerState.ChooseNumbers;
    }

    public virtual void AddNum(int value)
    {
        //设定房间的当前数字
        GameManager.Instance.AddNowNum(value);
        Debug.Log("+" + value);
        //结束我的回合
        myState = PlayerState.OthersTurn;
        GameManager.Instance.OnCompleteTurn();
    }//!

    public virtual void PlaceStar()
    {
        myState = PlayerState.PlaceStar;
        //启用鼠标图标渲染
        Hover.Instance.Activate(Hover.HoverState.Star);
        //等待BoardScript接收单击指令调用ClickGrid
    }
    protected virtual void ChooseStar(Vector2 pos)
    {
        myStarPos = pos;
    }

    public virtual void Attack(int ammos)
    {
        myState = PlayerState.Attack;
        //装填弹药
        myAmmos = ammos;
        //启用鼠标图标渲染
        Hover.Instance.Activate(Hover.HoverState.Attack);
        //等待BoardScript接收单击指令调用ClickGrid
    }
    public virtual void ClickGrid(BoardScript board)
    {
        switch (myState)
        {
            case PlayerState.PlaceStar:
                //1.必须是自己的面板
                //2.方块上必须什么都没有
                if (board.gridPos.playerIdx == myId)
                {
                    //将此玩家的星星值设置成方块的pos
                    ChooseStar(board.gridPos.pos);
                    //回调，将被点击方块改成星星
                    board.ChangeSprite(BoardScript.BoardState.Star);
                    //取消鼠标悬浮图标
                    Hover.Instance.Deactivate();
                    //放置完成，通知GameManager
                    myState = PlayerState.PlaceComplete;
                    GameManager.Instance.StarPlaced();
                }
                break;
            case PlayerState.Attack:
                //1.必须是别人的面板
                //2.方块上不能是已经被攻击过的
                if (board.gridPos.playerIdx != myId &&
                    board.boardState!=BoardScript.BoardState.Damaged)
                {
                    //剩余次数-1
                    myAmmos--;
                    //如果攻击到了星星
                    if(board.gridPos.pos == GameManager.Instance.players[1 - myId].myStarPos)
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
                    if(myAmmos==0)
                    {
                        //取消鼠标悬浮图标
                        Hover.Instance.Deactivate();
                        //通知GameManager，开始新的Turn
                        myState =PlayerState.OthersTurn;
                        GameManager.Instance.TurnBegin();
                    }
                }
                break;
            default:
                break;
        }
    }
}
