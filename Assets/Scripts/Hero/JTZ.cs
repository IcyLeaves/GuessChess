using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class JTZ : Hero
{
    //OnMyTurnStart:
    //在自己轮次开始时呈现是否可用
    //判断预测是否正确？发动Ability(local,.Reveal)：当作无事发生
    //OnMyTurnOver:在自己轮次结束时显示不可用
    //OnRoundStart:在回合开始时恢复可用
    //OnAbilityOver:技能结束后，设置不可用并呈现
    //OnEnemyNumberSelected:在对方选择数字后改变actualNum

    public bool canUse = false;
    public int predictNum = -1;
    public int actualNum = -1;
    public bool isPredict = false;
    public GameObject JTZ_Panel;
    private enum AbilityState
    {
        Predict, Reveal
    }
    private IEnumerator Fade(float seconds)
    {
        //seconds秒后才发送关闭Panel消息
        yield return new WaitForSeconds(seconds);
        SendOverMessage();
    }


    public override bool OnMyTurnStart()
    {
        if (isPredict)
        {
            if (predictNum > 0 && actualNum == predictNum)
            {
                SendStartMessage((int)AbilityState.Reveal);
            }
        }
        isPredict = false;
        return canUse;
    }

    public override bool OnRoundStart()
    {
        canUse = true;
        predictNum = -1;
        actualNum = -1;
        isPredict = false;
        return false;
    }

    public override bool OnAbilityOver()
    {
        base.OnAbilityOver();
        canUse = false;
        return false;
    }
    public override void OnEnemyNumberSelected(int selectNum)
    {
        actualNum = selectNum;
    }
    public override void Ability(bool isLocal, int abilityState)
    {
        //在Dark Panel下生成临时obj
        darkPanel.tempPanel = Instantiate(JTZ_Panel);
        darkPanel.tempPanel.transform.SetParent(darkPanel.transform);
        darkPanel.tempPanel.transform.localPosition = Vector2.zero;
        GameObject predictPanel = darkPanel.tempPanel.transform.GetChild(0).gameObject;
        GameObject revealPanel = darkPanel.tempPanel.transform.GetChild(1).gameObject;
        switch (abilityState)
        {
            case (int)AbilityState.Predict:
                //预测阶段：按钮点击后反馈预测数字
                predictPanel.SetActive(true);
                revealPanel.SetActive(false);
                if (isLocal)
                {
                    predictPanel.GetComponentInChildren<Text>().text = "预测对方下一轮选择的数字";
                    int i = 1;
                    foreach (var btn in predictPanel.GetComponentsInChildren<Button>())
                    {
                        int _i = i;
                        btn.interactable = true;
                        btn.onClick.AddListener(() =>
                        {
                            PredictNum(_i);
                        });
                        i++;
                    }
                    isPredict = true;
                }
                else
                {
                    predictPanel.GetComponentInChildren<Text>().text = "对方正在预测你下一轮选择的数字";
                }
                break;
            case (int)AbilityState.Reveal:
                //揭示阶段：如预测成功，则揭示结果
                revealPanel.SetActive(true);
                if (isLocal)
                {
                    revealPanel.GetComponentInChildren<Text>().text = "当前累计值为：\n" + GameManager.Instance.nowSum;
                }
                else
                {
                    revealPanel.GetComponentInChildren<Text>().text = "当前累计值为：\n" + "?";
                }
                //两秒后自动消失
                StartCoroutine(Fade(2f));
                break;
        }
        darkPanel.gameObject.SetActive(true);
    }

    public void PredictNum(int val)
    {
        predictNum = val;
        SendOverMessage();
    }
}
