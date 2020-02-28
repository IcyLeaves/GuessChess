using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class HeroManager : MonoBehaviourPunCallbacks
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

    public int numOfCards = 3;
    public int testCardIdx = -1;
    public int[] roomHeroCardIdxs;
    public int[] myHeroCardIdxs;

    private void Start()
    {
        Instance = this;
        roomHeroCardIdxs = new int[2 * numOfCards];
        myHeroCardIdxs = new int[numOfCards];
        RandomDoubleHeroCards();
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        object tempObj;
        //[卡牌列表]
        if (propertiesThatChanged.TryGetValue("roomHeroCardIdxs", out tempObj))
        {
            roomHeroCardIdxs = (int[])tempObj;
            GetHeroCards();
        }
    }

    private void GetHeroCards()
    {
        int offset = PhotonNetwork.IsMasterClient ? 0 : numOfCards;
        for (int i = 0; i < numOfCards; i++)
        {
            myHeroCardIdxs[i] = roomHeroCardIdxs[i + offset];
        }
        if (testCardIdx != -1)
        {
            myHeroCardIdxs[0] = testCardIdx;
        }
    }

    public void RandomDoubleHeroCards()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //数字列表
            List<int> nums = new List<int>();
            for (int i = 0; i < heroCardPrefabs.Length; i++)
            {
                nums.Add(i);
            }
            //抽一次去掉一个数字
            for (int i = 0; i < numOfCards * 2; i++)
            {
                var idx = Random.Range(0, nums.Count);
                roomHeroCardIdxs[i] = nums[idx];
                nums.RemoveAt(idx);
            }
            CustomProperties.SetRoomProp("roomHeroCardIdxs", roomHeroCardIdxs);
        }
    }
}
