using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;

//【圈套】游戏开始时：你可以额外放置5个诱捕器。每当你的1个诱捕器消亡，你获得<获胜：弹药+1>
public class Hero03 : Hero
{
    public int N = 5;
    public Sprite trapSprite;
    public Sprite dmgTrapSprite;
    public Sprite trapHoverSprite;
    public List<Vector2> trapPositions;
    public int dmgIdx;//受到攻击的trap索引

    public int extraAmmos = 0;

    #region override
    protected override void OtherPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        object tempObj;
        //[陷阱位置]
        if (changedProps.TryGetValue("trapPos", out tempObj))
        {
            //只接受远程数据
            if (!targetPlayer.IsLocal)
            {
                trapPositions.Add((Vector2)tempObj);
                if(trapPositions.Count==N)
                {
                    MyAnimation.Instance.StopAmmo(playerId);
                }
                //修改弹药面板
                MyAnimation.Instance.MinusAmmo(playerId, 1);
            }
               
        }
    }

    public override bool OnStarPlaced(bool isLocal)
    {
        if (isLocal)
            Hover.Instance.Activate(trapHoverSprite);
        MyAnimation.Instance.SkillTrigger(heroId, isLocal);
        MyAnimation.Instance.LoadAmmo(playerId, N, N, trapSprite);
        return true;
    }
    public override bool OnGameOver()
    {
        //只有对方的英雄才需要触发
        if (PhotonNetwork.LocalPlayer.ActorNumber == playerNum) return false;
        int playerId = GameManager.Instance.GetPlayerByActorNumber(playerNum).myIdx;
        foreach(var trapPos in trapPositions)
        {
            BoardManager.Instance.GetPosBoard(playerId, trapPos).ChangeSprite(BoardScript.BoardState.Trap,trapSprite);
        }
        return true;
    }
    public override string GetTrapName()
    {
        return "诱捕器";
    }
    public override bool PlaceTrap(BoardScript board)
    {
        //2.方块上必须什么都没有
        if (board.boardState == BoardScript.BoardState.Nothing)
        {
            //[本地端.本地方]和[远程端.本地方]的陷阱位置
            CustomProperties.SetPlayerProp("trapPos", board.gridPos.pos, playerNum);
            trapPositions.Add(board.gridPos.pos);
            //回调，将被点击方块改成陷阱
            board.ChangeSprite(BoardScript.BoardState.Trap, trapSprite);
            //修改弹药面板
            MyAnimation.Instance.MinusAmmo(playerId, 1);
        }
        return trapPositions.Count == N;
    }
    public override bool IsInTrap(Vector2 pos)
    {
        dmgIdx = trapPositions.FindIndex(p => p.x == pos.x && p.y == pos.y);
        return dmgIdx >= 0;
    }
    public override void Ability(BoardScript board)
    {
        board.ChangeSprite(BoardScript.BoardState.Damaged, dmgTrapSprite);
        trapPositions.RemoveAt(dmgIdx);//移除这个陷阱
        extraAmmos = N - trapPositions.Count;
        return;
    }
    public override int GetExtraAmmos()
    {
        return extraAmmos;
    }

    #endregion

}
