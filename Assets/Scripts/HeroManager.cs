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



}
