using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using System;

public abstract class Hero:MonoBehaviourPunCallbacks
{
    static protected  DarkPanelScript darkPanel;
    public int heroId = -1;
    public bool isPassive = false;
    public int playerNum=-1;
    public int playerId = -1;

    private void Start()
    {
        darkPanel = HeroManager.Instance.darkPanel.GetComponent<DarkPanelScript>();
        playerNum = gameObject.GetComponent<HeroIconScript>().playerNumber;
        playerId = gameObject.transform.parent.position.x < 0 ? 0 : 1;
    }
    public virtual void SendStartMessage(object val=null)
    {
        CustomProperties.SetPlayerProp("startAbility", val, playerNum);
    }
    public virtual void SendOverMessage(object val = null)
    {
        CustomProperties.SetPlayerProp("overAbility", val, playerNum);
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (targetPlayer.ActorNumber != playerNum) return;
        StartAbility(targetPlayer, changedProps);
        EndAbility(targetPlayer, changedProps);
        OtherPlayerPropertiesUpdate(targetPlayer, changedProps);
    }
    protected virtual void StartAbility(Player targetPlayer, Hashtable changedProps)
    {
        object tempObj;
        //[开始]
        if (changedProps.TryGetValue("startAbility", out tempObj))
        {
            bool isLocal = false;
            if (targetPlayer.IsLocal)
                isLocal = true;
            Ability(isLocal);
        }
    }
    protected virtual void EndAbility(Player targetPlayer,Hashtable changedProps)
    {
        object tempObj;
        //[结束]
        if (changedProps.TryGetValue("overAbility", out tempObj))
        {
            OnAbilityOver();
        }
    }
    protected virtual void OtherPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        return;
    }

    public virtual bool OnMyTurnStart()
    {
        return false;
    }
    public virtual bool OnFailure()
    {
        return false;
    }
    public virtual bool OnMyTurnOver()
    {
        return false;
    }
    public virtual bool OnEveryTurnOver()
    {
        return false;
    }
    public virtual bool OnRoundStart()
    {
        return false;
    }
    public virtual bool OnMyAttackStart()
    {
        return false;
    }
    public virtual bool OnMyOnceAttackOver()
    {
        return false;
    }
    public virtual int GetExtraAmmos()
    {
        return 0;
    }
    public virtual bool OnMyStarRuined()
    {
        return false;
    }
    public virtual bool OnAttackOver()
    {
        return false;
    }
    public virtual bool OnGameOver()
    {
        return false;
    }
    #region 【领地】
    public virtual bool OnStarPlaced(bool isLocal)
    {
        return false;
    }
    public virtual bool PlaceTrap(BoardScript board)
    {
        return false;
    }
    public virtual string GetTrapName()
    {
        return "";
    }
    public virtual bool IsInTrap(Vector2 pos)
    {
        return false;
    }
    #endregion
    public virtual bool OnAbilityOver()
    {
        darkPanel.OnCloseBtnClick();
        return false;
    }
    public virtual void OnEnemyNumberSelected(int selectNum)
    {
        return;
    }
    public virtual void Ability(bool isLocal)
    {
        return;
    }
    public virtual void Ability(bool isLocal,int abilityState)
    {
        return;
    }
    public virtual void Ability(BoardScript board)
    {
        return;
    }
}
