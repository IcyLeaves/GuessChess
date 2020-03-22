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

//【重整旗鼓】每回合限一次,你可以阻止自己在当前轮次失败,若阻止成功,本回合内阈值变为30
public class Hero07 : Hero
{
    public int newGoal = 30;//新阈值
    public bool canUse = false;
    public bool isProtect = false;
    public GameObject defense;
    private GameObject defenseInstance;
    public enum AbilityState
    {
        Protect, Escape
    }
    #region override
    protected override void StartAbility(Player targetPlayer, Hashtable changedProps)
    {
        
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
        canUse = false;
        return false;
    }

    public override bool OnEveryTurnOver()
    {
        isProtect = false;
        if(defenseInstance!=null)
        {
            Destroy(defenseInstance);
            defenseInstance = null;
        }
        return false;
    }
    public override bool OnFailure(bool isLocal)
    {
        if(isProtect)
        {
            MyAnimation.Instance.SkillTrigger(heroId, isLocal);
            StartCoroutine(MyAnimation.Instance.DelayToInvokeDo(delegate ()
            {
                Ability(isLocal, (int)AbilityState.Escape);
            }, MyAnimation.myAnimationTime));
            return true;
        }
        return false;
    }
    public override void Ability(bool isLocal, int abilityState)
    {

        switch (abilityState)
        {
            case (int)AbilityState.Protect:
                //保护阶段：保护后，这回合不会失败
                isProtect = true;
                defenseInstance = Instantiate(defense);
                OnAbilityOver();
                break;
            case (int)AbilityState.Escape:
                //逃跑阶段：保护成功，修改阈值。
                GameManager.Instance.goalNum = newGoal;
                MyAnimation.Instance.LoadSum(-1, 1);
                break;
        }

        #endregion
    }
}
