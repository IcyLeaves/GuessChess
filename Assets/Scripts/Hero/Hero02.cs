using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

//【赌怪】获胜：你可以改为从5个宝箱中获取弹药(可以随时放弃，因为其中一个宝箱会清零你的弹药)
public class Hero02 : Hero
{
    public int ammos;
    public GameObject EYXJ_Panel;
    public bool canUse = false;
    public int[] quests;
    public Sprite[] ammoSprites;


    private Button[] questBtns;
    private Button stopBtn;
    private TMP_Text ammoText;

    #region override
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (targetPlayer.ActorNumber != playerNum) return;
        object tempObj;
        //[开始]
        if (changedProps.TryGetValue("startAbility", out tempObj))
        {
            Hover.Instance.Deactivate();
            if (targetPlayer.IsLocal)
                Ability(true);
            else
                Ability(false);
        }
        //[结束]
        if (changedProps.TryGetValue("overAbility", out tempObj))
        {
            if (targetPlayer.IsLocal && ammos>0)
                Hover.Instance.Activate(Hover.HoverState.Attack);
            OnAbilityOver();
        }
        OtherPlayerPropertiesUpdate(targetPlayer, changedProps);
    }
    protected override void OtherPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        object tempObj;
        //[宝箱内容]
        if (changedProps.TryGetValue("quests", out tempObj))
        {
            if (!targetPlayer.IsLocal)//只接受远程数据
            {
                quests = (int[])tempObj;
            }
        }
        //[选择的宝箱序号]
        if (changedProps.TryGetValue("chooseQuest", out tempObj))
        {
            if (!targetPlayer.IsLocal)
            {
                ChooseQuest((int)tempObj);
            }
        }
    }

    public override bool OnMyAttackStart()
    {
        canUse = true;
        return canUse;
    }
    public override bool OnMyOnceAttackOver()
    {
        canUse = false;
        return canUse;
    }
    public override void Ability(bool isLocal)
    {
        //在Dark Panel下生成临时obj
        darkPanel.tempPanel = Instantiate(EYXJ_Panel);
        darkPanel.tempPanel.transform.SetParent(darkPanel.transform);
        darkPanel.tempPanel.transform.localPosition = Vector2.zero;
        ammoText = darkPanel.tempPanel.transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
        TMP_Text text = darkPanel.tempPanel.transform.GetChild(1).gameObject.GetComponent<TMP_Text>();
        questBtns = darkPanel.tempPanel.transform.GetChild(2).gameObject.GetComponentsInChildren<Button>();
        stopBtn = darkPanel.tempPanel.transform.GetChild(3).gameObject.GetComponent<Button>();
        ammos = 0;
        ammoText.text = "弹药 × 0";
        if (isLocal)
        {
            RandomQuests();
            //给宝箱添加点击事件
            for (int i = 0; i < questBtns.Length; i++)
            {
                var btn = questBtns[i];
                var _i = i;
                btn.interactable = true;
                btn.onClick.AddListener(() =>
                {
                    CustomProperties.SetPlayerProp("chooseQuest", _i, playerNum);
                    ChooseQuest(_i, btn);
                });
            }
            stopBtn.onClick.AddListener(() =>
            {
                Leave();
            });
        }
        else
        {
            text.text = "对方正在选择宝箱";
            stopBtn.gameObject.SetActive(false);
        }
        darkPanel.gameObject.SetActive(true);
    }
    public override bool OnAbilityOver()
    {
        canUse = false;
        GameManager.Instance.ammos = ammos;//将新的弹药数赋给游戏
        GameManager.Instance.isResetAmmo = true;//游戏准备重置弹药
        return base.OnAbilityOver();
    }
    #endregion

    private IEnumerator Fade(float seconds)
    {
        //seconds秒后才发送关闭Panel消息
        yield return new WaitForSeconds(seconds);
        SendOverMessage();
    }
    public void RandomQuests()
    {
        List<int> numbers = new List<int>() { 0, 2, 3, 4, 5 };
        quests = new int[5];
        for (int i = 0; i < quests.Length; i++)
        {
            int numIdx = Random.Range(0, numbers.Count);//选出取出哪个数字
            quests[i] = numbers[numIdx];//放入
            numbers.RemoveAt(numIdx);//去除这个数字
        }
        CustomProperties.SetPlayerProp("quests", quests,playerNum);
    }
    private void ChooseQuest(int idx, Button btn = null)
    {
        if (btn == null)
        {
            btn = questBtns[idx];
        }
        int val = quests[idx];//val为宝箱内的子弹数量
        SetBtnDisableSprite(btn, val);
        btn.interactable = false;//选过的宝箱不能再点击
        AddAmmos(val);
    }
    private void AddAmmos(int val)
    {
        if (val==0)
        {
            stopBtn.gameObject.SetActive(false);
            foreach(var btn in questBtns)
            {
                btn.interactable = false;
            }
            ammos = 0;
            StartCoroutine(Fade(2));
        }
        ammos += val;
        ammoText.text = "弹药 × " + ammos;
    }
    private void SetBtnDisableSprite(Button btn, int questVal)
    {
        SpriteState spriteState = new SpriteState();
        spriteState.disabledSprite = ammoSprites[questVal];
        btn.spriteState = spriteState;
    }
    private void Leave()
    {
        darkPanel.tempPanel.SetActive(false);//先让技能面板消失免得误操作
        StartCoroutine(Fade(0.01f));//马上关闭Dark Panel
    }
}
