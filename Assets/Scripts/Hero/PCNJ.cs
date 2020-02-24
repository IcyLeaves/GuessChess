using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;

public class PCNJ : Hero
{
    public int N = 5;
    public Sprite trapSprite;
    public Sprite dmgTrapSprite;
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
                trapPositions.Add((Vector2)tempObj);
        }
    }

    public override bool OnStarPlaced(bool isLocal)
    {
        if (isLocal)
            Hover.Instance.Activate(trapSprite);
        GameManager.Instance.logText.text = "请放置" + GetTrapName() + "，还剩"+N+"个";
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
            //修改提示文本
            GameManager.Instance.logText.text = "请放置诱捕器，还剩" + (N - trapPositions.Count) + "个";
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
