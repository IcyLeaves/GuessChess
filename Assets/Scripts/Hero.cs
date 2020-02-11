using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Hero : MonoBehaviour
{
    public Sprite heroSprite;

    public void SkillTrigger(string hero)
    {
        switch(hero)
        {
            case "EYXJ":
                EYXJ();
                break;
        }
    }

    #region 厄运小姐

    public void EYXJ()
    {
        EYXJ_RandomQuests();
    }
    public int[] EYXJ_RandomQuests()
    {
        List<int> numbers = new List<int>() { 0, 1, 2, 3, 4 };
        int[] quests = new int[5];
        for(int i=0;i<quests.Length;i++)
        {
            int numIdx = Random.Range(0, numbers.Count);//选出取出哪个数字
            quests[i] = numbers[numIdx];//放入
            numbers.RemoveAt(numIdx);//去除这个数字
        }
        return quests;
    }

    #endregion

    #region 解脱者
    private int JTZ_predict;
    private int JTZ_result;
    public void JTZ_Predict()
    {

    }
    #endregion

    #region 光辉女郎
    private int GHNL_N=5;
    public int[] GHNL_RandomNums(int nowNum,int goalNum)
    {
        int[] res = new int[GHNL_N];
        //比如nowNum为17，
        int minNum = Math.Max(0, nowNum - GHNL_N + 1);//minNum=13  (13,14,15,16,17)
        int maxNum = Math.Min(goalNum, nowNum + GHNL_N - 1);//maxNum=20  (16,17,18,19,20)
        int firstNum = Random.Range(minNum, maxNum - GHNL_N+2);//firstNum in {13,14,15,16}
        for(int i=0;i<res.Length;i++)
        {
            res[i] = firstNum + i;
        }
        return res;
    }
    #endregion

}
