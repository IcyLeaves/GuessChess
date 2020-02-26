using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class HeroManager : MonoBehaviour
{
    //0.光辉女郎
    //1.解脱者
    //2.厄运小姐
    //3.皮城女警
    //4.曙光女神
    //5.蛮族之王

    public static HeroManager Instance;
    public GameObject[] heroIconPrefabs;
    public GameObject[] heroCardPrefabs;
    public GameObject darkPanel;

    private void Start()
    {
        Instance = this;
    }



}
