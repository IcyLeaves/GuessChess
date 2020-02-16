using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GHNL : Hero
{
    private int N = 5;
    public bool canUse = false;
    public GameObject GHNL_Panel;

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
        yield return new WaitForSeconds(seconds);
        SendOverMessage();
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
        if(isLocal)
        {
            //技能内容，目前是生成预测文本
            GHNL_Panel.GetComponentInChildren<Text>().text = "累计值在这五个数之中：\n";
            var res = RandomNums(GameManager.Instance.nowSum, GameManager.Instance.goalNum);
            for (int i = 0; i < N; i++)
            {
                GHNL_Panel.GetComponentInChildren<Text>().text += res[i] + " ";
            }
        }
        else
        {
            //技能内容，目前是生成预测文本
            GHNL_Panel.GetComponentInChildren<Text>().text = 
                "累计值在这五个数之中：\n"+
                "? ? ? ? ?";
        }
        //在Dark Panel下生成临时obj
        darkPanel.tempPanel = Instantiate(GHNL_Panel);
        darkPanel.tempPanel.transform.SetParent(darkPanel.transform);
        darkPanel.tempPanel.transform.localPosition=Vector2.zero;
        darkPanel.gameObject.SetActive(true);
        //两秒后自动消失
        StartCoroutine(Fade(2f));
        return;
    }
}
