using System.Collections;
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
    public virtual void SendStartMessage()
    {
        CustomProperties.SetPlayerProp("startAbility", 0, playerNum);
    }
    public virtual void SendOverMessage()
    {
        CustomProperties.SetPlayerProp("overAbility", 0, playerNum);
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (targetPlayer.ActorNumber != playerNum) return;
        object tempObj;
        //[开始]
        if (changedProps.TryGetValue("startAbility", out tempObj))
        {
            if (targetPlayer.IsLocal)
                Ability(true);
            else
                Ability(false);
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
    public abstract void Ability(bool isLocal);
}
