﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;

//【堡垒】游戏开始时：你可以额外放置10个护盾。护盾能防止一次方格的消亡
public class Hero04 : Hero
{
    //游戏开始时的【放置陷阱】环节来放置10个护盾
    //护盾不会覆盖Board的状态，而是作为一个浮空的object盖在上面
    //护盾生效时起一次作用后，就没有用了，一个单纯的贴图Sprite
    //生成的位置需要借助board的position
    public int N = 10;
    public Sprite barrierSprite;
    public Sprite dmgBarrierSprite;
    public List<Vector2> barrierPositions;
    public List<GameObject> barriers;
    public int dmgIdx;//受到攻击的护盾的索引

    public GameObject barrier;

    #region override
    protected override void OtherPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        object tempObj;
        //[陷阱位置]
        if (changedProps.TryGetValue("barrierPos", out tempObj))
        {
            //只接受远程数据
            if (!targetPlayer.IsLocal)
            {
                barrierPositions.Add((Vector2)tempObj);
                if (barrierPositions.Count == N)
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
            Hover.Instance.Activate(barrierSprite);
        MyAnimation.Instance.SkillTrigger(heroId, isLocal);
        MyAnimation.Instance.LoadAmmo(playerId, N, N, barrierSprite);
        return true;
    }
    public override bool OnGameOver()
    {
        //只有对方的英雄才需要触发
        if (PhotonNetwork.LocalPlayer.ActorNumber == playerNum) return false;
        int playerId = GameManager.Instance.GetPlayerByActorNumber(playerNum).myIdx;
        foreach (var barrierPos in barrierPositions)
        {
            CreateBarrier(BoardManager.Instance.GetPosBoard(playerId,barrierPos).transform.position);//产生护盾
        }
        return true;
    }
    public override bool PlaceTrap(BoardScript board)
    {
        Vector2 pos = board.gridPos.pos;
        //2.方块上必须什么都没有
        if (!IsInTrap(pos))
        {
            //[本地端.本地方]和[远程端.本地方]的陷阱位置
            CustomProperties.SetPlayerProp("barrierPos", pos, playerNum);
            barrierPositions.Add(pos);
            barriers.Add(CreateBarrier(board.transform.position));//这里创建了对象
            //修改弹药面板
            MyAnimation.Instance.MinusAmmo(playerId, 1);
        }
        return barrierPositions.Count == N;
    }

    public override string GetTrapName()
    {
        return "护盾";
    }
    public override bool IsInTrap(Vector2 pos)
    {
        dmgIdx = barrierPositions.FindIndex(p => p.x == pos.x && p.y == pos.y);
        return dmgIdx >= 0;
    }
    public override void Ability(BoardScript board)
    {
        barrierPositions.RemoveAt(dmgIdx);//移除这个陷阱
        if (barriers.Count > 0)//说明是本地的护盾被击碎
        {
            barriers[dmgIdx].GetComponent<SpriteRenderer>().sprite = dmgBarrierSprite;//击碎护盾
            barriers.RemoveAt(dmgIdx);//移除对象
        }
        else//说明是对方的护盾被击碎
        {
            CreateBarrier(board.transform.position).GetComponent<SpriteRenderer>().sprite = dmgBarrierSprite;//击碎护盾
        }
        return;
    }


    #endregion

    private GameObject CreateBarrier(Vector3 pos)
    {
        return Instantiate(barrier, pos, Quaternion.identity);
    }
}
