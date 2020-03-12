using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyAnimation : MonoBehaviour
{
    public const float myAnimationTime=2.0f;
    public static MyAnimation Instance;
    public static bool locked { get; private set; }//锁，true代表上锁，false代表解锁

    public GameObject LOG;
    public SpriteRenderer background;
    public SpriteRenderer firstNum;
    public SpriteRenderer secondNum;
    public SpriteRenderer content;
    public SpriteRenderer skill;
    public List<Sprite> contentSprites;
    public List<Sprite> skillSprites;
    public List<Sprite> numSprites;
    public List<Sprite> skillContentSprites;
    public List<Sprite> backgroundSprites;
    public enum Contents
    {
        Star,Round,Win,Lose
    }

    public GameObject[] AMMO;
    public SpriteRenderer[,] ammoNums=new SpriteRenderer[2,4];
    public SpriteRenderer[] ammoCard;
    public Sprite[] upNumSprites;
    public Sprite[] downNumSprites;
    public Sprite defaultSprite;
    private int[] all=new int[2];
    private int[] rest = new int[2];

    public GameObject SUM;
    public SpriteRenderer[] sumNums;
    private int canExceed = -1;
    private int canThreshold = -1;
    public Sprite[] exceedSprites;
    public Sprite[] thresholdSprites;

    private void Start()
    {
        Instance = this;
        locked = false;
        ammoNums[0, 0] = AMMO[0].transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
        ammoNums[0, 1] = AMMO[0].transform.GetChild(2).gameObject.GetComponent<SpriteRenderer>();
        ammoNums[0, 2] = AMMO[0].transform.GetChild(3).gameObject.GetComponent<SpriteRenderer>();
        ammoNums[0, 3] = AMMO[0].transform.GetChild(4).gameObject.GetComponent<SpriteRenderer>();
        ammoNums[1, 0] = AMMO[1].transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
        ammoNums[1, 1] = AMMO[1].transform.GetChild(2).gameObject.GetComponent<SpriteRenderer>();
        ammoNums[1, 2] = AMMO[1].transform.GetChild(3).gameObject.GetComponent<SpriteRenderer>();
        ammoNums[1, 3] = AMMO[1].transform.GetChild(4).gameObject.GetComponent<SpriteRenderer>();
    }

    private void Activate(GameObject obj, float seconds)
    {
        locked = true;
        obj.SetActive(true);
        StartCoroutine(Deactivate(obj, seconds));//myAnimationTime后自动关闭并解锁
    }
    private IEnumerator Deactivate(GameObject obj,float seconds)
    {
        yield return new WaitForSeconds(seconds);
        obj.SetActive(false);
        locked = false;
    }
    //延时执行
    public IEnumerator DelayToInvokeDo(Action action, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        action();
    }

    #region Log
    //没有数字、插画，橙色背景的情况
    public void ShowOnlyContent(Contents state)
    {
        firstNum.gameObject.SetActive(false);
        secondNum.gameObject.SetActive(false);
        skill.gameObject.SetActive(false);
        background.sprite = backgroundSprites[0];
        content.sprite = contentSprites[(int)state];
        Activate(LOG, myAnimationTime);

    }
    public void SkillTrigger(int heroId,bool isLocal)
    {
        firstNum.gameObject.SetActive(false);
        secondNum.gameObject.SetActive(false);
        skill.gameObject.SetActive(true);
        background.sprite = backgroundSprites[isLocal?1:2];
        content.sprite = skillContentSprites[heroId];
        skill.sprite = skillSprites[heroId];
        Activate(LOG, myAnimationTime);
    }
    public void Round(int round)
    {
        firstNum.gameObject.SetActive(true);
        secondNum.gameObject.SetActive(true);
        skill.gameObject.SetActive(false);
        background.sprite = backgroundSprites[0];
        firstNum.sprite = numSprites[round / 10];
        secondNum.sprite = numSprites[round % 10];
        content.sprite = contentSprites[(int)Contents.Round];
        Activate(LOG, myAnimationTime);
    }
    #endregion

    #region Ammo
    public void numToSprite(int playerIdx)
    {
        int upFirst = rest[playerIdx] / 10;
        int upSecond = rest[playerIdx] % 10;
        int downFirst = all[playerIdx] / 10;
        int downSecond = all[playerIdx] % 10;
        ammoNums[playerIdx, 0].sprite = upNumSprites[upFirst];
        ammoNums[playerIdx, 1].sprite = upNumSprites[upSecond];
        ammoNums[playerIdx, 2].sprite = downNumSprites[downFirst];
        ammoNums[playerIdx, 3].sprite = downNumSprites[downSecond];
    }
    public void LoadAmmo(int playerIdx,int restammos,int ammos,Sprite sprite=null)
    {
        if (sprite == null) sprite = defaultSprite;
        rest[playerIdx] = restammos;
        all[playerIdx] = ammos;
        AMMO[playerIdx].SetActive(true);
        ammoCard[playerIdx].sprite = sprite;
        numToSprite(playerIdx);
    }
    public void MinusAmmo(int playerIdx, int nums)
    {
        rest[playerIdx] -= nums;
        numToSprite(playerIdx);
    }
    public void StopAmmo(int playerIdx=-1)
    {
        if(playerIdx>=0)
            AMMO[playerIdx].SetActive(false);
        else
        {
            AMMO[0].SetActive(false);
            AMMO[1].SetActive(false);
        }
    }
    #endregion

    #region Sum
    public void sumToSprite(int canUp, int canDown)
    {
        int upFirst = GameManager.Instance.nowSum / 10;
        int upSecond = GameManager.Instance.nowSum  % 10;
        int downFirst = GameManager.Instance.goalNum / 10;
        int downSecond = GameManager.Instance.goalNum % 10;
        if(canUp==0 || canUp ==1) canExceed = canUp;
        if (canDown == 0 || canDown == 1) canThreshold = canDown;
        //Debug.Log(upFirst+" "+ upSecond+" "+ downFirst+" "+ downSecond);
        sumNums[0].sprite = canExceed==1 ? exceedSprites[upFirst] : exceedSprites[10];
        sumNums[1].sprite = canExceed == 1 ? exceedSprites[upSecond] : exceedSprites[10];
        sumNums[2].sprite = canThreshold == 1 ? thresholdSprites[downFirst] : thresholdSprites[10];
        sumNums[3].sprite = canThreshold == 1 ? thresholdSprites[downSecond] : thresholdSprites[10];
    }
    /// <summary>
    /// 加载累计值面板
    /// </summary>
    /// <param name="ableNowSum">0：关闭，1：打开，其他值：维持原状</param>
    /// <param name="ableGoalNum">0：关闭，1：打开，其他值：维持原状</param>
    public void LoadSum(int ableNowSum,int ableGoalNum)
    {
        SUM.SetActive(true);
        sumToSprite(ableNowSum, ableGoalNum);
    }
    #endregion
}
