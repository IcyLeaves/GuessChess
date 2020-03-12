using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;

//【跃迁】回合开始时:本回合的阈值随机增加0-4点,然后你窥探这个阈值.
public class Hero06 : Hero
{
    int minN = 0;
    int maxN = 4;
    public GameObject Panel06;

    private TMP_Text numText;
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
            StartCoroutine(MyAnimation.Instance.DelayToInvokeDo(delegate ()
            {
                //Debug.Log(isLocal);
                Ability(isLocal);
            }, MyAnimation.myAnimationTime));
        }
    }
    protected override void OtherPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        object tempObj;
        //[阈值增加]
        if (changedProps.TryGetValue("thresholdAdd", out tempObj))
        {
            if(!targetPlayer.IsLocal)
            {
                GameManager.Instance.goalNum += (int)tempObj;
                //Debug.Log("other:"+GameManager.Instance.goalNum + " " + (int)tempObj);
            }

        }
    }

    public override bool OnRoundStart()
    {
        if(playerId==GameManager.Instance.localIdx)
        SendStartMessage();
        return true;
    }
    public override void Ability(bool isLocal)
    {
        if (isLocal)
        {
            int val = RandomNumber();
            
            GameManager.Instance.goalNum += val;
            //Debug.Log("local:" + GameManager.Instance.goalNum + " " + val);
            CustomProperties.SetPlayerProp("thresholdAdd", val, playerNum);
            MyAnimation.Instance.LoadSum(0, -1);//隐藏累计值，显示阈值
        }
        else
        {
            MyAnimation.Instance.LoadSum(-1, 0);
        }
        return;
    }
    #endregion

    private int RandomNumber()
    {
        return Random.Range(minN, maxN + 1);
    }
}
