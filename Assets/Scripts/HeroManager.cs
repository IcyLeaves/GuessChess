using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class HeroManager : MonoBehaviour
{
    public static HeroManager Instance;
    public GameObject[] heroIconPrefabs;
    public GameObject[] heroCardPrefabs;
    public GameObject darkPanel;

    private void Start()
    {
        Instance = this;
    }


    #region 1-解脱者
    private int JTZ_predict;
    private int JTZ_result;
    public void JTZ_Predict()
    {

    }
    #endregion

    #region 2-厄运小姐

    public void EYXJ()
    {
        EYXJ_RandomQuests();
    }
    public int[] EYXJ_RandomQuests()
    {
        List<int> numbers = new List<int>() { 0, 1, 2, 3, 4 };
        int[] quests = new int[5];
        for (int i = 0; i < quests.Length; i++)
        {
            int numIdx = Random.Range(0, numbers.Count);//选出取出哪个数字
            quests[i] = numbers[numIdx];//放入
            numbers.RemoveAt(numIdx);//去除这个数字
        }
        return quests;
    }

    #endregion



}
