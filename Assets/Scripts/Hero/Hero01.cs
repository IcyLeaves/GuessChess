using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

//【读心术】每回合限一次，你可以猜测对方下一轮选择的数字，如果猜对，则在你的轮次开始时，窥探当前累计值
public class Hero01 : Hero
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

    #region override
    protected override void StartAbility(Player targetPlayer, Hashtable changedProps)
    {
        object tempObj;
        //[开始]
        if (changedProps.TryGetValue("startAbility", out tempObj))
        {
            int state = tempObj == null ? (int)AbilityState.Predict : (int)AbilityState.Reveal;
            bool isLocal = false;
            if (targetPlayer.IsLocal)
                isLocal = true;
            MyAnimation.Instance.SkillTrigger(heroId, isLocal);
            StartCoroutine(MyAnimation.Instance.DelayToInvokeDo(delegate ()
            {
                Ability(isLocal,state);
            }, MyAnimation.myAnimationTime));
        }
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
        
        switch (abilityState)
        {
            case (int)AbilityState.Predict:
                //预测阶段：按钮点击后反馈预测数字
                //在Dark Panel下生成临时obj
                darkPanel.tempPanel = Instantiate(JTZ_Panel);
                darkPanel.tempPanel.transform.SetParent(darkPanel.transform);
                darkPanel.tempPanel.transform.localPosition = Vector2.zero;
                GameObject predictPanel = darkPanel.tempPanel.transform.GetChild(0).gameObject;
                predictPanel.SetActive(true);
                if (isLocal)
                {
                    predictPanel.GetComponentInChildren<TMP_Text>().text = "预测对方下一轮选择的数字";
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
                    predictPanel.GetComponentInChildren<TMP_Text>().text = "对方正在预测你下一轮选择的数字";
                }
                darkPanel.gameObject.SetActive(true);
                break;
            case (int)AbilityState.Reveal:
                //揭示阶段：如预测成功，则揭示结果
                if (isLocal)
                {
                    MyAnimation.Instance.LoadSum(1, -1);
                }
                break;
        }
        
    }
    #endregion

    //private IEnumerator Fade(float seconds)
    //{
    //    //seconds秒后才发送关闭Panel消息
    //    yield return new WaitForSeconds(seconds);
    //    SendOverMessage();
    //}
    public void PredictNum(int val)
    {
        predictNum = val;
        SendOverMessage();
    }
}
