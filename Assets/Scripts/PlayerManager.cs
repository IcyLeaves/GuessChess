using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public PlayerState myState;
    public enum PlayerState
    {
        ChooseNumbers,OthersTurn
    }
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
    }
}
