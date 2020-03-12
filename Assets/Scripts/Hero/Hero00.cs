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

//【天眼】每回合限一次，你可以窥探5个连续的数字，其中有1个是当前的累计值
public class Hero00 : Hero
{
    //OnMyTurnStart:在自己轮次开始时呈现是否可用
    //OnMyTurnOver:在自己轮次结束时显示不可用
    //OnRoundStart:在回合开始时恢复可用
    //OnAbilityOver:技能结束后，设置不可用并呈现

    private int N = 5;
    public bool canUse = false;
    public GameObject GHNL_Panel;

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
                Ability(isLocal);
            }, MyAnimation.myAnimationTime));
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
        base.OnAbilityOver();
        canUse = false;
        return false;
    }
    public override void Ability(bool isLocal)
    {
        if (isLocal)
        {
            //技能内容，目前是生成预测文本
            GHNL_Panel.GetComponentInChildren<TMP_Text>().text = "累计值在这五个数之中：\n";
            var res = RandomNums(GameManager.Instance.nowSum, GameManager.Instance.goalNum);
            for (int i = 0; i < N; i++)
            {
                GHNL_Panel.GetComponentInChildren<TMP_Text>().text += res[i] + " ";
            }
        }
        else
        {
            //敌方无法看见数字但能看见技能效果
            GHNL_Panel.GetComponentInChildren<TMP_Text>().text =
                "累计值在这五个数之中：\n" +
                "? ? ? ? ?";
        }
        //在Dark Panel下生成临时obj
        darkPanel.tempPanel = Instantiate(GHNL_Panel);
        darkPanel.tempPanel.transform.SetParent(darkPanel.transform);
        darkPanel.tempPanel.transform.localPosition = Vector2.zero;
        darkPanel.gameObject.SetActive(true);
        //两秒后自动消失
        StartCoroutine(Fade(2f));
        return;
    }
    #endregion

    private int[] RandomNums(int nowSum, int goalNum)
    {
        int[] res = new int[N];
        //比如nowNum为17，
        int minNum = Math.Max(0, nowSum - N + 1);//minNum=13  (13,14,15,16,17)
        int maxNum = Math.Min(goalNum, nowSum + N - 1);//maxNum=20  (16,17,18,19,20)
        int firstNum = Random.Range(minNum, maxNum - N + 2);//firstNum in {13,14,15,16}
        for (int i = 0; i < res.Length; i++)
        {
            res[i] = firstNum + i;
        }
        return res;
    }
    private IEnumerator Fade(float seconds)
    {
        //seconds秒后才发送关闭Panel消息
        yield return new WaitForSeconds(seconds);
        SendOverMessage();
    }

}
