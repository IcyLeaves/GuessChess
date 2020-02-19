using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public int myActorNumber;
    public int myIdx;
    public string myNickName;
    public Sprite mySprite;
    public Sprite[] specialSprites;
    public Vector2 myStarPos;
    public int myHp;
    public int myHeroId;
    public enum PlayerState
    {
        Idle, Ready,PlaceStar, PlaceComplete, ChooseNumbers, Attack, OthersTurn,
    }
    public PlayerState myState;

    public virtual void InitInfo()
    {
        myHp = 0;
        myHeroId = -1;
        myActorNumber = PhotonNetwork.PlayerList[myIdx].ActorNumber;
        myNickName = PhotonNetwork.CurrentRoom.Players[myActorNumber].NickName;
        switch(myNickName)
        {
            case "NPC":
                mySprite = specialSprites[2];
                break;
            default:
                mySprite = specialSprites[0];
                break;
        }
        
    }

    public virtual void SelectHero(int id)
    {
        //选完英雄要在远程和本地同步
        CustomProperties.SetPlayerProp("heroId", id, myActorNumber);
    }

    public virtual void PlaceStar()
    {
        //1.必须是[本地端.本地方]执行
        if(myIdx==GameManager.Instance.localIdx)
        {
            //只在本地修改状态，方便Board识别即可
            //若发送远程端会造成ready状态干扰
            myState = PlayerState.PlaceStar;
            //启用鼠标图标渲染
            Hover.Instance.Activate(Hover.HoverState.Star);
            //等待BoardScript接收单击指令调用PlaceStar(board)
        }
    }
    public virtual void PlaceStar(BoardScript board)
    {
        //1.必须是自己的面板
        //2.方块上必须什么都没有
        if (board.gridPos.playerIdx == myIdx && board.boardState==BoardScript.BoardState.Nothing)
        {
            //将[本地端.本地方]和[远程端.本地方]的星星位置
            CustomProperties.SetPlayerProp("starPos", board.gridPos.pos, myActorNumber);
            //回调，将被点击方块改成星星
            board.ChangeSprite(BoardScript.BoardState.Star);
            //取消鼠标悬浮图标
            Hover.Instance.Deactivate();
            //放置完成，则使[本地端.本地方]和[远程端.本地方]放置完成
            CustomProperties.SetPlayerProp("state", PlayerState.PlaceComplete, myActorNumber);
            myState = PlayerState.PlaceComplete;
        }
    }

    public virtual void ChooseNum()
    {
        //只在本地修改状态，方便Tile识别即可
        //若发送远程端会造成PlaceComplete状态干扰
        myState = PlayerState.ChooseNumbers;
    }
    public void ChooseNum(int val)
    {
        //val为单击的加数按钮的值
        CustomProperties.SetLocalPlayerProp("selectNum", val);
    }

    public virtual void Attack()
    {
        //Attack动作只在本地显示
        if(myIdx==GameManager.Instance.localIdx)
        {
            CustomProperties.SetPlayerProp("state", PlayerState.Attack, myActorNumber);
            myState = PlayerState.Attack;
            //启用鼠标图标渲染
            Hover.Instance.Activate(Hover.HoverState.Attack);
            //等待BoardScript接收单击指令调用ClickGrid
        }
    }
    private void Attack(BoardScript board)
    {
        //1.必须是别人的面板
        //2.不能是已攻击过的面板
        if (board.gridPos.playerIdx != myIdx &&
            board.boardState!=BoardScript.BoardState.Damaged)
        {
            //发送攻击坐标
            CustomProperties.SetPlayerProp("attackPos", board.gridPos.pos, myActorNumber);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        object tempObj;
        if (changedProps.TryGetValue("starPos", out tempObj))
        {
            Vector2 pos = (Vector2)tempObj;
            //1.若该脚本为对应Player
            if (myActorNumber == targetPlayer.ActorNumber)
            {
                //[玩家星星位置]
                myStarPos = pos;
                myHp++;
            }
        }
        if (changedProps.TryGetValue("heroId", out tempObj))
        {
            int heroId = (int)tempObj;
            //1.若该脚本为对应Player
            if (myActorNumber == targetPlayer.ActorNumber)
            {
                //[玩家所选英雄]
                myHeroId = heroId;
            }
        }
        if (changedProps.TryGetValue("state", out tempObj))
        {
            PlayerState state = (PlayerState)tempObj;
            //1.若该脚本为对应Player
            //2.且非本地数据
            if (myActorNumber == targetPlayer.ActorNumber &&
                !targetPlayer.IsLocal)
            {
                //[玩家状态]
                myState = state;
            }
        }
    }
    public virtual void ClickGrid(BoardScript board)
    {
        switch (myState)
        {
            case PlayerState.PlaceStar:
                PlaceStar(board);
                break;
            case PlayerState.Attack:
                Attack(board);
                break;
            default:
                break;
        }
    }


}
