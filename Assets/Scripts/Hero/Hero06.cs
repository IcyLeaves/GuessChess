using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;

//【跃迁】回合开始时:本回合阈值增加0-4点,然后你窥探阈值.在你每轮结束时,若累计值等于20,令其等于阈值
public class Hero06 : Hero
{
    public int minN = 0;
    public int maxN = 4;
    public int equalSum = 20;
    public int otherAdd = -1;
    public GameObject Panel06;

    private TMP_Text numText;
    #region override
    protected override void StartAbility(Player targetPlayer, Hashtable changedProps)
    {

    }
    protected override void OtherPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        object tempObj;
        //[阈值增加]
        if (changedProps.TryGetValue("thresholdAdd", out tempObj))
        {
            if (!targetPlayer.IsLocal)
            {
                otherAdd= (int)tempObj;
                
            }

        }
    }

    public override bool OnRoundStart()
    {
        MyAnimation.Instance.SkillTrigger(heroId, playerId == GameManager.Instance.localIdx);
        Ability(playerId == GameManager.Instance.localIdx,0);
        return true;
    }
    public override bool OnNowSumChanged(bool isLocal)
    {
        //累计值改变后时判断累计值和20是否相等
        if (GameManager.Instance.nowSum == equalSum)
        {
            Ability(isLocal, 1);
            return true;
        }
        return false;
    }

    public override void Ability(bool isLocal, int abilityState)
    {
        if (isLocal)
        {
            if (abilityState == 1)
            {
                MyAnimation.Instance.SkillTrigger(heroId, true);
                GameManager.Instance.nowSum = GameManager.Instance.goalNum;
                MyAnimation.Instance.LoadSum(1, 1);//显示累计值，显示阈值
            }
            else
            {
                int val = RandomNumber();

                GameManager.Instance.goalNum += val;
                Debug.Log("local:" + GameManager.Instance.goalNum + " " + val);
                CustomProperties.SetPlayerProp("thresholdAdd", val, playerNum);
                MyAnimation.Instance.LoadSum(0, -1);//隐藏累计值，显示阈值
            }
        }
        else
        {
            if (abilityState == 1)
            {
                MyAnimation.Instance.WaitForNull();//干等两秒
                GameManager.Instance.nowSum = GameManager.Instance.goalNum;
            }
            else
            {
                MyAnimation.Instance.LoadSum(-1, 0);
                StartCoroutine(WaitForOther());
            }

        }
        return;
    }
    #endregion

    private int RandomNumber()
    {
        return Random.Range(minN, maxN + 1);
    }

    private IEnumerator WaitForOther()
    {
        yield return new WaitUntil(()=>otherAdd >= 0);
        Debug.Log("other:" + GameManager.Instance.goalNum + " " + otherAdd);
        GameManager.Instance.goalNum += otherAdd;
    }
}
