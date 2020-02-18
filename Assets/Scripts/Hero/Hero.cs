﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;

public abstract class Hero:MonoBehaviourPunCallbacks
{
    static protected  DarkPanelScript darkPanel;
    public bool isPassive = false;
    public int playerNum=-1;

    private void Start()
    {
        darkPanel = HeroManager.Instance.darkPanel.GetComponent<DarkPanelScript>();
        playerNum = gameObject.GetComponent<HeroIconScript>().playerNumber;
    }
    public virtual void SendStartMessage(int val=0)
    {
        CustomProperties.SetPlayerProp("startAbility", val, playerNum);
    }
    public virtual void SendOverMessage(int val = 0)
    {
        CustomProperties.SetPlayerProp("overAbility", val, playerNum);
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (targetPlayer.ActorNumber != playerNum) return;
        object tempObj;
        //[开始]
        if (changedProps.TryGetValue("startAbility", out tempObj))
        {
            int state = (int)tempObj;
            if (targetPlayer.IsLocal)
                Ability(true, state);
            else
                Ability(false, state);
        }
        //[结束]
        if (changedProps.TryGetValue("overAbility", out tempObj))
        {
            OnAbilityOver();
        }
    }
    public virtual bool OnMyTurnStart()
    {
        return false;
    }
    public virtual bool OnMyTurnOver()
    {
        return false;
    }
    public virtual bool OnRoundStart()
    {
        return false;
    }
    public virtual bool OnAbilityOver()
    {
        darkPanel.OnCloseBtnClick();
        return false;
    }
    public virtual void OnEnemyNumberSelected(int selectNum)
    {
        return;
    }
    public abstract void Ability(bool isLocal,int abilityState=0);
}
