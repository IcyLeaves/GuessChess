using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;

//【坍缩】每回合限一次，你可以选择一次数字0，然后若当前累计值是5的正整数倍，对方随机消亡3个方格。
public class Hero08 : Hero
{
    public bool canUse = false;
    public int N = 3;
    public int damaged = 0;
    public float delayTime = 2f;
    public GameObject eclipseObj;
    public enum AbilityState
    {
        Use,Critical
    }

    #region override
    protected override void StartAbility(Player targetPlayer, Hashtable changedProps)
    {
        object tempObj;
        //[开始]
        if (changedProps.TryGetValue("startAbility", out tempObj))
        {
            bool isLocal = false;
            if (targetPlayer.IsLocal)
                isLocal = true;
            MyAnimation.Instance.SkillTrigger(heroId, isLocal);
            foreach (var tile in GameManager.Instance.tiles)
            {
                tile.ChangeSprite(TileScript.TileState.Disable);
            }
            StartCoroutine(MyAnimation.Instance.DelayToInvokeDo(delegate ()
            {
                Ability(isLocal);
            }, MyAnimation.myAnimationTime));
        }
    }
    protected override void OtherPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        object tempObj;
        //[随机消亡位置]
        if (changedProps.TryGetValue("randomPos", out tempObj))
        {
            Vector2 pos= (Vector2)tempObj;
            Ability(BoardManager.Instance.GetPosBoard(targetPlayer.IsLocal?GameManager.Instance.otherIdx:GameManager.Instance.localIdx,pos));
        }
    }

    public override bool OnMyTurnStart()
    {
        return canUse;
    }
    public override bool OnRoundStart()
    {
        canUse = true;
        return false;
    }
    public override bool OnAbilityOver()
    {
        damaged = 0;
        canUse = false;
        return false;
    }
    public override void Ability(bool isLocal)
    {
        if(isLocal)
        {
            damaged = 0;
            if (GameManager.Instance.nowSum % 5 == 0 && GameManager.Instance.nowSum / 5 > 0)
            {
                AttackOnce();
            }
            else
            {
                CustomProperties.SetLocalPlayerProp("selectNum", 0);
            }
        }
    }
    public override void Ability(BoardScript board)
    {
        GameManager.Instance.attackPos = board.gridPos.pos;
        GameManager.Instance.AttackBoard(1 - board.gridPos.playerIdx);
        //引用GameManager的流程
        if (GameManager.Instance.IsGameOver())
        {
            MyAnimation.Instance.GameLockedForever();
            int roundWinnerIdx = GameManager.Instance.SetRoundWinner();
            GameManager.Instance.InitGameOver(roundWinnerIdx);//显示游戏结束画面
            StartCoroutine(MyAnimation.Instance.DelayToInvokeDo(delegate ()
            {
                GameManager.Instance.RestartGame();//重新开始游戏
            }, 5.0f));
        }
        damaged++;
        if(damaged<N && board.gridPos.playerIdx==GameManager.Instance.otherIdx)
        {
            //Debug.Log("next");
            AttackOnce();
        }
        else if(damaged==N)
        {
            GameManager.Instance.selectNum = 0;
            OnAbilityOver();
        }
    }
    #endregion

    private void AttackOnce()
    {
        var board = BoardManager.Instance.PickRandomAlive(GameManager.Instance.otherIdx);
        var pos = board.gridPos.pos;
        var worldPos = board.transform.position;
        PlayAnimation(worldPos);
        StartCoroutine(MyAnimation.Instance.DelayToInvokeDo(delegate ()
        {
            CustomProperties.SetLocalPlayerProp("randomPos", pos);
        }, delayTime));
    }

    private void PlayAnimation(Vector3 worldPos)
    {
        var g = Instantiate(eclipseObj,worldPos,Quaternion.identity);
        g.GetComponent<Animator>().speed = 1f;
        StartCoroutine(MyAnimation.Instance.DelayToInvokeDo(delegate ()
        {
            Destroy(g);
        }, delayTime));
    }
}
