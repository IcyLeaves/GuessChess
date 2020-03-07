using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

//【跃迁】回合开始时:本回合的阈值随机增加0-4点,然后你窥探这个阈值.
public class Hero06 : Hero
{
    int minN = 0;
    int maxN = 4;
    public GameObject Panel06;

    private TMP_Text numText;
    #region override
    public override bool OnRoundStart()
    {
        SendStartMessage();
        return true;
    }
    public override void Ability(bool isLocal)
    {
        //在Dark Panel下生成临时obj
        darkPanel.tempPanel = Instantiate(Panel06);
        darkPanel.tempPanel.transform.SetParent(darkPanel.transform);
        darkPanel.tempPanel.transform.localPosition = Vector2.zero;
        numText= darkPanel.tempPanel.transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
        if (isLocal)
        {
            int val = RandomNumber();
            GameManager.Instance.goalNum += val;
            numText.text = "本回合的阈值为：\n" +
                GameManager.Instance.goalNum + "";
        }
        else
        {
            numText.text = "本回合的阈值为：\n" +
                "?";

        }
        darkPanel.gameObject.SetActive(true);
        //两秒后自动消失
        StartCoroutine(Fade(2f));
        return;
    }
    #endregion

    private int RandomNumber()
    {
        return Random.Range(minN, maxN + 1);
    }

    private IEnumerator Fade(float seconds)
    {
        //seconds秒后才发送关闭Panel消息
        yield return new WaitForSeconds(seconds);
        SendOverMessage();
    }
}
